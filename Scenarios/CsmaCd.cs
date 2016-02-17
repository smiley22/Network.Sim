using Network.Sim.Core;
using Network.Sim.Lan.Ethernet;
using Network.Sim.Link;
using Network.Sim.Network.Ip;

namespace Network.Sim.Scenarios {
	/// <summary>
	/// Demonstrates the CSMA/CD algorithm of IEEE 802.3.
	/// </summary>
	/// <remarks>
	/// Setting:
	///  2 stations attached to a 10Mbps link at a distance of 250 metres
	///  both start transmitting within a time difference of just 1000
	///  nanoseconds. Assuming a wave propagation speed of 0.66 times the
	///  speed of light, the carrier signal of the first transmission will
	///  not have reached the second station before it starts transmitting,
	///  resulting in a collision, which is then resolved with the CSMA/CD
	///  algorithm.
	/// </remarks>
	public static class CsmaCd {
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

			IpPacket dummyPacket1 = new IpPacket(new IpAddress("192.168.1.3"),
				new IpAddress("192.168.1.2"), IpProtocol.Tcp, new byte[] { 1, 2, 3, 4 });
			IpPacket dummyPacket2 = new IpPacket(new IpAddress("192.168.1.2"),
				new IpAddress("192.168.1.3"), IpProtocol.Tcp, new byte[] { 1, 2, 3, 4 });

			// Station H1 triggers a transmission at time t = 0ns.
			Simulation.Callback(0, () => {
				H1.Interfaces["eth0"].Output(new MacAddress("BB:BB:BB:BB:BB:BB"),
					dummyPacket1.Serialize());
			});
			// Station H2 triggers a transmissin at time t = 1000ns.
			Simulation.Callback(1000, () => {
				H2.Interfaces["eth0"].Output(new MacAddress("AA:AA:AA:AA:AA:AA"),
					dummyPacket2.Serialize());
			});

			Simulation.AddObject(H1.Hostname, H1);
			Simulation.AddObject(H2.Hostname, H2);
			Simulation.Start();
		}
	}
}
