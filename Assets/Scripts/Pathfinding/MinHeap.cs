using System.Collections.Generic;

namespace Sim
{
    ///Binary min-heap priority queue. Might switch back to unity !!
    public class MinHeap<T>
    {
        readonly List<(float pri, T item)> a = new List<(float, T)>();
        public int Count => a.Count;
        public void Clear() => a.Clear();

        public void Push(T item, float priority)
        {
            a.Add((priority, item));
            int i = a.Count - 1;
            while (i > 0)
            {
                int p = (i - 1) / 2;
                if (a[p].pri <= a[i].pri) break;
                (a[p], a[i]) = (a[i], a[p]);
                i = p;
            }
        }

        public T Pop(out float priority)
        {
            var top = a[0];
            int last = a.Count - 1;
            a[0] = a[last];
            a.RemoveAt(last);
            int i = 0;
            while (true)
            {
                int l = 2 * i + 1, r = l + 1, m = i;
                if (l < a.Count && a[l].pri < a[m].pri) m = l;
                if (r < a.Count && a[r].pri < a[m].pri) m = r;
                if (m == i) break;
                (a[m], a[i]) = (a[i], a[m]);
                i = m;
            }
            priority = top.pri;
            return top.item;
        }
    }
}
