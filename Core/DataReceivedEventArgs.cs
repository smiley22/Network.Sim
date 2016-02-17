using System;
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
