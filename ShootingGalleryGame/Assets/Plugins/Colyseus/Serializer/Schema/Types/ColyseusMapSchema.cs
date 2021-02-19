using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Colyseus.Schema
{
    /// <summary>
    ///     A <see cref="ColyseusSchema" /> dictionary of <typeparamref name="T" /> type objects
    /// </summary>
    /// <typeparam name="T">The type of object in this map</typeparam>
    public class ColyseusMapSchema<T> : IColyseusSchemaCollection
    {
        protected Dictionary<int, string> Indexes = new Dictionary<int, string>();

        /// <summary>
        ///     The contents of the <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        public OrderedDictionary items = new OrderedDictionary();

        public ColyseusMapSchema()
        {
            items = new OrderedDictionary();
        }

        public ColyseusMapSchema(OrderedDictionary items = null)
        {
            this.items = items ?? new OrderedDictionary();
        }

        /// <summary>
        ///     Accessor to get/set <see cref="items" /> with a <paramref name="key" />
        /// </summary>
        /// <param name="key"></param>
        public T this[string key]
        {
            get
            {
                T value;
                TryGetValue(key, out value);
                return value;
            }
            set { items[key] = value; }
        }

        /// <summary>
        ///     Getter for all of the keys in <see cref="items" />
        /// </summary>
        public ICollection Keys
        {
            get { return items.Keys; }
        }

        /// <summary>
        ///     Getter for all of the values in <see cref="items" />
        /// </summary>
        public ICollection Values
        {
            get { return items.Values; }
        }

        public int __refId { get; set; }

        /// <summary>
        ///     Set the <see cref="indexes" /> value
        /// </summary>
        /// <param name="index">The field index</param>
        /// <param name="dynamicIndex">The new dynamic Index value, cast to <see cref="int" /></param>
        public void SetIndex(int index, object dynamicIndex)
        {
            Indexes[index] = (string) dynamicIndex;
        }

        /// <summary>
        ///     Set an Item by it's <paramref name="dynamicIndex" />
        /// </summary>
        /// <param name="index">
        ///     Sets <see cref="Indexes" /> value at <paramref name="index" /> to <paramref name="dynamicIndex" />
        /// </param>
        /// <param name="dynamicIndex">
        ///     The index, cast to <see cref="string" />, in <see cref="items" /> that will be set to <paramref name="value" />
        /// </param>
        /// <param name="value">The new object to put into <see cref="items" /></param>
        public void SetByIndex(int index, object dynamicIndex, object value)
        {
            Indexes[index] = (string) dynamicIndex;
            items[dynamicIndex] = (T) value;
        }

        /// <summary>
        ///     Get the dynamic index value from <see cref="Indexes" />
        /// </summary>
        /// <param name="index">The location of the dynamic index to return</param>
        /// <returns>The dynamic index from <see cref="Indexes" />, if it exists. <see cref="string.Empty" /> if it does not</returns>
        public object GetIndex(int index)
        {
            string dynamicIndex;

            Indexes.TryGetValue(index, out dynamicIndex);

            return dynamicIndex;
        }

        /// <summary>
        ///     Get an item out of the <see cref="ColyseusMapSchema{T}" /> by it's index
        /// </summary>
        /// <param name="index">The index of the item</param>
        /// <returns>An object of type <typeparamref name="T" /> if it exists</returns>
        public object GetByIndex(int index)
        {
            string dynamicIndex = (string) GetIndex(index);
            return dynamicIndex != null && items.Contains(dynamicIndex)
                ? items[dynamicIndex]
                : GetTypeDefaultValue();
        }

        /// <summary>
        ///     Remove an item and it's dynamic index reference
        /// </summary>
        /// <param name="index">The index of the item</param>
        public void DeleteByIndex(int index)
        {
            string dynamicIndex = (string) GetIndex(index);
            if (items.Contains(dynamicIndex))
            {
                items.Remove(dynamicIndex);
                Indexes.Remove(index);
            }
        }

        /// <summary>
        ///     Clone this <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        /// <returns>A copy of this <see cref="ColyseusMapSchema{T}" /></returns>
        public IColyseusSchemaCollection Clone()
        {
            ColyseusMapSchema<T> clone = new ColyseusMapSchema<T>(items)
            {
                OnAdd = OnAdd,
                OnChange = OnChange,
                OnRemove = OnRemove
            };
            return clone;
        }

        /// <summary>
        ///     Determine what type of item this <see cref="ColyseusMapSchema{T}" /> contains
        /// </summary>
        /// <returns>
        ///     <code>typeof(<typeparamref name="T" />);</code>
        /// </returns>
        public Type GetChildType()
        {
            return typeof(T);
        }

        /// <summary>
        ///     Get the default value of <typeparamref name="T" />
        /// </summary>
        /// <returns>
        ///     <code>default(<typeparamref name="T" />);</code>
        /// </returns>
        public object GetTypeDefaultValue()
        {
            return default(T);
        }

        /// <summary>
        ///     Determine if this <see cref="ColyseusMapSchema{T}" /> contains <paramref name="key" />
        /// </summary>
        /// <param name="key">The key in <see cref="items" /> that will be checked for</param>
        /// <returns>True if <see cref="items" /> contains the <paramref name="key" />, false if not</returns>
        public bool ContainsKey(object key)
        {
            return items.Contains(key);
        }

        /// <summary>
        ///     Getter for <see cref="HasSchemaChild" />
        ///     <para>This calls: <code>Schema.CheckSchemaChild(typeof(T))</code></para>
        /// </summary>
        public bool HasSchemaChild { get; } = ColyseusSchema.CheckSchemaChild(typeof(T));

        /// <summary>
        ///     Getter/Setter of the <see cref="CSAType.ChildPrimitiveType" /> that this <see cref="ColyseusMapSchema{T}" />
        ///     contains
        /// </summary>
        public string ChildPrimitiveType { get; set; }

        /// <summary>
        ///     Accessor to get/set <see cref="items" /> with a <paramref name="key" />
        /// </summary>
        /// <param name="key"></param>
        public object this[object key]
        {
            get { return this[(string) key]; }
            set { items[(string) key] = HasSchemaChild ? (T) value : (T) Convert.ChangeType(value, typeof(T)); }
        }

        /// <summary>
        ///     Getter function to get all the <see cref="items" /> in this <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        /// <returns>
        ///     <see cref="items" />
        /// </returns>
        public IDictionary GetItems()
        {
            return items;
        }

        /// <summary>
        ///     Clear all items and indices
        /// </summary>
        /// <param name="refs">Passed in for garbage collection, if needed</param>
        public void Clear(ColyseusReferenceTracker refs = null)
        {
            if (refs != null && HasSchemaChild)
            {
                foreach (IColyseusRef item in items.Values)
                {
                    refs.Remove(item.__refId);
                }
            }

            Indexes.Clear();
            items.Clear();
        }

        /// <summary>
        ///     Getter for the amount of <see cref="items" /> in this <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        public int Count
        {
            get { return items.Count; }
        }

        /// <summary>
        ///     <b>UNIMPLEMENTED</b> setter function to set <see cref="items" />
        /// </summary>
        /// <param name="items">
        ///     The items to pass to the <see cref="ColyseusMapSchema{T}" />
        /// </param>
        public void SetItems(object items) //TODO: Is it ok if this is unimplemented?
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Invoke <see cref="OnAdd" /> on all <see cref="items" />
        /// </summary>
        public void TriggerAll()
        {
            if (OnAdd == null)
            {
                return;
            }

            foreach (DictionaryEntry item in items)
            {
                OnAdd.Invoke((T) item.Value, (string) item.Key);
            }
        }

        /// <summary>
        ///     Clone the Event Handlers from another <see cref="IColyseusRef" /> into this <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        /// <param name="previousInstance">The <see cref="IColyseusRef" /> with the EventHandlers to copy</param>
        public void MoveEventHandlers(IColyseusRef previousInstance)
        {
            OnAdd = ((ColyseusMapSchema<T>) previousInstance).OnAdd;
            OnChange = ((ColyseusMapSchema<T>) previousInstance).OnChange;
            OnRemove = ((ColyseusMapSchema<T>) previousInstance).OnRemove;
        }

        /// <summary>
        ///     Trigger <see cref="OnAdd" /> with a specific <paramref name="item" /> at an <paramref name="index" />
        /// </summary>
        /// <param name="item">The item to add, will be cast to <typeparamref name="T" /> </param>
        /// <param name="index">The index of the item, will be cast to <see cref="string" /></param>
        public void InvokeOnAdd(object item, object index)
        {
            OnAdd?.Invoke((T) item, (string) index);
        }

        /// <summary>
        ///     Trigger <see cref="OnChange" /> with a specific <paramref name="item" /> at an <paramref name="index" />
        /// </summary>
        /// <param name="item">The item to change, will be cast to <typeparamref name="T" /> </param>
        /// <param name="index">The index of the item, will be cast to <see cref="string" /></param>
        public void InvokeOnChange(object item, object index)
        {
            OnChange?.Invoke((T) item, (string) index);
        }

        /// <summary>
        ///     Trigger <see cref="OnRemove" /> with a specific <paramref name="item" /> at an <paramref name="index" />
        /// </summary>
        /// <param name="item">The item to remove, will be cast to <typeparamref name="T" /> </param>
        /// <param name="index">The index of the item, will be cast to <see cref="string" /></param>
        public void InvokeOnRemove(object item, object index)
        {
            OnRemove?.Invoke((T) item, (string) index);
        }

        /// <summary>
        ///     Event that is invoked when something is added to the <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        public event CSAKeyValueEventHandler<T, string> OnAdd;

        /// <summary>
        ///     Event that is invoked when something is changed in the <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        public event CSAKeyValueEventHandler<T, string> OnChange;

        /// <summary>
        ///     Event that is invoked when something is removed from the <see cref="ColyseusMapSchema{T}" />
        /// </summary>
        public event CSAKeyValueEventHandler<T, string> OnRemove;

        /// <summary>
        ///     Add a new item into <see cref="items" />
        /// </summary>
        /// <param name="item">The new entry for <see cref="items" /></param>
        public void Add(KeyValuePair<string, T> item)
        {
            items[item.Key] = item.Value;
        }

        /// <summary>
        ///     Check if an <paramref name="item" /> exists in <see cref="items" />
        /// </summary>
        /// <param name="item">The value to check for</param>
        /// <returns>True if <paramref name="item" /> is found in <see cref="items" />, false otherwise</returns>
        public bool Contains(KeyValuePair<string, T> item)
        {
            return items.Contains(item.Key);
        }

        /// <summary>
        ///     Remove an item from this <see cref="ColyseusMapSchema{T}" />'s <see cref="items" />
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>
        ///     True if <paramref name="item" /> was found and removed, false if it was not in the <see cref="items" /> to
        ///     begin with
        /// </returns>
        public bool Remove(KeyValuePair<string, T> item)
        {
            T value;
            if (TryGetValue(item.Key, out value) && Equals(value, item.Value))
            {
                Remove(item.Key);
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Determine if this <see cref="ColyseusMapSchema{T}" /> contains <paramref name="key" />
        /// </summary>
        /// <param name="key">The key in <see cref="items" /> that will be checked for</param>
        /// <returns>True if <see cref="items" /> contains the <paramref name="key" />, false if not</returns>
        public bool ContainsKey(string key)
        {
            return items.Contains(key);
        }

        /// <summary>
        ///     Add a new item to <see cref="items" />
        /// </summary>
        /// <param name="key">The field name</param>
        /// <param name="value">The data to add</param>
        public void Add(string key, T value)
        {
            items.Add(key, value);
        }

        public bool Remove(string key)
        {
            bool result = items.Contains(key);
            if (result)
            {
                items.Remove(key);
            }

            return result;
        }

        public bool TryGetValue(string key, out T value)
        {
            object foundValue;
            if ((foundValue = items[key]) != null || items.Contains(key))
            {
                // Either found with a non-null value, or contained value is null.
                value = (T) foundValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        ///     Function to iterate over <see cref="items" /> and perform an <see cref="Action{T}" /> upon each entry
        /// </summary>
        /// <param name="action">The <see cref="Action" /> to perform</param>
        public void ForEach(Action<string, T> action)
        {
            foreach (DictionaryEntry item in items)
            {
                action((string) item.Key, (T) item.Value);
            }
        }
    }
}