using System;
using System.IO;
using System.Text;
using ConsoleApplication36.Miscellaneous;

namespace ConsoleApplication36.Network.Ip {
	/// <summary>
	/// Represents an IPv4 packet.
	/// </summary>
	public class IpPacket : Serializable {
		/// <summary>
		/// The version of the IP packet.
		/// </summary>
		public IpVersion Version {
			get;
			private set;
		}

		/// <summary>
		/// The Internet Header Length, which is the number of 32-bit words
		/// in the header.
		/// </summary>
		public byte Ihl {
			get;
			private set;
		}

		/// <summary>
		/// The Differentiated Services Code Point (not implemented).
		/// </summary>
		public byte Dscp {
			get;
			private set;
		}

		/// <summary>
		/// The total length of the packet, including header and data, in bytes.
		/// </summary>
		public ushort TotalLength {
			get;
			private set;
		}

		/// <summary>
		/// The idenfication used for uniquely identifying fragments of a fragmented
		/// IP packet.
		/// </summary>
		public ushort Identification {
			get;
			private set;
		}

		/// <summary>
		/// The flags for controlling and identifying fragments set on this packet.
		/// </summary>
		public IpFlag Flags {
			get;
			private set;
		}

		/// <summary>
		/// The fragment offset of the packet.
		/// </summary>
		public ushort FragmentOffset {
			get;
			private set;
		}

		/// <summary>
		/// The hop count of the packet.
		/// </summary>
		public byte TimeToLive {
			get;
			set;
		}

		/// <summary>
		/// Indicates which transport protocol is encapsulated in the data section.
		/// </summary>
		public IpProtocol Protocol {
			get;
			private set;
		}

		/// <summary>
		/// The checksum of the header of this IP packet.
		/// </summary>
		public ushort Checksum {
			get;
			set;
		}

		/// <summary>
		/// The IPv4 address of the sender of the packet.
		/// </summary>
		public IpAddress Source {
			get;
			private set;
		}

		/// <summary>
		/// The IPv4 address of the receiver of the packet.
		/// </summary>
		public IpAddress Destination {
			get;
			private set;
		}

		/// <summary>
		/// The data section of the IP packet.
		/// </summary>
		public byte[] Data {
			get;
			private set;
		}

		/// <summary>
		/// Optional header fields.
		/// </summary>
		public byte[] Options {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the IpPacket class using the specified
		/// values.
		/// </summary>
		/// <param name="destination">The IPv4 address of the destination
		/// host.</param>
		/// <param name="source">The IPv4 address of the sending host.</param>
		/// <param name="type">The type of the transport protocol encapsulated in
		/// the IP packet's data section.</param>
		/// <param name="data">The transport data to transfer as part of the IP
		/// packet.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		public IpPacket(IpAddress destination, IpAddress source,
			IpProtocol type, byte[] data) {
				destination.ThrowIfNull("destination");
				source.ThrowIfNull("source");
				data.ThrowIfNull("data");
				Destination = destination;
				Source = source;
				Protocol = type;
				Data = data;
				TimeToLive = 64;
				Ihl = 5;
				TotalLength = (ushort) (20 + Data.Length);
				Version = IpVersion.Ipv4;
				Checksum = ComputeChecksum(this);
		}

		public IpPacket(IpAddress destination, IpAddress source,
			IpProtocol type, byte ihl, byte dscp, byte ttl, ushort identification,
			IpFlag flags, ushort fragmentOffset, byte[] data) {
			destination.ThrowIfNull("destination");
			source.ThrowIfNull("source");
			data.ThrowIfNull("data");
			if (ihl > 0x0F)
				throw new ArgumentException("The Internet Header Length field must " +
					"be in the range from 0 to 15.", "ihl");
			if (fragmentOffset > 0x1FFF)
				throw new ArgumentException("The Fragment Offset field must be in " +
					"the range from 0 to 8191.", "fragmentOffset");
			Version = IpVersion.Ipv4;
			Ihl = ihl;
			Dscp = dscp;
			Identification = identification;
			Flags = flags;
			FragmentOffset = fragmentOffset;
			TimeToLive = ttl;
			Protocol = type;
			Source = source;
			Destination = destination;
			Data = data;
			TotalLength = (ushort) (20 + Data.Length);
			Checksum = ComputeChecksum(this);
		}

		/// <summary>
		/// Private constructor used for deserialization.
		/// </summary>
		private IpPacket(IpVersion version, byte ihl, byte dscp, ushort totalLength,
			ushort identification, IpFlag flags, ushort fragmentOffset,
			byte ttl, IpProtocol protocol, ushort checksum, IpAddress source,
			IpAddress destination) {
			destination.ThrowIfNull("destination");
			source.ThrowIfNull("source");
			if (ihl > 0x0F)
				throw new ArgumentException("The Internet Header Length field must " +
					"be in the range from 0 to 15.", "ihl");
			if (fragmentOffset > 0x1FFF)
				throw new ArgumentException("The Fragment Offset field must be in " +
					"the range from 0 to 8191.", "fragmentOffset");
			Version = version;
			Ihl = ihl;
			Dscp = dscp;
			TotalLength = totalLength;
			Identification = identification;
			Flags = flags;
			FragmentOffset = fragmentOffset;
			TimeToLive = ttl;
			Protocol = protocol;
			Checksum = checksum;
			Source = source;
			Destination = destination;
			Data = new byte[0];
		}

