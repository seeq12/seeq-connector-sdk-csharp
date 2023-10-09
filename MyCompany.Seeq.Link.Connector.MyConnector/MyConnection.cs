using System;
using System.Collections.Generic;
using Seeq.Link.SDK;
using Seeq.Link.SDK.Interfaces;
using Seeq.Link.SDK.Utilities;
using Seeq.Sdk.Model;

namespace MyCompany.Seeq.Link.Connector {

    /// <summary>
    /// Represents a connection to a unique datasource. A connector can host any number of such connections to
    /// datasources.
    ///
    /// This example implements the <see cref="ISignalPullDatasourceConnection"/> interface, which means that
    /// the connection responds to on-demand requests from Seeq Server for Samples within a Signal and queries
    /// its datasource to produce the result.
    ///
    /// A connection can also implement <see cref="IConditionPullDatasourceConnection"/> to respond to on-demand
    /// requests from Seeq Server for Capsules within a Condition.
    ///
    /// Alternatively, a connection could choose to implement neither of the above interfaces and instead "push"
    /// Samples or Capsules into Seeq using the <see cref="ISignalsApi"/> or <see cref="IConditionsApi"/>
    /// obtained via <see cref="ISeeqApiProvider"/> on <see cref="IAgentService"/> on
    /// <see cref="IDatasourceConnectionServiceV2"/>.
    /// </summary>
    public class MyConnection : ISignalPullDatasourceConnection {
        private readonly MyConnector connector;
        private readonly MyConnectionConfigV1 connectionConfig;
        private IDatasourceConnectionServiceV2 connectionService;
        private DatasourceSimulator datasourceSimulator;
        private TimeSpan samplePeriod;

        public MyConnection(MyConnector connector, MyConnectionConfigV1 connectionConfig) {
            // You will generally want to accept a configuration object from your connector parent. Do not do any I/O in the
            // constructor -- leave that for the other functions like initialize() or connect(). Generally, you should just
            // be setting private fields in the constructor.
            this.connector = connector;
            this.connectionConfig = connectionConfig;
            this.datasourceSimulator = null;
        }

        public string DatasourceClass {
            get {
                // Return a string that identifies this type of datasource. Example: "ERP System"
                // This value will be seen in the Information panel in Seeq Workbench.
                return "My Connector Type";
            }
        }

        public string DatasourceName {
            get {
                // The name will appear in Seeq Workbench and can change (as long as the DatasourceId does not change)
                return this.connectionConfig.Name;
            }
        }

        public string DatasourceId {
            get {
                // This unique identifier usually must come from the configuration file and be unchanging
                return this.connectionConfig.Id;
            }
        }

        public IndexingDatasourceConnectionConfig Configuration {
            get {
                // The configuration should extend DefaultIndexingDatasourceConnectionConfig so that concerns like property
                // transforms and index scheduling are taken care of by the SDK.
                return this.connectionConfig;
            }
        }

        public void Initialize(IDatasourceConnectionServiceV2 connectionService) {
            // You probably won't do much in the initialize() function. But if you have to do some I/O that is separate
            // from the act of connecting, you could do it here.

            this.connectionService = connectionService;

            // It's your job to inspect your configuration to see if the user has enabled this connection.
            if (this.connectionConfig.Enabled) {
                // This will cause the connect/monitor thread to be spawned and Connect() to be called
                this.connectionService.Enable();
            }
        }

        public void Destroy() {
            // Perform any final cleanup in this method
        }

