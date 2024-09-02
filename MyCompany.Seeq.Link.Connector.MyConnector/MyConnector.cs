using System;
using Seeq.Link.SDK;
using Seeq.Link.SDK.Interfaces;

namespace MyCompany.Seeq.Link.Connector {

    /// <summary>
    /// Implements the <see cref="IConnectorV2"/> interface for facilitating dataflow from external systems with Seeq Server.
    /// </summary>
    public class MyConnector : IConnectorV2 {
        private IConnectorServiceV2 connectorService;
        private MyConnectorConfigV1 connectorConfig;

        public string Name {
            get {
                // This name will be used for the configuration file that is found in the data/configuration/link folder.
                return "MyCompany MyConnector";
            }
        }

        public void Initialize(IConnectorServiceV2 connectorService) {
            this.connectorService = connectorService;

            // First, load your configuration using the connector service. If the configuration file is not found, the first
            // object in the passed-in array is returned.
            ConfigObject configObj = this.connectorService.LoadConfig(new ConfigObject[] { new MyConnectorConfigV1() });

            this.connectorConfig = (MyConnectorConfigV1)configObj;

            // Check to see if there are any connections configured yet
            if (this.connectorConfig.Connections.Count == 0) {
                // Create a default connection configuration
                MyConnectionConfigV1 connectionConfig = new MyConnectionConfigV1();

                // The user will likely change this. It's what will appear in the list of datasources in Seeq Workbench.
                connectionConfig.Name = "My First Connection";

                // The identifier must be unique. It need not be a UUID, but that's recommended in lieu of anything else.
                connectionConfig.Id = Guid.NewGuid().ToString();

                // Normally you would probably leave the default connection disabled to start, but for this example we
                // want to start up in a functioning state.
                connectionConfig.Enabled = true;

                // These configuration variables are specific to the MyConnector example. You'll likely remove them.
                // We'll specify a large enough tag count that we observe the batching mechanism in action.
                connectionConfig.TagCount = 5000;
                connectionConfig.SamplePeriod = "00:15";

                // Add the new connection configuration to its parent connector
                this.connectorConfig.Connections.Add(connectionConfig);
            }

            // Now instantiate your connections based on the configuration.
            // Iterate through the configurations to create connection objects.
            foreach (MyConnectionConfigV1 connectionConfig in this.connectorConfig.Connections) {
                if (connectionConfig.Id == null) {
                    // If the ID is null, then the user likely copy/pasted an existing connection configuration and
                    // removed the ID so that a new one would be generated. Generate the new one!
                    connectionConfig.Id = Guid.NewGuid().ToString();
                }

                if (!connectionConfig.Enabled) {
                    // If the connection is not enabled, then do not add it to the list of connections
                    continue;
                }

                // do further validation of the connection configuration to ensure only property configured connections
                // are processed. In our case, we need a valid Sample Period, another field validate is the TagCount.
                if (!TimeSpan.TryParse(connectionConfig.SamplePeriod, out _)) {
                    // provide details of the invalid configuration so it can be addressed
                    this.connectorService.Log.WarnFormat("Connection '{0}' has an invalid Sample Period. It will be ignored.", connectionConfig.Name);

                    // you can also disable the connection so it is no longer processed until changes are made
                    connectionConfig.Enabled = false;

                    continue;
                }

                this.connectorService.AddConnection(new MyConnection(this, connectionConfig));
            }
            // Finally, save the connector configuration in a file for the user to view and modify as needed
            this.connectorService.SaveConfig(this.connectorConfig);
        }

        public void Destroy() {
            // Perform any connector-wide cleanup as necessary here
        }

        public void SaveConfig() {
            // This may be called after indexing activity to save the next scheduled indexing date/time
            this.connectorService.SaveConfig(this.connectorConfig);
        }
    }
}