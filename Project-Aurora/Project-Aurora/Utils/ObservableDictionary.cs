using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Aurora.Utils {

    /// <summary>
    /// Class that behaves as a Dictionary but has events that notify when the collection changes.
    /// </summary>
    public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged {
        
        /// <summary>Event that fires when items are added, removed or changed in the collection.
        /// <para>Note that properties changed on child items are NOTE notified (as they wouldn't with an ObservableCollection).</para></summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>Event that fires when one of the ObservableDictionary's properties changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>The private internal dictionary used as the store.</summary>
        private readonly Dictionary<TKey, TValue> internalDict;

        #region Constructors
        /// <summary>Creates a new, empty <see cref="ObservableDictionary{TKey, TValue}"/>.</summary>
        public ObservableDictionary() { internalDict = new Dictionary<TKey, TValue>(); }
        /// <summary>Creates an <see cref="ObservableDictionary{TKey, TValue}"/> by cloning the values from the given dictionary-like value.</summary>
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dictionaryLike) { internalDict = dictionaryLike.ToDictionary(kvp => kvp.Key, kvp => kvp.Value); }
        #endregion
        
        #region Properties
        /// <summary>Gets or sets the value in the dictionary with the given key.</summary>
        public TValue this[TKey key] {
            get => internalDict[key];
            set {
                // If the key is new, add it to the dictionary and notify changed with "Add"
                if (!internalDict.ContainsKey(key)) {
                    internalDict[key] = value;
                    NotifyCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
                }

                // Else if the key isn't new but the value is different, update and notify changed with "Replace"
                if (!internalDict[key].Equals(value)) {
                    var old = internalDict[key];
                    internalDict[key] = value;
                    NotifyCollectionChangedChange(key, value, old);
                }
            }
        }

        /// <summary>Gets an <see cref="ICollection{T}"/> containing all the keys of the <see cref="ObservableDictionary{TKey, TValue}"/>.</summary>
        public ICollection<TKey> Keys => internalDict.Keys;

        /// <summary>Gets an <see cref="ICollection{T}"/> containing all the values of the <see cref="ObservableDictionary{TKey, TValue}"/>.</summary>
        public ICollection<TValue> Values => internalDict.Values;

        /// <summary>Gets the number of entries in the <see cref="ObservableDictionary{TKey, TValue}"/>.</summary>
        public int Count => internalDict.Count;
        #endregion


        #region Methods
        /// <summary>Adds the specified key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.</summary>
        /// <exception cref="System.ArgumentNullException" />
        /// <exception cref="System.ArgumentException" />
        public void Add(TKey key, TValue value) {
            internalDict.Add(key, value);
            NotifyCollectionChanged(NotifyCollectionChangedAction.Add, key, value);
        }

        /// <summary>Removes all key-value pairs from the <see cref="ObservableDictionary{TKey, TValue}"/>.</summary>
        public void Clear() {
            internalDict.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            NotifyPropertyChanged("[]", "Count", "Keys", "Values");
        }

        /// <summary>Determines whether the <see cref="ObservableDictionary{TKey, TValue}"/> contains the specified key.</summary>
        public bool ContainsKey(TKey key) => internalDict.ContainsKey(key);

        /// <summary>Determines whether the <see cref="ObservableDictionary{TKey, TValue}"/> contains the specified value.</summary>
        public bool ContainsValue(TValue value) => internalDict.ContainsValue(value);
        
        /// <summary>Removes the value with the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.</summary>
        public bool Remove(TKey key) {
            var old = this[key];
            var result = internalDict.Remove(key);
            if (result) NotifyCollectionChanged(NotifyCollectionChangedAction.Remove, key, old);
            return result;
        }

        /// <summary>Gets the value associated with the specified key.</summary>
        public bool TryGetValue(TKey key, out TValue value) => internalDict.TryGetValue(key, out value);
        #endregion

        #region Event methods
        /// <summary>Invokes <see cref="PropertyChanged"/> for each of the given properties.</summary>
        private void NotifyPropertyChanged(params string[] properties) =>
            properties.ForEach(prop => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop)));

        /// <summary>Invokes <see cref="CollectionChanged"/> event and also notifies of the relevant property changes for an add, remove or clear action.</summary>
        private void NotifyCollectionChanged(NotifyCollectionChangedAction action, TKey newKey, TValue newValue) {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, new KeyValuePair<TKey, TValue>(newKey, newValue)));
            NotifyPropertyChanged("[]", "Count", "Keys", "Values");
        }

        /// <summary>Invokes <see cref="CollectionChanged"/> event and also notifies of the relevant property changes for a change action.</summary>
        private void NotifyCollectionChangedChange(TKey key, TValue newValue, TValue oldValue) {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, newValue), new KeyValuePair<TKey, TValue>(key, oldValue)));
            NotifyPropertyChanged("[]", "Values");
        }
        #endregion


        #region Interface-hidden implementations
        // These members do no appear on the ObservableCollection but can be called by casting it to one of the interfaces it implements.
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)internalDict).IsReadOnly;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)internalDict).CopyTo(array, arrayIndex);
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)internalDict).Contains(item);
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

        IEnumerator IEnumerable.GetEnumerator() => ((IDictionary<TKey, TValue>)internalDict).GetEnumerator();
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => internalDict.GetEnumerator();
        #endregion
    }
}
