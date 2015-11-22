using System;

namespace ConsoleApplication36.Core {
	[Flags]
	public enum OutputLevel {
		Physical		= 0x01,
		Arp					= 0x02,
		Datalink		= 0x06,	// Includes ARP.

		Icmp = 0x08,
		Network			= 0x18, // Includes ICMP.

		Simulation	= 0x20
	}
}
