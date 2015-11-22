
namespace ConsoleApplication36.Core {
	public class SignalSenseEvent : Event {
		public Connector Connector {
			get;
			private set;
		}

		public SignalSenseEvent(ulong timeout, Connector connector, object sender = null)
			: base(timeout, sender) {
				Connector = connector;
		}

		public override void Run() {
			Connector.RaiseSignalSense(Sender);
		}
	}
}
