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
    public class MyConnection : ISignalPullDatasourceConnection, IConditionPullDatasourceConnection {
        private readonly MyConnector connector;
        private readonly MyConnectionConfigV1 connectionConfig;
        private IDatasourceConnectionServiceV2 connectionService;
        private DatasourceSimulator datasourceSimulator;

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
            TimeSpan samplePeriod = TimeSpan.Parse(this.connectionConfig.SamplePeriod.ToUpper());
            TimeSpan signalPeriod = new TimeSpan(samplePeriod.Ticks * 100);

            // Use logging statements to show important information in the log files. These logging statements will be
            // output to the console when you're in the IDE and also to
            // "csharp/Seeq.Link.SDK.Debugging.Agent/bin/Debug/log/net-debugging-agent.log" within the Connector SDK.
            // When you have deployed your connector, the log statements
            // will go to the "log/net-link.log" file in the Seeq data folder.
            this.connectionService.Log.DebugFormat("Sample period parsed as '{0}'", samplePeriod);
            this.connectionService.Log.DebugFormat("Signal period determined to be '{0}'", signalPeriod);

            // Second, perform whatever I/O is necessary to establish a connection to your datasource. For example, you
            // might instantiate an ODBC connection object and connect to a SQL database.
            this.datasourceSimulator = new DatasourceSimulator(samplePeriod, signalPeriod);

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
            // An asset tree is exactly what it sounds like; a tree that describes your asset hierarchies and the relationships
            // between them. This means there needs to be a starting point; a root. This example shows how to create the root
            // asset in the Seeq database.
            string rootAssetId = this.syncRootAsset();

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
            this.syncAssets(rootAssetId);
        }

        public IEnumerable<Sample> GetSamples(GetSamplesParameters parameters) {
            try {
                // This is an example of how you may query your datasource for tag values and is specific to the
                // simulator example. This should be replaced with your own datasource-specific call.
                IEnumerable<DatasourceSimulator.Tag.Value> tagValues = this.datasourceSimulator.GetTagValues(
                    parameters.DataId,
                    parameters.StartTime,
                    parameters.EndTime,
                    parameters.SampleLimit
                );

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
                foreach (DatasourceSimulator.Tag.Value tagValue in tagValues) {
                    yield return new Sample(tagValue.Timestamp, tagValue.Measure);
                }

                // Warning: Any code you put outside of the main for-loop may not be executed. Use the finally block
                //          for any cleanup you might have to do.
            } finally {
                // If you have any cleanup to do, do it in this finally block. This is guaranteed to be called if
                // iteration is short-circuited for any reason.
            }
        }

        public IEnumerable<Capsule> GetCapsules(GetCapsulesParameters parameters) {
            try {
                // If a "last certain key" is requested, then you MUST specify one by calling setLastCertainKey. This informs
                // Seeq that there is a time boundary between certain and uncertain capsules: everything at or before the key
                // is certain, whereas any capsule after the key is uncertain. If setLastCertainKey is not called, then all
                // capsules are treated as uncertain
                //
                // This example connector does not explicitly control capsule certainty, so we don't make a call below, but we
                // show how it would be done in your connector.
                if (parameters.IsLastCertainKeyRequested) {
                    // ...but if we did make a call, this is what it would look like:
                    // parameters.SetLastCertainKey(new TimeInstant(...));
                }

                // This is an example of how you may query your datasource for alarm events and is specific to the
                // simulator example. This should be replaced with your own datasource-specific call.
                IEnumerable<DatasourceSimulator.Alarm.Event> events = this.datasourceSimulator.GetAlarmEvents(
                    parameters.DataId,
                    parameters.StartTime,
                    parameters.EndTime,
                    parameters.CapsuleLimit
                );

                // Return an enumeration to iterate through all the capsules in the time range.
                //
                // IEnumerable is important to use here to avoid bringing all of the data into memory to satisfy the
                // request. The Seeq connector host will automatically "page" the data upload so that we don't hit memory
                // ceilings on large requests. You can use C#'s "yield return" keyword to easily create lazy enumerations.
                //
                // The code within this function is largely specific to the simulator example. But it should give you an idea of
                // some of the concerns you'll need to attend to.
                foreach (DatasourceSimulator.Alarm.Event @event in events) {
                    TimeInstant start = new TimeInstant(@event.Start);
                    TimeInstant end = new TimeInstant(@event.End);
                    List<Capsule.Property> capsuleProperties = new List<Capsule.Property> {
                        new Capsule.Property("Intensity", @event.Intensity.ToString(), "rads")
                    };
                    yield return new Capsule(start, end, capsuleProperties);
                }

                // Warning: Any code you put outside of the main for-loop may not be executed. Use the finally block
                //          for any cleanup you might have to do.
            } finally {
                // If you have any cleanup to do, do it in this finally block. This is guaranteed to be called if
                // iteration is short-circuited for any reason.
            }
        }

        public void SaveConfig() {
            // Configuration persistence is typically managed by the connector, which stores a list of all connection
            // configurations.
            this.connector.SaveConfig();
        }

        private string syncRootAsset() {
            string datasourceDataId = this.connectionService.Datasource.Id;

            // create the root asset
            AssetInputV1 rootAsset = new AssetInputV1 {
                DataId = datasourceDataId,
                Name = "My Datasource Name"
            };
            this.connectionService.PutRootAsset(rootAsset);

            return rootAsset.DataId;
        }

        private void syncAssets(string rootAssetId) {
            foreach (DatasourceSimulator.Element database in this.datasourceSimulator.GetDatabases()) {
                this.syncDatabase(database);
                this.linkToParentAsset(rootAssetId, database.Id);

                IEnumerable<DatasourceSimulator.Tag> tags = this.datasourceSimulator.GetTagsForDatabase(database.Id);

                foreach (DatasourceSimulator.Tag tag in tags) {
                    this.syncSignal(tag);
                    this.linkToParentAsset(database.Id, tag.Id);
                }

                IEnumerable<DatasourceSimulator.Alarm> alarms = this.datasourceSimulator.GetAlarmsForDatabase(database.Id);

                foreach (DatasourceSimulator.Alarm alarm in alarms) {
                    this.syncCondition(alarm);
                    this.linkToParentAsset(database.Id, alarm.Id);
                }

                IEnumerable<DatasourceSimulator.Constant> constants = this.datasourceSimulator.GetConstantsForDatabase(database.Id);

                foreach (DatasourceSimulator.Constant constant in constants) {
                    this.syncScalar(constant);
                    this.linkToParentAsset(database.Id, constant.Id);
                }
            }
        }

        private void syncDatabase(DatasourceSimulator.Element subElement) {
            // create the child asset
            AssetInputV1 childAsset = new AssetInputV1 {
                DataId = subElement.Id,
                Name = subElement.Name
            };
            this.connectionService.PutAsset(childAsset);
        }

        private void linkToParentAsset(string parentAssetId, string dataId) {
            // create the child asset/condition/signal relationship to its parent
            AssetTreeSingleInputV1 relationship = new AssetTreeSingleInputV1 {
                ChildDataId = dataId,
                ParentDataId = parentAssetId
            };
            this.connectionService.PutRelationship(relationship);
        }

        private void syncSignal(DatasourceSimulator.Tag tag) {
            SignalWithIdInputV1 signal = new SignalWithIdInputV1();

            // The Data ID is a string that is unique within the data source, and is used by Seeq when referring
            // to signal/asset data. It is important that the Data ID be consistent across connections which means
            // that transient values like generated GUID/UUIDs or the Datasource name would not be ideal. The
            // Data ID is a string and does not need to be numeric, even though we are just using a number in
            // this example.
            signal.DataId = tag.Id;

            // The Name is a string that is displayed in the UI. It can change (typically as a result of a
            // rename operation happening in the source system), but the unique Data ID preserves appropriate
            // linkages.
            signal.Name = tag.Name;

            // The interpolation method is the readonly piece of critical information for a signal.
            signal.InterpolationMethod = tag.Stepped
                    ? InterpolationMethod.Step
                    : InterpolationMethod.Linear;

            // Additional Properties are used to store and track "Scalars" which are "facts" about the signal/
            // condition/asset i.e. a piece of data that does not change. Special care should be taken to
            // ensure only non-null values are provided.
            signal.AdditionalProperties = new List<ScalarPropertyV1> {
                    new ScalarPropertyV1 {
                        Name = "Provider",
                        Value = "Seeq"
                    }
                };

            // PutSignal() queues items up for performance reasons and writes them in batch to the server.
            //
            // If you need the signals to be written to Seeq Server before any other work continues, you can
            // call FlushSignals() on the connection service.
            this.connectionService.PutSignal(signal);
        }

        private void syncCondition(DatasourceSimulator.Alarm alarm) {
            ConditionUpdateInputV1 condition = new ConditionUpdateInputV1();

            // The Data ID is a string that is unique within the data source, and is used by Seeq when referring
            // to condition data. It is important that the Data ID be consistent across connections which means
            // that transient values like generated GUID/UUIDs or the Datasource name would not be ideal. The
            // Data ID is a string and does not need to be numeric, even though we are just using a number in
            // this example.
            condition.DataId = alarm.Id;

            // The Name is a string that is displayed in the UI. It can change (typically as a result of a
            // rename operation happening in the source system), but the unique Data ID preserves appropriate
            // linkages.
            condition.Name = alarm.Name;

            // The Maximum Duration is a time span (made up by a combination of a value and a time unit e.g. 1h,
            // 2m etc.) that indicates the maximum duration of capsules in this series and is required for stored
            // conditions like this example.
            condition.MaximumDuration = "2h";

            // PutCondition() queues items up for performance reasons and writes them in batch to the server.
            //
            // If you need the conditions to be written to Seeq Server before any other work continues, you can
            // call FlushConditions() on the connection service.
            this.connectionService.PutCondition(condition);
        }

        private void syncScalar(DatasourceSimulator.Constant constant) {
            ScalarInputV1 scalar = new ScalarInputV1();

            // The Data ID is a string that is unique within the data source, and is used by Seeq when referring
            // to scalar data. It is important that the Data ID be consistent across connections which means
            // that transient values like generated GUID/UUIDs or the Datasource name would not be ideal. The
            // Data ID is a string and does not need to be numeric, even though we are just using a number in
            // this example.
            scalar.DataId = constant.Id;

            // The Name is a string that is displayed in the UI. It can change (typically as a result of a
            // rename operation happening in the source system), but the unique Data ID preserves appropriate
            // linkages.
            scalar.Name = constant.Name;

            // The Unit Of Measure is a string that denotes what the unit of measure of the scalar value is.
            scalar.UnitOfMeasure = constant.UnitOfMeasure;

            scalar.Formula = this.getFormula(constant.Value);

            // PutScalar() queues items up for performance reasons and writes them in batch to the server.
            //
            // If you need the scalars to be written to Seeq Server before any other work continues, you can
            // call FlushScalars() on the connection service.
            this.connectionService.PutScalar(scalar);
        }

        private String getFormula(Object value) {
            if (value.GetType() == typeof(string)) {
                return FormulaHelper.EscapeStringAsFormula((string) value);
            } else if (value.GetType() == typeof(DateTime)) {
                TimeInstant timeInstant = new TimeInstant((DateTime) value);
                return timeInstant.Timestamp + "ns";
            } else {
                return value.ToString();
            }
        }
    }
}