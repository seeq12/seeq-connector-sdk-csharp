﻿using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Seeq.Link.SDK;
using Seeq.Link.SDK.Interfaces;
using Seeq.Link.SDK.TestFramework;

namespace MyCompany.Seeq.Link.Connector.MyConnectorTest {

    [TestFixture]
    public class MyConnectorConditionStandardTest : ConditionPullConnectionTestSuite<MyConnection, MyConnector,
        MyConnectionConfigV1, MyConnectorConfigV1> {
        private MyConnector myConnector;
        private MyConnection myConnection;

        private static Dictionary<StandardTest, string> dataIdsForStandardTests = new Dictionary<StandardTest, string>()
        {
            { StandardTest.CapsulesStartingAfterIntervalOnly , "condition-data-id-1" },
            { StandardTest.CapsulesStartingBeforeIntervalOnly , "condition-data-id-2" },
            { StandardTest.CapsuleStartsAtEndTime , "condition-data-id-3" },
            { StandardTest.CapsuleStartsAtStartTime , "condition-data-id-4" },
            { StandardTest.CapsuleStartsOneNanosecondAfterStart , "condition-data-id-5" },
            { StandardTest.CapsuleStartsOneNanosecondBeforeEnd , "condition-data-id-6" },
        };

        public override MyConnection Connection => myConnection;

        public override MyConnector Connector => myConnector;

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

        public override void ConditionPullConnectionOneTimeSetUp() {
        }

        public override List<ConfigObject> ConnectorConfigVersions() => new List<ConfigObject>
        {
            new MyConnectorConfigV1()
        };

        public override string DataIdForTest(string testName) {
            return dataIdsForStandardTests[(StandardTest)Enum.Parse(typeof(StandardTest), testName)];
        }

        public override List<IgnoredTest> IgnoredTests() => new List<IgnoredTest>();

        public override void IndexingConnectionOneTimeSetUp() {
        }

        public override void PullConnectionOneTimeSetUp() {
        }
    }
}