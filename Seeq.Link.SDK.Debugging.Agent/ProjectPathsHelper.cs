using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Seeq.Link.Debugging.Agent {

    internal class ProjectPathsHelper {

        public static string GetConnectorSdkRoot() {
            var executingAssemblyLocation = Assembly.GetExecutingAssembly().Location;
            return Path.GetFullPath(Path.Combine(executingAssemblyLocation, "..", "..", "..", "..", ".."));
        }

        public static string GetSeeqDataFolder() {
            return Path.GetFullPath(Path.Combine(GetConnectorSdkRoot(), "data"));
        }
    }
}