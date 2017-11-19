using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public sealed class SafedQueue<T> {
    private Queue<T> queue;

    public int Count {
        get {
            return queue.Count;
        }
    }

    public SafedQueue() {
        queue = new Queue<T>();
    }

    public SafedQueue(IEnumerable<T> es) {
        queue = new Queue<T>(es);
    }

    public void Enqueue(T t) {
        lock (this) {
            this.queue.Enqueue(t);
        }
    }

    public void EnqueueRange(IEnumerable<T> ts) {
        lock (this) {
            foreach(T t in ts) {
                this.queue.Enqueue(t);
            }
        }
    }

    public bool TryDequeue(out T v) {
        lock (this) {
            if(this.queue.Count > 0) {
                v = this.queue.Dequeue();
                return true;
            } else {
                v = default(T);
                return false;
            }
        }
    }

    public bool TryDequeue() {
        lock (this) {
            if(this.queue.Count > 0) {
                this.queue.Dequeue();
                return true;
            } else {
                return false;
            }
        }
    }

    public bool TryPeek(out T v) {
        lock (this) {
            if(this.queue.Count > 0) {
                v = this.queue.Peek();
                return true;
            } else {
                v = default(T);
                return false;
            }
        }
    }

    public void Clear() {
        lock (this) {
            this.queue.Clear();
        }
    }

    public bool Any(Func<T, bool> predicate) {
        lock (this) {
            return this.queue.Any(predicate);
        }
    }

    public T[] ToArray() {
        lock (this) {
            return this.queue.ToArray();
        }
    }

    public void Foreach(Action<T> act) {
        lock (this) {
            foreach(T t in this.queue) {
                act(t);
            }
        }
    }
}