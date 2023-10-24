using System;
using System.Collections.Generic;
using System.Linq;
using Seeq.Link.SDK.Utilities;

namespace MyCompany.Seeq.Link.Connector {

    public class DatasourceSimulator {

        // NOTE: the data structures in this file are purely for illustration purposes only
        // and are here solely to approximate datasource response structures for syncing
        public class Element {
            public string Id { get; }

            public string Name { get; }

            public Element(int elementId) {
                Id = elementId.ToString();
                Name = $"Simulated Element #{elementId}";
            }
        }

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

        // This is NOT intended for production use and is solely to model possible
        // datasource tag and measurement structures that can used when syncing signals.
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

        // To be able to yield consistent, reproducible tag values, we need a constant seed. This helps us
        // approximate the behaviour of a real datasource which should be deterministic.
        private const int RandomnessSeed = 1_000_000;

        private readonly Random RNG = new Random(RandomnessSeed);

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
            int databaseCount = RNG.Next(10);
            return Enumerable.Range(1, databaseCount)
                .Select(elementId => new Element(elementId));
        }

        public IEnumerable<Alarm> GetAlarmsForDatabase(string elementId) {
            int alarmCount = RNG.Next(10);
            return Enumerable.Range(1, alarmCount)
                .Select(alarmId => new Alarm(elementId, alarmId));
        }

        public IEnumerable<Tag> GetTagsForDatabase(string elementId) {
            int tagCount = RNG.Next(10);
            return Enumerable.Range(1, tagCount)
                .Select(tagId => new Tag(elementId, tagId, tagId % 2 == 0));
        }

        public IEnumerable<Constant> GetConstantsForDatabase(string elementId) {
            int constantCount = RNG.Next(10);
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
            DateTime endTime = endTimestamp.ToDateTimeRoundUpTo100ns();
            long timespanInMs = (long)(endTime - startTime).TotalMilliseconds;
            long timestampIncrement = timespanInMs / limit;

            for (int i = 1; i <= limit; i++) {
                DateTime start = startTime + TimeSpan.FromMilliseconds(timestampIncrement * i);
                DateTime end = start + TimeSpan.FromMilliseconds(10);
                yield return new Alarm.Event(start, end, RNG.NextDouble());
            }
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
    }
}