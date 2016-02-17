
namespace Network.Sim.Network.Ip.Icmp {
	/// <summary>
	/// Represents possible values for the type field of the ICMP header.
	/// Only a subset of the possible values are supported.
	/// </summary>
	public enum IcmpType : byte {
		/// <summary>
		/// Echo Reply (used to ping).
		/// </summary>
		EchoReply = 0x00,
		/// <summary>
		/// The destination is unreachable.
		/// </summary>
		Unreachable = 0x03,
		/// <summary>
		/// Source quench (congestion control).
		/// </summary>
		SourceQuench = 0x04,
		/// <summary>
		/// Echo Request (used to ping).
		/// </summary>
		EchoRequest = 0x08,
		/// <summary>
		/// Time exceeded (TTL).
		/// </summary>
		TimeExceed = 0x11
	}
}
