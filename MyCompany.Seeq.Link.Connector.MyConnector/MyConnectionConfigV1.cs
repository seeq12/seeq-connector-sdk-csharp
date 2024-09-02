using Seeq.Link.SDK;

namespace MyCompany.Seeq.Link.Connector {

    /// <summary>
    /// The configuration object should be a Plain Old C# Object with little to no logic, just fields.
    /// </summary>
    public class MyConnectionConfigV1 : PullDatasourceConnectionConfig {
        public int? TagCount;

        public string SamplePeriod;
    }
}