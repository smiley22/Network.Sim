using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Network.Sim.Core;
using Network.Sim.Link;
using Network.Sim.Miscellaneous;
using Network.Sim.Network.Ip.Arp;
using Network.Sim.Network.Ip.Icmp;
using Network.Sim.Network.Ip.Routing;

namespace Network.Sim.Network.Ip {
	/// <summary>
	/// Represents the Network layer of the OSI model (Layer 3).
	/// </summary>
	public class Ipv4 {
		/// <summary>
		/// A set of packets and their destination IP addresses put on hold
		/// until said destination addresses have been resolved. Each
		/// interface has its own set, because each interface maintains its
		/// own ARP table.
		/// </summary>
		readonly IDictionary<string, ISet<Tuple<IpAddress, IpPacket>>> packetsWaitingOnArpResolve =
			new Dictionary<string, ISet<Tuple<IpAddress, IpPacket>>>();

		/// <summary>
		/// The FIFO buffers for temporarily buffering to-be-send packets.
		/// Each Interface has its own output FIFO.
		/// </summary>
		readonly IDictionary<string, CappedQueue<Tuple<MacAddress, Serializable>>> outputQueue =
			new Dictionary<string, CappedQueue<Tuple<MacAddress, Serializable>>>();

		/// <summary>
		/// The queue of received packets waiting to be processed.
		/// </summary>
		readonly CappedQueue<Tuple<IpPacket, Interface>> inputQueue =
			new CappedQueue<Tuple<IpPacket, Interface>>();

		/// <summary>
		/// An enumerable collection of available interfaces of the host
		/// running the network stack.
		/// </summary>
		readonly IEnumerable<Interface> interfaces;

		/// <summary>
		/// The host's routing table.
		/// </summary>
		readonly RoutingTable routingTable;

		/// <summary>
		/// The nodal processing delay imposed by the host on which the network
		/// stack is running, in nanoseconds.
		/// </summary>
		readonly ulong nodalProcessingDelay;

		/// <summary>
		/// Stores related fragments until the original fragmented packet can be re-
		/// assembled and handed up.
		/// </summary>
		readonly IDictionary<string, ISet<IpPacket>> fragments =
			new Dictionary<string, ISet<IpPacket>>();

		/// <summary>
		/// The ARP module instance for resolving IP addresses to MAC addresses.
		/// </summary>
		readonly Arp.Arp arp;

	    /// <summary>
	    /// Initializes a new instance of the Network class.
	    /// </summary>
	    /// <param name="interfaces">An enumerable collection of interfaces
	    /// installed on the host.</param>
	    /// <param name="routingTable">The routing table to use.</param>
	    /// <param name="nodalProcessingDelay">The nodal processing delay, in
	    /// nanoseconds.</param>
	    /// <exception cref="ArgumentNullException">Thrown if the interfaces
	    /// parameter is null.</exception>
	    public Ipv4(IEnumerable<Interface> interfaces, RoutingTable routingTable,
			ulong nodalProcessingDelay) {
				interfaces.ThrowIfNull("interfaces");
				this.interfaces = interfaces;
				this.routingTable = routingTable;
				this.nodalProcessingDelay = nodalProcessingDelay;

				arp = new Arp.Arp(Output);
		}

