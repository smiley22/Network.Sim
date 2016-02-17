using System;

namespace Network.Sim.Core {
    /// <summary>
    /// The event arguments for the SignalCease event.
    /// </summary>
	public class SignalCeaseEventArgs : EventArgs {
        /// <summary>
        /// The transmitted data.
        /// </summary>
		public byte[] Data {
			get;
			private set;
		}

        /// <summary>
        /// True if the signal was a Jam signal; Otherwise false.
        /// </summary>
		public bool IsJam {
			get {
				return Data == null;
			}
		}

        /// <summary>
        /// Initializes a new instance of the SignalCeaseEventArgs class.
        /// </summary>
        /// <param name="data">The data that was transmitted.</param>
		public SignalCeaseEventArgs(byte[] data) {
			Data = data;
		}
	}
}