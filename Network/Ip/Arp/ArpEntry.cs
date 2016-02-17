using System;
using Network.Sim.Core;
using Network.Sim.Link;

namespace Network.Sim.Network.Ip.Arp {
	/// <summary>
	/// Represents an ARP entry in the ARP cache.
	/// </summary>
	public class ArpEntry {
		/// <summary>
		/// The maximum age of an entry before it must be re-newed, in
		/// simulation time. The value corresponds to 10 minutes and
		/// is given in nanoseconds.
		/// </summary>
		static readonly ulong maxAge = 600000000000;

		/// <summary>
		/// The MAC-48 address of the entry.
		/// </summary>
		public MacAddress MacAddress {
			get;
			private set;
		}

		/// <summary>
		/// The IPv4 address of the entry.
		/// </summary>
		public IpAddress IpAddress {
			get;
			private set;
		}

		/// <summary>
		/// The time at which the entry expires, in nanoseconds.
		/// </summary>
		public ulong ExpiryTime {
			get;
			private set;
		}

		/// <summary>
		/// Determines if the entry has expired.
		/// </summary>
		public bool Expired {
			get {
				return Simulation.Time > ExpiryTime;
			}
		}

		/// <summary>
		/// Initializes a new instance of the ArpEntry class using the specified
		/// values.
		/// </summary>
		/// <param name="ipAddress">The IPv4 address to initialize this entry
		/// with.</param>
		/// <param name="macAddress">The MAC-48 address to initialize this entry
		/// with.</param>
		/// <exception cref="ArgumentNullException">Thrown if either argument is
		/// null.</exception>
		public ArpEntry(IpAddress ipAddress, MacAddress macAddress) {
			ipAddress.ThrowIfNull("ipAddress");
			macAddress.ThrowIfNull("macAddress");
			IpAddress = ipAddress;
			MacAddress = macAddress;
			
			ExpiryTime = Simulation.Time + maxAge;
		}

		/// <summary>
		/// Returns a textual description of this entry.
		/// </summary>
		/// <returns>A textual representation of this cache entry.</returns>
		public override string ToString() {
			return IpAddress + " -> " + MacAddress + " (Expires " + ExpiryTime + ")";
		}
	}
}
