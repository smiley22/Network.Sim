using System;
using Network.Sim.Core;
using Network.Sim.Lan.Ethernet;
using Network.Sim.Link;
using Network.Sim.Network.Ip;

namespace Network.Sim.Scenarios {
	/// <summary>
	/// Demonstrates the workings of the Address Resolution Protocol (ARP).
	/// </summary>
	/// <remarks>
	/// Setting:
	///  2 stations are attached to a 10Mbps (10BASE5) link. Station A 
	///  sends an IP packet to station B which results in a transmission
	///  of multiple ARP messages to resolve Station B's IP address to a
	///  Layer-2 MAC-48 address prior to the sending of the actual
	///  IP packet.
	/// </remarks>
	public static class Arp {
		/// <summary>
		/// Runs the ARP scenario.
		/// </summary>
		public static void Run() {
			Host H1 = new Host("H1"), H2 = new Host("H2");
			H1.RegisterInterface(new Interface(new Nic(
				new MacAddress("AA:AA:AA:AA:AA:AA")), "eth0", "192.168.1.2/24",
				"192.168.1.1"));
			H2.RegisterInterface(new Interface(new Nic(
				new MacAddress("BB:BB:BB:BB:BB:BB")), "eth0", "192.168.1.3/24",
				"192.168.1.1"));
			// Attach both stations to a "thick" Ethernet 10BASE5 cable.
			new C10Base5(250)
				.Pierce(0, H1.Interfaces["eth0"].Nic.Connector)
				.Pierce(250, H2.Interfaces["eth0"].Nic.Connector);
			// Station H1 triggers a transmission at time t = 0ns.
			Simulation.Callback(0, () => {
				H1.Output("eth0", new IpAddress("192.168.1.3"),
					new byte[] { 1, 2, 3, 4 });
			});

			Simulation.AddObject(H1.Hostname, H1);
			Simulation.AddObject(H2.Hostname, H2);
			Simulation.Start();
		}
	}
}
