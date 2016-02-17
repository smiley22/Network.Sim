using System;
using System.IO;
using System.Text;
using Network.Sim.Link;
using Network.Sim.Miscellaneous;

namespace Network.Sim.Network.Ip.Arp {
	/// <summary>
	/// Represents an ARP message which is either a request or response
	/// message.
	/// </summary>
	public class ArpPacket : Serializable {
		/// <summary>
		/// The physical broadcast address to reach all stations.
		/// </summary>
		static readonly MacAddress broadcastAddress =
			new MacAddress("FF:FF:FF:FF:FF:FF");

		/// <summary>
		/// Determines whether this ARP packet is a request message.
		/// </summary>
		public bool IsRequest {
			get;
			private set;
		}

		/// <summary>
		/// Determines whether this ARP packet is a response message.
		/// </summary>
		public bool IsResponse {
			get {
				return !IsRequest;
			}
		}

		/// <summary>
		/// The MAC-48 address of the sender of this ARP packet.
		/// </summary>
		public MacAddress MacAddressSender {
			get;
			private set;
		}

		/// <summary>
		/// The MAC-48 address of the target of this ARP packet. This
		/// field is ignored in requests.
		/// </summary>
		public MacAddress MacAddressTarget {
			get;
			private set;
		}

		/// <summary>
		/// The IPv4 address of the sender of this ARP packet.
		/// </summary>
		public IpAddress IpAddressSender {
			get;
			private set;
		}

		/// <summary>
		/// The IPv4 address of the target of this ARP packet.
		/// </summary>
		public IpAddress IpAddressTarget {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the ArpPacket class creating an ARP
		/// request message.
		/// </summary>
		/// <param name="macAddressSender">The MAC-48 address of the sender.</param>
		/// <param name="ipAddressSender">The IPv4 address of the sender.</param>
		/// <param name="ipAddressTarget">The IPv4 address of the target.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		public ArpPacket(MacAddress macAddressSender, IpAddress ipAddressSender,
			IpAddress ipAddressTarget) {
				macAddressSender.ThrowIfNull("macAddressSender");
				ipAddressSender.ThrowIfNull("ipAddressSender");
				ipAddressTarget.ThrowIfNull("ipAddressTarget");
				IsRequest = true;
				MacAddressSender = macAddressSender;
				IpAddressSender = ipAddressSender;
				IpAddressTarget = ipAddressTarget;
				// Broadcast to all stations.
				MacAddressTarget = broadcastAddress;
		}

		/// <summary>
		/// Initializes a new instance of the ArpPacket class creating an ARP
		/// response message.
		/// </summary>
		/// <param name="macAddressSender">The MAC-48 address of the sender.</param>
		/// <param name="ipAddressSender">The IPv4 address of the sender.</param>
		/// <param name="macAddressTarget">The MAC-48 address of the target.</param>
		/// <param name="ipAddressTarget">The IPv4 address of the target.</param>
		/// <exception cref="ArgumentNullException">Thrown if any of the arguments
		/// is null.</exception>
		public ArpPacket(MacAddress macAddressSender, IpAddress ipAddressSender,
			MacAddress macAddressTarget, IpAddress ipAddressTarget)
			: this(macAddressSender, ipAddressSender, ipAddressTarget) {
				macAddressTarget.ThrowIfNull("macAddressTarget");
				MacAddressTarget = macAddressTarget;
				IsRequest = false;
		}

		/// <summary>
		/// Serializes this instance of the ArpPacket class into a sequence of
		/// bytes.
		/// </summary>
		/// <returns>A sequence of bytes representing this instance of the
		/// ArpPacket class.</returns>
		public byte[] Serialize() {
			return new ByteBuilder()
				.Append(IsRequest)
				.Append(MacAddressSender.Bytes)
				.Append(IpAddressSender.Bytes)
				.Append(MacAddressTarget.Bytes)
				.Append(IpAddressTarget.Bytes)
				.ToArray();
		}

		/// <summary>
		/// Deserializes an ArpPacket instance from the specified sequence of
		/// bytes.
		/// </summary>
		/// <param name="data">The sequence of bytes to deserialize an ArpPacket
		/// object from.</param>
		/// <returns>A deserialized ArpPacket object.</returns>
		public static ArpPacket Deserialize(byte[] data) {
			using (MemoryStream ms = new MemoryStream(data)) {
				using (BinaryReader reader = new BinaryReader(ms)) {
					bool isRequest = reader.ReadBoolean();
					MacAddress macSender = new MacAddress(reader.ReadBytes(6));
					IpAddress ipSender = new IpAddress(reader.ReadBytes(4));
					MacAddress macTarget = new MacAddress(reader.ReadBytes(6));
					IpAddress ipTarget = new IpAddress(reader.ReadBytes(4));
					if (isRequest)
						return new ArpPacket(macSender, ipSender, ipTarget);
					else
						return new ArpPacket(macSender, ipSender, macTarget, ipTarget);
				}
			}
		}

		/// <summary>
		/// Returns a textual description of the ArpPacket instance.
		/// </summary>
		/// <returns>A textual description of this ArpPacket instance.</returns>
		public override string ToString() {
			return new StringBuilder()
				.AppendLine("Type: " + (IsRequest ? "Request" : "Response"))
				.AppendLine("MAC-48 Sender: " + MacAddressSender)
				.AppendLine("MAC-48 Target: " + MacAddressTarget)
				.AppendLine("IPv4 Sender: " + IpAddressSender)
				.AppendLine("IPv4 Target: " + IpAddressTarget)
				.ToString();
		}
	}
}
