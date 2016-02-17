using System;

namespace Network.Sim.Core {
    /// <summary>
    /// The different output-levels for outputting simulation events to the interpreter's console
    /// window.
    /// </summary>
	[Flags]
	public enum OutputLevel {
        /// <summary>
        /// Outputs events for the physical layer.
        /// </summary>
		Physical = 0x01,
        /// <summary>
        /// Outputs events for the Address Resolution Protocol.
        /// </summary>
		Arp = 0x02,
        /// <summary>
        /// Outputs events for the data-link layer including any ARP events.
        /// </summary>
		Datalink = 0x06,
        /// <summary>
        /// Outputs events for the Internet Control Message Protocol.
        /// </summary>
		Icmp = 0x08,
        /// <summary>
        /// Outputs events for the IP layer including any ICMP events.
        /// </summary>
		Network = 0x18,
        /// <summary>
        /// Outputs events of the simulation.
        /// </summary>
		Simulation	= 0x20
	}
}