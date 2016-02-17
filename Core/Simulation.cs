using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Network.Sim.Miscellaneous;

namespace Network.Sim.Core {
	/// <summary>
	/// Represents the simulation.
	/// </summary>
	public static class Simulation {
		/// <summary>
		/// The current time of the simulation, in nanoseconds.
		/// </summary>
		public static ulong Time {
			get;
			private set;
		}

		/// <summary>
		/// A queue of simulation events, ordered by their respective timeout
		/// values.
		/// </summary>
		static readonly BlockingPriorityQueue<Event> events =
			new BlockingPriorityQueue<Event>();

		/// <summary>
		/// Adds the specified event to the simulation.
		/// </summary>
		/// <param name="ev">The event instance to add to the simulation.</param>
		public static void AddEvent(Event ev) {
			WriteLine(OutputLevel.Simulation, "Adding \"" + ev +
				"\" with expiry time = " + ev.Time + " ns.");
			events.Enqueue(ev);
		}

        /// <summary>
        /// Schedules the specified callback to run after the specified number of nanoseconds,
        /// in simulation time.
        /// </summary>
        /// <param name="timeout">The number of nanoseconds, in simulation timer after which to
        /// run the callback method.</param>
        /// <param name="callback">The callback method to run.</param>
		public static void Callback(ulong timeout, Action callback) {
			AddEvent(new CallbackEvent(timeout, callback));
		}

		/// <summary>
		/// A delegate used for defining the conditions of the elements to remove
		/// from the simulation using the RemoveEvents method.
		/// </summary>
		/// <param name="ev">The event to examine.</param>
		/// <returns>true to remove the event from the simulation; otherwise
		/// false.</returns>
		public delegate bool RemoveEventsHandler(Event ev);

		/// <summary>
		/// Removes the events that match the conditions of the specified delegate.
		/// </summary>
		/// <param name="handler">The delegate that defines the conditions of the
		/// events to remove.</param>
		/// <returns>The number of events removed from the simulation.</returns>
		public static int RemoveEvents(RemoveEventsHandler handler) {
			return events.Remove(handler.Invoke);
		}

		/// <summary>
		/// Advances the simulation to the specified moment in time.
		/// </summary>
		/// <param name="time">The moment in time to advance the simulation
		/// to, in nanoseconds.</param>
		public static void RunTo(ulong time) {
			while (Time < time) {
				var peek = events.Peek();
				if (peek == null || peek.Time > time) {
					Time = time;
				} else {
					var ev = events.Dequeue();
					Time = ev.Time;
					ev.Run();
				}
			}
		}

        /// <summary>
        /// Starts and runs the command-line interpreter.
        /// </summary>
		public static void Start() {
			while (true) {
				if (Interpreter.ReadCommand())
					continue;
				var ev = events.Dequeue();
				// Advance simulation time.
				Time = ev.Time;
				WriteLine(OutputLevel.Simulation, "Executing \"" + ev + "\"");
				// Execute event.
				ev.Run();
			}
		}

		static Simulation() {
			OutputLevel = OutputLevel.Physical | OutputLevel.Datalink | OutputLevel.Network;
			OutputLevel = 0;
		}

        /// <summary>
        /// Determines the granularity of the output.
        /// </summary>
		public static OutputLevel OutputLevel {
			get;
			set;
		}

        /// <summary>
        /// Writes the specified string followed by a newline to the output using the specified
        /// output-level and optionally the specified color.
        /// </summary>
        /// <param name="level">The output-level to output the string with.</param>
        /// <param name="s">The string to output to the console.</param>
        /// <param name="c">The color to use.</param>
		public static void WriteLine(OutputLevel level, string s, ConsoleColor c = ConsoleColor.DarkRed) {
			Write(level, s + Environment.NewLine, c);
		}

        /// <summary>
        /// Writes the specified string to the output using the specified output-level and optionally
        /// the specified color.
        /// </summary>
        /// <param name="level">The output-level to output the string with.</param>
        /// <param name="s">The string to output to the console.</param>
        /// <param name="c">The color to use.</param>
		public static void Write(OutputLevel level, string s, ConsoleColor c) {
			if (OutputLevel > 0 && !OutputLevel.HasFlag(level))
				return;
			var color = Console.ForegroundColor;
			Console.ForegroundColor = c;
			Console.Write(Time.ToString("D8") + " ns| " + s);
			Console.ForegroundColor = color;
		}

		public static IReadOnlyDictionary<string, object> Objects {
			get {
				return new ReadOnlyDictionary<string, object>(objects);
			}
		}
		static readonly IDictionary<string, object> objects =
			new Dictionary<string, object>();
		public static void AddObject(string name, object o) {
			objects[name] = o;
		}
	}
}
