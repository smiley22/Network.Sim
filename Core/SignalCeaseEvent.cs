
namespace Network.Sim.Core {
	public class SignalCeaseEvent : Event {
		Connector connector;
		byte[] data;

		public SignalCeaseEvent(ulong timeout, Connector connector, byte[] data, object sender = null)
			: base(timeout, sender) {
				this.connector = connector;
				this.data = data;
		}

		public override void Run() {
			connector.RaiseSignalCease(Sender, data);
		}
	}
}
