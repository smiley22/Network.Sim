
namespace Network.Sim.Core {
    /// <summary>
    /// The simulation event that is raised when a signal is sensed by an Ethernet transceiver.
    /// </summary>
	public class SignalSenseEvent : Event {
        /// <summary>
        /// The connector of the transceiver sensing the signal.
        /// </summary>
		public Connector Connector {
			get;
			private set;
		}

        /// <summary>
        /// Initializes a new instance of the SignalSenseEvent class.
        /// </summary>
	    /// <param name="timeout">The time at which the event expires, in
	    /// nanoseconds.</param>
        /// <param name="connector">The connector of the transceiver.</param>
        /// <param name="sender">The sender of the event.</param>
		public SignalSenseEvent(ulong timeout, Connector connector, object sender = null)
			: base(timeout, sender) {
				Connector = connector;
		}

        /// <summary>
        /// The method which executes when the event fires.
        /// </summary>
        public override void Run() {
			Connector.RaiseSignalSense(Sender);
		}
	}
}