
namespace Network.Sim.Miscellaneous {
	/// <summary>
	/// Implements a very simple union-find data structure.
	/// </summary>
	/// <remarks>Cmp. "Algorithms, Fourth Edition", R. Sedgewick, AW.</remarks>
	public class UnionFind {
		/// <summary>
		/// The array of identifiers.
		/// </summary>
		int[] id;

		/// <summary>
		/// The number of connected components.
		/// </summary>
		public int Count {
			get;
			private set;
		}

		/// <summary>
		/// Initializes a new instance of the UnionFind class with the
		/// specified number of elements.
		/// </summary>
		/// <param name="size">The number of isolated components within the
		/// union-find structure.</param>
		public UnionFind(int size) {
			id = new int[size];
			Count = size;
			for (int i = 0; i < size; i++)
				id[i] = i;
		}

		/// <summary>
		/// Determines whether the specified elements are in the same component.
		/// </summary>
		/// <param name="p">The first element.</param>
		/// <param name="q">The second element.</param>
		/// <returns>true if both elements are in the same component; Otherwise
		/// false.</returns>
		public bool Connected(int p, int q) {
			return id[p] == id[q];
		}

		/// <summary>
		/// Merges the components containing the specified elements into a
		/// single component.
		/// </summary>
		/// <param name="p">The first element.</param>
		/// <param name="q">The second element.</param>
		public void Union(int p, int q) {
			if (Connected(p, q))
				return;
			int pid = id[p];
			for (int i = 0; i < id.Length; i++) {
				if (id[i] == pid)
					id[i] = id[q];
			}
			Count--;
		}
	}
}
