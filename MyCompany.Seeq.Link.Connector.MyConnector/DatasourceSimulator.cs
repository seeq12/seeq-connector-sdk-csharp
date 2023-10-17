using System;
using System.Collections.Generic;
using System.Linq;
using Seeq.Link.SDK.Utilities;

namespace MyCompany.Seeq.Link.Connector {

    public class DatasourceSimulator {
        private bool connected;
        private readonly int tagCount;
        private TimeSpan signalPeriod;

        public class Tag {
            private readonly int id;
            private readonly string name;
            private readonly bool stepped;

            public Tag(int id, string name, bool stepped) {
                this.id = id;
                this.name = name;
                this.stepped = stepped;
            }

            public int Id {
                get {
                    return this.id;
                }
            }

            public string Name {
                get {
                    return this.name;
                }
            }

            public bool Stepped {
                get {
                    return this.stepped;
                }
            }
        }

        public class TagValue {
            public DateTime Start { get; }

            public DateTime End { get; }

            public double Value { get; }

            public TagValue(DateTime start, DateTime end, double value) {
                this.Start = start;
                this.End = end;
                this.Value = value;
            }
        }

        public DatasourceSimulator(int tagCount, TimeSpan signalPeriod) {
            this.tagCount = tagCount;
            this.signalPeriod = signalPeriod;
        }

        public bool Connect() {
            this.connected = true;

            return true;
        }

        public bool IsConnected {
            get {
                return this.connected;
            }
        }

        public void Disconnect() {
        }

        public int TagCount {
            get {
                return this.tagCount;
            }
        }

        public IEnumerator<Tag> Tags {
            get {
                int nextTagNumber = 0;

                while (nextTagNumber < this.tagCount) {
                    nextTagNumber++;
                    yield return new Tag(
                            nextTagNumber,
                            string.Format("Simulated Tag #{0}", nextTagNumber),
                            nextTagNumber % 2 == 0);
                }
            }
        }

        public enum Waveform {
            SINE
        }

        public double Query(Waveform waveform, long timestamp) {
            long signalPeriodInNanos = this.signalPeriod.Ticks * 100;
            double waveFraction = ((double)timestamp % signalPeriodInNanos) / signalPeriodInNanos;
            double value;

            switch (waveform) {
                default:
                case Waveform.SINE:
                    value = Math.Sin(waveFraction * 2.0d * Math.PI);
                    break;
            }

            return value;
        }

        public TagValue RequestLastTagValue(string dataId, TimeInstant startTimestamp, TimeInstant endTimestamp) {
            return this.Query(dataId, startTimestamp, endTimestamp, 1).FirstOrDefault();
        }

        public IEnumerable<TagValue> Query(string dataId, TimeInstant startTimestamp, TimeInstant endTimestamp,
            int limit) {
            // To be able to yield consistent, reproducible tag values, we need a constant seed. This helps us
            // approximate the behaviour of a real datasource which should be deterministic. 
            const int seed = 1_000_000;
            var random = new Random(seed);
            var startTime = startTimestamp.ToDateTimeRoundDownTo100ns();
            var endTime = endTimestamp.ToDateTimeRoundUpTo100ns();
            var timespanInMs = (long)(endTime - startTime).TotalMilliseconds;
            var timestampIncrement = timespanInMs / limit;

            for (var i = 1; i <= limit; i++) {
                var start = startTime + TimeSpan.FromMilliseconds(timestampIncrement * i);
                var end = start + TimeSpan.FromMilliseconds(10);
                yield return new TagValue(start, end, random.NextDouble());
            }
        }
    }
}