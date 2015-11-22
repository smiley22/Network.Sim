using System;
using System.Collections.Generic;
using ConsoleApplication36.Miscellaneous;
using System.Collections.ObjectModel;

namespace ConsoleApplication36.Core {
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
		static BlockingPriorityQueue<Event> events =
			new BlockingPriorityQueue<Event>();

		/// <summary>
		/// The console-command interpreter of the simulation.
		/// </summary>
		static Interpreter interpreter = new Interpreter();

		/// <summary>
		/// Adds the specified event to the simulation.
		/// </summary>
		/// <param name="ev">The event instance to add to the simulation.</param>
		public static void AddEvent(Event ev) {
			WriteLine(OutputLevel.Simulation, "Adding \"" + ev +
				"\" with expiry time = " + ev.Time + " ns.");

			events.Enqueue(ev);
		}

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
			return events.Remove(ev => handler.Invoke(ev));
		}

		/// <summary>
		/// Advances the simulation to the specified moment in time.
		/// </summary>
		/// <param name="time">The moment in time to advance the simulation
		/// to, in nanoseconds.</param>
		public static void RunTo(ulong time) {
			while (Time < time) {
				Event peek = events.Peek();
				if (peek == null || peek.Time > time) {
					Time = time;
				} else {
					Event ev = events.Dequeue();
					Time = ev.Time;
					ev.Run();
				}
			}
		}

		public static void Start() {
			while (true) {
				if (interpreter.ReadCommand())
					continue;
				Event ev = events.Dequeue();
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

		public static OutputLevel OutputLevel {
			get;
			set;
		}

		public static void WriteLine(OutputLevel level, string s, ConsoleColor c = ConsoleColor.DarkRed) {
			Write(level, s + Environment.NewLine, c);
		}

		public static void Write(OutputLevel level, string s, ConsoleColor c) {
			if (OutputLevel > 0 && !OutputLevel.HasFlag(level))
				return;
			ConsoleColor color = Console.ForegroundColor;
			Console.ForegroundColor = c;
			Console.Write(Time.ToString("D8") + " ns| " + s);
			Console.ForegroundColor = color;
		}

		public static IReadOnlyDictionary<string, object> Objects {
			get {
				return new ReadOnlyDictionary<string, object>(objects);
			}
		}
		static IDictionary<string, object> objects =
			new Dictionary<string, object>();
		public static void AddObject(string name, object o) {
			objects[name] = o;
		}
	}
}
