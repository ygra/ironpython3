// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace IronPython.Runtime {

    public class ListGenericWrapper<T> : IList<T> {
        private readonly IList<object> _value;

        public ListGenericWrapper(IList<object> value) { _value = value; }

        #region IList<T> Members

        public int IndexOf(T item) {
            return _value.IndexOf(item);
        }

        public void Insert(int index, T item) {
            _value.Insert(index, item);
        }

        public void RemoveAt(int index) {
            _value.RemoveAt(index);
        }

        public T this[int index] {
            get => (T)_value[index];
            set => _value[index] = value;
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item) {
            _value.Add(item);
        }

        public void Clear() {
            _value.Clear();
        }

        public bool Contains(T item) {
            return _value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            for (int i = 0; i < _value.Count; i++) {
                array[arrayIndex + i] = (T)_value[i];
            }
        }

        public int Count {
            get { return _value.Count; }
        }

        public bool IsReadOnly {
            get { return _value.IsReadOnly; }
        }

        public bool Remove(T item) {
            return _value.Remove(item);
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            return new IEnumeratorOfTWrapper<T>(_value.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return _value.GetEnumerator();
        }

        #endregion
    }

    public class DictionaryGenericWrapper<K, V> : IDictionary<K, V> {
        private readonly IDictionary<object, object> self;

        public DictionaryGenericWrapper(IDictionary<object, object> self) {
            this.self = self;
        }

        #region IDictionary<K,V> Members

        public void Add(K key, V value) {
            self.Add(key, value);
        }

        public bool ContainsKey(K key) {
            return self.ContainsKey(key);
        }

        public ICollection<K> Keys {
            get {
                List<K> res = new List<K>();
                foreach (object o in self.Keys) {
                    res.Add((K)o);
                }
                return res;
            }
        }

        public bool Remove(K key) {
            return self.Remove(key);
        }

        public bool TryGetValue(K key, out V value) {
            if (self.TryGetValue(key, out object outValue)) {
                value = (V)outValue;
                return true;
            }
            value = default;
            return false;
        }

        public ICollection<V> Values {
            get {
                List<V> res = new List<V>();
                foreach (object o in self.Values) {
                    res.Add((V)o);
                }
                return res;
            }
        }

        public V this[K key] {
            get {
                return (V)self[key];
            }
            set {
                self[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<K,V>> Members

        public void Add(KeyValuePair<K, V> item) {
            self.Add(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        public void Clear() {
            self.Clear();
        }

        public bool Contains(KeyValuePair<K, V> item) {
            return self.Contains(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) {
            foreach (KeyValuePair<K, V> kvp in this) {
                array[arrayIndex++] = kvp;
            }
        }

        public int Count {
            get { return self.Count; }
        }

        public bool IsReadOnly {
            get { return self.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<K, V> item) {
            return self.Remove(new KeyValuePair<object, object>(item.Key, item.Value));
        }

        #endregion

        #region IEnumerable<KeyValuePair<K,V>> Members

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() {
            foreach (KeyValuePair<object, object> kv in self) {
                yield return new KeyValuePair<K, V>((K)kv.Key, (V)kv.Value);
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return self.GetEnumerator();
        }

        #endregion
    }

    public class IEnumeratorOfTWrapper<T> : IEnumerator<T> {
        private readonly IEnumerator enumerable;

        public IEnumeratorOfTWrapper(IEnumerator enumerable) {
            this.enumerable = enumerable;
        }

        #region IEnumerator<T> Members
        public T Current {
            get {
                try {
                    return (T)enumerable.Current;
                } catch (System.InvalidCastException iex) {
                    throw new System.InvalidCastException(string.Format("Error in IEnumeratorOfTWrapper.Current. Could not cast: {0} in {1}", typeof(T).ToString(), enumerable.Current.GetType().ToString()), iex);
                }
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose() {
        }

        #endregion

        #region IEnumerator Members

        object IEnumerator.Current {
            get { return enumerable.Current; }
        }

        public bool MoveNext() {
            return enumerable.MoveNext();
        }

        public void Reset() {
            enumerable.Reset();
        }

        #endregion
    }

    [PythonType("enumerable_wrapper")]
    public class IEnumerableOfTWrapper<T> : IEnumerable<T>, IEnumerable {
        private readonly IEnumerable enumerable;

        public IEnumerableOfTWrapper(IEnumerable enumerable) {
            this.enumerable = enumerable;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator() {
            return new IEnumeratorOfTWrapper<T>(enumerable.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }

#nullable enable

    public sealed class MemoryBufferWrapper : IPythonBuffer {
        private readonly ReadOnlyMemory<byte> _rom;
        private readonly Memory<byte>? _memory;
        private readonly BufferFlags _flags;

        public MemoryBufferWrapper(ReadOnlyMemory<byte> memory, BufferFlags flags) {
            _rom = memory;
            _memory = null;
            _flags = flags;
        }

        public MemoryBufferWrapper(Memory<byte> memory, BufferFlags flags) {
            _rom = memory;
            _memory = memory;
            _flags = flags;
        }

        public void Dispose() { }

        public object Object => _memory ?? _rom;

        public bool IsReadOnly => !_memory.HasValue;

        public ReadOnlySpan<byte> AsReadOnlySpan() => _rom.Span;

        public Span<byte> AsSpan() => _memory.HasValue ? _memory.Value.Span : throw new InvalidOperationException("ReadOnlyMemory is not writable");

        public int Offset => 0;

        public string? Format => _flags.HasFlag(BufferFlags.Format) ? "B" : null;

        public int ItemCount => _rom.Length;

        public int ItemSize => 1;

        public int NumOfDims => 1;

        public IReadOnlyList<int>? Shape => null;

        public IReadOnlyList<int>? Strides => null;

        public IReadOnlyList<int>? SubOffsets => null;
    }

    public class MemoryBufferProtocolWrapper : IBufferProtocol {
        private readonly ReadOnlyMemory<byte> _rom;
        private readonly Memory<byte>? _memory;

        public MemoryBufferProtocolWrapper(ReadOnlyMemory<byte> memory) {
            _rom = memory;
            _memory = null;
        }

        public MemoryBufferProtocolWrapper(Memory<byte> memory) {
            _rom = memory;
            _memory = memory;
        }

        public IPythonBuffer GetBuffer(BufferFlags flags) {
            if (_memory.HasValue) {
                return new MemoryBufferWrapper(_memory.Value, flags);
            }

            if (flags.HasFlag(BufferFlags.Writable)) {
                throw Operations.PythonOps.BufferError("ReadOnlyMemory is not writable.");
            }

            return new MemoryBufferWrapper(_rom, flags);
        }
    }

#nullable restore
}
