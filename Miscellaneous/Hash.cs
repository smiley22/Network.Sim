using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Network.Sim.Miscellaneous {
	/// <summary>
	/// Provides various hashing utility methods.
	/// </summary>
	public static class Hash {
		/// <summary>
		/// Calculates the SHA-256 hash value for the specified string.
		/// </summary>
		/// <param name="s">The string to calculate the SHA-256 hash value for.</param>
		/// <returns>A SHA-256 hash value.</returns>
		/// <exception cref="ArgumentException">Thrown if the input string is
		/// null.</exception>
		public static string Sha256(string s) {
			if (s == null)
				throw new ArgumentException("input string must not be null");
			var bytes = Encoding.UTF8.GetBytes(s);
			return Sha256(bytes);
		}

		/// <summary>
		/// Calculates the SHA-256 hash value for the specified byte array.
		/// </summary>
		/// <param name="bytes">The byte array to calculcate the SHA-256 hash
		/// value for.</param>
		/// <returns>A SHA-256 hash value.</returns>
		/// <exception cref="ArgumentException">Thrown if the bytes parameter
		/// is null.</exception>
		public static string Sha256(byte[] bytes) {
			var hash = new SHA256Managed().ComputeHash(bytes);
			var builder = new StringBuilder();
			foreach (var h in hash)
				builder.Append(h.ToString("x2"));
			return builder.ToString();
		}

		/// <summary>
		/// Calculates the SHA-256 hash value for the specified input stream.
		/// </summary>
		/// <param name="stream">A stream to calculate the SHA-256 hash value
		/// for.</param>
		/// <returns>A SHA-256 hash value.</returns>
		/// <exception cref="ArgumentException">Thrown if the stream parameter
		/// is null.</exception>
		public static string Sha256(Stream stream) {
			var hash = new SHA256Managed().ComputeHash(stream);
			var builder = new StringBuilder();
			foreach (var h in hash)
				builder.Append(h.ToString("x2"));
			return builder.ToString();
		}
	}
}