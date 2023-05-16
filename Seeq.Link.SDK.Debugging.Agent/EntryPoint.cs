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

            Program.Configuration config = Program.GetDefaultConfiguration();

            // Provide a name for the agent that differentiates it from the "normal" .NET Agent
            config.Name = ".NET Connector SDK Debugging Agent";
            config.SeeqUrl = new Uri("https://yourserver.seeq.host");
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

            new Program().Run(new ClassFactory(), new Seeq.Link.SDK.ClassFactory(), config);
        }
    }
}