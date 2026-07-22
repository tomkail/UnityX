using System.Collections.Generic;

// Queue that dequeues automatically upon growing larger than
public class FixedSizedQueue<T> : Queue<T> {

    public int Size { get; private set; }

    public FixedSizedQueue(int size) {
        Size = size;
    }

    public new void Enqueue(T obj) {
        base.Enqueue(obj);
        while (Count > Size) {
			Dequeue();
		}
    }
}