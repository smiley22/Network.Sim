using System;

namespace Network.Sim.Link {
	/// <summary>
	/// Represents the EtherType field of an Ethernet frame. It indicates which
	/// protocol is encapsulated in the payload of an Ethernet Frame.
	/// </summary>
	/// <remarks>EtherType values are identical to SNAP Protocol Ids and as
	/// such are not only used with Ethernet, contrary to their name.</remarks>
	public enum EtherType : short {
		/// <summary>
		/// Internet Protocol version 4.
		/// </summary>
		IPv4 = 0x0800,
		/// <summary>
		/// Address Resolution Protocol.
		/// </summary>
		ARP = 0x0806
	}
}
