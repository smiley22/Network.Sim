using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Network.Sim.Miscellaneous;
using System.Linq;
using Network.Sim.Lan.Ethernet;
using Network.Sim.Network.Ip.Arp;
using Network.Sim.Network.Ip.Routing;

namespace Network.Sim.Core {
	/// <summary>
	/// Implements the console interpreter of the simulation.
	/// </summary>
	public class Interpreter {
		/// <summary>
		/// The dictionary of commands supported by the interpreter.
		/// </summary>
		static Dictionary<string, Action<string[]>> commands =
			new Dictionary<string, Action<string[]>>(
				StringComparer.InvariantCultureIgnoreCase) {
					{ "Show", Show },
					{ "RunTo", RunTo },
					{ "RunFor", RunFor },
					{ "Output", Output },
					{ "Help", Help },
					{ "?", Help }
			};

		/// <summary>
		/// Reads a new line from the console and interprets it.
		/// </summary>
		/// <returns>true if a command was entered; Otherwise false.</returns>
		public bool ReadCommand() {
			Console.Write("$ ");
			string[] token = Console.ReadLine().Split(' ');
			string predicate = token[0];
			if (predicate == String.Empty) {
				ClearLastConsoleLine();
				return false;
			}
			if (!commands.ContainsKey(predicate)) {
				Print("Unknown command '" + predicate + "'", ConsoleColor.Yellow);
				return true;
			}
			try {
				commands[predicate].Invoke(token.Slice(1));
			} catch (ArgumentException e) {
				Print(e.Message, ConsoleColor.Yellow);
			} catch (FormatException e) {
				Print(e.Message, ConsoleColor.Yellow);
			}
			return true;
		}

		/// <summary>
		/// Implements the 'Show' command.
		/// </summary>
		/// <param name="args">An array of arguments passed to the command.</param>
		/// <exception cref="ArgumentException">Thrown if an argument is missing or
		/// is invalid.</exception>
		static void Show(string[] args) {
			var dict = new Dictionary<string, Action<string[]>>(
				StringComparer.InvariantCultureIgnoreCase) {
					{ "ArpTable", ShowArpTable },
					{ "Ifs", ShowInterfaces },
					{ "OutputQueue", ShowOutputQueue },
					{ "RoutingTable", ShowRoutingTable },
					{ "Hosts", ShowHosts },
					{ "Objects", ShowObjects },
					{ "ForwardTable", ShowForwardTable }
			};
			try {
				dict[args[0]].Invoke(args.Slice(1));
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing object (syntax: 'Show Object [params]').");
			} catch (KeyNotFoundException) {
				throw new ArgumentException("Unknown object '" + args[0] +
					"' (try: " + String.Join(", ", dict.Keys) + ").");
			}
		}

		/// <summary>
		/// Implements the 'RunTo' command.
		/// </summary>
		/// <param name="args">an array of arguments passed to the command.</param>
		static void RunTo(string[] args) {
			try {
				Match m = Regex.Match(args[0], @"^(\d+)(ns|µs|ms|s)$");
				if(!m.Success)
					throw new ArgumentException("Invalid time value " +
						"(ex. 'RunTo 2000µs').");
				var multipliers = new Dictionary<string, ulong>() {
					{ "ns", 1 }, { "µs", 1000 }, { "ms", 1000000 },
					{ "s", 1000000000 }
				};
				ulong timestamp = UInt64.Parse(m.Groups[1].ToString()) *
					multipliers[m.Groups[2].ToString()];
				if (timestamp <= Simulation.Time)
					throw new ArgumentException("Timestamp must be greater than current " +
						"simulation time (" + Simulation.Time + "ns).");
				Print("Fast-Forwarding simulation to time = " + timestamp.ToString("D8") + " ns.",
					ConsoleColor.Green);
				Simulation.RunTo(timestamp);
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing value (ex. 'RunTo 10000ms').");
			}
		}

		/// <summary>
		/// Implements the 'RunFor' command.
		/// </summary>
		/// <param name="args">an array of arguments passed to the command.</param>
		static void RunFor(string[] args) {
			try {
				Match m = Regex.Match(args[0], @"^(\d+)(ns|µs|ms|s)$");
				if (!m.Success)
					throw new ArgumentException("Invalid time value " +
						"(ex. 'RunFor 2000µs').");
				var multipliers = new Dictionary<string, ulong>() {
					{ "ns", 1 }, { "µs", 1000 }, { "ms", 1000000 },
					{ "s", 1000000000 }
				};
				ulong timestamp = UInt64.Parse(m.Groups[1].ToString()) *
					multipliers[m.Groups[2].ToString()];
				Print("Running simulation for " + timestamp.ToString("D8") + "ns.",
					ConsoleColor.Green);
				Simulation.RunTo(Simulation.Time + timestamp);
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing value (ex. 'RunFor 10000ms').");
			}
		}

