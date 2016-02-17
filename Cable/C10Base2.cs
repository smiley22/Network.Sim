using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network.Sim.Core;

namespace Network.Sim.Core {
	public class C10Base2 : Cable {
		// 10BASE2 runs at a speed of 10Mbps.
		static readonly int maxBitrate = 10000000;
		// 10BASE2 uses RG-58A/U coaxial cables. 
		static readonly double velocityFactor = 0.66;

		public C10Base2(int length)
			: base(length, maxBitrate, velocityFactor, false) {
		}

		public void Attach(double position, Connector connector) {
			if (connectors.Values.Contains(position))
				throw new InvalidOperationException("The position is already taken.");
			if ((position % 0.5) != 0.0)
				throw new InvalidOperationException("Position must be a multiple of 0.5.");
			if (connectors.ContainsKey(connector))
				throw new InvalidOperationException("Connector is already attached at " +
					connectors[connector] + ".");
			connectors.Add(connector, position);
			connector.Cable = this;
		}
	}
}
