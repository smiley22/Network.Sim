using System.Collections.Generic;
using Network.Sim.Core;

namespace Network.Sim.Network.Ip.Routing {
    /// <summary>
    /// Represents the routing table of a host.
    /// </summary>
	public class RoutingTable : List<Route> {
        /// <summary>
        /// Initializes a new instance of the RoutingTable class.
        /// </summary>
        public RoutingTable() { }

        /// <summary>
        /// Initializes a new instance of the RoutingTable class.
        /// </summary>
        /// <param name="collection">A collection of routes to initialize the routing
        /// table with.</param>
		public RoutingTable(IEnumerable<Route> collection)
			: base(collection) {
		}

        /// <summary>
        /// Adds a route to the routing table.
        /// </summary>
        /// <param name="destination">The IP address of the destination network. This,
        /// together with the netmask describes the destination network id.</param>
        /// <param name="netmask">The netmask that, together with the destination
        /// parameter describes the destination network id.</param>
        /// <param name="gateway">The gateway through which the destination network
        /// can be reached.</param>
        /// <param name="interface">The local interface through which the gateway
        /// can be reached.</param>
        /// <param name="metric">The metric of using the route.</param>
		public void Add(IpAddress destination, IpAddress netmask,
			IpAddress gateway, Interface @interface, int metric) {
			Add(new Route(destination, netmask, gateway, @interface,
				metric));
		}
	}
}