using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Network.Sim.Core;
using Network.Sim.Link;
using Network.Sim.Miscellaneous;

namespace Network.Sim.Lan.Ethernet {
	/// <summary>
	/// Represents an Ethernet bridge.
	/// </summary>
	public class Bridge {
		/// <summary>
		/// The bridges's I/O ports.
		/// </summary>
		public IReadOnlyList<Connector> Ports {
			get {
				return ports;
			}
		}

		/// <summary>
		/// The bridges's inherent processing delay.
		/// </summary>
		public ulong Delay {
			get;
			private set;
		}

		/// <summary>
		/// The bridges's I/O ports.
		/// </summary>
		List<Connector> ports = new List<Connector>();

		/// <summary>
		/// Initializes a new instance of the Bridge class.
		/// </summary>
		/// <param name="numPorts">The number of I/O ports.</param>
		/// <param name="delay">The inherent propagation delay of the bridge.</param>
		public Bridge(int numPorts, ulong delay) {
			for (int i = 0; i < numPorts; i++) {
				Connector c = new Connector();
				c.SignalSense += (sender, e) => OnSignalSense(c);
				c.SignalCease += (sender, e) => OnSignalCease(c, sender, e);
				ports.Add(c);
				// House-keeping.
				InitFields(c);
			}
		}

		/// <summary>
		/// Writes the specified string value to the simulation output stream.
		/// </summary>
		/// <param name="s">The value to write.</param>
		void WriteMac(string s) {
			Simulation.WriteLine(OutputLevel.Datalink, this +
				": " + s, ConsoleColor.Gray);
		}

		/// <summary>
		/// Writes the specified string value to the simulation output stream.
		/// </summary>
		/// <param name="s">The value to write.</param>
		void WritePhy(string s) {
			Simulation.WriteLine(OutputLevel.Physical, this +
				": " + s, ConsoleColor.DarkGray);
		}

		/// <summary>
		/// Creates the needed entries in the various dictionaries for the
		/// specified connector.
		/// </summary>
		/// <param name="connector">The connector to initialize.</param>
		void InitFields(Connector connector) {
			tx.Add(connector, false);
			rx.Add(connector, false);
			retransmissionCount.Add(connector, 0);
			transmissionData.Add(connector, null);

			inputFifo.Add(connector, new CappedQueue<Frame>());
			outputFifo.Add(connector, new CappedQueue<Frame>());
			waitTime.Add(connector, 0);
			isIdle.Add(connector, true);
		}

		#region MAC (Media Access Control Implementation)
		/// <summary>
		/// Returns a read-only collection of entries of the bridge's forwarding
		/// table.
		/// </summary>
		public IReadOnlyDictionary<MacAddress, Connector> ForwardTable {
			get {
				return new ReadOnlyDictionary<MacAddress, Connector>(forwardTable);
			}
		}

		/// <summary>
		/// A list of input buffers for each of the bridge's receivers.
		/// </summary>
		IDictionary<Connector, CappedQueue<Frame>> inputFifo =
			new Dictionary<Connector, CappedQueue<Frame>>();

		/// <summary>
		/// A list of output buffers for each of the bridge's transmitters.
		/// </summary>
		IDictionary<Connector, CappedQueue<Frame>> outputFifo =
			new Dictionary<Connector, CappedQueue<Frame>>();

		/// <summary>
		/// Determines whether the fifos are currently being emptied.
		/// </summary>
		bool emptyingFifos;

		/// <summary>
		/// The bridge's forwarding table.
		/// </summary>
		IDictionary<MacAddress, Connector> forwardTable =
			new Dictionary<MacAddress, Connector>();

		/// <summary>
		/// Saves a timestamp for each connector at which the next transmission
		/// may, at earliest, occur.
		/// </summary>
		IDictionary<Connector, ulong> waitTime =
			new Dictionary<Connector, ulong>();

		/// <summary>
		/// Determines whether a transmitter is currently able to transmit data.
		/// </summary>
		IDictionary<Connector, bool> isIdle =
			new Dictionary<Connector, bool>();

