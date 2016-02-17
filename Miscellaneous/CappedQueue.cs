using System;
using System.Collections.Generic;

namespace Network.Sim.Miscellaneous {
	/// <summary>
	/// Represents a first-in, first-out collection of objects, optionally with
	/// a maximum threshold.
	/// </summary>
	/// <typeparam name="T">The type of the objects stord in the
	/// queue.</typeparam>
	/// <remarks>Unfortunately, the .Net Framework's queue does not provide
	/// a means to specify a maximum threshold.</remarks>
	public class CappedQueue<T> : IEnumerable<T> {
		/// <summary>
		/// Gets the maximum number of elements the queue can store.
		/// </summary>
		public int MaxCapacity {
			get;
			private set;
		}

		/// <summary>
		/// Gets the number of elements contained in the queue.
		/// </summary>
		public int Count {
			get {
				return queue.Count;
			}
		}

		/// <summary>
		/// The underlying datatype for storing the elements.
		/// </summary>
		readonly Queue<T> queue = new Queue<T>();

		/// <summary>
		/// Initializes a new instance of the CappedQueue class.
		/// </summary>
		/// <param name="maxCapacity">The maximum number of elements the
		/// queue can store.</param>
		public CappedQueue(int maxCapacity = int.MaxValue) {
			MaxCapacity = maxCapacity;
		}

		/// <summary>
		/// Adds an object to the end of the queue.
		/// </summary>
		/// <param name="item">The element to add to the end of the
		/// queue.</param>
		/// <exception cref="InvalidOperationException">Thrown if the specified
		/// element could not be added to the queue because the maximum capacity
		/// has been reached.</exception>
		public void Enqueue(T item) {
			if (queue.Count >= MaxCapacity)
				throw new InvalidOperationException("The queue is full.");
			queue.Enqueue(item);
		}

		/// <summary>
		/// Removes and returns the element at the beginning of the queue.
		/// </summary>
		/// <returns>The object that is removed from the beginning of the
		/// queue.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the queue
		/// is empty.</exception>
		public T Dequeue() {
			return queue.Dequeue();
		}

		/// <summary>
		/// Returns the element at the beginning of the queue without removing
		/// it.
		/// </summary>
		/// <returns>The object at the beginning of the queue.</returns>
		/// <exception cref="InvalidOperationException">Thrown if the queue
		/// is empty.</exception>
		public T Peek() {
			return queue.Peek();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the queue.
		/// </summary>
		/// <returns>An enumerator that iterates through the queue.</returns>
		public IEnumerator<T> GetEnumerator() {
			return queue.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through the queue.
		/// </summary>
		/// <returns>An iterator that iterates through the queue.</returns>
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return queue.GetEnumerator();
		}
	}
}
