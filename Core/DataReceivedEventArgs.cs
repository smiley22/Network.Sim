using Network.Sim.Lan.Ethernet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Network.Sim.Link;

namespace Network.Sim.Core {
	public class DataReceivedEventArgs : EventArgs {
		public byte[] Data {
			get;
			private set;
		}

		public EtherType Type {
			get;
			private set;
		}

		public DataReceivedEventArgs(byte[] data, EtherType type) {
			Data = data;
			Type = type;
		}
	}
}
