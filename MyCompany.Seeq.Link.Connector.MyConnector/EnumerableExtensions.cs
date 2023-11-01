using System.Collections.Generic;

namespace MyCompany.Seeq.Link.Connector {

    public static class EnumerableExtensions {

        public static IEnumerable<long> RangeClosed(long start, long end) {
            for (long i = start; i <= end; i++) {
                yield return i;
            }
        }
    }
}