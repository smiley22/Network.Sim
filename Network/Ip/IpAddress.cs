using System;
using System.Globalization;
using System.Text;

namespace Network.Sim.Network.Ip {
	/// <summary>
	/// Represents an IPv4 address.
	/// </summary>
	public class IpAddress {
		/// <summary>
		/// The byte array storing the address.
		/// </summary>
		byte[] address = new byte[4];

		/// <summary>
		/// The IP address as an array of bytes.
		/// </summary>
		public byte[] Bytes {
			get {
				return address;
			}
		}

		/// <summary>
		/// Initializes a new instance of the IpAddress class using the specified
		/// byte array.
		/// </summary>
		/// <param name="address">An array containing 4 bytes to initialize this
		/// instance with.</param>
		public IpAddress(byte[] address) {
			if (address.Length != 4)
				throw new ArgumentException("The array must have a size of 4 bytes.");
			for (int i = 0; i < address.Length; i++)
				this.address[i] = address[i];
		}

		/// <summary>
		/// Initializes a new instance of the IpAddress class using the specified
		/// 32-bit integer value.
		/// </summary>
		/// <param name="address">A 32-bit integer to initialize this instance
		/// with.</param>
		public IpAddress(Int32 address) {
			for (int i = 0; i < this.address.Length; i++) 
				this.address[i] = (byte)((address >> ((3 - i) * 8)) & 0xFF);
		}

		/// <summary>
		/// Initializes a new instance of the IpAddress class using the specified
		/// string.
		/// </summary>
		/// <param name="address">A string containing an IPv4 address in dot-decimal
		/// notation.</param>
		/// <exception cref="ArgumentNullException">Thrown if the address argument
		/// is null.</exception>
		public IpAddress(string address) {
			address.ThrowIfNull("address");
			this.address = Parse(address);
		}

		/// <summary>
		/// Returns a textual representation of this instance.
		/// </summary>
		/// <returns>A textual representation of this IP address.</returns>
		public override string ToString() {
			StringBuilder b = new StringBuilder();
			for (int i = 0; i < address.Length; i++) {
				b.Append(address[i].ToString("D"));
				if (i < (address.Length - 1))
					b.Append(".");
			}
			return b.ToString();
		}

		/// <summary>
		/// Determines whether the specified object is equal to this IpAddress
		/// instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>True if the specified object is semantically equal to this
		/// IpAddress instance; Otherwise false.</returns>
		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			IpAddress other = obj as IpAddress;
			if (other == null)
				return false;
			for (int i = 0; i < address.Length; i++) {
				if (address[i] != other.address[i])
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the hash code of this instance.
		/// </summary>
		/// <returns>The hash code of this IpAddress instance.</returns>
		public override int GetHashCode() {
			int hash = 13;
			foreach (byte b in address)
				hash = (hash * 7) + b.GetHashCode();
			return hash;
		}

		/// <summary>
		/// Determines whether the specified IpAddress objects are equal.
		/// </summary>
		/// <param name="a">The first object.</param>
		/// <param name="b">The second object.</param>
		/// <returns>True if the specified objects are semantically equal;
		/// Otherwise false.</returns>
		public static bool operator ==(IpAddress a, IpAddress b) {
			if (System.Object.ReferenceEquals(a, b))
				return true;
			if (((object) a == null) || ((object) b == null))
				return false;
			for (int i = 0; i < a.address.Length; i++)
				if (a.address[i] != b.address[i])
					return false;
			return true;
		}

		/// <summary>
		/// Determines whether the specified IpAddress objects are unequal.
		/// </summary>
		/// <param name="a">The first object.</param>
		/// <param name="b">The second object.</param>
		/// <returns>True if the specified objects are not semantically equal;
		/// Otherwise false.</returns>
		public static bool operator !=(IpAddress a, IpAddress b) {
			return !(a == b);
		}

		/// <summary>
		/// Performs a bit-wise OR of the specified IpAddress objects.
		/// </summary>
		/// <param name="a">The first object.</param>
		/// <param name="b">The second object.</param>
		/// <returns>An IpAddress instance obtained by bit-wise ORing the
		/// two specified IP addresses.</returns>
		public static IpAddress operator &(IpAddress a, IpAddress b) {
			byte[] bytes = new byte[a.Bytes.Length];
			for (int i = 0; i < a.Bytes.Length; i++)
				bytes[i] = (byte) (a.Bytes[i] & b.Bytes[i]);
			return new IpAddress(bytes);
		}

		/// <summary>
		/// Parses an IPv4 address from the specified string.
		/// </summary>
		/// <param name="address">A string containing an IPv4 address in the
		/// dot-decimal notation.</param>
		/// <returns>The IPv4 address as an array of 4 bytes.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the address
		/// argument is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the specified string
		/// does not contain a valid IP-Address in dot-decimal notation.
		/// </exception>
		public static byte[] Parse(string address) {
			address.ThrowIfNull("address");
			string[] p = address.Split('.');
			if (p.Length != 4)
				throw new ArgumentException("Invalid IP address.");
			byte[] addr = new byte[4];
			try {
				for (int i = 0; i < p.Length; i++)
					addr[i] = Byte.Parse(p[i], NumberStyles.None);
			} catch {
				throw new ArgumentException("Invalid IP address.");
			}
			return addr;
		}

		/// <summary>
		/// Parses an IP address/netmask combination in CIDR notation.
		/// </summary>
		/// <param name="ipAddress">The network id in CIDR notation.</param>
		/// <returns>A tuple containing the IP address as well as subnet mask
		/// constructed from the parsed CIDR string.</returns>
		public static Tuple<IpAddress, IpAddress> ParseCIDRNotation(string ipAddress) {
			ipAddress.ThrowIfNull("ipAddress");
			string[] p = ipAddress.Split('/');
			if (p.Length != 2)
				throw new ArgumentException("Invalid CIDR notation.");
			int bits = Int32.Parse(p[1]), mask = 0;
			for (int i = 31; bits > 0; i--, bits--)
				mask |= (1 << i);
			return new Tuple<IpAddress, IpAddress>(new IpAddress(p[0]),
				new IpAddress(mask));
		}
	}
}
