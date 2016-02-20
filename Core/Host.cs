using System;
using System.Collections.Generic;
using Network.Sim.Network.Ip;
using Network.Sim.Network.Ip.Routing;

namespace Network.Sim.Core {
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
		readonly RoutingTable routingTable = new RoutingTable();

	    /// <summary>
	    /// Initializes a new instance of the Host class.
	    /// </summary>
	    /// <param name="hostName">The host's name.</param>
	    /// <param name="nodalProcessingDelay"></param>
	    /// <param name="interfaces"></param>
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
				foreach (var ifc in interfaces)
					RegisterInterface(ifc);
			}
		}

	    /// <summary>
	    /// Registers a new network interface with the host.
	    /// </summary>
	    /// <param name="ifc"></param>
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

        /// <summary>
        /// Adds a route to the host's routing table.
        /// </summary>
        /// <param name="route">The route to add to the host's routing table.</param>
        /// <param name="index">The row number to insert the route add or null to add the route
        /// to the end of the routing table.</param>
        /// <exception cref="ArgumentNullException">The route parameter is null.</exception>
		public void AddRoute(Route route, int? index = null) {
            route.ThrowIfNull(nameof(route));
			if (!index.HasValue)
				routingTable.Add(route);
			else
				routingTable.Insert(index.Value, route);
		}

        /// <summary>
        /// Adds a route to the host's routing table.
        /// </summary>
        /// <param name="cidrNetworkId">The destination subnet specified in CIDR notation.</param>
        /// <param name="gateway">The gateway through which the destination network can be
        /// reached.</param>
        /// <param name="interface">The local interface through which the gateway can be
        /// reached.</param>
        /// <param name="metric">The added cost of using the route.</param>
        /// <param name="index">The index or row number at which to insert the route into
        /// the routing table.</param>
		public void AddRoute(string cidrNetworkId, string gateway,
			Interface @interface, int metric, int? index = null) {
				AddRoute(new Route(cidrNetworkId, gateway, @interface, metric));
		}

        /// <summary>
        /// Adds a route to the host's routing table.
        /// </summary>
        /// <param name="cidrNetworkId">The destination subnet specified in CIDR notation.</param>
        /// <param name="gateway">The gateway through which the destination network can be
        /// reached.</param>
        /// <param name="ifName">The name of the local interface through which the gateway
        /// can be reached.</param>
        /// <param name="metric">The added cost of using the route.</param>
        /// <param name="index">The index or row number at which to insert the route into
        /// the routing table. If this is null, the route is appended to the end of the
        /// routing table.</param>
		public void AddRoute(string cidrNetworkId, string gateway,
			string ifName, int metric, int? index = null) {
				if (!Interfaces.ContainsKey(ifName))
					throw new ArgumentException("The interface '" + ifName + "' " +
						"could not be found", nameof(ifName));
				var ifc = Interfaces[ifName];
				AddRoute(cidrNetworkId, gateway, ifc, metric, index);
		}

        /// <summary>
        /// Removes the specified route from the host's routing table.
        /// </summary>
        /// <param name="route">The route to remove.</param>
        /// <exception cref="ArgumentNullException">The route parameter is null.</exception>
		public bool RemoveRoute(Route route) {
            route.ThrowIfNull(nameof(route));
			return routingTable.Remove(route);
		}

        /// <summary>
        /// Removes the route at the specified row number from the host's routing table.
        /// </summary>
        /// <param name="index">The row number of the route to remove.</param>
		public void RemoveRoute(int index) {            
			routingTable.RemoveAt(index);
		}


        /// <summary>
        /// Sends the specified data to the specified destination address through the
        /// specified local interface.
        /// </summary>
        /// <param name="ifName">The name of the local interface to send the data
        /// through.</param>
        /// <param name="destination">The IP-address of the destination host.</param>
        /// <param name="data">The data to transmit.</param>
		public void Output(string ifName, IpAddress destination, byte[] data) {
			Network.Output(Interfaces[ifName],
				destination,
				data,
				IpProtocol.Tcp);
		}
	}
}
