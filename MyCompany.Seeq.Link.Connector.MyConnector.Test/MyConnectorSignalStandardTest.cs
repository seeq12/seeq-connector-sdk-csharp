using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Seeq.Link.SDK;
using Seeq.Link.SDK.Interfaces;
using Seeq.Link.SDK.TestFramework;

namespace MyCompany.Seeq.Link.Connector.MyConnectorTest {

    [TestFixture]
    public class MyConnectorSignalStandardTest : SignalPullConnectionTestSuite<MyConnection, MyConnector, MyConnectionConfigV1, MyConnectorConfigV1> {
        private MyConnector myConnector;
        private MyConnection myConnection;

        private static Dictionary<StandardTest, string> dataIdsForStandardTests = new Dictionary<StandardTest, string>
        {
            { StandardTest.NoSamplesOutsideBoundary , "signal-data-id-1" },
            { StandardTest.SampleOneNanosecondAfterEnd , "signal-data-id-2" },
            { StandardTest.SampleOneNanosecondAfterStart , "signal-data-id-3" },
            { StandardTest.SampleOneNanosecondBeforeEnd , "signal-data-id-4" },
            { StandardTest.SampleOneNanosecondBeforeStart , "signal-data-id-5" },
            { StandardTest.SampleOnLeftBoundary , "signal-data-id-6" },
            { StandardTest.SampleOnRightBoundary , "signal-data-id-7" },
            { StandardTest.SamplesOutsideBoundaryOnly , "signal-data-id-8" },
        };

        private static Dictionary<string, string> dataIdsForCustomTests = new Dictionary<string, string> {
            { "ConnectionWithoutTagCount", "signal-data-id-9" },
            { "ConnectionWithInvalidSamplePeriod", "signal-data-id-10" }
        };

        public override MyConnection Connection => myConnection;

        public override MyConnector Connector => myConnector;

        public override List<IgnoredTest> IgnoredTests() {
            // We are using a shared reason because we are testing a simulated datasource. If you choose to ignore a
            // test against a real datasource, you will need to give individual, detailed reasons for each skipped test
            const string sharedIgnoreReason = "Our simulated datasource only returns floating point values";
            return new List<IgnoredTest>
            {
                new IgnoredTest(StandardTest.BooleanValuedSamples,sharedIgnoreReason),
                new IgnoredTest(StandardTest.EnumerationValuedSamples,sharedIgnoreReason),
                new IgnoredTest(StandardTest.IntegerValuedSamples, sharedIgnoreReason),
                new IgnoredTest(StandardTest.StringValuedSamples, sharedIgnoreReason),
                new IgnoredTest(StandardTest.NoSamplesAtAll, "The simulated datasouce will always return at least one sample"),
            };
        }

        public override void BaseConnectionOneTimeSetUp() {
            var connectionConfig = new MyConnectionConfigV1 {
                SamplePeriod = "00:00:01",
                TagCount = 5,
                Enabled = true
            };
            var connectorConfig = new MyConnectorConfigV1 {
                Connections = new List<MyConnectionConfigV1> { connectionConfig }
            };

            var mockConnectorService = new Mock<IConnectorServiceV2>();
            mockConnectorService.Setup(x => x.LoadConfig(It.IsAny<ConfigObject[]>()))
                .Returns(connectorConfig);

            myConnector = new MyConnector();
            myConnector.Initialize(mockConnectorService.Object);

            myConnection = new MyConnection(myConnector, connectionConfig);
        }

        public override List<ConfigObject> ConnectorConfigVersions() => new List<ConfigObject> {
            new MyConnectorConfigV1()
        };

        public override void IndexingConnectionOneTimeSetUp() {
        }

        public override void PullConnectionOneTimeSetUp() {
        }

        public override string DataIdForTest(string testName) {
            return Enum.TryParse(testName, out StandardTest standardTest) ? dataIdsForStandardTests[standardTest] : dataIdsForCustomTests[testName];
        }

        public override void SignalPullConnectionOneTimeSetUp() {
        }
    }
}