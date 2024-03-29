using System;
using System.Collections.Generic;
using System.Linq;
using Seeq.Link.SDK.Utilities;

namespace MyCompany.Seeq.Link.Connector {

    public class DatasourceSimulator {

        // To be able to yield consistent, reproducible tag values, we need a constant seed. This helps us
        // approximate the behaviour of a real datasource which should be deterministic.
        private const int RandomnessSeed = 1000000;

        private static readonly Random Rng = new Random(RandomnessSeed);

        private bool connected;
        private TimeSpan samplePeriod;
        private TimeSpan signalPeriod;

        public DatasourceSimulator(TimeSpan samplePeriod, TimeSpan signalPeriod) {
            this.samplePeriod = samplePeriod;
            this.signalPeriod = signalPeriod;
        }

        public bool IsConnected {
            get {
                return this.connected;
            }
        }

        public bool Connect() {
            this.connected = true;

            return true;
        }

        public void Disconnect() {
        }

        public IEnumerable<Element> GetDatabases() {
            int databaseCount = Rng.Next(10);
            return Enumerable.Range(1, databaseCount)
                .Select(elementId => new Element(elementId));
        }

        public IEnumerable<Alarm> GetAlarmsForDatabase(string elementId) {
            int alarmCount = Rng.Next(10);
            return Enumerable.Range(1, alarmCount)
                .Select(alarmId => new Alarm(elementId, alarmId));
        }

        public IEnumerable<Tag> GetTagsForDatabase(string elementId) {
            int tagCount = Rng.Next(10);
            return Enumerable.Range(1, tagCount)
                .Select(tagId => new Tag(elementId, tagId, tagId % 2 == 0));
        }

        public IEnumerable<Constant> GetConstantsForDatabase(string elementId) {
            int constantCount = Rng.Next(10);
            return Enumerable.Range(1, constantCount)
                .Select(constId => new Constant(elementId, constId, "°C", constId * 10));
        }

        public enum Waveform {
            SINE
        }

        public IEnumerable<Tag.Value> GetTagValues(string dataId, TimeInstant startTimestamp, TimeInstant endTimestamp,
            int limit) {
            long samplePeriodInNanos = this.samplePeriod.Ticks * 100;
            return EnumerableExtensions.RangeClosed(
                    (long)Math.Floor(startTimestamp.Timestamp / (double)samplePeriodInNanos),
                    (long)Math.Ceiling(endTimestamp.Timestamp / (double)samplePeriodInNanos)
                )
                .Select(index => {
                    TimeInstant key = new TimeInstant(index * samplePeriodInNanos);
                    double value = this.getWaveformValue(Waveform.SINE, key.Timestamp);
                    return new Tag.Value(key, value);
                })
                .Take(limit);
        }

        public IEnumerable<Alarm.Event> GetAlarmEvents(string dataId, TimeInstant startTimestamp, TimeInstant endTimestamp,
            int limit) {
            long capsulePeriodInNanos = this.samplePeriod.Ticks * 100;
            return EnumerableExtensions.RangeClosed(
                    (long)Math.Floor(startTimestamp.Timestamp / (double)capsulePeriodInNanos),
                    (long)Math.Ceiling(endTimestamp.Timestamp / (double)capsulePeriodInNanos)
                )
                .Select(index => {
                    DateTime start = new TimeInstant(index * capsulePeriodInNanos).ToDateTimeRoundDownTo100ns();
                    DateTime end = start + TimeSpan.FromTicks(100);
                    return new Alarm.Event(start, end, Rng.NextDouble());
                })
                .Take(limit);
        }

        private double getWaveformValue(Waveform waveform, long timestamp) {
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

        // NOTE: the data structures in this file are purely for illustration purposes only
        // and are here solely to approximate datasource response structures for syncing

        /// <summary>
        /// This class defines an element that can be used for syncing assets
        /// </summary>
        public class Element {
            public string Id { get; set; }

            public string Name { get; set; }

            public Element(int elementId) {
                Id = elementId.ToString();
                Name = string.Format("Simulated Element #{0}", elementId);
            }
        }

        /// <summary>
        /// This class defines an alarm that can be used for syncing conditions
        /// </summary>
        public class Alarm {
            public string Id { get; set; }

            public string Name { get; set; }

            public Alarm(string elementId, int alarmId) {
                Id = string.Format("Element={0};Alarm={1}", elementId, alarmId);
                Name = string.Format("Simulated Alarm #{0}", alarmId);
            }

            public class Event {
                public DateTime Start { get; set; }

                public DateTime End { get; set; }

                public object Intensity { get; set; }

                public Event(DateTime start, DateTime end, object intensity) {
                    this.Start = start;
                    this.End = end;
                    this.Intensity = intensity;
                }
            }
        }

        /// <summary>
        /// This class defines a tag that can be used for syncing signals
        /// </summary>
        public class Tag {
            public string Id { get; set; }

            public string Name { get; set; }

            public bool Stepped { get; set; }

            public Tag(string elementId, int tagId, bool stepped) {
                Id = string.Format("Element={0};Tag={1}", elementId, tagId);
                this.Name = string.Format("Simulated Tag #{0}", tagId);
                this.Stepped = stepped;
            }

            public class Value {
                public TimeInstant Timestamp { get; set; }

                public object Measure { get; set; }

                public Value(TimeInstant timestamp, object value) {
                    this.Timestamp = timestamp;
                    this.Measure = value;
                }
            }
        }

        /// <summary>
        /// This class defines a constant that can be used for syncing scalars
        /// </summary>
        public class Constant {
            public string Id { get; set; }

            public string Name { get; set; }

            public string UnitOfMeasure { get; set; }

            public object Value { get; set; }

            public Constant(string elementId, int constantId, string unitOfMeasure, object value) {
                this.Id = string.Format("Element={0};Constant={1}", elementId, constantId);
                this.Name = string.Format("Simulated Constant #{0}", constantId);
                this.UnitOfMeasure = unitOfMeasure;
                this.Value = value;
            }
        }
    }
}