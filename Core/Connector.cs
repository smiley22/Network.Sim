using System;

namespace Network.Sim.Core {
	/// <summary>
	/// Represents a connector of a device such as a NIC, a Hub or an Ethernet Bridge.
	/// </summary>
	public class Connector {
        /// <summary>
        /// The event that is raised when the connector detects a signal.
        /// </summary>
		public event EventHandler SignalSense;

        /// <summary>
        /// The event that is raised when the connector detects a signal cease.
        /// </summary>
		public event EventHandler<SignalCeaseEventArgs> SignalCease;
        /// <summary>
        /// The cable attached to the connector.
        /// </summary>
		public Cable Cable {
			get;
			set;
		}

        /// <summary>
        /// Determines whether the connector is currently wired up.
        /// </summary>
		public bool IsConnected {
			get {
				return Cable != null;
			}
		}

        /// <summary>
        /// Outputs data to the cable connected to the connector.
        /// </summary>
        /// <param name="data">The data to output.</param>
		public void Transmit(byte[] data) {
			if (IsConnected)
				Cable.Transmit(this, data);
		}

        /// <summary>
        /// Outputs a jam signal to the connected. cable
        /// </summary>
        /// <returns>The time it takes to transmit the Jam signal, in nanoseconds.</returns>
		public ulong Jam() {
			return Cable.Jam(this);
		}

        /// <summary>
        /// Raises the signal sense event.
        /// </summary>
        /// <param name="sender">A reference to the object raising the event.</param>
		public void RaiseSignalSense(object sender) {
			SignalSense.RaiseEvent(sender, null);
		}

        /// <summary>
        /// Raises the signal cease event.
        /// </summary>
        /// <param name="sender">A reference to the object raising the event.</param>
        /// <param name="data">The data that was transmitted.</param>
        public void RaiseSignalCease(object sender, byte[] data) {
			SignalCease.RaiseEvent(sender, new SignalCeaseEventArgs(data));
		}
	}
}