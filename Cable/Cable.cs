using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Network.Sim.Core {
	/// <summary>
	/// Represents a wired communication channel over which signals can be
	/// transmitted.
	/// </summary>
	public abstract class Cable {
		/// <summary>
		/// The length of the cable, in metres.
		/// </summary>
		public int Length {
			get;
			private set;
		}

		/// <summary>
		/// The absolute speed at which signals propagate through the cable,
		/// measured in metres per second.
		/// </summary>
		public double PropagationSpeed {
			get;
			private set;
		}

		/// <summary>
		/// The number of bits transfered per second.
		/// </summary>
		public int Bitrate {
			get;
			private set;
		}

		/// <summary>
		/// Determines whether the cable is full- or half-duplex.
		/// </summary>
		public bool FullDuplex {
			get;
			private set;
		}

		/// <summary>
		/// The ratio of bits altered during the transmission over the cable due
		/// to noise, interference or distortion.
		/// </summary>
		public double BitErrorRate {
			get;
			private set;
		}

		/// <summary>
		/// The minimum length of burst errors that can occur when transmitting
		/// signals over the cable, measured in number of bits.
		/// </summary>
		public int MinBurstErrorLength {
			get;
			private set;
		}

		/// <summary>
		/// The maximum length of burst errors that can occur when transmitting
		/// signals over the cable, measured in number of bits.
		/// </summary>
		public int MaxBurstErrorLength {
			get;
			private set;
		}

		/// <summary>
		/// Determines whether signal distortions can occur when transmitting data
		/// over the cable.
		/// </summary>
		public bool HasNoise {
			get {
				return BitErrorRate > 0.0;
			}
		}

        /// <summary>
        /// The set of devices attached to the cable.
        /// </summary>
        public IReadOnlyDictionary<Connector, double> Connectors {
			get {
				return new ReadOnlyDictionary<Connector, double>(connectors);
			}
		}

		/// <summary>
		/// The set of devices attached to the cable.
		/// </summary>
		protected readonly IDictionary<Connector, double> connectors =
			new Dictionary<Connector, double>();

	    /// <summary>
	    /// The maximum propagation speed which is the speed of light.
	    /// </summary>
	    const double maxPropSpeed = 299792458;

	    /// <summary>
		/// Initializes a new instance of the Cable class using the specified properties.
		/// </summary>
		/// <param name="length">The length of the cable, in metres.</param>
		/// <param name="bitRate">The number of bits transfered per second.</param>
		/// <param name="velocityFactor">The speed at which a signal propagates through
		/// the cable, expressed as a fraction of the speed of light.</param>
		/// <param name="fullDuplex">true if the cable allows communication in both
		/// directions simultaneously; otherwise false.</param>
		/// <param name="bitErrorRate">The ratio of bits altered during transmission
		/// over the cable due to noise, interference or distortion.</param>
		/// <param name="minBurstErrorLength">The minimum length of burst errors that
		/// can occur when transmitting signals over the cable, measured in number
		/// of bits.</param>
		/// <param name="maxBurstErrorLength">The maximum length of burst errors that
		/// can occur when transmitting signals over the cable, measured in number
		/// of bits.</param>
		/// <exception cref="ArgumentException">Thrown if any argument contains an
		/// illegal value.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if any argument does
		/// not fall within its expected range.</exception>
	    protected Cable(int length, int bitRate, double velocityFactor, bool fullDuplex = true,
			double bitErrorRate = .0, int minBurstErrorLength = 0, int maxBurstErrorLength = 0) {
			if (length <= 0)
				throw new ArgumentException("Length must be greater than 0.", nameof(length));
			if (bitRate <= 0)
				throw new ArgumentException("The bitrate must be greater than 0.", nameof(bitRate));
			if (velocityFactor <= 0.0 || velocityFactor > 1.0)
				throw new ArgumentOutOfRangeException(nameof(velocityFactor), "The velocity factor " +
					"must be between 0.0 and 1.0.");
			if (bitErrorRate < 0.0 || bitErrorRate > 1.0)
				throw new ArgumentOutOfRangeException(nameof(bitErrorRate), "The bit error rate " +
					"must be between 0.0 and 1.0.");
			if (minBurstErrorLength < 0)
				throw new ArgumentException("The minimum burst error length must be greater " +
					"than or equal to 0.", nameof(minBurstErrorLength));
			if (maxBurstErrorLength < minBurstErrorLength)
				throw new ArgumentException("The maximum burst error length must be greater " +
					"than or equal to the minimum burst error length", nameof(maxBurstErrorLength));
			Length = length;
			PropagationSpeed = maxPropSpeed * velocityFactor;
			Bitrate = bitRate;
			FullDuplex = fullDuplex;
			BitErrorRate = bitErrorRate;
			MinBurstErrorLength = minBurstErrorLength;
			MaxBurstErrorLength = maxBurstErrorLength;
		}

        /// <summary>
        /// Simulates the transmission of data from the specified source.
        /// </summary>
        /// <param name="source">The connector from which data is being transmitted.</param>
        /// <param name="data">The data that is being transmitted from the source.</param>
		public void Transmit(Connector source, byte[] data) {
			source.ThrowIfNull("source");
			data.ThrowIfNull("data");
			var position = connectors[source];
			// Simulate physical frame corruption.
			if (HasNoise)
				data = DistortSignal(data);
			// Calculate transmission time = Size / Bitrate.
			var transTimeNs = (ulong) (1000000000 *
				(data.Length * 8 / (double) Bitrate));
			// Calculate events for each device on the cable.
			foreach (var pair in connectors) {
				var distance = Math.Abs(position - pair.Value);
				// Propagation delay in nanoseconds.
				var propDelayNs = (ulong) (1000000000 *
					(distance / PropagationSpeed));
				var deliveryTimeNs = propDelayNs + transTimeNs;
				Simulation.AddEvent(
					new SignalSenseEvent(propDelayNs, pair.Key, source));
				Simulation.AddEvent(
					new SignalCeaseEvent(deliveryTimeNs, pair.Key, data, source));
			}
		}

        /// <summary>
        /// Simulates the transmission of a Jam signal.
        /// </summary>
        /// <param name="source">The source sending the Jam signal.</param>
        /// <returns>The time it takes to transmit the Jam signal, in nanoseconds.</returns>
		public ulong Jam(Connector source) {
			Simulation.RemoveEvents(ev => ev is SignalCeaseEvent &&
				ev.Sender == source);
			var position = connectors[source];
			// Calculate the transmission time.
			var transTimeNs = (ulong) (1000000000 * (48 / (double) Bitrate));
			// Calculate the propagation delay and delivery time for each
			// NIC within the collision domain.
			foreach (var pair in connectors) {
				var distance = Math.Abs(position - pair.Value);
				var propDelayNs = (ulong) (1000000000 *
					(distance / PropagationSpeed));
				var deliveryTimeNs = propDelayNs + transTimeNs;
				Simulation.AddEvent(
					new SignalCeaseEvent(deliveryTimeNs, pair.Key, null, source));
			}
			return transTimeNs;
		}

		/// <summary>
		/// Simulates the distortion of signals due to noise, interference, etc.
		/// </summary>
		/// <param name="data">The data to distort using this cables specific
		/// bit error rate and properties.</param>
		/// <returns>An array of the same size as the input data array containing
		/// the modified data.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the data parameter
		/// is null.</exception>
		/// <remarks>
		/// For details, cmp.:
		/// http://stackoverflow.com/questions/17885617/algorithm-for-simulating-burst-errors
		/// 
		/// Expected number of good bits per iteration = 1
		/// Expected number of error bits per iteration = 8/32
		/// Error rate = (8/32)/(1+8/32) = 8/40 = 2/10
		///
		/// 2/10 = (8/x) / (1 + 8/x)
		/// (2/10) * (1 + 8/x) = (8/x)
		/// (2/10) + (16/10x) = (8/x)
		/// (2/10) = (8/x) - (16/10x)
		/// (2/10) = (80/10x) - (16/10x)
		/// (2/10) = (64/10x)
		/// (2/10) * 10x = 64
		/// 20x/10 = 64
		/// 2x = 64
		/// x = 32
		///
		/// ber = bit error rate
		/// n = average length of burst-error
		///
		/// ber = (n/x) / (1 + n/x)
		/// ber * (1 + n/x) = (n/x)
		/// ber + (ber * n) / x = (n/x)
		/// ber = (n/x) - ((ber * n) / x)
		/// ber = (n - (ber * n)) / x
		/// ber * x = (n - (ber * n))
		/// x = (n - (ber * n)) / ber
		/// </remarks>
		byte[] DistortSignal(byte[] data) {
			data.ThrowIfNull("data");
			if (!HasNoise)
				return data;
			var random = new Random();
			var distorted = new byte[data.Length];
			Array.Copy(data, distorted, data.Length);
			double expectedBurstLength = 0;
			for (var i = MinBurstErrorLength; i <= MaxBurstErrorLength; i++)
				expectedBurstLength += i;
			expectedBurstLength = expectedBurstLength / (MaxBurstErrorLength - MinBurstErrorLength + 1);
			var p = (expectedBurstLength -
				BitErrorRate * expectedBurstLength) / BitErrorRate;
			for (var i = 0; i < distorted.Length * 8; i++) {
				if (random.NextDouble() < 1 / p) {
					var n = random.Next(MinBurstErrorLength, MaxBurstErrorLength + 1);
					// Distort the next n bits.
					for (var c = i; c < i + n && c < distorted.Length * 8; c++) {
						int offset = c / 8, bit = c % 8;
						distorted[offset] &= (byte) ~(1 << bit);
						distorted[offset] |= (byte) (random.Next(2) << bit);
					}
					i += n;
				}
				i++;
			}
			return distorted;
		}
	}
}