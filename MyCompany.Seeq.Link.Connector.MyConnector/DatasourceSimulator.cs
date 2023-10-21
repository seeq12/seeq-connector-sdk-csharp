using System;
using System.Collections.Generic;
using System.Linq;
using Seeq.Link.SDK.Utilities;

namespace MyCompany.Seeq.Link.Connector {

    public class DatasourceSimulator {
        
        // To be able to yield consistent, reproducible tag values, we need a constant seed. This helps us
        // approximate the behaviour of a real datasource which should be deterministic. 
        private const int RandomnessSeed = 1_000_000;
        private readonly Random RNG = new Random(RandomnessSeed);
        
        private bool connected;
        private readonly int tagCount;
        private TimeSpan signalPeriod;

        // This is NOT intended for production use and is solely to model possible 
        // datasource tag structures that can used when syncing signals.
        public class Tag {
            public int Id { get; }

            public string Name { get; }

            public bool Stepped { get; }

            public Tag(int id, string name, bool stepped) {
                this.Id = id;
                this.Name = name;
                this.Stepped = stepped;
            }
 }

        // This is NOT intended for production use and is solely to model possible 
        // datasource event structures that can used when syncing conditions
        public class Event {
            public DateTime Start { get; }

            public DateTime End { get; }

            public double Intensity { get; }

            public Event(DateTime start, DateTime end, double intensity) {
                this.Start = start;
                this.End = end;
                this.Intensity = intensity;
            }
        }

        public DatasourceSimulator(int tagCount, TimeSpan signalPeriod) {
            this.tagCount = tagCount;
            this.signalPeriod = signalPeriod;
        }

        public int AssetLevels
        {
            get
            {
                // We do not want to exceed three levels to keep complexity low
                return RNG.Next(2, 4);
            }
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

        public IEnumerable<Event> Query(string dataId, TimeInstant startTimestamp, TimeInstant endTimestamp,
            int limit) {
            var startTime = startTimestamp.ToDateTimeRoundDownTo100ns();
            var endTime = endTimestamp.ToDateTimeRoundUpTo100ns();
            var timespanInMs = (long)(endTime - startTime).TotalMilliseconds;
            var timestampIncrement = timespanInMs / limit;

            for (var i = 1; i <= limit; i++) {
                var start = startTime + TimeSpan.FromMilliseconds(timestampIncrement * i);
                var end = start + TimeSpan.FromMilliseconds(10);
                yield return new Event(start, end, RNG.NextDouble());
            }
        }
        
        private static int 
    }
}