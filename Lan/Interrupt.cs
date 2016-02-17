using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network.Sim.Lan {
	/// <summary>
	/// The different kinds of interrupts that can be caused by a Network
	/// Interface Card.
	/// </summary>
	public enum Interrupt {
		/// <summary>
		/// The interrupt that is caused when the NIC has received a frame.
		/// </summary>
		DataReceived,
		/// <summary>
		/// The interrupt that is caused when the NIC's output FIFO has been
		/// emptied.
		/// </summary>
		SendFifoEmpty
	}
}
