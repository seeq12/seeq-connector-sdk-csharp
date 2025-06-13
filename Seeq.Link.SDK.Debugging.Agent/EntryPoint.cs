using System;
using System.IO;
using System.Reflection;
using log4net.Config;
using Seeq.Link.Agent;

namespace Seeq.Link.Debugging.Agent {

    /// <summary>
    /// This class "wraps" the Seeq .NET Agent such that it functions appropriately for connector development and debugging.
    /// It assumes that Seeq Server is installed on the development machine, and performs a similar function to the one that
    /// Supervisor performs in the "real" production environment: It assembles an appropriate set of command line arguments
    /// to connect to the server and load the connector that is under development.
    /// </summary>
    public class EntryPoint {

        public static void Main(string[] args) {
            XmlConfigurator.Configure();

            const string agentName = ".NET Connector SDK Debugging Agent";
            var seeqDataFolder = ProjectPathsHelper.GetSeeqDataFolder();

            AgentOtpHelper.SetupAgentOtp(seeqDataFolder, agentName);

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

            string connectorSdkRoot = ProjectPathsHelper.GetConnectorSdkRoot();
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