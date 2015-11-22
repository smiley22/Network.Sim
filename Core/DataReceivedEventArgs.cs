using ConsoleApplication36.Lan.Ethernet;
using ConsoleApplication36.Link;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication36.Core {
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
