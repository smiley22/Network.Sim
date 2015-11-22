using ConsoleApplication36.Core;
using ConsoleApplication36.Link;
using ConsoleApplication36.Miscellaneous;
using System;
using System.Collections.Generic;

namespace ConsoleApplication36.Network.Ip.Arp {
	/// <summary>
	/// Implements the Address Resolution Protocol.
	/// </summary>
	public class Arp {
		/// <summary>
		/// The ARP caches. Each interface maintains its own ARP cache. 
		/// </summary>
		IDictionary<string, ArpCache> cache = new Dictionary<string, ArpCache>();

		/// <summary>
		/// A set of IPv4 addresses for which ARP requests are currently in
		/// progress.
		/// </summary>
		ISet<IpAddress> arpInProgress = new HashSet<IpAddress>();

		/// <summary>
		/// The delegate for invoking a transmission of the network layer.
		/// </summary>
		Output output;

		/// <summary>
		/// The layer-2 physical address used for broadcasting to all stations
		/// on the wire.
		/// </summary>
		static readonly MacAddress broadcastAddress =
			new MacAddress("FF:FF:FF:FF:FF:FF");

		/// <summary>
		/// Initializes a new instance of the Arp class.
		/// </summary>
		/// <param name="output">The delegate for invoking the network layer's
		/// output method.</param>
		/// <exception cref="ArgumentNullException">Thrown if the output parameter
		/// is null.</exception>
		public Arp(Output output) {
			output.ThrowIfNull("output");
			this.output = output;
		}

		/// <summary>
		/// The delegate for invoking the network layer's output method.
		/// </summary>
		public delegate void Output(Interface ifc, MacAddress destination,
			Serializable packet);

		/// <summary>
		/// Looks up the Layer-2 (MAC-48) physical address in the ARP cache for
		/// the specified IPv4 address.
		/// </summary>
		/// <param name="ifc">The interface to look an address up for.</param>
		/// <param name="ipAddress">The IPv4 address to look up the respective
		/// MAC-48 address for.</param>
		/// <returns>A MAC-48 address or null if the lookup could not be satisfied
		/// from the ARP cache.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the ipAddress parameter
		/// is null.</exception>
		public MacAddress Lookup(Interface ifc, IpAddress ipAddress) {
			ipAddress.ThrowIfNull("ipAddress");
			if (!cache.ContainsKey(ifc.Name))
				cache.Add(ifc.Name, new ArpCache());
			ArpEntry entry = cache[ifc.Name][ipAddress];
			if (entry != null && entry.Expired == false)
				return entry.MacAddress;
			return null;
		}

		/// <summary>
		/// Resolves the specified IPv4 address to a Layer-2 (MAC-48)
		/// physical address.
		/// </summary>
		/// <param name="ipAddress">The IPv4 address to resolve.</param>
		/// <exception cref="ArgumentNullException">Thrown if either of the
		/// arguments is null.</exception>
		/// <remarks>This API is exposed to the next higher-up layer. In other
		/// words, it is called by the Network layer to resolve an IPv4 address
		/// to the corresponding MAC-48 address.</remarks>
		public void Resolve(Interface ifc, IpAddress ipAddress) {
			ifc.ThrowIfNull("ifc");
			ipAddress.ThrowIfNull("ipAddress");
			// If there's already a pending ARP request for the IP, don't
			// issue another one.
			if (arpInProgress.Contains(ipAddress))
				return;
			// Output an ARP request to physical broadcast address
			// FF:FF:FF:FF:FF:FF.
			arpInProgress.Add(ipAddress);
			ArpPacket packet = new ArpPacket(ifc.Nic.MacAddress, ifc.IpAddress,
				ipAddress);
			WriteLine(ifc.FullName + " is constructing an ARP request for " +
				ipAddress + ".");
			output(ifc, broadcastAddress, packet);
		}

		/// <summary>
		/// Examines and processes the ARP message contained in the specified byte
		/// array.
		/// </summary>
		/// <param name="ifc">The interface through which the data was
		/// received.</param>
		/// <param name="data">A sequence of bytes containing an ARP packet.</param>
		/// <exception cref="ArgumentNullException">Thrown if either parameter
		/// is null.</exception>
		/// <exception cref="SerializationException">Thrown if the data array does
		/// not contain a valid ARP packet.</exception>
		public void OnInput(Interface ifc, byte[] data) {
			ifc.ThrowIfNull("ifc");
			data.ThrowIfNull("data");
			WriteLine(ifc.FullName + " has received an ARP message.");
			ArpPacket packet = ArpPacket.Deserialize(data);
			// If it's our own packet, don't do anything with it.
			if (packet.MacAddressSender == ifc.Nic.MacAddress)
				return;
			// Update our ARP cache with the sender's information.
			if (!cache.ContainsKey(ifc.Name))
				cache.Add(ifc.Name, new ArpCache());
			cache[ifc.Name].Add(packet.IpAddressSender,
				new ArpEntry(packet.IpAddressSender, packet.MacAddressSender));
			if(packet.IsResponse)
				WriteLine(ifc.FullName + " is updating its ARP table with [" +
					packet.IpAddressSender + ", " + packet.MacAddressSender + "]");
			// Remove IP address from pending list, if present.
			arpInProgress.Remove(packet.IpAddressSender);
			if (packet.IsRequest) {
				// It's an ARP request and we are the recipient, so send an ARP reply
				// back to the sender.
				if (packet.IpAddressTarget == ifc.IpAddress) {
					ArpPacket response = new ArpPacket(ifc.Nic.MacAddress,
						packet.IpAddressTarget, packet.MacAddressSender,
						packet.IpAddressSender);
					WriteLine(ifc.FullName + " is constructing an ARP response for " +
						packet.IpAddressSender + ".");
					output(ifc, packet.MacAddressSender, response);
				}
			}
		}

		/// <summary>
		/// Returns an enumerable collection of ARP entries for the specified
		/// interface.
		/// </summary>
		/// <param name="ifc">The interface whose ARP table to return.</param>
		/// <returns>An enumerable collection of ARP entries.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the ifc parameter
		/// is null.</exception>
		public IEnumerable<ArpEntry> ArpTableOf(Interface ifc) {
			ifc.ThrowIfNull("ifc");
			if (!cache.ContainsKey(ifc.Name))
				cache.Add(ifc.Name, new ArpCache());
			return cache[ifc.Name].Values;
		}

		/// <summary>
		/// Writes the specified string value to the output stream of the
		/// simulation.
		/// </summary>
		/// <param name="s">The string value to output.</param>
		static void WriteLine(string s) {
			Simulation.WriteLine(OutputLevel.Arp, s, ConsoleColor.Cyan);
		}
	}
}
