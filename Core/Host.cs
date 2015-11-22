using ConsoleApplication36.Lan.Ethernet;
using ConsoleApplication36.Network.Ip;
using ConsoleApplication36.Network.Ip.Routing;
using System;
using System.Collections.Generic;

namespace ConsoleApplication36.Core {
	/// <summary>
	/// Represents a network host.
	/// </summary>
	public class Host {
		/// <summary>
		/// The IP stack instance of the host.
		/// </summary>
		public Ipv4 Network {
			get;
			set;
		}

		/// <summary>
		/// The host's name.
		/// </summary>
		public string Hostname {
			get;
			private set;
		}

		/// <summary>
		/// A collection of network interfaces installed in the host.
		/// </summary>
		public IDictionary<string, Interface> Interfaces {
			get;
			private set;
		}

		/// <summary>
		/// The nodal processing delay imposed by the host's processing
		/// speed, in nanoseconds.
		/// </summary>
		public ulong NodalProcessingDelay {
			get;
			private set;
		}

		/// <summary>
		/// An enumerable collection of routes present in the host's routing table.
		/// </summary>
		public IEnumerable<Route> Routes {
			get;
			private set;
		}

		/// <summary>
		/// The host's routing table.
		/// </summary>
		RoutingTable routingTable = new RoutingTable();

		/// <summary>
		/// Initializes a new instance of the Host class.
		/// </summary>
		/// <param name="hostName">The host's name.</param>
		/// <exception cref="ArgumentNullException">Thrown if the hostName
		/// parameter is null.</exception>
		public Host(string hostName, ulong nodalProcessingDelay = 20000,
			IEnumerable<Interface> interfaces = null) {
			hostName.ThrowIfNull("hostName");
			Hostname = hostName;
			NodalProcessingDelay = nodalProcessingDelay;
			Interfaces = new Dictionary<string, Interface>();
			Network = new Ipv4(Interfaces.Values, routingTable,
				nodalProcessingDelay);
			Routes = routingTable;

			if (interfaces != null) {
				foreach (Interface ifc in interfaces)
					RegisterInterface(ifc);
			}
		}

		/// <summary>
		/// Registers a new network interface with the host.
		/// </summary>
		/// <param name="nic">The network interface card (NIC) to associate with
		/// the new interface.</param>
		/// <param name="name">The name of the interface.</param>
		/// <param name="ipAddress">The IPv4 address to configure the interface
		/// with.</param>
		/// <param name="netmask">The subnetmask to configure the interface
		/// with.</param>
		/// <param name="gateway">The IPv4 address of the default gateway to
		/// configure the interface with.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the
		/// arguments is null.</exception>
		public void RegisterInterface(Interface ifc) {
			ifc.ThrowIfNull("ifc");
			Interfaces.Add(ifc.Name, ifc);

			ifc.Hostname = Hostname;

			// Delegate the interface's events to the network stack.
			ifc.DataReceivedEvent += (sender, args) =>
				Network.OnInput(sender as Interface, args.Data, args.Type);
			ifc.SendFifoEmptyEvent += (sender, args) =>
				Network.OnAvailableToSent(sender as Interface);
		}


		public void AddRoute(Route route, int? index = null) {
			if (!index.HasValue)
				routingTable.Add(route);
			else
				routingTable.Insert(index.Value, route);
		}

		public void AddRoute(string cidrNetworkId, string gateway,
			Interface @interface, int metric, int? index = null) {
				AddRoute(new Route(cidrNetworkId, gateway, @interface, metric));
		}

		public void AddRoute(string cidrNetworkId, string gateway,
			string ifName, int metric, int? index = null) {
				if (!Interfaces.ContainsKey(ifName))
					throw new ArgumentException("The interface '" + ifName + "' " +
						"could not be found", "ifName");
				Interface ifc = Interfaces[ifName];
				AddRoute(cidrNetworkId, gateway, ifc, metric, index);
		}

		public void RemoveRoute(Route route) {
			routingTable.Remove(route);
		}

		public void RemoveRoute(int index) {
			routingTable.RemoveAt(index);
		}


		public void Output(string ifName, IpAddress destination, byte[] data) {
			Network.Output(Interfaces[ifName],
				destination,
				data,
				IpProtocol.Tcp);
		}
	}
}
