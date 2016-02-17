
namespace Network.Sim.Miscellaneous {
	/// <summary>
	/// Represents a class which can be serialized into a sequence of bytes.
	/// </summary>
	public interface Serializable {
		/// <summary>
		/// Serializes this instance into a sequence of bytes.
		/// </summary>
		/// <returns>A sequence of bytes representing this instance.</returns>
		byte[] Serialize();
	}
}