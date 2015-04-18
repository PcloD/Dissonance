using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


// Based on http://stackoverflow.com/questions/10983110/a-star-a-and-generic-find-method

class PriorityQueue<V, P> {
    private SortedDictionary<P, Queue<V>> list = new SortedDictionary<P, Queue<V>>();
    // private HashSet<V> values;
    public void Enqueue(V value, P priority) {
        Queue<V> q;
        if (!list.TryGetValue(priority, out q)) {
            q = new Queue<V>();
            list.Add(priority, q);
        }
        q.Enqueue(value);
        // values.Add(value);
    }

    public V Dequeue() {
        // will throw if there isn’t any first element!
        var pair = list.First();
        var v = pair.Value.Dequeue();
        if (pair.Value.Count == 0) // nothing left of the top priority.
            list.Remove(pair.Key);
        return v;
    }

    public V Peek () {
      return list.First().Value.Peek();
    }

    public bool IsEmpty {
        get { return list.Count == 0; }
    }
}

/*

// Based on http://visualstudiomagazine.com/Articles/2012/11/01/Priority-Queues-with-C.aspx?Page=2
public class PriorityQueue<T> where T : System.IEquatable<T> {
    private List<CostNode> data;

    public PriorityQueue() {
      this.data = new List<CostNode>();
    }

    private struct CostNode {
      public float cost;
      public T value;
      public CostNode (T v, float c) {
        cost = c;
        value = v;
      }
    }

    public void Enqueue(T item, float cost) {
      CostNode node = new CostNode(item, cost);
      data.Add(node);
      int ci = data.Count - 1; // child index; start at end
      while (ci > 0) {
        int pi = (ci - 1) / 2; // parent index
        if (data[ci].cost >= data[pi].cost) break; // child item is larger than (or equal) parent so we're done
        // if (data[ci].cost <= data[pi].cost) break;
        CostNode tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
        ci = pi;
      }
    }

    public T Dequeue() {
      // assumes pq is not empty; up to calling code
      int li = data.Count - 1; // last index (before removal)
      CostNode frontItem = data[0];   // fetch the front
      data[0] = data[li];
      data.RemoveAt(li);

      --li; // last index (after removal)
      int pi = 0; // parent index. start at front of pq
      while (true) {
        int ci = pi * 2 + 1; // left child index of parent
        if (ci > li) break;  // no children so done
        int rc = ci + 1;     // right child
        if (rc <= li && data[rc].cost < data[ci].cost) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
          ci = rc;
        if (data[pi].cost <= data[ci].cost) break; // parent is smaller than (or equal to) smallest child so done
        // if (data[pi].cost >= data[ci].cost) break;
        CostNode tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
        pi = ci;
      }
      return frontItem.value;
    }

    public T Peek() {
      CostNode frontItem = data[0];
      return frontItem.value;
    }

    public int Count() {
      return data.Count;
    }

    public bool Contains (T item) {
      for (int i = 0; i < this.data.Count; i++) {
        if ((System.IEquatable<T>)this.data[i].value == (System.IEquatable<T>)item) {
          return true;
        }
      }
      return false;
    }

    public bool Empty {
      get { return data.Count == 0; }
    }

    public override string ToString() {
      string s = "";
      for (int i = 0; i < data.Count; ++i) {
        s += data[i].ToString() + " ";
      }
      s += "count = " + data.Count;
      return s;
    }

    public bool IsConsistent() {
      // is the heap property true for all data?
      if (data.Count == 0) { return true; }
      int li = data.Count - 1; // last index
      for (int pi = 0; pi < data.Count; ++pi) { // each parent index
        int lci = 2 * pi + 1; // left child index
        int rci = 2 * pi + 2; // right child index

        if (lci <= li && data[pi].cost > data[lci].cost) return false; // if lc exists and it's greater than parent then bad.
        if (rci <= li && data[pi].cost > data[rci].cost) return false; // check the right child too.
      }
      return true; // passed all checks
    } // IsConsistent
} // PriorityQueue

*/