        public void Connect() {
            // First, notify the connection service that you're attempting to connect. You must go through this CONNECTING
            // state before you go to CONNECTED, otherwise the CONNECTED state will be ignored.
            this.connectionService.ConnectionState = ConnectionState.CONNECTING;

            // These lines are specific to the simulator example.
            this.samplePeriod = TimeSpan.Parse(this.connectionConfig.SamplePeriod.ToUpper());
            TimeSpan signalPeriod = new TimeSpan(this.samplePeriod.Ticks * 100);

            // Use logging statements to show important information in the log files. These logging statements will be
            // output to the console when you're in the IDE and also to
            // "csharp/Seeq.Link.SDK.Debugging.Agent/bin/Debug/log/net-debugging-agent.log" within the Connector SDK.
            // When you have deployed your connector, the log statements
            // will go to the "log/net-link.log" file in the Seeq data folder.
            this.connectionService.Log.DebugFormat("Sample period parsed as '{0}'", this.samplePeriod);
            this.connectionService.Log.DebugFormat("Signal period determined to be '{0}'", signalPeriod);

            // Second, perform whatever I/O is necessary to establish a connection to your datasource. For example, you
            // might instantiate an ODBC connection object and connect to a SQL database.
            this.datasourceSimulator = new DatasourceSimulator(this.connectionConfig.TagCount, signalPeriod);

            if (this.datasourceSimulator.Connect()) {
                // If the connection is successful, transition to the CONNECTED state. The monitor() function will then
                // be called periodically to ensure the connection is "live".
                this.connectionService.ConnectionState = ConnectionState.CONNECTED;
            } else {
                // If the connection is unsuccessful, transition to the DISCONNECTED state. This connect() function will
                // be called periodically to attempt to connect again.
                this.connectionService.ConnectionState = ConnectionState.DISCONNECTED;
            }
        }

        public bool Monitor() {
            // This function will be called periodically to ensure the connection is "live". Do whatever makes sense for
            // your datasource.
            if (!this.datasourceSimulator.IsConnected) {
                // If the connection is dead, return false. This will cause disconnect() to be called so you can clean
                // up resources and transition to DISCONNECTED.
                return false;
            }

            return true;
        }

        public void Disconnect() {
            // Transition to the disconnected state.
            this.connectionService.ConnectionState = ConnectionState.DISCONNECTED;

            // Do whatever is necessary to clean up your connection and free up allocated resources.
            this.datasourceSimulator.Disconnect();
        }

        public void Index(SyncMode syncMode) {
            // Do whatever is necessary to generate the list of signals you want to show up in Seeq. It is generally
            // preferable to use a "streaming" method of iterating through the tags. I.e., try not to hold them all in
            // memory because it is harder to scale to indexing hundreds of thousands of signals. These examples
            // use IEnumerator (which can also be composed in a 'lazy' manner), and are ideal for using C#'s "yield
            // return" keyword.

            // This function will potentially be called twice in a row. The first time, the DatasourceConnectionService
            // will count and checksum all the calls you make to PutSignal(), PutAsset() etc. It will then use that
            // information to determine if anything has changed since the last index operation. The first time it makes
            // this Index() call, the syncMode will be SyncMode.INVENTORY.

            // Loop through all of the tags in our simulated datasource and tell Seeq Server about them
            IEnumerator<DatasourceSimulator.Tag> tags = this.datasourceSimulator.Tags;
            while (tags.MoveNext()) {
                DatasourceSimulator.Tag tag = tags.Current;
                SignalWithIdInputV1 signal = new SignalWithIdInputV1();

                // The Data ID is a string that is unique within the data source, and is used by Seeq when referring
                // to signal / condition / asset data. Data ID is a string and does not need to be numeric, even
                // though we are just using a number in this example.
                signal.DataId = string.Format("{0}", tag.Id);

                // The Name is a string that is displayed in the UI. It can change (typically as a result of a
                // rename operation happening in the source system), but the unique Data ID preserves appropriate
                // linkages.
                signal.Name = tag.Name;

                // The interpolation method is the readonly piece of critical information for a signal.
                signal.InterpolationMethod = tag.Stepped
                        ? InterpolationMethod.Step
                        : InterpolationMethod.Linear;

                // PutSignal() queues items up for performance reasons and writes them in batch to the server.
                //
                // If you need the signals to be written to Seeq Server before any other work continues, you can
                // call FlushSignals() on the connection service.
                this.connectionService.PutSignal(signal);
                
                
                ConditionInputV1 condition = new ConditionInputV1();

                // The Data ID is a string that is unique within the data source, and is used by Seeq when referring
                // to condition data. Data ID is a string and does not need to be numeric, even though we are just
                // using a number in this example.
                condition.DataId = string.Format("{0}", tag.Id);

                // The Name is a string that is displayed in the UI. It can change (typically as a result of a
                // rename operation happening in the source system), but the unique Data ID preserves appropriate
                // linkages.
                condition.Name = tag.Name;

                // PutCondition() queues items up for performance reasons and writes them in batch to the server.
                //
                // If you need the conditions to be written to Seeq Server before any other work continues, you can
                // call FlushConditions() on the connection service.
                this.connectionService.PutCondition(condition);
            }
        }

