using System;
using Network.Sim.Core;

namespace Network.Sim.Network.Ip.Routing {
    /// <summary>
    /// Represents a route for reaching a particular network destination.
    /// </summary>
	public class Route {
        /// <summary>
        /// The IP address of the destination network.
        /// </summary>
        /// <remarks>This together with the netmask forms the network destination
        /// id.</remarks>
		public IpAddress Destination {
			get;
			private set;
		}

        /// <summary>
        /// The network mask that, together with the destination IP address describes the
        /// network id.
        /// </summary>
		public IpAddress Netmask {
			get;
			private set;
		}

        /// <summary>
        /// The gateway through which the destination network can be reached.
        /// </summary>
		public IpAddress Gateway {
			get;
			private set;
		}

        /// <summary>
        /// The local interface through which the gateway can be reached.
        /// </summary>
		public Interface Interface {
			get;
			private set;
		}

        /// <summary>
        /// The metric, i.e. cost of using the route.
        /// </summary>
		public int Metric {
			get;
			private set;
		}

        /// <summary>
        /// Initializes a new instance of the Route class.
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
		public Route(IpAddress destination, IpAddress netmask,
			IpAddress gateway, Interface @interface, int metric) {
				Init(destination, netmask, gateway, @interface, metric);
		}

        /// <summary>
        /// Initializes a new instance of the Route class.
        /// </summary>
        /// <param name="cidrNetworkId">The network id of the destination network in
        /// CIDR notation.</param>
        /// <param name="gateway">The gateway through which the destination network
        /// can be reached.</param>
        /// <param name="interface">The local interface through which the gateway
        /// can be reached.</param>
        /// <param name="metric">The metric of using the route.</param>
        public Route(string cidrNetworkId, string gateway,
			Interface @interface, int metric) {
				var tuple = IpAddress.ParseCIDRNotation(cidrNetworkId);
				Init(tuple.Item1, tuple.Item2, gateway != null ? new IpAddress(gateway)
					: null, @interface, metric);
		}

        /// <summary>
        /// Initializes a new instance.
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
        void Init(IpAddress destination, IpAddress netmask, IpAddress gateway,
			Interface @interface, int metric) {
			destination.ThrowIfNull(nameof(destination));
			netmask.ThrowIfNull(nameof(netmask));
			@interface.ThrowIfNull(nameof(@interface));
			if (metric < 0)
				throw new ArgumentException("The metric value must be greater than " +
					"or equal to zero.", nameof(metric));
			Destination = destination;
			Netmask = netmask;
			Gateway = gateway;
			Interface = @interface;
			Metric = metric;
		}

        /// <summary>
        /// Returns a textual representation of this instance of the Route class.
        /// </summary>
        /// <returns></returns>
		public override string ToString() {
			return Destination + " | " + Netmask + " | " +
				(Gateway != null ? Gateway.ToString() : " - ") + " | " +
				Interface.Name + " | " + Metric;
		}
	}
}