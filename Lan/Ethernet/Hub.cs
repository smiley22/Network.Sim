using System;
using System.Collections.Generic;
using Network.Sim.Core;

namespace Network.Sim.Lan.Ethernet {
	/// <summary>
	/// Represents an Ethernet hub.
	/// </summary>
	public class Hub {
		/// <summary>
		/// The hub's I/O ports.
		/// </summary>
		public IReadOnlyList<Connector> Ports {
			get {
				return ports;
			}
		}

		/// <summary>
		/// The hub's inherent propagation delay.
		/// </summary>
		public ulong Delay {
			get;
			private set;
		}

		/// <summary>
		/// The hub's I/O ports.
		/// </summary>
		List<Connector> ports = new List<Connector>();

		/// <summary>
		/// Initializes a new instance of the Hub class.
		/// </summary>
		/// <param name="numPorts">The number of I/O ports.</param>
		/// <param name="delay">The inherent propagation delay of the hub.</param>
		public Hub(int numPorts, ulong delay) {
			for (int i = 0; i < numPorts; i++) {
				Connector c = new Connector();
				c.SignalSense += (sender, e) => {
					Repeat(c, (other, propDelay) =>
						Simulation.AddEvent(new SignalSenseEvent(propDelay,
							other, sender)));
				};
				c.SignalCease += (sender, e) => {
					Repeat(c, (other, propDelay) =>
						Simulation.AddEvent(new SignalCeaseEvent(propDelay,
							other, e.Data, sender)));
				};
				ports.Add(c);
			}
		}

		/// <summary>
		/// Invokes the specified action for every connector within the
		/// collision domain, excluding those connected through the specified
		/// inport.
		/// </summary>
		/// <param name="inport">The port through which the signal is being
		/// received.</param>
		/// <param name="action">The action to invoke.</param>
		void Repeat(Connector inport, Action<Connector, ulong> action) {
			foreach (Connector c in ports) {
				if (!c.IsConnected || c == inport)
					continue;
				double pos = c.Cable.Connectors[c];
				foreach (var pair in c.Cable.Connectors) {
					if (pair.Key == c)
						continue;
					double dist = Math.Abs(pos - pair.Value);
					ulong propDelayNs = (ulong) (1000000000 *
						(dist / (double) c.Cable.PropagationSpeed));
					action(pair.Key, Delay + propDelayNs);
				}
			}
		}
	}
}