		/// <summary>
		/// Wraps the specified higher-level data into IP packets and
		/// transmits them to the specified destination.
		/// </summary>
		/// <param name="ifc">The interface through which to output the data.</param>
		/// <param name="destination">The logical address of the destination
		/// host.</param>
		/// <param name="data">The higher-level data to transmit.</param>
		/// <param name="type">The type of the higher-level data.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		/// <remarks>This API is exposed to the next higher-up layer.</remarks>
		public void Output(Interface ifc, IpAddress destination, byte[] data,
			IpProtocol type) {
			ifc.ThrowIfNull("ifc");
			destination.ThrowIfNull("destination");
			data.ThrowIfNull("data");
			// Construct IP packets of the size of the MTU of the data-link.
			var maxDataSize = ifc.MaximumTransmissionUnit - 20;
			var numPackets = (int) Math.Ceiling(data.Length / (double)maxDataSize);
			var sameSubnet = (destination & ifc.Netmask) == (ifc.IpAddress & ifc.Netmask);
			for (int i = 0, index = 0; i < numPackets; i++) {
				var numBytes = Math.Min(maxDataSize, data.Length - index);
				var packetData = new byte[numBytes];
				Array.Copy(data, index, packetData, 0, numBytes);
				index = index + numBytes;
				// Construct the packet.
				var packet = new IpPacket(destination, ifc.IpAddress, type, packetData);
				// If source and destination are in the same subnet, we can deliver the
				// packet directly. Otherwise send it to the configured default gateway.
				Output(ifc, sameSubnet ? destination : ifc.Gateway, packet);
			}
		}

		/// <summary>
		/// Returns an enumerable collection of ARP entries for the specified interface.
		/// </summary>
		/// <param name="ifc">The interface whose ARP table to return.</param>
		/// <returns>An enumerable collection of ARP entries for the specified
		/// interface.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the ifc parameter is
		/// null.</exception>
		/// <remarks>This method is only needed for simulation output.</remarks>
		public IEnumerable<ArpEntry> ArpTableOf(Interface ifc) {
			ifc.ThrowIfNull("ifc");
			return arp.ArpTableOf(ifc);
		}

		/// <summary>
		/// Resolves the specified IPv4 destination address to a physical
		/// address and hands the specified IPv4 packet down to the link
		/// layer.
		/// </summary>
		/// <param name="ifc">The interface through which to output the
		/// data.</param>
		/// <param name="destination">The logical address of the destination
		/// host, which can different from the final destination address
		/// contained in the IP packet.</param>
		/// <param name="packet">The IPv4 packet to transmit.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		void Output(Interface ifc, IpAddress destination, IpPacket packet) {
			ifc.ThrowIfNull("ifc");
			destination.ThrowIfNull("destination");
			packet.ThrowIfNull("packet");
			// Translate IP address into MAC-Address.
			var macDestination = arp.Lookup(ifc, destination);
			// IP address is not in our ARP table.
			if (macDestination == null) {
				// Put packet on hold until the MAC-48 destination address has
				// been figured out.
				WaitingPacketsOf(ifc).Add(
					new Tuple<IpAddress, IpPacket>(destination, packet));
				WriteLine(ifc.FullName + " is putting IP packet on-hold due to pending ARP request.");
				// Schedule a new ARP request.
				arp.Resolve(ifc, destination);
			} else {
				WriteLine(ifc.FullName + " is queueing IP packet for " + destination);
				Output(ifc, macDestination, packet);
			}
		}

