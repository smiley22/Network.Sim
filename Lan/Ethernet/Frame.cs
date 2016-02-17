using System;
using System.IO;
using Network.Sim.Link;
using Network.Sim.Miscellaneous;

namespace Network.Sim.Lan.Ethernet {
	/// <summary>
	/// Represents an Ethernet frame.
	/// </summary>
	public class Frame : Serializable {
		/// <summary>
		/// The physical address of the destination host of this frame.
		/// </summary>
		public MacAddress Destination {
			get;
			private set;
		}

		/// <summary>
		/// The physical address of the host which sent this frame.
		/// </summary>
		public MacAddress Source {
			get;
			private set;
		}

		/// <summary>
		/// Indicates which protocol is encapsulated in the payload field.
		/// </summary>
		public EtherType Type {
			get;
			private set;
		}

		/// <summary>
		/// The payload data of the frame which will usually be encapsulated
		/// IP data.
		/// </summary>
		public byte[] Payload {
			get;
			private set;
		}

		/// <summary>
		/// The Frame-Check-Sequence which is a 4-byte cyclic redundancy check.
		/// </summary>
		public uint CheckSequence {
			get;
			private set;
		}

		/// <summary>
		/// The minimum size of a frame's payload field, in bytes.
		/// </summary>
		private static readonly int minimumPayloadSize = 46;

		/// <summary>
		/// The maximum size of a frame's payload field, in bytes.
		/// </summary>
		public static readonly int MaximumPayloadSize = 1500;

		/// <summary>
		/// Initializes a new EthernetFrame instance using the specified values.
		/// </summary>
		/// <param name="destination">The physical address of the destination
		/// host.</param>
		/// <param name="source">The physical address of the source host.</param>
		/// <param name="payload">The data transmitted in the frame's payload
		/// field.</param>
		/// <param name="type">The type of the encapsulated protocol.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the
		/// arguments is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the payload parameter
		/// does not satisfy the Ethernet 802.3 length requirements.</exception>
		public Frame(MacAddress destination, MacAddress source,
			byte[] payload, EtherType type) {
				destination.ThrowIfNull("destination");
				source.ThrowIfNull("source");
				payload.ThrowIfNull("payload");
				if (payload.Length > MaximumPayloadSize)
					throw new ArgumentException("The frame payload data must " +
						"not exceed 1500 bytes of data.", "payload");
				Destination = destination;
				Source = source;
				Payload = payload;
				Type = type;
				CheckSequence = Crc32.Compute(
					new ByteBuilder()
					.Append(Destination.Bytes)
					.Append(Source.Bytes)
					.Append((short)Type)
					.Append(Payload)
					.ToArray()
				);
		}

		/// <summary>
		/// Initializes a new instance of the EthernetFrame class using the
		/// specified values.
		/// </summary>
		/// <param name="destination">The physical address of the destination
		/// host.</param>
		/// <param name="source">The physical address of the source host.</param>
		/// <param name="payload">The data transmitted in the frame's payload
		/// field.</param>
		/// <param name="checkSequence">The value to set as the frame's
		/// check sequence.</param>
		/// <param name="type">The type of the encapsulated protocol.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the
		/// arguments is null.</exception>
		/// <remarks>This is a private constructor solely used for
		/// deserialization.</remarks>
		private Frame(MacAddress destination, MacAddress source,
			byte[] payload, uint checkSequence, EtherType type) {
				destination.ThrowIfNull("destination");
				source.ThrowIfNull("source");
				payload.ThrowIfNull("payload");
				Destination = destination;
				Source = source;
				Payload = payload;
				Type = type;
				CheckSequence = checkSequence;
		}

		/// <summary>
		/// Serializes this instance of the EthernetFrame class into a sequence of
		/// bytes.
		/// </summary>
		/// <returns>A sequence of bytes representing this instance of the
		/// EthernetFrame class.</returns>
		public byte[] Serialize() {
			// Ensure the frame meets the minimum size requirements.
			byte[] payloadBuffer = Payload;
			if (Payload.Length < minimumPayloadSize) {
				payloadBuffer = new byte[minimumPayloadSize];
				Array.Copy(Payload, payloadBuffer, Payload.Length);
			}
			return new ByteBuilder()
				.Append(Destination.Bytes)
				.Append(Source.Bytes)
				.Append((short) Type)
				// Note: The payload length is not actually part of a frame. In
				// reality, the length of a frame is derived from the physical coding
				// sublayer, i.e. a frame is over when the carrier drops.
				.Append(Payload.Length)
				.Append(payloadBuffer)
				.Append(CheckSequence)
				.ToArray();
		}

		/// <summary>
		/// Deserializes an EthernetFrame instance from the specified sequence of
		/// bytes.
		/// </summary>
		/// <param name="data">The sequence of bytes to deserialize an EthernetFrame
		/// object from.</param>
		/// <returns>A deserialized EthernetFrame object.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the data argument is
		/// null.</exception>
		public static Frame Deserialize(byte[] data) {
			data.ThrowIfNull("data");
			using (MemoryStream ms = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(ms)) {
					MacAddress dest = new MacAddress(reader.ReadBytes(6)),
						source = new MacAddress(reader.ReadBytes(6));
					EtherType type = (EtherType) reader.ReadInt16();
					int payloadLen = reader.ReadInt32();
					byte[] payload = reader.ReadBytes(payloadLen);
					// Skip the padding bytes, if any.
					if (payloadLen < minimumPayloadSize)
						reader.ReadBytes(minimumPayloadSize - payloadLen);
					uint fcs = reader.ReadUInt32();
					return new Frame(dest, source, payload, fcs, type);
				}
			}
		}

		/// <summary>
		/// Computes the Frame-Check-Sequence (FCS) of the specified Ethernet
		/// frame.
		/// </summary>
		/// <param name="frame">The Ethernet frame to compute the FCS for.</param>
		/// <returns>The Frame-Check-Sequence of the specified Ethernet
		/// frame.</returns>
		public static uint ComputeCheckSequence(Frame frame) {
			frame.ThrowIfNull("frame");
			return Crc32.Compute(
				new ByteBuilder()
				.Append(frame.Destination.Bytes)
				.Append(frame.Source.Bytes)
				.Append((short) frame.Type)
				.Append(frame.Payload).ToArray()
			);
		}
	}
}
