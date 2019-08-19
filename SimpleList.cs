using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASD.Graphs
{
    /// <summary>
    /// Prosta lista elementów klucz-wartość
    /// </summary>
    /// <typeparam name="TKey">Typ kluczy elementów przechowywanych na liście</typeparam>
    /// <typeparam name="TValue">Typ wartości elementów przechowywanych na liście</typeparam>
    /// <remarks>Wartości kluczy muszą być unikalne.</remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class SimpleList<TKey, TValue> : IAbstractDictionary<TKey, TValue>
    {
        private readonly List<KeyValuePair<TKey, TValue>> _pairs;
        [NonSerialized]
        private Action _access;
        private string _serializedAccess;
        
        public SimpleList(Action access = null, int capacity = 8)
        {
            SetAccess(access);
            _pairs = new List<KeyValuePair<TKey, TValue>>(capacity);
        }
        
        public int Count => _pairs.Count;

        public void SetAccess(Action access)
        {
            _access = access ?? CMonDoSomething.Nothing;
        }
        
        internal void Insert(KeyValuePair<TKey, TValue> keyValuePair_0)
        {
            Insert(keyValuePair_0.Key, keyValuePair_0.Value);
        }

        public bool Insert(TKey k, TValue v)
        {
            foreach (var t in _pairs)
            {
                _access();
                if (t.Key.Equals(k)) return false;
            }
            _access();
            _pairs.Add(new KeyValuePair<TKey, TValue>(k, v));
            return true;
        }

        public bool Remove(TKey k)
        {
            for (var i = 0; i < _pairs.Count; i++)
            {
                _access();
                if (!_pairs[i].Key.Equals(k)) continue;
                _pairs.RemoveAt(i);
                return true;
            }
            return false;
        }

        public bool Modify(TKey key, TValue value)
        {
            for (var i = 0; i < _pairs.Count; i++)
            {
                _access();
                if (!_pairs[i].Key.Equals(key)) continue;
                _pairs[i] = new KeyValuePair<TKey, TValue>(key, value);
                return true;
            }
            return false;
        }

        public bool Search(TKey key, out TValue value)
        {
            foreach (var t in _pairs)
            {
                _access();
                if (!t.Key.Equals(key)) continue;
                value = t.Value;
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _pairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext streamingContext)
        {
            _serializedAccess = DelegateSerializationHelper.Serialize(_access);
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext streamingContext)
        {
            _serializedAccess = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            _access = (Action) DelegateSerializationHelper.Deserialize(_serializedAccess);
            _serializedAccess = null;
        }
    }
}
