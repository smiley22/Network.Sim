using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Network.Sim.Scenarios;

namespace Network.Sim {
	class Program {
        /// <summary>
        /// The program's entry point.
        /// </summary>
		static void Main() {
            var scenarios = GetInstances<Scenario>();
		    Scenario scenario = null;
		    while (scenario == null) {
                Print("Select a simulation scenario:", ConsoleColor.Yellow);
                Print(string.Empty);
                for (var i = 1; i <= scenarios.Count; i++)
                    Print(i + " - " + scenarios[i - 1].Name);
                Print(string.Empty);
		        var num = int.MaxValue;
		        while (num > scenarios.Count)
		            num = ReadInt();
		        var s = scenarios[num - 1];
		        Print(s.Description, ConsoleColor.White);
                Print(string.Empty);
                Print("Run this scenario? (Y/N)", ConsoleColor.Yellow);
		        if (char.ToLower(ReadChar('Y', 'y', 'N', 'n')) == 'y')
		            scenario = s;
                Console.Clear();		      
		    }
            scenario.Run();
		}

        /// <summary>
        /// Prints the specified string on standard out using the specified
        /// text color.
        /// </summary>
        /// <param name="s">The string to print on standard out.</param>
        /// <param name="color">The text color to use for printing.</param>
        static void Print(string s, ConsoleColor color = ConsoleColor.Gray) {
            var old = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(s);
            Console.ForegroundColor = old;
        }

        /// <summary>
        /// Deletes the last line of console input.
        /// </summary>
        static void ClearLastConsoleLine() {
            var currentLineCursor = Console.CursorTop - 1;
            Console.SetCursorPosition(0, Console.CursorTop);
            for (var i = 0; i < Console.WindowWidth; i++)
                Console.Write(" ");
            Console.SetCursorPosition(0, currentLineCursor);
        }

        /// <summary>
        /// Reads an integer from the console.
        /// </summary>
        /// <returns>The read integer.</returns>
	    static int ReadInt() {
	        while (true) {
	            var key = Console.ReadKey(true);
	            int num;
                if (int.TryParse(key.KeyChar.ToString(), out num))
                    return num;
	        }
	    }

        /// <summary>
        /// Reads a character from the console.
        /// </summary>
        /// <param name="subset">A subset of allowed characters.</param>
        /// <returns>The read character.</returns>
	    static char ReadChar(params char[] subset) {
	        while (true) {
	            var key = Console.ReadKey(true);
	            if (!subset.Any() || subset.Contains(key.KeyChar))
	                return key.KeyChar;
	        }
	    }

        /// <summary>
        /// Gets a list of instances of all types of the executing assembly that are derived from
        /// the specified base-type.
        /// </summary>
        /// <typeparam name="T">The base-type.</typeparam>
        /// <returns>A list of instances of all types derived from the specified base-type.</returns>
        static IList<T> GetInstances<T>() {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.BaseType == typeof(T) && t.GetConstructor(Type.EmptyTypes) != null
                    select (T)Activator.CreateInstance(t)).ToList();
        }
    }
}