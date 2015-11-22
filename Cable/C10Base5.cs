using ConsoleApplication36.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication36.Test {
	public class C10Base5 : Cable {

		static readonly int maxBitrate = 10000000;
		static readonly double velocityFactor = 0.66;

		public C10Base5(int length)
			: base(length, maxBitrate, velocityFactor, false) {
		}

		public C10Base5 Pierce(double position, Connector connector) {
			if (connectors.Values.Contains(position))
				throw new InvalidOperationException("The position is already taken.");
			if ((position % 2.5) != 0.0)
				throw new InvalidOperationException("Position must be a multiple of 2.5.");
			connectors.Add(connector, position);
			connector.Cable = this;

			return this;
		}

	}
}
