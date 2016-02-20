using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Network.Sim.Scenarios;

namespace Network.Sim {
	class Program {
		static void Main(string[] args) {
            Print("Select a simulation scenario:", ConsoleColor.Yellow);
            Print(string.Empty);
		    var scenarios = GetInstances<Scenario>();
            for(var i = 1; i <= scenarios.Count; i++)
                Print(i + " - " + scenarios[i - 1].Name);
            Print(string.Empty);
		    Console.ReadLine();
//	Arp.Run();
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

        private static IList<T> GetInstances<T>() {
            return (from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.BaseType == typeof(T) && t.GetConstructor(Type.EmptyTypes) != null
                    select (T)Activator.CreateInstance(t)).ToList();
        }

    }
}
