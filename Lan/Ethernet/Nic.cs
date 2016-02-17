using System;
using Network.Sim.Core;
using Network.Sim.Link;
using Network.Sim.Miscellaneous;

namespace Network.Sim.Lan.Ethernet {
	/// <summary>
	/// Represents an Ethernet Network Interface Card (NIC) which implements
	/// OSI Layer 1 and 2.
	/// </summary>
	public class Nic : Lan.Nic {
		/// <summary>
		/// The random number generator needed for CSMA/CD.
		/// </summary>
		static readonly Random random = new Random();

		/// <summary>
		/// Initializes a new instance of the Nic class using the specified
		/// MAC address.
		/// </summary>
		/// <param name="address">A MAC-48 address to assign to the NIC. If this
		/// is null, the NIC is assigned a random MAC address.</param>
		public Nic(MacAddress address = null)
			: base(address) {
			Connector.SignalSense += OnSignalSense;
			Connector.SignalCease += OnSignalCease;
		}

		/// <summary>
		/// Writes the specified string value to the simulation output stream.
		/// </summary>
		/// <param name="s">The value to write.</param>
		void WriteMac(string s) {
			Simulation.WriteLine(OutputLevel.Datalink, MacAddress +
				": " + s, ConsoleColor.Gray);
		}

		/// <summary>
		/// Writes the specified string value to the simulation output stream.
		/// </summary>
		/// <param name="s">The value to write.</param>
		void WritePhy(string s) {
			Simulation.WriteLine(OutputLevel.Physical, MacAddress +
				": " + s, ConsoleColor.DarkGray);
		}

		#region MAC (Media Access Control Implementation)
		/// <summary>
		/// The event that is raised when the NIC causes an interrupt.
		/// </summary>
		public override event EventHandler Interrupt;

		/// <summary>
		/// Returns the maximum transmission unit (MTU) of the NIC.
		/// </summary>
		public override int MaximumTransmissionUnit {
			get {
				return Frame.MaximumPayloadSize;
			}
		}

		/// <summary>
		/// The MAC address used for broadcasts in Ethernet networks.
		/// </summary>
		static readonly MacAddress broadcastAddress =
					new MacAddress("FF:FF:FF:FF:FF:FF");

		/// <summary>
		/// The NIC's output FIFO buffer.
		/// </summary>
		CappedQueue<Frame> sendFifo = new CappedQueue<Frame>();

		/// <summary>
		/// Determines whether the output FIFO is currently being emptied.
		/// </summary>
		bool emptyingFifo;

		/// <summary>
		/// Wraps the specified data into an Ethernet frame and queues it
		/// for transmission.
		/// </summary>
		/// <param name="destination">The MAC address of the destination
		/// host.</param>
		/// <param name="data">The data to transmit as part of the frame's
		/// payload.</param>
		/// <param name="type">The type of the data.</param>
		public override void Output(MacAddress destination, byte[] data,
			EtherType type = EtherType.IPv4) {
			destination.ThrowIfNull("destination");
			data.ThrowIfNull("data");
			// Start emptying the FIFO, if we're not already doing it.
			if (emptyingFifo == false)
				Simulation.Callback(0, EmptySendFifo);
			// Enqueue the data.
			sendFifo.Enqueue(new Frame(destination, MacAddress, data, type));
		}

		/// <summary>
		/// Removes a queued frame from the output FIFO and transmits it.
		/// </summary>
		void EmptySendFifo() {
			Frame frame = sendFifo.Dequeue();
			WriteMac("Outputting an Ethernet frame for " + frame.Destination + ".");
			emptyingFifo = true;
			// Serialize the Ethernet frame object into a sequence of bytes.
			byte[] frameBytes = frame.Serialize();
			// Call into the physical layer to put the bytes "on the wire".
			Transmit(frameBytes);
		}

		/// <summary>
		/// Invoked on behalf of PHY whenever new data has been received.
		/// </summary>
		/// <param name="data">The data that was received.</param>
		void OnDataReceived(byte[] data) {
			data.ThrowIfNull("data");
			Frame frame = Frame.Deserialize(data);
			WriteMac("Received an Ethernet frame.");
			// Compute checksum and compare to the one contained in the frame.
			uint fcs = Frame.ComputeCheckSequence(frame);
			if (fcs != frame.CheckSequence) {
				WriteMac("Detected a bad frame check sequence, discarding.");
				return;
			}
			// Drop our own frames.
			if (frame.Source == MacAddress)
				return;
			// Examine the frame and see if it's for us; If not, discard it.
			if (frame.Destination != MacAddress &&
				frame.Destination != broadcastAddress) {
				WriteMac("Recipient mismatch, discarding.");
				return;
			}
			//Extract the payload and hand it up to the Network layer.
			InterruptReason = Lan.Interrupt.DataReceived;
			Interrupt.RaiseEvent(this,
				new DataReceivedEventArgs(frame.Payload, frame.Type));
		}