		/// <summary>
		/// Hands the specified packet down to the link layer for transmission to
		/// the specified physical address.
		/// </summary>
		/// <param name="ifc">The interface through which to output the
		/// data.</param>
		/// <param name="destination">The physical address of the destination
		/// host.</param>
		/// <param name="packet">The packet to transmit.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		void Output(Interface ifc, MacAddress destination, Serializable packet) {
			var queue = OutputQueueOf(ifc);
			// Start emptying the fifo of the interface, if we're not already
			// doing it and enqueue the packet.
			if (queue.Count == 0)
				Simulation.Callback(0, () => EmptySendFifo(ifc));
			queue.Enqueue(new Tuple<MacAddress, Serializable>(destination, packet));
		}

		/// <summary>
		/// Returns the set of packets put-on-hold for the specified interface.
		/// </summary>
		/// <param name="ifc">The interface to return the set of waiting
		/// packets for.</param>
		/// <returns>A set of packets put on hold due to a pending ARP
		/// request.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the ifc parameter
		/// is null.</exception>
		ISet<Tuple<IpAddress, IpPacket>> WaitingPacketsOf(Interface ifc) {
			ifc.ThrowIfNull("ifc");
			if (!packetsWaitingOnArpResolve.ContainsKey(ifc.Name)) {
				packetsWaitingOnArpResolve.Add(ifc.Name,
					new HashSet<Tuple<IpAddress, IpPacket>>());
			}
			return packetsWaitingOnArpResolve[ifc.Name];
		}

		/// <summary>
		/// Returns the output queue of the specified interface.
		/// </summary>
		/// <param name="ifc">The interface to return the output queue
		/// for.</param>
		/// <returns>The output queue of the specified interface.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the ifc parameter
		/// is null.</exception>
		/// <remarks>Public only so that it can be output from within the
		/// interpreter.</remarks>
		public CappedQueue<Tuple<MacAddress, Serializable>> OutputQueueOf(Interface ifc) {
			ifc.ThrowIfNull("ifc");
			if (!outputQueue.ContainsKey(ifc.Name))
				outputQueue.Add(ifc.Name, new CappedQueue<Tuple<MacAddress, Serializable>>());
			return outputQueue[ifc.Name];
		}

		/// <summary>
		/// Removes data from the specified interface's output fifo and dispatches
		/// it to the link layer.
		/// </summary>
		void EmptySendFifo(Interface ifc) {
			var tuple = OutputQueueOf(ifc).Dequeue();
			WriteLine(ifc.FullName + " is outputting next " + tuple.Item2.GetType().Name +
				" packet from its output queue.");
			// Figure out whether we're sending an IP or an ARP packet.
			var type = tuple.Item2 is IpPacket ? EtherType.IPv4 : EtherType.ARP;
			ifc.Output(tuple.Item1, tuple.Item2.Serialize(), type);
		}

		/// <summary>
		/// Processes the queue of incoming IP packets.
		/// </summary>
		void ProcessPackets() {
			try {
				var tuple = inputQueue.Dequeue();
				var packet = tuple.Item1;
				var ifc = tuple.Item2;
				packet.TimeToLive--;
				// Drop packet and send "TTL Expired"-ICMP back to packet originator.
				if (packet.TimeToLive == 0) {
					if(packet.Protocol != IpProtocol.Icmp)
						SendIcmp(ifc, packet.Source, IcmpPacket.TimeExceeded(packet));
					return;
				}
				// Incrementally update the checksum.
			    var sum = (uint) (packet.Checksum + 0x01);
				packet.Checksum = (ushort) (sum + (sum >> 16));
				if (IsPacketForUs(packet)) {
					// See if we have all parts and can reassemble the original packet.
					if (IsFragment(packet))
						ReassemblePacket(packet);
					else
						// If it's not a fragment, hand the data up to the transport layer.
						HandUp(packet.Data, packet.Protocol);
				} else {
					// If it's not for us, see if we can forward it to its destination.
					RoutePacket(packet, ifc);
				}
			} finally {
				// Nodal processing delay is the time it takes on average to process
				// a single IP packet.
				if (inputQueue.Count > 0)
					Simulation.Callback(nodalProcessingDelay, ProcessPackets);
			}
		}

		/// <summary>
		/// Attempts to route the specified packet.
		/// </summary>
		/// <param name="packet">The packet to route.</param>
		/// <param name="ifc">The interface through which the packet was
		/// received.</param>
		void RoutePacket(IpPacket packet, Interface ifc) {
			// See if we can find a machting route in our routing table.
			var route = FindRoute(packet);
			// Drop packet and send "Unreachable" ICMP back to packet originator.
			if (route == null) {
				SendIcmp(ifc, packet.Source, IcmpPacket.Unreachable(packet));
				return;
			}
			// Do we have to fragment the packet?
			if (route.Interface.MaximumTransmissionUnit < packet.TotalLength) {
				// Drop packet and send "Fragmentation Required" ICMP back to
				// packet originator.
				if (packet.Flags.HasFlag(IpFlag.DontFragment)) {
					SendIcmp(ifc, packet.Source, IcmpPacket.FragmentationRequired(packet));
					return;
				}
				var packets = FragmentPacket(packet,
					route.Interface.MaximumTransmissionUnit);
				// Forward fragmented packets.
				foreach (var p in packets)
					Output(route.Interface, route.Gateway != null ? route.Gateway :
						p.Destination, p);
			} else {
				// Forward packet.
				Output(route.Interface, route.Gateway != null ? route.Gateway :
					packet.Destination, packet);
			}
		}

		/// <summary>
		/// Traverses the routing table and returns the best route for the
		/// specified packet.
		/// </summary>
		/// <param name="packet">The packet to find a route for.</param>
		/// <returns>The best route found in the routing table or null if
		/// no route was found.</returns>
		Route FindRoute(IpPacket packet) {
			Route best = null;
			foreach (var r in routingTable) {
				if ((r.Destination & r.Netmask) ==
					(packet.Destination & r.Netmask)) {
					if (best == null)
						best = r;
					else if (best.Metric > r.Metric)
						best = r;
				}
			}
			return best;
		}

		/// <summary>
		/// Fragments the specified packet into multiple packets taking into
		/// account the specified maximum transmission unit.
		/// </summary>
		/// <param name="packet">The packet to fragment.</param>
		/// <param name="Mtu">The maximum transmission unit; The maximum size,
		/// in bytes, each of the fragmented packets may have.</param>
		/// <returns>An enumerable collection of packet fragments.</returns>
		public IEnumerable<IpPacket> FragmentPacket(IpPacket packet, int Mtu) {
			// The maximum size of a segment is the MTU minus the IP header size.
			var maxSegmentSize = Mtu - 20;
			var numSegments = (int) Math.Ceiling(packet.Data.Length /
				(double)maxSegmentSize);
			var list = new List<IpPacket>();
			ushort ident = (ushort) (Simulation.Time % 0xFFFF), offset = 0;
			for (var i = 0; i < numSegments; i++) {
				// Set MoreFragments flag for all but the last segment.
				var mf = i < (numSegments - 1);
				var flags = mf ? (packet.Flags | IpFlag.MoreFragments) :
					packet.Flags;
				var dataSize = Math.Min(maxSegmentSize,
					packet.Data.Length - offset * 8);
				var data = new byte[dataSize];
				// Add offset of original packet, as the original packet might be
				// a fragment itself.
				var packetOffset = 
					(ushort) (packet.FragmentOffset + offset);
				Array.Copy(packet.Data, offset * 8, data, 0, dataSize);
				var segment = new IpPacket(packet.Destination, packet.Source,
					packet.Protocol, packet.Ihl, packet.Dscp, packet.TimeToLive,
					ident, flags, packetOffset, data);
				offset = (ushort) (offset + (maxSegmentSize / 8));
				list.Add(segment);
			}
			return list;
		}

		/// <summary>
		/// Determines whether the specified IP packet is for us.
		/// </summary>
		/// <param name="packet">The IP packet to examine.</param>
		/// <returns>True if the IP packet's destination matches one of the
		/// host's interfaces' addresses; Otherwise false.</returns>
		bool IsPacketForUs(IpPacket packet) {
			foreach (var ifc in interfaces) {
				if (ifc.IpAddress == packet.Destination)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Determines whether the specified IP packet is a fragment.
		/// </summary>
		/// <param name="packet">The IP packet to examine.</param>
		/// <returns>true if the packet is a fragment. Otherwise
		/// false.</returns>
		bool IsFragment(IpPacket packet) {
			if(packet.Flags.HasFlag(IpFlag.MoreFragments))
				return true;
			// The last fragment does not have the MoreFragments flag set, but has
			// a non-zero Fragment Offset field, differentiating it from an
			// unfragmented packet.
			if (packet.FragmentOffset > 0)
				return true;
			return false;
		}

		/// <summary>
		/// Reassembles fragmented IP packets and hands them up to the transport
		/// layer once they have been fully reassembled.
		/// </summary>
		/// <param name="packet">An IP packet representing a fragment of a
		/// fragmented packet.</param>
		public void ReassemblePacket(IpPacket packet) {
			// Fragments belong to the same datagram if they have the same source,
			// destination, protocol, and identifier fields (RFC 791, p. 28).			
			var hash = Hash.Sha256(packet.Source +
				packet.Destination.ToString() + packet.Protocol +
				packet.Identification
			);
			// Group related fragments in a set under the same dictionary key.
			if (!fragments.ContainsKey(hash))
				fragments.Add(hash, new HashSet<IpPacket>());
			fragments[hash].Add(packet);
			// Figure out if we already have all fragments so that we can reassemble
			// the original packet.
			var uf = new UnionFind(65536);
			var originalDataSize = 0;
			foreach (var p in fragments[hash]) {
				var from = p.FragmentOffset * 8;
				var to = from + p.Data.Length - 1;
				uf.Union(from, to);
				uf.Union(to, to + 1);
				// Derive original packet size from last fragment.
				if (!p.Flags.HasFlag(IpFlag.MoreFragments))
					originalDataSize = from + p.Data.Length;
			}
			// If this is still 0, last segment has not arrived yet.
			if (originalDataSize == 0)
				return;
			// If the first and the last byte are not part of the same component,
			// not all fragments have arrived yet.
			if (!uf.Connected(0, originalDataSize))
				return;
			var data = new byte[originalDataSize];
			foreach (var p in fragments[hash])
				Array.Copy(p.Data, 0, data, p.FragmentOffset * 8, p.Data.Length);
			// Hand up reassembled data to transport layer.
			HandUp(data, packet.Protocol);
		}

		/// <summary>
		/// Sends the specified ICMP packet to the specified destination host.
		/// </summary>
		/// <param name="ifc">The interface through which the ICMP packet
		/// should be sent.</param>
		/// <param name="destination">The IP address of the destination host
		/// of the ICMP packet.</param>
		/// <param name="packet">The ICMP packet to send.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		void SendIcmp(Interface ifc, IpAddress destination, IcmpPacket packet) {
			ifc.ThrowIfNull("ifc");
			destination.ThrowIfNull("destination");
			packet.ThrowIfNull("packet");
			Simulation.WriteLine(OutputLevel.Icmp, ifc.FullName + " is queueing a " +
				packet.Type + " ICMP packet to " + destination, ConsoleColor.Magenta);
			Output(ifc, destination, packet.Serialize(), IpProtocol.Icmp);
		}

		/// <summary>
		/// Hands up the data part of the specified IP packet to the transport
		/// layer.
		/// </summary>
		/// <param name="data">The data to hand up.</param>
		/// <param name="protocol">The protocol of the data.</param>
		static void HandUp(byte[] data, IpProtocol protocol) {
			// ICMP is a special case since it's not a tranport protocol
			// but is implemented as part of the network layer.
			if (protocol == IpProtocol.Icmp)
				WriteLine("Handling ICMP message");
			else
				WriteLine("Handing up IP data to Transport Layer: " + data.Length);
		}

		/// <summary>
		/// Writes the specified string value to the output stream of the
		/// simulation.
		/// </summary>
		/// <param name="s">The string value to output.</param>
		static void WriteLine(string s) {
			Simulation.WriteLine(OutputLevel.Network, s, ConsoleColor.White);
		}

		#region Interrupt methods invoked on behalf of the link layer.
		/// <summary>
		/// Interrupt method invoked whenever the link layer has emptied its
		/// FIFO and is ready to accept new data for transmission.
		/// </summary>
		public void OnAvailableToSent(Interface ifc) {
			// Keep outputting if there's more data in the interface's output
			// fifo.
			if (OutputQueueOf(ifc).Count > 0)
				Simulation.Callback(0, () => EmptySendFifo(ifc));
		}

		/// <summary>
		/// Interrupt method invoked on behalf of the link-layer of the specified
		/// interface whenever frame payload data can be delivered to the network
		/// layer.
		/// </summary>
		/// <param name="ifc">The interface through which the data was received.</param>
		/// <param name="type">The type of the received data.</param>
		/// <param name="data">A sequence of bytes received.</param>
		/// <exception cref="ArgumentNullException">Thrown if the ifc or the data
		/// argument is null.</exception>
		/// <remarks>This API is exposed to the data-link layer.</remarks>
		public void OnInput(Interface ifc, byte[] data, EtherType type) {
			ifc.ThrowIfNull("ifc");
			data.ThrowIfNull("data");
			switch (type) {
				case EtherType.ARP:
					OnArpInput(ifc, data);
					break;
				case EtherType.IPv4:
					OnIpInput(ifc, data);
					break;
				default:
					WriteLine("Unsupported Ethertype (" + type + "), ignoring data");
					break;
			}
		}

		/// <summary>
		/// Invoked when IP data is being delivered through the specified interface.
		/// </summary>
		/// <param name="ifc">The interface through which the data was received.</param>
		/// <param name="data">A sequence of bytes received.</param>
		/// <exception cref="ArgumentNullException">Thrown if either argument
		/// is null.</exception>
		void OnIpInput(Interface ifc, byte[] data) {
			ifc.ThrowIfNull("ifc");
			data.ThrowIfNull("data");
			WriteLine(ifc.FullName + " has received new IP packet.");
			try {
				var packet = IpPacket.Deserialize(data);
				// This method is called in "interrupt context" and can execute
				// on behalf of different NIC's simultaneously. The IP stack
				// is usually single threaded however, so incoming packets are queued
				// globally and processed sequentially.
				if (inputQueue.Count == 0)
					Simulation.Callback(nodalProcessingDelay, ProcessPackets);
				try {
					// Enqueue the packet and the interface through which it was
					// received.
					inputQueue.Enqueue(new Tuple<IpPacket, Interface>(packet, ifc));
				} catch (InvalidOperationException) {
					// If the host's input queue is full, we must drop the packet.
					WriteLine("IP input queue overflow, dropping packet.");
					// Send a "Source Quench" ICMP to the packet's originator.
					SendIcmp(ifc, packet.Source, IcmpPacket.SourceQuench(packet));
				}
			} catch (SerializationException) {
				WriteLine(ifc.FullName + " has detected a bad checksum, " +
					"discarding IP packet.");
			}
		}

		/// <summary>
		/// Invoked when ARP data is being delivered through the specified interface.
		/// </summary>
		/// <param name="ifc">The interface through which the data was received.</param>
		/// <param name="data">A sequence of bytes received.</param>
		/// <exception cref="ArgumentNullException">Thrown if either argument
		/// is null.</exception>
		void OnArpInput(Interface ifc, byte[] data) {
			// Delegate to ARP module.
			arp.OnInput(ifc, data);
			var waitingSet = WaitingPacketsOf(ifc);
			// See if we can schedule any queued IP packets.
			ISet<Tuple<IpAddress, IpPacket>> sendable =
				new HashSet<Tuple<IpAddress, IpPacket>>();
			foreach (var tuple in waitingSet) {
				if (arp.Lookup(ifc, tuple.Item1) != null)
					sendable.Add(tuple);
			}
			if (sendable.Count > 0)
				WriteLine(ifc.FullName + " can now queue " + sendable.Count +
					" pending packets.");
			foreach (var tuple in sendable) {
				waitingSet.Remove(tuple);
				Output(ifc, tuple.Item1, tuple.Item2);
			}
		}
		#endregion
	}
}
