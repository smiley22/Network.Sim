using System;
using System.IO;

namespace Network.Sim {
	/// <summary>
	/// Adds a couple of useful extensions to reference types.
	/// </summary>
	internal static class Extensions {
		/// <summary>
		/// Throws an ArgumentNullException if the given data item is null.
		/// </summary>
		/// <param name="data">The item to check for nullity.</param>
		/// <param name="name">The name to use when throwing an
		/// exception, if necessary</param>
		public static void ThrowIfNull<T>(this T data, string name)
			where T : class {
			if (data == null)
				throw new ArgumentNullException(name);
		}

		/// <summary>
		/// Throws an ArgumentNullException if the given data item is null.
		/// </summary>
		/// <param name="data">The item to check for nullity.</param>
		public static void ThrowIfNull<T>(this T data)
			where T : class {
			if (data == null)
				throw new ArgumentNullException();
		}

		/// <summary>
		/// Throws an ArgumentException if the given string is null or
		/// empty.
		/// </summary>
		/// <param name="data">The string to check for nullity and
		/// emptiness.</param>
		public static void ThrowIfNullOrEmpty(this string data) {
			if (String.IsNullOrEmpty(data))
				throw new ArgumentException();
		}

		/// <summary>
		/// Throws an ArgumentException if the given string is null or
		/// empty.
		/// </summary>
		/// <param name="data">The string to check for nullity and
		/// emptiness.</param>
		/// <param name="name">The name to use when throwing an
		/// exception, if necessary</param>
		public static void ThrowIfNullOrEmpty(this string data, string name) {
			if (String.IsNullOrEmpty(data))
				throw new ArgumentException("The " + name +
					" parameter must not be null or empty");
		}

		/// <summary>
		/// Reads and returns all remaining bytes from the underyling
		/// stream.
		/// </summary>
		/// <param name="reader">Extension method for BinaryReader.</param>
		/// <returns>An array containing all remaining bytes read from the
		/// underlying stream.</returns>
		public static byte[] ReadAllBytes(this BinaryReader reader) {
			const int bufferSize = 4096;
			using (var ms = new MemoryStream()) {
				byte[] buffer = new byte[bufferSize];
				int count;
				while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
					ms.Write(buffer, 0, count);
				return ms.ToArray();
			}
		}

		/// <summary>
		/// Raises the event in a thread-safe manner.
		/// </summary>
		/// <param name="event">Extension method for EventHandler.</param>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event arguments.</param>
		static public void RaiseEvent(this EventHandler @event, object sender, EventArgs e) {
			if (@event != null)
				@event(sender, e);
		}

		/// <summary>
		/// Raises the event in a thread-safe manner.
		/// </summary>
		/// <param name="event">Extension method for EventHandler.</param>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The event arguments.</param>
		static public void RaiseEvent<T>(this EventHandler<T> @event, object sender, T e)
				where T : EventArgs {
			if (@event != null)
				@event(sender, e);
		}

		/// <summary>
		/// Returns the selected elements of the array as a new array instance.
		/// </summary>
		/// <typeparam name="T">The type of the elements contained in the array.</typeparam>
		/// <param name="data">Extension method for Array.</param>
		/// <param name="index">The index at which to start the selection.</param>
		/// <param name="length">The number of elements to extract starting from the
		/// index position.</param>
		/// <returns>A new array instance containing the requested number of
		/// elements.</returns>
		public static T[] Slice<T>(this T[] data, int index, int? length = null) {
			int len = length.HasValue ? length.Value : (data.Length - index);
			T[] result = new T[len];
			if (len > 0)
				Array.Copy(data, index, result, 0, len);
			return result;
		}
	}
}
