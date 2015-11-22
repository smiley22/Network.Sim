using System;

namespace ConsoleApplication36.Core {
	/// <summary>
	/// The abstract base class from which all simulation events must
	/// derive.
	/// </summary>
	public abstract class Event : IComparable<Event> {
		/// <summary>
		/// The absolute time at which the event fires, in nanoseconds.
		/// </summary>
		public ulong Time {
			get;
			private set;
		}

		public object Sender {
			get;
			private set;
		}

		/// <summary>
		/// The method which executes when the event fires.
		/// </summary>
		public abstract void Run();

		/// <summary>
		/// Initializes a new instance of the Event class using the specified
		/// timeout value.
		/// </summary>
		/// <param name="timeout">The time at which the event expires, in
		/// nanoseconds.</param>
		public Event(ulong timeout, object sender = null) {
			Time = Simulation.Time + timeout;
			Sender = sender;
		}

		/// <summary>
		/// Compares this instance to a specified event and returns an indication
		/// of their relative timeout values.
		/// </summary>
		/// <param name="other">The Event instance to compare this instance with.</param>
		/// <returns>A signed number indicating the relative timeout values of this
		/// instance and other.</returns>
		public int CompareTo(Event other) {
			if (other == null)
				return 1;
			return Time.CompareTo(other.Time);

		}
	}
}
