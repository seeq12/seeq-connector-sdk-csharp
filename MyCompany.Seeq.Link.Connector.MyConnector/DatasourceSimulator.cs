using System;
using System.Collections.Generic;
using System.Linq;
using Seeq.Link.SDK.Utilities;

namespace MyCompany.Seeq.Link.Connector {

    public class DatasourceSimulator {

        // To be able to yield consistent, reproducible tag values, we need a constant seed. This helps us
        // approximate the behaviour of a real datasource which should be deterministic.
        private const int RandomnessSeed = 1_000_000;

        private static readonly Random Rng = new Random(RandomnessSeed);

        private bool connected;
        private TimeSpan signalPeriod;

        public DatasourceSimulator(TimeSpan signalPeriod) {
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
            long samplePeriodInNanos = this.signalPeriod.Ticks * 100;
            long leftBoundTimestamp = startTimestamp.Timestamp / samplePeriodInNanos;
            long rightBoundTimestamp = (endTimestamp.Timestamp + samplePeriodInNanos - 1) / samplePeriodInNanos;

            for (long sampleIndex = leftBoundTimestamp;
                 sampleIndex <= rightBoundTimestamp && sampleIndex - leftBoundTimestamp < limit; sampleIndex++) {
                TimeInstant key = new TimeInstant(sampleIndex * samplePeriodInNanos);
                double value = this.getWaveformValue(Waveform.SINE, key.Timestamp);
                yield return new Tag.Value(key, value);
            }
        }

        public IEnumerable<Alarm.Event> GetAlarmEvents(string dataId, TimeInstant startTimestamp, TimeInstant endTimestamp,
            int limit) {
            DateTime startTime = startTimestamp.ToDateTimeRoundDownTo100ns();
            long eventPeriodInNanos = (endTimestamp.Timestamp - startTimestamp.Timestamp) / limit;
            return Enumerable.Range(0, limit)
                .Select(index => {
                    DateTime start = startTime + TimeSpan.FromTicks(index * eventPeriodInNanos / 100);
                    DateTime end = start + TimeSpan.FromMilliseconds(10);
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
            public string Id { get; }

            public string Name { get; }

            public Element(int elementId) {
                Id = elementId.ToString();
                Name = $"Simulated Element #{elementId}";
            }
        }

        /// <summary>
        /// This class defines an alarm that can be used for syncing conditions
        /// </summary>
        public class Alarm {
            public string Id { get; }

            public string Name { get; }

            public Alarm(string elementId, int alarmId) {
                Id = $"Element={elementId};Alarm={alarmId}";
                Name = $"Simulated Alarm #{alarmId}";
            }

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
        }

        /// <summary>
        /// This class defines a tag that can be used for syncing signals
        /// </summary>
        public class Tag {
            public string Id { get; }

            public string Name { get; }

            public bool Stepped { get; }

            public Tag(string elementId, int tagId, bool stepped) {
                Id = $"Element={elementId};Tag={tagId}";
                this.Name = $"Simulated Tag #{tagId}";
                this.Stepped = stepped;
            }

            public class Value {
                public TimeInstant Timestamp { get; }

                public double Measure { get; }

                public Value(TimeInstant timestamp, double value) {
                    this.Timestamp = timestamp;
                    this.Measure = value;
                }
            }
        }

        /// <summary>
        /// This class defines a constant that can be used for syncing scalars
        /// </summary>
        public class Constant {
            public string Id { get; }

            public string Name { get; }

            public string UnitOfMeasure { get; }

            public object Value { get; }

            public Constant(string elementId, int constantId, string unitOfMeasure, object value) {
                this.Id = $"Element={elementId};Constant={constantId}";
                this.Name = $"Simulated Constant #{constantId}";
                this.UnitOfMeasure = unitOfMeasure;
                this.Value = value;
            }
        }
    }
}