using System;
using Network.Sim.Lan;
using Network.Sim.Link;
using Network.Sim.Network.Ip;

namespace Network.Sim.Core {
	/// <summary>
	/// Represents an abstract network interface object which acts
	/// as an interface between the network software stack and the
	/// NIC hardware.
	/// </summary>
	public class Interface {
		/// <summary>
		/// The NIC that is associated with the interface.
		/// </summary>
		public Nic Nic {
			get;
			private set;
		}

		/// <summary>
		/// The logical layer-3 IP address of the interface.
		/// </summary>
		public IpAddress IpAddress {
			get;
			private set;
		}

		/// <summary>
		/// The subnet mask of the interface.
		/// </summary>
		public IpAddress Netmask {
			get;
			private set;
		}

		/// <summary>
		/// The default gateway of the interface.
		/// </summary>
		public IpAddress Gateway {
			get;
			private set;
		}

		/// <summary>
		/// The name associated with the interface.
		/// </summary>
		public string Name {
			get;
			private set;
		}

		/// <summary>
		/// The full name including Host prefix. This is only used for printing
		/// to the simulation console.
		/// </summary>
		public string FullName {
			get {
				return Hostname + "::" + Name;
			}
		}

	    /// <summary>
	    /// The hostname set on the interface.
	    /// </summary>
	    public string Hostname {
	        get;
            set;
	    }

		/// <summary>
		/// The maximum transmission unit (MTU) of the data-link layer
		/// implementation of the NIC.
		/// </summary>
		public int MaximumTransmissionUnit {
			get {
				return Nic.MaximumTransmissionUnit;
			}
		}

		/// <summary>
		/// The event that is raised when the interface has received new data.
		/// </summary>
		public event EventHandler<DataReceivedEventArgs> DataReceivedEvent;

		/// <summary>
		/// The event that is raised when the output FIFO of the NIC has been
		/// fully consumed.
		/// </summary>
		public event EventHandler SendFifoEmptyEvent;

		/// <summary>
		/// Initializes a new instance of the Interface class using the specified
		/// parameters.
		/// </summary>
		/// <param name="nic">The network interface card (NIC) this interface
		/// represents.</param>
		/// <param name="name">A unique name (such as eth0) for identifing the
		/// interface.</param>
		/// <param name="ipAddress">The IPv4 address to associate with this
		/// interface.</param>
		/// <param name="netmask">The subnetmask to assign to this interface.</param>
		/// <param name="gateway">The IPv4 address of the default gateway to
		/// configure this interface with.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		public Interface(Nic nic, string name, IpAddress ipAddress, IpAddress netmask,
			IpAddress gateway = null) {
				Init(nic, name, ipAddress, netmask, gateway);
		}

        /// <summary>
        /// Initializes a new instance of the Interface class using the specified
        /// parameters.
        /// </summary>
        /// <param name="nic">The network interface card (NIC) this interface
        /// represents.</param>
        /// <param name="name">A unique name (such as eth0) for identifing the
        /// interface.</param>
        /// <param name="cidrIpAddress">The IPv4 address to associate with this
        /// interface in CIDR notation.</param>
        /// <param name="gateway">The IPv4 address of the default gateway to
        /// configure this interface with.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the arguments
        /// is null.</exception>
        public Interface(Nic nic, string name, string cidrIpAddress,
			string gateway = null) {
			var t = IpAddress.ParseCIDRNotation(cidrIpAddress);
			Init(nic, name, t.Item1, t.Item2, gateway != null ?
				new IpAddress(gateway) : null);
		}

        /// <summary>
        /// Hands down the specified data to the data-link layer implementation of the NIC.
        /// </summary>
		/// <param name="destination">The phyical destination address.</param>
		/// <param name="data">The data to send as the frame's payload.</param>
		/// <param name="type">The type of the data.</param>
		public void Output(MacAddress destination, byte[] data, EtherType type =
			EtherType.IPv4) {
				Nic.Output(destination, data, type);
		}

        /// <summary>
        /// Initializes the interface.
        /// </summary>
		/// <param name="nic">The network interface card (NIC) this interface
		/// represents.</param>
		/// <param name="name">A unique name (such as eth0) for identifing the
		/// interface.</param>
		/// <param name="ipAddress">The IPv4 address to associate with this
		/// interface.</param>
		/// <param name="netmask">The subnetmask to assign to this interface.</param>
		/// <param name="gateway">The IPv4 address of the default gateway to
		/// configure this interface with.</param>
		void Init(Nic nic, string name, IpAddress ipAddress, IpAddress netmask,
			IpAddress gateway) {
			nic.ThrowIfNull("nic");
			name.ThrowIfNull("name");
			ipAddress.ThrowIfNull("ipAddress");
			netmask.ThrowIfNull("netmask");
			Nic = nic;
			Name = name;
			IpAddress = ipAddress;
			Netmask = netmask;
			Gateway = gateway;
			Nic.Interrupt += OnInterrupt;
		}

        /// <summary>
        /// Called when the NIC causes an interrupt.
        /// </summary>
        /// <param name="sender">A reference to the object that caused the event.</param>
        /// <param name="e">The event arguments.</param>
		void OnInterrupt(object sender, EventArgs e) {
			// Figure out the reason for the interrupt.
			switch (Nic.InterruptReason) {
				case Interrupt.DataReceived:
					DataReceivedEvent.RaiseEvent(this, e as DataReceivedEventArgs);
					break;
				case Interrupt.SendFifoEmpty:
					SendFifoEmptyEvent.RaiseEvent(this, e);
					break;
			}
		}
	}
}