		/// <summary>
		/// Implements the 'Output' command.
		/// </summary>
		/// <param name="args">an arry of arguments passed to the command.</param>
		static void Output(string[] args) {
			Dictionary<string, OutputLevel> opts =
				new Dictionary<string, OutputLevel>(StringComparer.InvariantCultureIgnoreCase) {
					{ "Simulation", OutputLevel.Simulation },
					{ "Physical", OutputLevel.Physical },
					{ "datalink", OutputLevel.Datalink },
					{ "Arp", OutputLevel.Arp },
					{ "Network", OutputLevel.Network },
					{ "Icmp", OutputLevel.Icmp }
			};
			try {
				string[] parts = args[0].Split('|');
				OutputLevel level = 0;
				foreach (string s in parts) {
					if (opts.ContainsKey(s))
						level |= opts[s];
					else
						throw new ArgumentException("Invalid value '" + s + "' " +
							"(ex. Output Physical|datalink|Network'). Possible values: " +
							"Simulation, Physical, datalink, Arp, Network, Icmp.");
				}
				Print("Setting OutputLevel to [" + level + "]", ConsoleColor.Green);
				Simulation.OutputLevel = level;
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing value " +
					"(ex. 'Output Physical|datalink'). Possible values: " +
					"Simulation, Physical, datalink, Arp, Network, Icmp.");
			}
		}

		/// <summary>
		/// Implements the 'Help' command.
		/// </summary>
		/// <param name="args">an array of arguments passed to the command.</param>
		static void Help(string[] args) {
			Print("Press [Enter] to run to next simulation event or enter any " +
				"of the following commands: ", ConsoleColor.Cyan);
			foreach (string command in commands.Keys)
				Print(" - " + command, ConsoleColor.Yellow);
		}

		/// <summary>
		/// Implements the 'ArpTable' method of the 'Show' command.
		/// </summary>
		/// <param name="args">an array of arguments passed to the command.</param>
		/// <exception cref="ArgumentException">Thrown if an argument is missing or
		/// is invalid.</exception>
		static void ShowArpTable(string[] args) {
			try {
				var tuple = ParseHostInterface(args[0]);
				Host host = GetHostByName(tuple.Item1);
				Interface ifc = GetInterfaceByName(tuple.Item1, tuple.Item2);
				Print("Showing ARP table of interface '" + tuple.Item2 + "' of " +
					"host '" + tuple.Item1 + "':", ConsoleColor.Green);
				Print(String.Format("{0,15} | {1,20} | {2,15} |",
					"IP Address", "MAC Address", "Expiry Time"));
				Print("");
				foreach (ArpEntry e in host.Network.ArpTableOf(ifc)) {
					string s = String.Format("{0,15} | {1,20} | {2,15} |",
						e.IpAddress, e.MacAddress, e.ExpiryTime);
					Print(s, ConsoleColor.Red);			
				}
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing noun (ex. 'Show ArpTable [Router::eth0]')");
			}
		}

		/// <summary>
		/// Implements the 'Ifs' method of the 'Show' command.
		/// </summary>
		/// <param name="args">an array of arguments passed to the command.</param>
		/// <exception cref="ArgumentException">Thrown if an argument is missing or
		/// is invalid.</exception>
		static void ShowInterfaces(string[] args) {
			try {
				string hostName = ParseHost(args[0]);
				Host host = GetHostByName(hostName);
				Print("Showing Interfaces of host '" + hostName + "':", ConsoleColor.Green);
				Print(String.Format("{0,5} | {1,16} | {2,15} | {3,15} | {4,20} |  ",
					"Name", "IP Address", "Subnetmask", "Gateway", "MAC Address"));
				Print("");
				foreach (Interface ifc in host.Interfaces.Values) {
					string s = String.Format("{0,5} | {1,16} | {2,15} | {3,15} | {4,20} |",
						ifc.Name, ifc.IpAddress, ifc.Netmask, ifc.Gateway,
						ifc.Nic.MacAddress);
					Print(s, ConsoleColor.Cyan);
				}
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing noun (ex. 'Show Ifs [Router]')");
			}
		}