        public IEnumerable<Sample> GetSamples(GetSamplesParameters parameters) {
            // Return an enumeration to iterate through all of the samples in the time range.
            //
            // Very important: You must return one sample 'on or earlier' than the requested interval and one sample 'on or
            // later' (if such samples exist). This allows Seeq to interpolate appropriately to the edge of the requested
            // time range.
            //
            // IEnumerable is important to use here to avoid bringing all of the data into memory to satisfy the
            // request. The Seeq connector host will automatically "page" the data upload so that we don't hit memory
            // ceilings on large requests. You can use C#'s "yield return" keyword to easily create lazy enumerations.
            //
            // The code within this function is largely specific to the simulator example. But it should give you an idea of
            // some of the concerns you'll need to attend to.
            long samplePeriodInNanos = this.samplePeriod.Ticks * 100;
            long leftBoundTimestamp = parameters.StartTime.Timestamp / samplePeriodInNanos;
            long rightBoundTimestamp = (parameters.EndTime.Timestamp + samplePeriodInNanos - 1) / samplePeriodInNanos;

            // If a "last certain key" is requested, then you can specify one by calling SetLastCertainKey. This informs
            // Seeq that there is a time boundary between certain and uncertain samples: everything at or before the key
            // is certain, whereas any sample after the key is uncertain.
            //
            // This connector does not explicitly control sample certainty, so we don't make a call below.
            if (parameters.IsLastCertainKeyRequested) {
                // ...but if we did make a call, this is what it would look like:
                // parameters.SetLastCertainKey(new TimeInstant(...));
            }

            try {
                for (long sampleIndex = leftBoundTimestamp;
                    sampleIndex <= rightBoundTimestamp && sampleIndex - leftBoundTimestamp < parameters.SampleLimit; sampleIndex++) {
                    TimeInstant key = new TimeInstant(sampleIndex * samplePeriodInNanos);
                    double value = this.datasourceSimulator.Query(DatasourceSimulator.Waveform.SINE, key.Timestamp);

                    yield return new Sample(key, value);
                }

                // Warning: Any code you put outside of the main for-loop may not be executed. Use the finally block
                //          for any cleanup you might have to do.
            } finally {
                // If you have any cleanup to do, do it in this finally block. This is guaranteed to be called if
                // iteration is short-circuited for any reason.
            }
        }

        public int? MaxConcurrentRequests {
            get {
                // This parameter can help control the load that Seeq puts on an external datasource. It is typically
                // controlled from the configuration file.
                return this.connectionConfig.MaxConcurrentRequests;
            }
        }

        public int? MaxResultsPerRequest {
            get {
                // This parameter can help control the load and memory usage that Seeq puts on an external datasource.
                // It is typically controlled from the configuration file.
                return this.connectionConfig.MaxResultsPerRequest;
            }
        }

        public void SaveConfig() {
            // Configuration persistence is typically managed by the connector, which stores a list of all connection
            // configurations.
            this.connector.SaveConfig();
        }
    }
}