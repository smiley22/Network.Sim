using System;
using Network.Sim.Core;
using Network.Sim.Link;

namespace Network.Sim.Lan {
	/// <summary>
	/// Represents the abstract base class from which all Network Card Interface
	/// (NIC) implementations (Ethernet, TokenRing, etc.) must derive.
	/// </summary>
	public abstract class Nic {
		/// <summary>
		/// The Layer-2 physical address of the NIC which is stored
		/// inside ROM.
		/// </summary>
		public MacAddress MacAddress {
			get;
			private set;
		}

		/// <summary>
		/// The maximum transmission unit (MTU) of the data-link layer
		/// implementation of the NIC.
		/// </summary>
		public abstract int MaximumTransmissionUnit {
			get;
		}

		/// <summary>
		/// The NIC's connector outlet.
		/// </summary>
		public Connector Connector {
			get;
			private set;
		}

		/// <summary>
		/// The event that is raised when the NIC causes an interrupt.
		/// </summary>
		public abstract event EventHandler Interrupt;

		/// <summary>
		/// The status register which contains the reason for the interrupt caused
		/// by the NIC.
		/// </summary>
		public Interrupt InterruptReason {
			get;
			protected set;
		}

		/// <summary>
		/// Constructs a new data-link frame for the specified destination,
		/// containing the specified data as payload.
		/// </summary>
		/// <param name="destination">The phyical destination address.</param>
		/// <param name="data">The data to send as the frame's payload.</param>
		/// <param name="type">The type of the data.</param>
		/// <exception cref="ArgumentNullException">Thrown if the destination
		/// or the data parameter is null.</exception>
		public abstract void Output(MacAddress destination, byte[] data,
			EtherType type);

		/// <summary>
		/// Initializes a new instance of the Nic class using the specified
		/// MAC address.
		/// </summary>
		/// <param name="address">A MAC-48 address to assign to the NIC. If this
		/// is null, the NIC is assigned a random MAC address.</param>
		public Nic(MacAddress address = null) {
			if (address != null)
				MacAddress = address;
			else
				MacAddress = new MacAddress();
			Connector = new Connector();
		}
	}
}
