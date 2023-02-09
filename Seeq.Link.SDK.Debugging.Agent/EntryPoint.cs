using System;
using System.IO;
using System.Reflection;
using Seeq.Utilities;

namespace Seeq.Link.Debugging.Agent {

    /// <summary>
    /// This class "wraps" the Seeq .NET Agent such that it functions appropriately for connector development and debugging.
    /// It assumes that Seeq Server is installed on the development machine, and performs a similar function to the one that
    /// Supervisor performs in the "real" production environment: It assembles an appropriate set of command line arguments
    /// to connect to the server and load the connector that is under development.
    /// </summary>
    public class EntryPoint {

        public static void Main(string[] args) {
            log4net.Config.XmlConfigurator.Configure();

            Seeq.Link.Agent.Program.Configuration config =
                    Seeq.Link.Agent.Program.GetDefaultConfiguration();

            // Provide a name for the agent that differentiates it from the "normal" .NET Agent
            config.Name = ".NET Connector SDK Debugging Agent";
            config.SeeqUrl = new Uri("https://yourserver.seeq.host");

            // Specify the data folder; change this if you've configured Seeq to use a different location!
            config.DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Seeq", "data");
            config.IsRemoteAgent = true;
            // Set the connectorSearchPaths to only find connectors within the connector-sdk folder

            string executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            config.DataFolder = Path.Combine(Path.GetDirectoryName(executingAssemblyLocation), "data");

            string connectorSdkRoot = Path.GetFullPath(Path.Combine(executingAssemblyLocation, "..", "..", "..", "..", ".."));
            string configuration = "Release";
#if DEBUG
            configuration = "Debug";
#endif
            string searchPath = connectorSdkRoot.ToString() + "/*Seeq.Link.Connector*/bin/" + configuration + "/*Seeq.Link.Connector*.dll";

            string platform = Environment.Is64BitProcess ? "x64" : "x86";
            string platformSpecificSearchPath = connectorSdkRoot.ToString() + "/*Seeq.Link.Connector*/bin/" + platform + "/" +
                configuration + "/*Seeq.Link.Connector*.dll";

            config.ConnectorSearchPaths = searchPath + ";" + platformSpecificSearchPath;

            new Seeq.Link.Agent.Program().Run(new Seeq.Link.Agent.ClassFactory(), new Seeq.Link.SDK.ClassFactory(), config);
        }
    }
}