using System;

namespace Network.Sim.Core {
    /// <summary>
    /// Represents a cable as is used in 10Base2 Ethernet installations.
    /// </summary>
	public class C10Base2 : Cable {
        /// <summary>
        /// The maximum number of transferable bits per second.
        /// </summary>
        /// <remarks>
        ///  10BASE2 runs at a speed of 10Mbps.
        /// </remarks>
        const int maxBitrate = 10000000;

        /// <summary>
        /// The speed at which a signal propagates through the cable, expressed as a fraction of
        /// the speed of light.
        /// </summary>
        /// <remarks>
        /// 10BASE2 uses RG-58A/U coaxial cables.
        /// </remarks>
        const double velocityFactor = 0.66;

        /// <summary>
        /// Initializes a new instance of the C10Base2 class.
        /// </summary>
        /// <param name="length">
        /// The length of the cable, in metres.
        /// </param>
        public C10Base2(int length) : base(length, maxBitrate, velocityFactor, false) {
		}

        /// <summary>
        /// Attach the specified connector to the cable at the specified position.
        /// </summary>
        /// <param name="position">The position at which to connect the connector to the
        /// cable.</param>
        /// <param name="connector">The connector to connect to the cable at the specified
        /// position.</param>
        /// <exception cref="InvalidOperationException">Another connector has already been
        /// installed at the specified position.</exception>
        /// <exception cref="InvalidOperationException">As per 10BASE2 specification the
        /// position at which a station is connected must be a multiple of 0.5 metres.
        /// </exception>
		public void Attach(double position, Connector connector) {
			if (connectors.Values.Contains(position))
				throw new InvalidOperationException("The position is already taken.");
			if (Math.Abs(position % 0.5) > .0)
				throw new InvalidOperationException("Position must be a multiple of 0.5.");
			if (connectors.ContainsKey(connector))
				throw new InvalidOperationException("Connector is already attached at " +
					connectors[connector] + ".");
			connectors.Add(connector, position);
			connector.Cable = this;
		}
	}
}