		/// <summary>
		/// Invoked om behalf of PHY whenever a frame has been transmitted.
		/// </summary>
		void OnDataTransmitted() {
			WriteMac("Finished transmitting Ethernet frame.");
			// The idle period between two consecutive frames must at least be 96
			// bittimes long as per IEEE 802.3 specification.
			emptyingFifo = sendFifo.Count != 0;
			// If the FIFO's not empty yet, issue another transmission. Otherwise
			// let the upper layer know we're ready for more data.
			if (sendFifo.Count > 0)
				Simulation.Callback(interframeGapTime, EmptySendFifo);
			else
				Simulation.Callback(interframeGapTime, () => {
						InterruptReason = Lan.Interrupt.SendFifoEmpty;
						Interrupt.RaiseEvent(this, null);
				});
		}
		#endregion

		#region PHY (Physical Layer Implementation)
		/// <summary>
		/// The transmitter signal.
		/// </summary>
		bool tx;

		/// <summary>
		/// The receiver signal.
		/// </summary>
		bool rx;

		/// <summary>
		/// The bitrate with which the NIC is configured.
		/// </summary>
		int configuredBitrate {
			get {
				// Emulation of Autonegotiate.
				return Connector.Cable.Bitrate;
			}
		}

		/// <summary>
		/// The interframe gap (IFG) idle period, in nanoseconds.
		/// </summary>
		/// <remarks>IEEE 802.3 defines the IFG as being 96 bit times.</remarks>
		ulong interframeGapTime {
			get {
				return (ulong) ((96 / (double) configuredBitrate) *
					1000000000);
			}
		}

		/// <summary>
		/// The maximum number of attempted retransmissions in CSMA/CD.
		/// </summary>
		static readonly int maxRetransmissions = 15;

		/// <summary>
		/// The maximum exponentiation used for backing off.
		/// </summary>
		static readonly int maxExponentiation = 10;

		/// <summary>
		/// The number of attempted retransmissions.
		/// </summary>
		int retransmissionCount;

		/// <summary>
		/// The data that is currently being transmitted.
		/// </summary>
		byte[] transmissionData;

		/// <summary>
		/// Invoked whenever the receiver senses a signal.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSignalSense(object sender, EventArgs e) {
			WritePhy("Sensing a carier.");
			if (rx && tx) {
				// Collision.
				WritePhy("Collision detected.");
				ulong jamTime = Connector.Jam();
				ExponentialBackoff(jamTime);
			} else {
				rx = true;
			}
		}

		/// <summary>
		/// Invoked whenever the carrier signal ceases.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void OnSignalCease(object sender, SignalCeaseEventArgs e) {
			if(tx)
				WritePhy("Finished transmitting bits.");
			rx = false;
			tx = false;
			// Hand data to MAC-layer.
			if (e.IsJam) {
				WritePhy("Receiving a jam signal.");
				return;
			}
			if (sender == Connector)
				OnDataTransmitted();
			else
				OnDataReceived(e.Data);
		}

		/// <summary>
		/// Transmits the specified data.
		/// </summary>
		/// <param name="data">The data to transmit.</param>
		void Transmit(byte[] data) {
			// Defer transmission until medium becomes idle.
			if (rx) {
				// Poll medium about every 10µs.
				ulong timeout = (ulong) (10000 + random.Next(5000));
				WritePhy("Deferring transmission, next try at " + timeout);
				Simulation.Callback(timeout, () => Transmit(data));
			} else {
				// Must wait another interframe-gap, before we can proceed.
				Simulation.Callback(interframeGapTime, () => StartTransmission(data));
			}
		}

		/// <summary>
		/// Starts the actual data transfer.
		/// </summary>
		/// <param name="data">The data to transmit.</param>
		void StartTransmission(byte[] data) {
			// If the medium is no longer idle at this point, start over.
			if (rx) {
				Transmit(data);
				return;
			}
			WritePhy("Starting transmission.");
			tx = true;
			transmissionData = data;
			Connector.Transmit(data);
		}

		/// <summary>
		/// Aborts a scheduled data transmission.
		/// </summary>
		void AbortTransmission() {
			WritePhy("Transmission failed! Maximum retransmission " +
				"threshold reached.");
			// Reset retransmission counter.
			retransmissionCount = 0;
		}

		/// <summary>
		/// Performs the exponential backoff algorithm of CSMA/CD.
		/// </summary>
		/// <param name="deltaTime"></param>
		void ExponentialBackoff(ulong deltaTime) {
			retransmissionCount++;
			if (retransmissionCount >= maxRetransmissions) {
				AbortTransmission();
			} else {
				// Wait a random number of slot-times, with a slot-time being
				// defined as the transmission time of 512 bits.
				// (Cmp, "Computer Networks", 5th Ed., A. Tanenbaum, p.285)
				int c = random.Next((int) Math.Pow(2,
					Math.Min(retransmissionCount, maxExponentiation)));
				// (Ex., the slot-time on 10Mbps Ethernet is 51.2µsec)
				ulong slotTime = (ulong) ((512 / (double) configuredBitrate) *
					1000000000);
				ulong waitTime = (ulong) c * slotTime;
				WritePhy("Waiting for " + waitTime + " (" + c +
					" slot times), " + retransmissionCount + ".Try, Total = " +
					(deltaTime + waitTime));
				Simulation.Callback(deltaTime + waitTime,
					() => Transmit(transmissionData));
			}
		}
		#endregion
	}
}
