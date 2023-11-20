using System;
using log4net;
using Moq;
using NFluent;
using NUnit.Framework;
using Seeq.Link.SDK.Interfaces;
using Seeq.Link.SDK.Utilities;

namespace MyCompany.Seeq.Link.Connector.MyConnector.Test {

    [TestFixture]
    public class MyConnectionTest {

        [Test]
        public void GetSamples() {
            MyConnectionConfigV1 config = new MyConnectionConfigV1();

            config.SamplePeriod = "0:00:01";
            config.TagCount = 100;

            Mock<IDatasourceConnectionServiceV2> connectionServiceMock = new Mock<IDatasourceConnectionServiceV2>();
            connectionServiceMock.Setup(x => x.Log).Returns(new Mock<ILog>().Object);

            MyConnection connection = new MyConnection(null, config);
            connection.Initialize(connectionServiceMock.Object);
            connection.Connect();

            Check.That(connection.GetSamples(new GetSamplesParameters("MyDataId1",
                    new TimeInstant(2 * 1000000000L - 100), new TimeInstant(4 * 1000000000L + 100), 0, 10, null, "")))
                .ContainsExactly(
                    new Sample(new TimeInstant(1000000000L), 0.06279051952931337),
                    new Sample(new TimeInstant(2000000000L), 0.12533323356430426),
                    new Sample(new TimeInstant(3000000000L), 0.1873813145857246),
                    new Sample(new TimeInstant(4000000000L), 0.2486898871648548),
                    new Sample(new TimeInstant(5000000000L), 0.3090169943749474));

            // Ensure sampleLimit works
            Check.That(connection.GetSamples(new GetSamplesParameters("MyDataId2",
                    new TimeInstant(2 * 1000000000L - 100), new TimeInstant(4 * 1000000000L + 100), 0, 2, null, "")))
                .ContainsExactly(
                    new Sample(new TimeInstant(1000000000L), 0.06279051952931337),
                    new Sample(new TimeInstant(2000000000L), 0.12533323356430426));
        }
    }
}