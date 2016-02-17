using System;

namespace Network.Sim.Core {
	/// <summary>
	/// An event for scheduling the execution of arbitrary callback methods.
	/// </summary>
	public class CallbackEvent : Event {
		/// <summary>
		/// The callback method.
		/// </summary>
		public Action Callback {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the CallbackEvent class using the
		/// specified values.
		/// </summary>
		/// <param name="timeout">The time at which the event expires, in
		/// nanoseconds.</param>
		/// <param name="callback">The callback method to execute when the
		/// event fires.</param>
		public CallbackEvent(ulong timeout, Action callback)
			: base(timeout) {
			Callback = callback;
		}

		/// <summary>
		/// The method which executes when the event fires.
		/// </summary>
		public override void Run() {
			Callback();
		}

		/// <summary>
		/// Returns a textual description of this instance.
		/// </summary>
		/// <returns>A textual description of this instance.</returns>
		public override string ToString() {
			return "Callback Event";
		}
	}
}
