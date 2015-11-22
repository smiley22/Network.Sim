using ConsoleApplication36.Test;
using System;

namespace ConsoleApplication36.Core {
	public class Connector {
		public event EventHandler SignalSense;
		public event EventHandler<SignalCeaseEventArgs> SignalCease;
		public Cable Cable {
			get;
			set;
		}

		public bool IsConnected {
			get {
				return Cable != null;
			}
		}

		public void Transmit(byte[] data) {
			if (IsConnected)
				Cable.Transmit(this, data);
		}
		public ulong Jam() {
			return Cable.Jam(this);
		}

		public void RaiseSignalSense(object sender) {
			SignalSense.RaiseEvent(sender, null);
		}

		public void RaiseSignalCease(object sender, byte[] data) {
			SignalCease.RaiseEvent(sender, new SignalCeaseEventArgs(data));
		}
	}
}
