using System;
using System.Globalization;
using System.Text;

namespace Network.Sim.Link {
	/// <summary>
	/// Represents a 48-bit MAC address.
	/// </summary>
	public class MacAddress {
		static readonly Random random = new Random();

		/// <summary>
		/// The byte array storing the address.
		/// </summary>
		readonly byte[] address = new byte[6];

		/// <summary>
		/// The MAC address as an array of bytes.
		/// </summary>
		public byte[] Bytes {
			get {
				return address;
			}
		}

		/// <summary>
		/// Initializes a new instance of the MacAddress class, generating a
		/// random 6-byte MAC address.
		/// </summary>
		public MacAddress() {
			random.NextBytes(address);
		}

		/// <summary>
		/// Initializes a new instance of the MacAddress class using the specified
		/// byte array.
		/// </summary>
		/// <param name="address">An array containing 6 bytes to initialize this
		/// instance with.</param>
		/// <exception cref="ArgumentNullException">thrown if the address argument
		/// is null.</exception>
		public MacAddress(byte[] address) {
			address.ThrowIfNull("address");
			if (address.Length != 6)
				throw new ArgumentException("The array must have a size of 6 bytes.",
					nameof(address));
			for (var i = 0; i < address.Length; i++)
				this.address[i] = address[i];
		}

		/// <summary>
		/// Initializes a new instance of the MacAddress class using the specified
		/// string.
		/// </summary>
		/// <param name="address">A MAC address as a string of six groups of two
		/// hexadecimal digits, separated by hyphens or colons.</param>
		public MacAddress(string address) {
			this.address = Parse(address);
		}

		/// <summary>
		/// Returns a textual representation of this instance.
		/// </summary>
		/// <returns>A textual representation of this MAC address.</returns>
		public override string ToString() {
			var b = new StringBuilder();
			for (var i = 0; i < address.Length; i++) {
				b.Append(address[i].ToString("X2"));
				if (i < address.Length - 1)
					b.Append(":");
			}
			return b.ToString();
		}

		/// <summary>
		/// Determines whether the specified object is equal to this MacAddress
		/// instance.
		/// </summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>True if the specified object is semantically equal to this
		/// MacAddress instance; Otherwise false.</returns>
		public override bool Equals(object obj) {
			if (obj == null)
				return false;
			var other = obj as MacAddress;
			if (other == null)
				return false;
			for (var i = 0; i < address.Length; i++) {
				if (address[i] != other.address[i])
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns the hash code of this instance.
		/// </summary>
		/// <returns>The hash code of this MacAddress instance.</returns>
		public override int GetHashCode() {
			var hash = 13;
			foreach (var b in address)
				hash = hash * 7 + b.GetHashCode();
			return hash;
		}

		/// <summary>
		/// Determines whether the specified MacAddress objects are equal.
		/// </summary>
		/// <param name="a">The first object.</param>
		/// <param name="b">The second object.</param>
		/// <returns>True if the specified objects are semantically equal;
		/// Otherwise false.</returns>
		public static bool operator ==(MacAddress a, MacAddress b) {
			if (ReferenceEquals(a, b))
				return true;
			if (((object) a == null) || ((object) b == null))
				return false;
			for (var i = 0; i < a.address.Length; i++)
				if (a.address[i] != b.address[i])
					return false;
			return true;
		}

		/// <summary>
		/// Determines whether the specified MacAddress objects are unequal.
		/// </summary>
		/// <param name="a">The first object.</param>
		/// <param name="b">The second object.</param>
		/// <returns>True if the specified objects are not semantically equal;
		/// Otherwise false.</returns>
		public static bool operator !=(MacAddress a, MacAddress b) {
			return !(a == b);
		}

		/// <summary>
		/// Parses a MAC-48 address from the specified string.
		/// </summary>
		/// <param name="address">A string containing a MAC-48 address as
		/// six groups of two hexadecimal digits, separated by hyphens or
		/// colons.</param>
		/// <returns>The MAC-48 address as an array of 6 bytes.</returns>
		/// <exception cref="ArgumentNullException">Thrown if the address
		/// argument is null.</exception>
		/// <exception cref="ArgumentException">Thrown if the specified string
		/// does not contain a valid MAC-48 address.
		/// </exception>
		private static byte[] Parse(string address) {
			address.ThrowIfNull("address");
			var p = address.Split(':', '-');
			if (p.Length != 6)
				throw new ArgumentException("Invalid MAC address.");
			var addr = new byte[6];
			try {
				for (var i = 0; i < p.Length; i++)
					addr[i] = byte.Parse(p[i], NumberStyles.HexNumber);
			} catch {
				throw new ArgumentException("Invalid MAC address.");
			}
			return addr;
		}
	}
}
