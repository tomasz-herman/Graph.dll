using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASD.Graphs
{
    public class RealPriorityQueue<TPriority, TValue> where TPriority: IComparable
    {
        //TODO
        
        //The one below is shit
    }
    
    
    [Serializable]
    public class PriorityQueue<TKey, TPriority>
    {
        private List<KeyValuePair<TKey, TPriority>> elements;
        private HashTable<TKey, int> index;
        [NonSerialized]
        private Func<KeyValuePair<TKey, TPriority>, KeyValuePair<TKey, TPriority>, bool> cmp;
        private string serializedCmp;
        [NonSerialized]
        private Action access;
        private string serializedAccess;
        
        public PriorityQueue(Func<KeyValuePair<TKey, TPriority>, KeyValuePair<TKey, TPriority>, bool> cmp, Action access = null)
        {
            this.cmp = cmp;
            this.access = access ?? CMonDoSomething.Nothing;
            elements = new List<KeyValuePair<TKey, TPriority>>();
            index = new HashTable<TKey, int>(access);
        }
        
        public bool Empty => elements.Count == 0;

        public int Count => elements.Count;

        public bool Contains(TKey k)
        {
            return index.Search(k, out _);
        }

        public bool Put(TKey k, TPriority p)
        {
            if (Contains(k))
                return false;
            var keyValuePair = new KeyValuePair<TKey, TPriority>(k, p);
            elements.Add(keyValuePair);
            var num = elements.Count - 1;
            var i = (elements.Count - 2) >> 1;
            while (i >= 0 && cmp(keyValuePair, elements[i]))
            {
                elements[num] = elements[i];
                index[elements[num].Key] = num;
                num = i;
                i = num - 1 >> 1;
                access();
            }
            elements[num] = keyValuePair;
            index[k] = num;
            access();
            return true;
        }

        public TKey Get()
        {
            var key = elements[0].Key;
            index.Remove(key);
            var keyValuePair = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            var num = 0;
            var i = 1;
            while (i < elements.Count)
            {
                if (i + 1 < elements.Count)
                    if (cmp(elements[i + 1], elements[i]))
                        i++;
                access();
                if (!cmp(elements[i], keyValuePair))
                    break;
                elements[num] = elements[i];
                index[elements[num].Key] = num;
                num = i;
                i = (num << 1) + 1;
                access();
            }

            if (elements.Count <= 0) return key;
            elements[num] = keyValuePair;
            index[keyValuePair.Key] = num;
            access();
            return key;
        }

        public TKey Peek()
        {
            access();
            return elements[0].Key;
        }

        public TPriority BestPriority()
        {
            access();
            return elements[0].Value;
        }

        public bool ImprovePriority(TKey k, TPriority p)
        {
            var keyValuePair = new KeyValuePair<TKey, TPriority>(k, p);
            var num = index[k];
            access();
            if (!cmp(keyValuePair, elements[num]))
                return false;
            var i = num - 1 >> 1;
            while (i >= 0)
            {
                if (!cmp(keyValuePair, elements[i]))
                {
                    break;
                }
                elements[num] = elements[i];
                index[elements[num].Key] = num;
                num = i;
                i = num - 1 >> 1;
                access();
            }
            elements[num] = keyValuePair;
            index[k] = num;
            access();
            return true;
        }

        public TKey[] ToArray()
        {
            var array = new TKey[elements.Count];
            for (var i = 0; i < elements.Count; i++)
            {
                array[i] = elements[i].Key;
                access();
            }
            return array;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext streamingContext)
        {
            serializedCmp = DelegateSerializationHelper.Serialize(cmp);
            serializedAccess = DelegateSerializationHelper.Serialize(access);
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext streamingContext)
        {
            serializedCmp = null;
            serializedAccess = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            cmp = (Func<KeyValuePair<TKey, TPriority>, KeyValuePair<TKey, TPriority>, bool>)DelegateSerializationHelper.Deserialize(serializedCmp);
            access = (Action)DelegateSerializationHelper.Deserialize(serializedAccess);
            serializedCmp = null;
            serializedAccess = null;
        }

    }
}
