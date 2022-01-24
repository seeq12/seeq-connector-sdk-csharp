using System;
using System.Collections.Generic;

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
    }
}