		/// <summary>
		/// Invoked on behalf of PHY whenever new data has been received.
		/// </summary>
		/// <param name="data">The data that was received.</param>
		void OnDataReceived(Connector connector, byte[] data) {
			data.ThrowIfNull("data");
			WriteMac("Received an Ethernet frame.");
			waitTime[connector] = Simulation.Time + interframeGapTime(connector);
			Frame frame = Frame.Deserialize(data);
			// Compute checksum and compare to the one contained in the frame.
			uint fcs = Frame.ComputeCheckSequence(frame);
			if (fcs != frame.CheckSequence) {
				WriteMac("Detected a bad frame check sequence, discarding.");
				return;
			}
			// Remember the port through which the frame came in.
			forwardTable[frame.Source] = connector;
			// If we know the destination and it's on the same port as the
			// source, discard the frame.
			if (forwardTable.ContainsKey(frame.Destination)) {
				if (forwardTable[frame.Source] == forwardTable[frame.Destination])
					return;
			}
			// Start emptying the FIFO, if we're not already doing it.
			if (emptyingFifos == false)
				Simulation.Callback(0, EmptyFifos);
			// Enqueue the data.
			inputFifo[connector].Enqueue(frame);
		}

		/// <summary>
		/// Empties the input and output buffers of the bridge. Processes one
		/// input and one output entry per call.
		/// </summary>
		void EmptyFifos() {
			// Scoop frames into their respective output buffers.
			foreach (var pair in inputFifo) {
				// FIFO is empty.
				if (pair.Value.Count == 0)
					continue;
				Frame frame = pair.Value.Dequeue();
				// We already know where to forward this frame to.
				if (forwardTable.ContainsKey(frame.Destination)) {
					WriteMac("Queueing frame for I/O port.");
					Connector outport = forwardTable[frame.Destination];
					outputFifo[outport].Enqueue(frame);
				} else {
					WriteMac("Flooding");
					// Flood it out on all other ports.
					foreach (Connector port in ports) {
						if (forwardTable[frame.Source] == port)
							continue;
						if (!port.IsConnected)
							continue;
						outputFifo[port].Enqueue(frame);
					}
				}
				break;
			}
			// See if we can kick-off any pending frames.
			foreach (var pair in outputFifo) {
				if (pair.Value.Count == 0)
					continue;
				if (!isIdle[pair.Key])
					continue;
				if (waitTime[pair.Key] > Simulation.Time)
					continue;
				Frame frame = pair.Value.Dequeue();
				isIdle[pair.Key] = false;
				Transmit(pair.Key, frame.Serialize());
				break;
			}
			// Processing this method takes 'Delay' nanoseconds.
			emptyingFifos = NumQueuedFrames() > 0;
			if (emptyingFifos)
				Simulation.Callback(Delay, EmptyFifos);
		}

		/// <summary>
		/// Invoked om behalf of PHY whenever a frame has been transmitted.
		/// </summary>
		void OnDataTransmitted(Connector connector) {
			WriteMac("Finished transmitting Ethernet frame.");
			isIdle[connector] = true;
			waitTime[connector] = Simulation.Time + interframeGapTime(connector);
		}

		/// <summary>
		/// Returns the cumultative sum of frames currently queued in the bridge's
		/// in- and output buffers.
		/// </summary>
		/// <returns>The total number of frames currently enqueued.</returns>
		int NumQueuedFrames() {
			int numFrames = 0;
			foreach (var fifo in inputFifo.Values)
				numFrames = numFrames + fifo.Count;
			foreach (var fifo in outputFifo.Values)
				numFrames = numFrames + fifo.Count;
			return numFrames;
		}
		#endregion

		#region PHY (Physical Layer Implementation)
		/// <summary>
		/// The random number generator needed for CSMA/CD.
		/// </summary>
		static readonly Random random = new Random();

		/// <summary>
		/// The transmitter signal of each of the bridge's transmitters.
		/// </summary>
		IDictionary<Connector, bool> tx = new Dictionary<Connector, bool>();

		/// <summary>
		/// The receiver signal of each of the bridge's receivers.
		/// </summary>
		IDictionary<Connector, bool> rx = new Dictionary<Connector, bool>();

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
		IDictionary<Connector, int> retransmissionCount =
			new Dictionary<Connector, int>();

