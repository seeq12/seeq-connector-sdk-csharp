using System.Collections.Generic;
using NUnit.Framework;
using Seeq.Link.SDK;
using Seeq.Link.SDK.TestFramework;

namespace MyCompany.Seeq.Link.Connector.MyConnectorTest {

    [TestFixture]
    public class MyConnectorConditionStandardTest : ConditionPullConnectionTestSuite<MyConnection, MyConnector, MyConnectionConfigV1, MyConnectorConfigV1> {
        public override MyConnection Connection => throw new System.NotImplementedException();

        public override MyConnector Connector => throw new System.NotImplementedException();

        public override void BaseConnectionOneTimeSetUp() {
            throw new System.NotImplementedException();
        }

        public override void ConditionPullConnectionOneTimeSetUp() {
        }

        public override List<ConfigObject> ConnectorConfigVersions() => new List<ConfigObject>
            {
                new MyConnectorConfigV1()
            };

        public override string DataIdForTest(string testName) {
            throw new System.NotImplementedException();
        }

        public override List<IgnoredTest> IgnoredTests() => new List<IgnoredTest>();

        public override void IndexingConnectionOneTimeSetUp() {
        }

        public override void PullConnectionOneTimeSetUp() {
        }
    }
}