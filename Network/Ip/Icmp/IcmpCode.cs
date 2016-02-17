
namespace Network.Sim.Network.Ip.Icmp {
	/// <summary>
	/// Represents the possible values for the code field of the ICMP header.
	/// Only a subset of the possible values are supported.
	/// </summary>
	public enum IcmpCode : byte {
		/// <summary>
		/// The ICMP message is a reply to an echo request.
		/// </summary>
		EchoReply = 0x00,
		/// <summary>
		/// The destination network is unreachable.
		/// </summary>
		DestinationNetworkUnreachable = 0x00,
		/// <summary>
		/// The IP packet's Time-to-live has expired.
		/// </summary>
		TtlExpired = 0x00,
		/// <summary>
		/// The IP packet must be fragmented but has the "Dont-Fragment"
		/// flag set.
		/// </summary>
		FragmentationRequired = 0x08
	}
}
