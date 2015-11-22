
namespace ConsoleApplication36.Network.Ip {
	/// <summary>
	/// Represents possible values for the Protocol field of the IP header. It
	/// indicates which higher-level protocol is encapsulated in the data section
	/// of an IP packet.
	/// </summary>
	public enum IpProtocol : byte {
		/// <summary>
		/// Internet Control Message Protocol.
		/// </summary>
		Icmp = 0x01,
		/// <summary>
		/// Transmission Control Protocol.
		/// </summary>
		Tcp = 0x06,
		/// <summary>
		/// User Datagram Protocol.
		/// </summary>
		Udp = 0x11
	}
}
