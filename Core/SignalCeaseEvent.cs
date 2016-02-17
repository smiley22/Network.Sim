
namespace Network.Sim.Core {
    /// <summary>
    /// The simulation event that is raised when a signal is no longer sensed by an Ethernet transceiver.
    /// </summary>
    public class SignalCeaseEvent : Event {
        /// <summary>
        /// The connector of the transceiver sensing the signal.
        /// </summary>
        readonly Connector connector;
        /// <summary>
        /// The transmitted data.
        /// </summary>
	    readonly byte[] data;

        /// <summary>
        /// Initializes a new instance of the SignalSenseEvent class.
        /// </summary>
        /// <param name="timeout">The time at which the event expires, in
        /// nanoseconds.</param>
        /// <param name="connector">The connector of the transceiver.</param>
        /// <param name="data">The data that was transmitted.</param>
        /// <param name="sender">The sender of the event.</param>
        public SignalCeaseEvent(ulong timeout, Connector connector, byte[] data, object sender = null)
			: base(timeout, sender) {
				this.connector = connector;
				this.data = data;
		}

        /// <summary>
        /// The method which executes when the event fires.
        /// </summary>
        public override void Run() {
			connector.RaiseSignalCease(Sender, data);
		}
	}
}