		/// <summary>
		/// The data that is currently being transmitted.
		/// </summary>
		IDictionary<Connector, byte[]> transmissionData =
			new Dictionary<Connector, byte[]>();

		int configuredBitrate(Connector connector) {
			return connector.Cable.Bitrate;
		}

		ulong interframeGapTime(Connector connector) {
			return (ulong) ((96 / (double) configuredBitrate(connector)) *
				1000000000);
		}

		/// <summary>
		/// Invoked whenever the receiver senses a signal.
		/// </summary>
		/// <param name="connector">The connector sensing a signal.</param>
		void OnSignalSense(Connector connector) {
			WritePhy("Sensing a carier.");
			if (rx[connector] && tx[connector]) {
				// Collision.
				WritePhy("Collision detected.");
				ulong jamTime = connector.Jam();
				ExponentialBackoff(connector, jamTime);
			} else {
				rx[connector] = true;
			}
		}

		/// <summary>
		/// Invoked whenever the carrier signal ceases.
		/// </summary>
		/// <param name="connector">The connector which is no longer
		/// sensing a signal.</param>
		void OnSignalCease(Connector connector, object sender, SignalCeaseEventArgs e) {
			if (tx[connector])
				WritePhy("Finished transmitting bits.");
			rx[connector] = false;
			tx[connector] = false;
			// Hand data to MAC-layer.
			if (e.IsJam) {
				WritePhy("Receiving a jam signal.");
				return;
			}
			if (sender == connector)
				OnDataTransmitted(connector);
			else
				OnDataReceived(connector, e.Data);
		}

		/// <summary>
		/// Transmits the specified data.
		/// </summary>
		/// <param name="data">The data to transmit.</param>
		void Transmit(Connector connector, byte[] data) {
			// Defer transmission until medium becomes idle.
			if (rx[connector]) {
				// Poll medium about every 10µs.
				ulong timeout = (ulong) (10000 + random.Next(5000));
				WritePhy("Deferring transmission, next try at " + timeout);
				Simulation.Callback(timeout, () => Transmit(connector, data));
			} else {
				// Must wait another interframe-gap, before we can proceed.
				Simulation.Callback(interframeGapTime(connector),
					() => StartTransmission(connector, data));
			}
		}

		/// <summary>
		/// Starts the actual data transfer.
		/// </summary>
		/// <param name="data">The data to transmit.</param>
		void StartTransmission(Connector connector, byte[] data) {
			// If the medium is no longer idle at this point, start over.
			if (rx[connector]) {
				Transmit(connector, data);
				return;
			}
			WritePhy("Starting transmission.");
			tx[connector] = true;
			transmissionData[connector] = data;
			connector.Transmit(data);
		}

		/// <summary>
		/// Aborts a scheduled data transmission.
		/// </summary>
		void AbortTransmission(Connector connector) {
			WritePhy("Transmission failed! Maximum retransmission " +
				"threshold reached.");
			// Reset retransmission counter.
			retransmissionCount[connector] = 0;
		}

		/// <summary>
		/// Performs the exponential backoff algorithm of CSMA/CD.
		/// </summary>
		/// <param name="deltaTime"></param>
		void ExponentialBackoff(Connector connector, ulong deltaTime) {
			retransmissionCount[connector]++;
			if (retransmissionCount[connector] >= maxRetransmissions) {
				AbortTransmission(connector);
			} else {
				// Wait a random number of slot-times, with a slot-time being
				// defined as the transmission time of 512 bits.
				// (Cmp, "Computer Networks", 5th Ed., A. Tanenbaum, p.285)
				int c = random.Next((int) Math.Pow(2,
					Math.Min(retransmissionCount[connector], maxExponentiation)));
				// (Ex., the slot-time on 10Mbps Ethernet is 51.2µsec)
				ulong slotTime = (ulong) ((512 / (double)
					configuredBitrate(connector)) * 1000000000);
				ulong waitTime = (ulong) c * slotTime;
				WritePhy("Waiting for " + waitTime + " (" + c +
					" slot times), " + retransmissionCount + ".Try, Total = " +
					(deltaTime + waitTime));
				Simulation.Callback(deltaTime + waitTime,
					() => Transmit(connector, transmissionData[connector]));
			}
		}
		#endregion
	}
}