		/// <summary>
		/// Implements the 'OutputQueue' method of the 'Show' command.
		/// </summary>
		/// <param name="args">an array of arguments passed to the command.</param>
		/// <exception cref="ArgumentException">Thrown if an argument is missing or
		/// is invalid.</exception>
		static void ShowOutputQueue(string[] args) {
			try {
				var tuple = ParseHostInterface(args[0]);
				Host host = GetHostByName(tuple.Item1);
				Interface ifc = GetInterfaceByName(tuple.Item1, tuple.Item2);
				Print("Showing output queue of interface '" + tuple.Item2 + "' of " +
					"host '" + tuple.Item1 + "':", ConsoleColor.Green);
				var queue = host.Network.OutputQueueOf(ifc);
				int percent = (int) ((queue.Count / (double) queue.MaxCapacity) * 100);
				Print(queue.Count + " IP packets enqueued.", ConsoleColor.Yellow);
				StringBuilder b = new StringBuilder();
				percent = 73;
				int numSpaces = 40;
				int numBars = (int)((numSpaces / 100.0) * percent);
				b.Append("[");
				for (int i = 0; i < numSpaces; i++)
					b.Append(i < numBars ? "|" : " ");
				b.Append("] " + (100 - percent) + "% queue capacity left.");
				ConsoleColor color = percent < 30 ? ConsoleColor.Green :
					(percent < 60 ? ConsoleColor.Yellow : ConsoleColor.Red);
				Print(b.ToString(), color);
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing noun (ex. 'Show OutputQueue [Router::eth0]')");
			}
		}

		/// <summary>
		/// Implements the 'RoutingTable' method of the 'Show' command.
		/// </summary>
		/// <param name="args">an array of arguments passed to the command.</param>
		/// <exception cref="ArgumentException">Thrown if an argument is missing or
		/// is invalid.</exception>
		static void ShowRoutingTable(string[] args) {
			try {
				string hostName = ParseHost(args[0]);
				Host host = GetHostByName(hostName);
				Print("Showing routing table of host '" + hostName + "':", ConsoleColor.Green);
				Print(String.Format("{0,15} | {1,15} | {2,15} | {3,15} | {4,7} | ",
					"Destination", "Subnetmask", "Gateway", "Interface", "Metric"));
				Print("");
				foreach (Route route in host.Routes) {
					string s = String.Format("{0,15} | {1,15} | {2,15} | {3,15} | {4,7} | ",
					route.Destination, route.Netmask, route.Gateway,
					route.Interface.IpAddress, route.Metric);
					Print(s, ConsoleColor.Cyan);
				}
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing noun (ex. 'Show RoutingTable [Router]')");
			}
		}

		/// <summary>
		/// Implements the 'Hosts' method of the 'Show' command.
		/// </summary>
		/// <param name="args"></param>
		static void ShowHosts(string[] args) {
			Print("Listing all Hosts:", ConsoleColor.Green);
			Print(String.Format("{0,25} | {1,25} | {2,25} | ",
				"Hostname", "No. of Interfaces", "Nodal Processing Delay"));
			Print("");
			foreach (var pair in Simulation.Objects) {
				Host host = pair.Value as Host;
				if (host == null)
					continue;
				string s = String.Format("{0,25} | {1,25} | {2,25} | ",
				host.Hostname, host.Interfaces.Count, host.NodalProcessingDelay + "ns");
				Print(s, ConsoleColor.Cyan);
			}
		}

		/// <summary>
		/// Implements the 'Objects' method of the 'Show' command.
		/// </summary>
		/// <param name="args"></param>
		static void ShowObjects(string[] args) {
			Print("Listing all objects registered with the simulation:",
				ConsoleColor.Green);
			Print(String.Format("{0,25} | {1,25} | ",
				"Name", "Type"));
			Print("");
			foreach (var pair in Simulation.Objects) {
				string s = String.Format("{0,25} | {1,25} | ", pair.Key,
					pair.Value.GetType().Name);
				Print(s, ConsoleColor.Cyan);
			}
		}

