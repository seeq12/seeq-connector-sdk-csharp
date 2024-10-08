using System;
using System.IO;
using System.Reflection;
using log4net.Config;
using Seeq.Link.Agent;
using Seeq.Link.SDK.Services;
using Seeq.Link.SDK.Utilities;
using Seeq.Utilities;

namespace Seeq.Link.Debugging.Agent {

    /// <summary>
    /// This class "wraps" the Seeq .NET Agent such that it functions appropriately for connector development and debugging.
    /// It assumes that Seeq Server is installed on the development machine, and performs a similar function to the one that
    /// Supervisor performs in the "real" production environment: It assembles an appropriate set of command line arguments
    /// to connect to the server and load the connector that is under development.
    /// </summary>
    public class EntryPoint {
        private const string AGENT_API_KEY_PLACEHOLDER = "<your_agent_api_key>";
        private const string AGENT_ONE_TIME_PASSWORD_PLACEHOLDER = "<your_one_time_password>";

        public static void Main(string[] args) {
            XmlConfigurator.Configure();
            
            const string agentName = ".NET Connector SDK Debugging Agent";
            var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            var seeqDataFolder = Path.Combine(Path.GetDirectoryName(executingAssemblyLocation), "data");

            var agentKeyPath = Path.Combine(seeqDataFolder, "keys", "agent.key");
            var agentKeyReader = new AgentKeyReader(agentKeyPath);

            // if this agent has been configured with an agent key, then the agent will handle provisioning and no additional
            // provisioning is required. Otherwise, set the pre-provisioned one-time password and allow the agent finish up
            // provisioning.
            if (string.IsNullOrWhiteSpace(agentKeyReader.AgentKeyCredential.AgentKeyPassword) 
                || agentKeyReader.AgentKeyCredential.AgentKeyPassword == AGENT_API_KEY_PLACEHOLDER) {
                const string agentOneTimePassword = AGENT_ONE_TIME_PASSWORD_PLACEHOLDER;

                if (agentOneTimePassword != AGENT_ONE_TIME_PASSWORD_PLACEHOLDER) {
                    var agentHelper = new AgentHelper(agentName);
                    var secretsPath = Path.Combine(seeqDataFolder, SeeqNames.Agents.AgentKeysFolderName, "agent.keys");
                    var secretsManager = new FileBasedSecretsManager(secretsPath);

                    // set the agent's pre-provisioned one-time password
                    var preProvisionedOneTimePasswordSecretName =
                        $"{agentHelper.ProvisionedAgentUsername}|PRE_PROVISIONED_ONE_TIME_PASSWORD";
                    secretsManager.PutSecret(preProvisionedOneTimePasswordSecretName, agentOneTimePassword);
                }
            }
            
            Program.Configuration config = Program.GetDefaultConfiguration();

            const string seeqHostUrl = "https://yourserver.seeq.host";
            config.SeeqUrl = new Uri(seeqHostUrl);
            config.SeeqExternalUrl = new Uri(seeqHostUrl);
            config.SeeqWebSocketUrl = new Uri(seeqHostUrl);

            config.IsRemoteAgent = true;
            // Provide a name for the agent that differentiates it from the "normal" .NET Agent
            config.Name = agentName;
            // Set the connectorSearchPaths to only find connectors within the connector-sdk folder
            config.DataFolder = seeqDataFolder;

            string connectorSdkRoot = Path.GetFullPath(Path.Combine(executingAssemblyLocation, "..", "..", "..", "..", ".."));
            string configuration = "Release";
#if DEBUG
            configuration = "Debug";
#endif
            string searchPath = connectorSdkRoot + "/*Seeq.Link.Connector*/bin/" + configuration + "/*Seeq.Link.Connector*.dll";

            string platform = Environment.Is64BitProcess ? "x64" : "x86";
            string platformSpecificSearchPath = connectorSdkRoot + "/*Seeq.Link.Connector*/bin/" + platform + "/" +
                configuration + "/*Seeq.Link.Connector*.dll";

            config.ConnectorSearchPaths = searchPath + ";" + platformSpecificSearchPath;

            new Program().Run(new ClassFactory(), new SDK.ClassFactory(), config);
        }
    }
}