using System;
using Network.Sim.Link;

namespace Network.Sim.Core {
    /// <summary>
    /// The event arguments for the SignalCease event.
    /// </summary>
	public class DataReceivedEventArgs : EventArgs {
        /// <summary>
        /// The transmitted data.
        /// </summary>
        public byte[] Data {
			get;
			private set;
		}

        /// <summary>
        /// The protocol encapsulated in the payload of the Ethernet frame.
        /// </summary>
        public EtherType Type {
			get;
			private set;
        }

        /// <summary>
        /// Initializes a new instance of the DataReceivedEventArgs class.
        /// </summary>
        /// <param name="data">The data that was transmitted.</param>
        /// <param name="type">The protocol encapsulated in the payload of the Ethernet
        /// frame.</param>
		public DataReceivedEventArgs(byte[] data, EtherType type) {
			Data = data;
			Type = type;
		}
	}
}