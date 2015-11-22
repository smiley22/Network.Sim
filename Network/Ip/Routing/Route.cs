using ConsoleApplication36.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication36.Network.Ip.Routing {
	public class Route {
		public IpAddress Destination {
			get;
			private set;
		}

		public IpAddress Netmask {
			get;
			private set;
		}

		public IpAddress Gateway {
			get;
			private set;
		}

		public Interface Interface {
			get;
			private set;
		}

		public int Metric {
			get;
			private set;
		}

		public Route(IpAddress destination, IpAddress netmask,
			IpAddress gateway, Interface @interface, int metric) {
				Init(destination, netmask, gateway, @interface, metric);
		}

		public Route(string cidrNetworkId, string gateway,
			Interface @interface, int metric) {
				Tuple<IpAddress, IpAddress> tuple =
					IpAddress.ParseCIDRNotation(cidrNetworkId);
				Init(tuple.Item1, tuple.Item2, gateway != null ? new IpAddress(gateway)
					: null, @interface, metric);
		}


		void Init(IpAddress destination, IpAddress netmask, IpAddress gateway,
			Interface @interface, int metric) {
			destination.ThrowIfNull("destination");
			netmask.ThrowIfNull("netmask");
			@interface.ThrowIfNull("interface");
			if (metric < 0)
				throw new ArgumentException("The metric value must be greater than " +
					"or equal to zero.", "metric");
			Destination = destination;
			Netmask = netmask;
			Gateway = gateway;
			Interface = @interface;
			Metric = metric;
		}

		public override string ToString() {
			return Destination + " | " + Netmask + " | " +
				(Gateway != null ? Gateway.ToString() : " - ") + " | " +
				Interface.Name + " | " + Metric;
		}
	}
}
