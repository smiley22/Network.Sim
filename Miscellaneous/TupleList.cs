using System;
using System.Collections.Generic;

namespace Network.Sim.Miscellaneous {
	/// <summary>
	/// A list of 2-tuple elements.
	/// </summary>
	/// <typeparam name="T1">The type of the first element of the tuple.</typeparam>
	/// <typeparam name="T2">The type of the second element of the tuple.</typeparam>
	public class TupleList<T1, T2> : List<Tuple<T1, T2>> {
		/// <summary>
		/// Creates a new tuple instance from the specified values and adds it
		/// to the end of the tuple list.
		/// </summary>
		/// <param name="item">The first element of the tuple.</param>
		/// <param name="item2">The second element of the tuple.</param>
		/// <remarks>The purpose of this method is to allow for the shortcut
		/// initializer syntax to be used with tuples.</remarks>
		public void Add(T1 item, T2 item2) {
			Add(new Tuple<T1, T2>(item, item2));
		}
	}
}
