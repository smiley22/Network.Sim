using System;

namespace Network.Sim.Core {
	public class SignalCeaseEventArgs : EventArgs {
		public byte[] Data {
			get;
			private set;
		}

		public bool IsJam {
			get {
				return Data == null;
			}
		}

		public SignalCeaseEventArgs(byte[] data) {
			Data = data;
		}
	}
}