		/// <summary>
		/// Serializes this instance of the IpPacket class into a sequence of
		/// bytes.
		/// </summary>
		/// <returns>A sequence of bytes representing this instance of the
		/// IpPacket class.</returns>
		public byte[] Serialize() {
			// The version and IHL fields are 4 bit wide each.
			byte vi = (byte) (((Ihl & 0x0F) << 4) | (((int) Version) & 0x0F));
			// The flags field is 3 bits and the fragment offset 13 bits wide.
			ushort ffo = (ushort) (((FragmentOffset & 0x1FFF) << 3) |
				((int)Flags & 0x07));
			return new ByteBuilder()
				.Append(vi)
				.Append(Dscp)
				.Append(TotalLength)
				.Append(Identification)
				.Append(ffo)
				.Append(TimeToLive)
				.Append((byte) Protocol)
				.Append(Checksum)
				.Append(Source.Bytes)
				.Append(Destination.Bytes)
				.Append(Data)
				.ToArray();
		}

		/// <summary>
		/// Deserializes an IpPacket instance from the specified sequence of
		/// bytes.
		/// </summary>
		/// <param name="data">The sequence of bytes to deserialize an IpPacket
		/// object from.</param>
		/// <returns>A deserialized IpPacket object.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the data argument is
		/// null.</exception>
		/// <exception cref="SerializationException">Thrown if the IP packet could
		/// not be deserialized from the specified byte array.</exception>
		public static IpPacket Deserialize(byte[] data) {
			data.ThrowIfNull("data");
			using (MemoryStream ms = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(ms)) {
					byte vi = reader.ReadByte();
					IpVersion version = (IpVersion)(vi & 0x0F);
					byte ihl = (byte) (vi >> 4), dscp = reader.ReadByte();
					ushort totalLength = reader.ReadUInt16(),
						identification = reader.ReadUInt16(), ffo = reader.ReadUInt16();
					IpFlag flags = (IpFlag) (ffo & 0x07);
					ushort fragmentOffset = (ushort) (ffo >> 3);
					byte ttl = reader.ReadByte();
					IpProtocol type = (IpProtocol) reader.ReadByte();
					ushort checksum = reader.ReadUInt16();
					IpAddress src = new IpAddress(reader.ReadBytes(4)),
						dst = new IpAddress(reader.ReadBytes(4));

					IpPacket packet = new IpPacket(version, ihl, dscp, totalLength,
						identification, flags, fragmentOffset, ttl, type, checksum,
						src, dst);
					// Computing the checksum should yield a value of 0 unless errors are
					// detected.
					if (ComputeChecksum(packet, true) != 0)
						throw new System.Runtime.Serialization.
							SerializationException("The IPv4 header is corrupted.");
					// If no errors have been detected, read the data section.
					packet.Data = reader.ReadBytes(totalLength - 20);
					return packet;
				}
			}
		}

		/// <summary>
		/// Computes the 16-bit checksum of the specified IPv4 packet.
		/// </summary>
		/// <param name="packet">The packet to compute the checksum for.</param>
		/// <param name="withChecksumField">true to include the packet's
		/// checksum field in the calculation; otherwise false.</param>
		/// <returns>The checksum of the specified IPv4 packet.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the packet
		/// argument is null.</exception>
		public static ushort ComputeChecksum(IpPacket packet,
			bool withChecksumField = false) {
			packet.ThrowIfNull("packet");
			// The version and IHL fields are 4 bit wide each.
			byte vi = (byte) (((packet.Ihl & 0x0F) << 4) |
				(((int) packet.Version) & 0x0F));
			// The flags field is 3 bits and the fragment offset 13 bits wide.
			ushort ffo = (ushort) (((packet.FragmentOffset & 0x1FFF) << 3) |
				((int) packet.Flags & 0x07));
			byte[] bytes = new ByteBuilder()
				.Append(vi)
				.Append(packet.Dscp)
				.Append(packet.TotalLength)
				.Append(packet.Identification)
				.Append(ffo)
				.Append(packet.TimeToLive)
				.Append((byte) packet.Protocol)
				.Append(withChecksumField ? packet.Checksum : (ushort)0)
				.Append(packet.Source.Bytes)
				.Append(packet.Destination.Bytes)
				.ToArray();
			int sum = 0;
			// Treat the header bytes as a sequence of unsigned 16-bit values and
			// sum them up.
			for (int n = 0; n < bytes.Length; n += 2)
				sum += BitConverter.ToUInt16(bytes, n);
			// Use carries to compute the 1's complement sum.
			sum = (sum >> 16) + (sum & 0xFFFF);
			// Return the inverted 16-bit result.
			return (ushort)(~ sum);
		}

		/// <summary>
		/// Returns a textual description of the IpPacket instance.
		/// </summary>
		/// <returns>A textual description of this IpPacket instance.</returns>
		public override string ToString() {
			return new StringBuilder()
				.AppendLine("Version: " + Version)
				.AppendLine("IHL: " + Ihl)
				.AppendLine("DSCP: " + Dscp)
				.AppendLine("Total Length: " + TotalLength)
				.AppendLine("Identification: " + Identification)
				.AppendLine("Flags: " + Flags)
				.AppendLine("Fragment Offset: " + FragmentOffset)
				.AppendLine("TTL: " + TimeToLive)
				.AppendLine("Protocol: " + Protocol)
				.AppendLine("Checksum: " + Checksum)
				.AppendLine("Source: " + Source)
				.AppendLine("Destination: " + Destination)
				.AppendLine("Data Length: " + Data.Length)
				.ToString();
		}
	}
}
