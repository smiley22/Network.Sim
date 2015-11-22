using System;
using System.IO;
using ConsoleApplication36.Miscellaneous;

namespace ConsoleApplication36.Network.Ip.Icmp {
	/// <summary>
	/// Represents an ICMP packet.
	/// </summary>
	public class IcmpPacket : Serializable {
		/// <summary>
		/// The type of the packet.
		/// </summary>
		public IcmpType Type {
			get;
			private set;
		}

		/// <summary>
		/// The subtype to the type of the packet.
		/// </summary>
		public IcmpCode Code {
			get;
			private set;
		}

		/// <summary>
		/// The checksum of the ICMP packet.
		/// </summary>
		public ushort Checksum {
			get;
			private set;
		}

		/// <summary>
		/// The variable-length data segment of the ICMP packet.
		/// </summary>
		public byte[] Data {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the IcmpPacket class using the specified
		/// values.
		/// </summary>
		/// <param name="type">The ICMP type of the packet.</param>
		/// <param name="code">The subtype to the type of the packet.</param>
		/// <param name="data">The data to transfer as part of the ICMP
		/// packet.</param>
		/// <exception cref="ArgumentNullException">Thrown if the data parameter
		/// is null.</exception>
		public IcmpPacket(IcmpType type, IcmpCode code, byte[] data) {
			data.ThrowIfNull("data");
			Type = type;
			Code = code;
			// Many ICMP packets include the IP header and the first 8 bytes of
			// an IP packet as data.
			Data = data;
			Checksum = ComputeChecksum(this);
		}

		/// <summary>
		/// Creates a new "Time Exceeded" ICMP packet.
		/// </summary>
		/// <param name="packet">The IP packet whose TTL value expired.</param>
		/// <returns>An initialized instance of the IcmpPacket class representing
		/// a "TTL expired in transit" ICMP packet.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the packet parameter
		/// is null.</exception>
		public static IcmpPacket TimeExceeded(IpPacket packet) {
			packet.ThrowIfNull("packet");
			return new IcmpPacket(IcmpType.TimeExceed, IcmpCode.TtlExpired,
				GetIpData(packet));
		}

		/// <summary>
		/// Creates a new "Destination Unreachable" ICMP packet.
		/// </summary>
		/// <param name="packet">The IP packet whose destination is
		/// unreachable.</param>
		/// <returns>An initialized instance of the IcmpPacket class representing
		/// a "Destination Network Unreachable" ICMP packet.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the packet parameter
		/// is null.</exception>
		public static IcmpPacket Unreachable(IpPacket packet) {
			packet.ThrowIfNull("packet");
			return new IcmpPacket(IcmpType.Unreachable,
				IcmpCode.DestinationNetworkUnreachable, GetIpData(packet));
		}

		/// <summary>
		/// Creates a new "Fragmentation Required" ICMP packet.
		/// </summary>
		/// <param name="packet">The IP packet for which fragmentation is
		/// required.</param>
		/// <returns>An initialized instance of the IcmpPacket class representing
		/// a "Fragmentation Required" ICMP packet.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the packet parameter
		/// is null.</exception>
		public static IcmpPacket FragmentationRequired(IpPacket packet) {
			packet.ThrowIfNull("packet");
			return new IcmpPacket(IcmpType.Unreachable, IcmpCode.FragmentationRequired,
				GetIpData(packet));
		}

		/// <summary>
		/// Creates a new "Source Quench" ICMP packet.
		/// </summary>
		/// <param name="packet">The IP packet in whose response the source
		/// quench packet is being sent.</param>
		/// <returns>An initialized instance of the IcmpPacket class representing
		/// a "Source Quench" ICMP packet.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the packet parameter
		/// is null.</exception>
		public static IcmpPacket SourceQuench(IpPacket packet) {
			packet.ThrowIfNull("packet");
			return new IcmpPacket(IcmpType.SourceQuench, 0, GetIpData(packet));
		}

		/// <summary>
		/// Returns the IP header and the first 8 byte of the IP data segment
		/// as an array of bytes.
		/// </summary>
		/// <param name="packet">The IP packet whose data to return.</param>
		/// <returns>An arry of bytes containing the IP header and the first
		/// 8 bytes of IP data of the specified IP packet.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the packet
		/// parameter is null.</exception>
		/// <remarks>Many ICMP packets include the IP header and the first 8
		/// bytes of data of an IP packet.</remarks>
		static byte[] GetIpData(IpPacket packet) {
			packet.ThrowIfNull("packet");
			// Many ICMP packets include the 20-byte IP header and the first
			// 8 bytes of an IP packet.
			byte[] serialized = packet.Serialize();
			int size = Math.Min(20 + 8, serialized.Length);
			byte[] data = new byte[size];
			Array.Copy(serialized, data, data.Length);
			return data;
		}

		/// <summary>
		/// Private constructor used for deserialization.
		/// </summary>
		private IcmpPacket(IcmpType type, IcmpCode code, ushort checksum,
			byte[] data) {
				data.ThrowIfNull("data");
				Type = type;
				Code = code;
				Checksum = checksum;
				Data = data;
		}

		/// <summary>
		/// Serializes this instance of the IcmpPacket class into a sequence of
		/// bytes.
		/// </summary>
		/// <returns>A sequence of bytes representing this instance of the
		/// IcmpPacket class.</returns>
		public byte[] Serialize() {
			return new ByteBuilder()
				.Append((byte) Type)
				.Append((byte) Code)
				.Append(Checksum)
				.Append(Data)
				.ToArray();
		}

		/// <summary>
		/// Deserializes an IcmpPacket instance from the specified sequence of
		/// bytes.
		/// </summary>
		/// <param name="data">The sequence of bytes to deserialize an IcmpPacket
		/// object from.</param>
		/// <returns>A deserialized IpPacket object.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the data argument is
		/// null.</exception>
		/// <exception cref="SerializationException">Thrown if the ICMP packet could
		/// not be deserialized from the specified byte array.</exception>
		public static IcmpPacket Deserialize(byte[] data) {
			data.ThrowIfNull("data");
			using (MemoryStream ms = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(ms)) {
					IcmpType type = (IcmpType) reader.ReadByte();
					IcmpCode code = (IcmpCode) reader.ReadByte();
					ushort checksum = reader.ReadUInt16();
					byte[] icmpData = reader.ReadAllBytes();
					IcmpPacket packet = new IcmpPacket(type, code, checksum, icmpData);
					if(ComputeChecksum(packet, true) != 0)
						throw new System.Runtime.Serialization.
							SerializationException("The ICMP header is corrupted.");
					return packet;
				}
			}
		}

		/// <summary>
		/// Computes the 16-bit checksum of the specified ICMP packet.
		/// </summary>
		/// <param name="packet">The packet to compute the checksum for.</param>
		/// <param name="withChecksumField">true to include the packet's
		/// checksum field in the calculation; otherwise false.</param>
		/// <returns>The checksum of the specified ICMP packet.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the packet
		/// argument is null.</exception>
		public static ushort ComputeChecksum(IcmpPacket packet,
			bool withChecksumField = false) {
			packet.ThrowIfNull("packet");
			byte[] bytes = new ByteBuilder()
				.Append((byte) packet.Type)
				.Append((byte) packet.Code)
				.Append(withChecksumField ? packet.Checksum : (ushort) 0)
				.Append(packet.Data)
				.ToArray();
			int sum = 0;
			// Treat the header bytes as a sequence of unsigned 16-bit values and
			// sum them up.
			for (int n = 0; n < bytes.Length; n += 2)
				sum += BitConverter.ToUInt16(bytes, n);
			// Use carries to compute the 1's complement sum.
			sum = (sum >> 16) + (sum & 0xFFFF);
			// Return the inverted 16-bit result.
			return (ushort) (~sum);
		}
	}
}
