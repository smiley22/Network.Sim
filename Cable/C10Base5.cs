using System;

namespace Network.Sim.Core {
    /// <summary>
    /// Represents a cable as is used in 10Base5 Ethernet installations.
    /// </summary>
	public class C10Base5 : Cable {
        /// <summary>
        /// The maximum number of transferable bits per second.
        /// </summary>
	    const int maxBitrate = 10000000;

        /// <summary>
        /// The speed at which a signal propagates through the cable, expressed as a fraction of
        /// the speed of light.
        /// </summary>
	    const double velocityFactor = 0.66;

        /// <summary>
        /// Initializes a new instance of the C10Base5 class.
        /// </summary>
        /// <param name="length">
        /// The length of the cable, in metres.
        /// </param>
        public C10Base5(int length) : base(length, maxBitrate, velocityFactor, false) {
        }

        /// <summary>
        /// Pierces the cable and wires the specified connector to at the specified position.
        /// </summary>
        /// <param name="position">The position at which to pierce the cable.</param>
        /// <param name="connector">The connector to wire to the cable at the pierced
        /// position.</param>
        /// <returns>A reference to the C10Base5 instance for chaining.</returns>
        /// <exception cref="InvalidOperationException">Another connector has already been
        /// installed at the specified position.</exception>
        /// <exception cref="InvalidOperationException">As per IEEE 802.3 specification the
        /// position at which the cable is pierced must be a multiple of 2.5 metres.</exception>
		public C10Base5 Pierce(double position, Connector connector) {
			if (connectors.Values.Contains(position))
				throw new InvalidOperationException("The position is already taken.");
			if (Math.Abs(position % 2.5) > 0.1)
				throw new InvalidOperationException("Position must be a multiple of 2.5.");
			connectors.Add(connector, position);
			connector.Cable = this;
			return this;
		}
	}
}