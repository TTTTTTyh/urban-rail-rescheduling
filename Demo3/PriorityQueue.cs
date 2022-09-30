using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Demo3
{
    public class PriorityQueue<T>
    {
        Func<T, T, int> comparer;

        public T[] heap;

        public int Count { get; private set; }

        public PriorityQueue() : this(null) { }
        public PriorityQueue(int capacity) : this(capacity, null) { }
        public PriorityQueue(Func<T, T, int> comparer) : this(16, comparer) { }

        public PriorityQueue(int capacity, Func<T, T, int> comparer)
        {
            this.comparer = (comparer == null) ? Comparer<T>.Default.Compare : comparer;
            this.heap = new T[capacity];
        }
        public void Clear()
        {
            Array.Clear(heap);
            Count = 0;
        }
        public void Enqueue(T v)
        {
            if (Count >= heap.Length) Array.Resize(ref heap, Count * 2);
            heap[Count] = v;
            SiftUp(Count++);
        }

        public T Dequeue()
        {
            var v = Peek();
            heap[0] = heap[--Count];
            if (Count > 0) SiftDown(0);
            return v;
        }

        public T Peek()
        {
            if (Count > 0) return heap[0];
            throw new InvalidOperationException("优先队列为空");
        }

        void SiftUp(int n)
        {
            var v = heap[n];
            for (var n2 = n / 2; n > 0 && comparer(v, heap[n2]) > 0; n = n2, n2 /= 2) heap[n] = heap[n2];
            heap[n] = v;
        }

        void SiftDown(int n)
        {
            var v = heap[n];
            for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
            {
                if (n2 + 1 < Count && comparer(heap[n2 + 1], heap[n2]) > 0) n2++;
                if (comparer(v, heap[n2]) >= 0) break;
                heap[n] = heap[n2];
            }
            heap[n] = v;
        }

        public void Remove(T t)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (heap[i].Equals(t))
                {
                    heap[i] = heap[--Count];
                    SiftDown(i);
                    return;
                }
            }
        }

    }
    public class EventPQ
    {
        List<Train> _heap = new List<Train>();
        Dictionary<Train, int> _ref = new Dictionary<Train, int>();
        //int _size;
        void SiftUp(int i)
        {
            int j = (i - 1) / 2;
            while (j > 0)
            {
                if (_heap[j].NowEvent < _heap[i].NowEvent)
                {
                    break;
                }
                Swap(i, j);
                i = j;j = (i - 1) / 2;
            }
        }
        void Swap(int i,int j)
        {
            var temp = _heap[i];
            _heap[i] = _heap[j];
            _heap[j] = temp;
            _ref[_heap[i]] = i;
            _ref[_heap[j]] = j;
        }
        bool SiftDown(int idx,int n)
        {
            int i = idx;
            int j = i * 2 + 1;
            while (j < n)
            {
                if (j + 1 < n && _heap[j + 1].NowEvent < _heap[j].NowEvent) ++j;
                if (_heap[i].NowEvent < _heap[j].NowEvent) break;
                Swap(i, j);
                i = j;
                j = i * 2 + 1;
            }
            return i > idx;
        }
        public int Count { get { return _heap.Count; } }
        public TrainEventNode Dequeue()
        {
            Debug.Assert(Count > 0);
            var res = _heap[0];
            Remove(_heap[0]);
            return res.NowEvent;
        }
        public TrainEventNode Peek()
        {
            return _heap[0].NowEvent;
        }
        public void Enqueue(TrainEventNode node)
        {
            var train = node.train;
            train.NowEvent = node;
            if (_ref.ContainsKey(train))
            {
                int i = _ref[train];
                if (!SiftDown(i, _heap.Count))
                {
                    SiftUp(i);
                }
            }
            else
            {
                int i = _heap.Count;
                _ref[train]=i;
                _heap.Add(train);
                SiftUp(i);
            }
        }
        public bool IsInqueue(Train train)
        {
            return _ref.ContainsKey(train);
        }
        public void Adjust(TrainEventNode nowEvent)
        {
            Enqueue(nowEvent);
        }
        public void Remove(Train train)
        {
            int i = _ref[train];
            int n= _heap.Count-1;
            if (i < n)
            {
                Swap(i, n);
                if (!SiftDown(i, n))
                {
                    SiftUp(i);
                }
            }
            _ref.Remove(train);
            _heap.RemoveAt(_heap.Count-1);
        }

        public void RemainOne()
        {
            while (Count > 1)
            {
                Dequeue();
            }
        }
    }
}
