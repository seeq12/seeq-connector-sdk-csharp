using System.Collections.Generic;
using NUnit.Framework;
using Seeq.Link.SDK;
using Seeq.Link.SDK.TestFramework;

namespace MyCompany.Seeq.Link.Connector.MyConnectorTest {

    [TestFixture]
    public class MyConnectorSignalStandardTest : SignalPullConnectionTestSuite<MyConnection, MyConnector, MyConnectionConfigV1, MyConnectorConfigV1> {
        public override MyConnection Connection { get; }
        public override MyConnector Connector { get; }
        public override List<IgnoredTest> IgnoredTests() => throw new System.NotImplementedException();
        public override void BaseConnectionOneTimeSetUp() { throw new System.NotImplementedException(); }
        public override List<ConfigObject> ConnectorConfigVersions() => throw new System.NotImplementedException();
        public override void IndexingConnectionOneTimeSetUp() { throw new System.NotImplementedException(); }
        public override void PullConnectionOneTimeSetUp() { throw new System.NotImplementedException(); }
        public override string DataIdForTest(string testName) => throw new System.NotImplementedException();
        public override void SignalPullConnectionOneTimeSetUp() { throw new System.NotImplementedException(); }
    }
}