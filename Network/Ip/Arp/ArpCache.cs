using System.Collections.Generic;
using Network.Sim.Network.Ip;

namespace Network.Sim.Network.Ip.Arp {
	/// <summary>
	/// Represents a cache for storing and retrieving ARP entries.
	/// </summary>
	public class ArpCache : Dictionary<IpAddress, ArpEntry> {
		/// <summary>
		/// Returns the ArpEntry for the specified IpAddress key or null
		/// if no such entry exists.
		/// </summary>
		/// <param name="key">The IP-Address to retrieve the ArpEntry for.</param>
		/// <returns>An ArpEntry or null if no entry exists.</returns>
		public new ArpEntry this[IpAddress key] {
			get {
				ArpEntry entry;
				if (this.TryGetValue(key, out entry))
					return entry;
				return null;
			}
		}

		/// <summary>
		/// Adds a new ArpEntry using the specified IpAddress as key.
		/// </summary>
		/// <param name="key">The IPv4 address to use as key.</param>
		/// <param name="value">The ArpEntry to add to the cache.</param>
		public new void Add(IpAddress key, ArpEntry value) {
			if (ContainsKey(key))
				base[key] = value;
			else
				base.Add(key, value);
		}
	}
}
