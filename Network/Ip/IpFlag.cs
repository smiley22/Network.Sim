using System;

namespace ConsoleApplication36.Network.Ip {
	/// <summary>
	/// Represents the Flags field of an IP-packet.
	/// </summary>
	[Flags]
	public enum IpFlag {
		/// <summary>
		/// If DontFrament is specified and fragmentation is required to
		/// route the packet, then the packet is dropped.
		/// </summary>
		DontFragment = 0x01,

		/// <summary>
		/// Set if the packet is a fragmented packet.
		/// </summary>
		MoreFragments = 0x02
	}
}