		/// <summary>
		/// Implements the 'ForwardTable' method of the 'Show' command.
		/// </summary>
		/// <param name="args"></param>
		static void ShowForwardTable(string[] args) {
			try {
				string name = ParseHost(args[0]);
				if (!Simulation.Objects.ContainsKey(name))
					throw new ArgumentException("An object with the name of '" + name +
						"' could not be found.");
				Bridge bridge = Simulation.Objects[name] as Bridge;
				if (bridge == null)
					throw new ArgumentException("The object with the name of '" + name +
						"' is not of type Bridge.");
				Print("Showing forwarding table of bridge '" + name + "':",
					ConsoleColor.Green);
				Print(String.Format("{0,25} | {1,25} | ", "MAC Address", "Port"));
				Print("");
				foreach (var pair in bridge.ForwardTable) {
					int index = 0;
					for (int i = 0; i < bridge.Ports.Count; i++) {
						if (bridge.Ports[i] == pair.Value) {
							index = i;
							break;
						}
					}
					string s = String.Format("{0,25} | {1,25} | ",
						pair.Key, index);
					Print(s, ConsoleColor.Cyan);
				}
			} catch (IndexOutOfRangeException) {
				throw new ArgumentException("Missing noun (ex. 'Show ForwardTable " +
					"[Bridge]')");
			}
		}

		/// <summary>
		/// Parses host and interface names from a token in the form of
		/// [Router::eth0].
		/// </summary>
		/// <param name="token">The token to parse.</param>
		/// <returns>A tuple containing the parsed hostname as well as the
		/// interface name.</returns>
		/// <exception cref="FormatException">Thrown if the specified token
		/// is not in the expected format.</exception>
		static Tuple<string, string> ParseHostInterface(string token) {
			Match m = Regex.Match(token, @"^\[(.+)::(.+)\]$");
			if (!m.Success)
				throw new FormatException("Invalid host and/or interface " +
					"(ex. '[Router::eth0]').");
			return new Tuple<string, string>(m.Groups[1].ToString(),
				m.Groups[2].ToString());
		}

		/// <summary>
		/// Parses a hostname from a token in the form of [Router].
		/// </summary>
		/// <param name="token">The token to parse.</param>
		/// <returns>The parsed hostname.</returns>
		/// <exception cref="FormatException">Thrown if the specified token
		/// is not in the expected format.</exception>
		static string ParseHost(string token) {
			Match m = Regex.Match(token, @"^\[(.+)\]$");
			if(!m.Success)
				throw new FormatException("Invalid host format " +
					"(ex. '[Router]').");
			return m.Groups[1].ToString();
		}

		/// <summary>
		/// Returns the host with the specified hostname.
		/// </summary>
		/// <param name="hostName">The name of the host to return.</param>
		/// <returns>The instance of the Host class with the specified
		/// host name.</returns>
		/// <exception cref="ArgumentException">Thrown if no host with a
		/// matching hostname could be found.</exception>
		static Host GetHostByName(string hostName) {
			if(!Simulation.Objects.ContainsKey(hostName))
				throw new ArgumentException("A host with the specified hostname of " +
					"'" + hostName + "' could not be found.");
			Host host = Simulation.Objects[hostName] as Host;
			if(host == null)
				throw new ArgumentException("The object with the name of " +
					"'" + hostName + "' is not of type Host.");
			return host;
		}

		/// <summary>
		/// Returns the interface with the specified name of the host
		/// with the specified host name.
		/// </summary>
		/// <param name="hostName">The name of the host whose interface
		/// to return.</param>
		/// <param name="ifName">The name of the interface to
		/// return.</param>
		/// <returns>The instance of the Interface class with the specified
		/// name.</returns>
		/// <exception cref="ArgumentException">Thrown if no host with a
		/// matching hostname could be found or none of the hosts interfaces
		/// match the specified interface name.</exception>
		static Interface GetInterfaceByName(string hostName, string ifName) {
			Host host = GetHostByName(hostName);
			foreach (string name in host.Interfaces.Keys) {
				if (name == ifName)
					return host.Interfaces[name];
			}
			throw new ArgumentException("None of the host's interfaces matched the " +
				"interface name '" + ifName + "'.");
		}

		/// <summary>
		/// Prints the specified string on standard out using the specified
		/// text color.
		/// </summary>
		/// <param name="s">The string to print on standard out.</param>
		/// <param name="color">The text color to use for printing.</param>
		static void Print(string s, ConsoleColor color = ConsoleColor.Gray) {
			ConsoleColor old = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(s);
			Console.ForegroundColor = old;
		}

		/// <summary>
		/// Deletes the last line of console input.
		/// </summary>
		static void ClearLastConsoleLine() {
			int currentLineCursor = Console.CursorTop - 1;
			Console.SetCursorPosition(0, Console.CursorTop);
			for (int i = 0; i < Console.WindowWidth; i++)
				Console.Write(" ");
			Console.SetCursorPosition(0, currentLineCursor);
		}
	}
}