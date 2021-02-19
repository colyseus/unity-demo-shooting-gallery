using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

// ReSharper disable InconsistentNaming

/***
  //Allowed primitive types:
  //  "string"
  //  "number"
  //  "boolean"
  //  "int8"
  //  "uint8"
  //  "int16"
  //  "uint16"
  //  "int32"
  //  "uint32"
  //  "int64"
  //  "uint64"
  //  "float32"
  //  "float64"

  //Allowed reference types:
  //  "ref"
  //  "array"
  //  "map"
***/

namespace Colyseus.Schema
{
    /// <summary>
    ///     <see cref="ColyseusSchema" /> <see cref="Attribute" /> wrapper class
    ///     <para>Allowed primitive types:</para>
    ///     <para>
    ///         <em>
    ///             "string", "number", "boolean", "int8", "uint8", "int16", "uint16", "int32", "uint32", "int64", "uint64",
    ///             "float32", "float64"
    ///         </em>
    ///     </para>
    ///     <para>Allowed reference types:</para>
    ///     <para>
    ///         <em>"ref", "array", "map"</em>
    ///     </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class CSAType : Attribute
    {
        /// <summary>
        ///     The <see cref="FieldType" /> of the <see cref="ChildType" />
        /// </summary>
        public string ChildPrimitiveType;

        /// <summary>
        ///     What type of <see cref="ColyseusSchema" /> this attribute is (can be null)
        /// </summary>
        public Type ChildType;

        /// <summary>
        ///     The field type this <see cref="Attribute" /> represents
        /// </summary>
        public string FieldType;

        /// <summary>
        ///     The index of where this will be stored in the <see cref="ColyseusSchema" />
        /// </summary>
        public int Index;

        public CSAType(int index, string type, Type childType = null, string childPrimitiveType = null)
        {
            Index = index; // GetType().GetFields() doesn't guarantee order of fields, need to manually track them here!
            FieldType = type;
            ChildType = childType;
            ChildPrimitiveType = childPrimitiveType;
        }
    }

    /// <summary>
    ///     Wrapper class containing an <see cref="int" /> offset value
    /// </summary>
    public class CSAIterator
    {
        /// <summary>
        ///     The value used to offset when we encode/decode data
        /// </summary>
        public int Offset;
    }

    /// <summary>
    ///     Byte flags used to signal specific operations to be performed on <see cref="ColyseusSchema" /> data
    /// </summary>
    public enum CSASPEC : byte
    {
        /// <summary>
        ///     A decode can be done, begin that process
        /// </summary>
        SWITCH_TO_STRUCTURE = 255,

        /// <summary>
        ///     The following bytes will indicate the <see cref="ColyseusSchema" /> type
        /// </summary>
        TYPE_ID = 213
    }

    /// <summary>
    ///     Byte flags for <see cref="CSADataChange" /> operations that can be done
    /// </summary>
    [SuppressMessage("ReSharper", "MissingXmlDoc")]
    public enum CSAOPERATION : byte
    {
        ADD = 128,
        REPLACE = 0,
        DELETE = 64,
        DELETE_AND_ADD = 192,
        CLEAR = 10
    }

    /// <summary>
    ///     Wrapper class for a <see cref="ColyseusSchema" /> change
    /// </summary>
    public class CSADataChange
    {
        /// <summary>
        ///     The field index of the data change
        /// </summary>
        public object DynamicIndex;

        /// <summary>
        ///     The field name of the data
        /// </summary>
        public string Field;

        /// <summary>
        ///     An <see cref="CSAOPERATION" /> flag for this DataChange
        /// </summary>
        public byte Op;

        /// <summary>
        ///     The value of the old data
        /// </summary>
        public object PreviousValue;

        /// <summary>
        ///     The value of the new data
        /// </summary>
        public object Value;
    }

    /// <summary>
    ///     Delegate function for handling a List of <see cref="Colyseus.CSADataChange" />
    /// </summary>
    /// <param name="changes">The changes that have occurred</param>
    public delegate void CSAOnChangeEventHandler(List<CSADataChange> changes);

    /// <summary>
    ///     Delegate for handling events given a <paramref name="key" /> and a <paramref name="value" />
    /// </summary>
    /// <param name="value">The affected value</param>
    /// <param name="key">The key we're affecting</param>
    /// <typeparam name="T">The <see cref="ColyseusSchema" /> type</typeparam>
    /// <typeparam name="K">The type of <see cref="object" /> we're attempting to access</typeparam>
    public delegate void CSAKeyValueEventHandler<T, K>(T value, K key);

    /// <summary>
    ///     Delegate function for handling <see cref="ColyseusSchema" /> removal
    /// </summary>
    public delegate void CSAOnRemoveEventHandler();

    /// <summary>
    ///     Interface for a collection of multiple <see cref="ColyseusSchema" />s
    /// </summary>
    [SuppressMessage("ReSharper", "MissingXmlDoc")]
    public interface IColyseusSchemaCollection : IColyseusRef
    {
        bool HasSchemaChild { get; }
        string ChildPrimitiveType { get; set; }

        int Count { get; }
        object this[object key] { get; set; }
        void InvokeOnAdd(object item, object index);
        void InvokeOnChange(object item, object index);
        void InvokeOnRemove(object item, object index);
        IDictionary GetItems();
        void SetItems(object items);
        void TriggerAll();
        void Clear(ColyseusReferenceTracker refs);

        Type GetChildType();
        object GetTypeDefaultValue();
        bool ContainsKey(object key);

        void SetIndex(int index, object dynamicIndex);
        object GetIndex(int index);
        void SetByIndex(int index, object dynamicIndex, object value);

        IColyseusSchemaCollection Clone();
    }

    /// <summary>
    ///     Interface for an object that can be tracked by a <see cref="ColyseusReferenceTracker" />
    /// </summary>
    [SuppressMessage("ReSharper", "MissingXmlDoc")]
    public interface IColyseusRef
    {
        /// <summary>
        ///     The ID with which this <see cref="IColyseusRef" /> instance will be tracked
        /// </summary>
        int __refId { get; set; }

        object GetByIndex(int index);
        void DeleteByIndex(int index);

        void MoveEventHandlers(IColyseusRef previousInstance);
    }

    /// <summary>
    ///     Data structure representing a <see cref="ColyseusRoom{T}" />'s state (synchronizeable data)
    /// </summary>
    public class ColyseusSchema : IColyseusRef
    {
        /// <summary>
        ///     Map of the <see cref="CSAType.ChildPrimitiveType" />s that this schema uses
        /// </summary>
        protected Dictionary<string, string> fieldChildPrimitiveTypes = new Dictionary<string, string>();

        /// <summary>
        ///     Map of the <see cref="CSAType.ChildType" />s that this schema uses
        /// </summary>
        protected Dictionary<string, Type> fieldChildTypes = new Dictionary<string, Type>();

        /// <summary>
        ///     Map of the fields in this schema using {<see cref="CSAType.Index" />,
        /// </summary>
        protected Dictionary<int, string> fieldsByIndex = new Dictionary<int, string>();

        /// <summary>
        ///     Map of the field types in this schema
        /// </summary>
        protected Dictionary<string, string> fieldTypes = new Dictionary<string, string>();

        private ColyseusReferenceTracker refs;

        public ColyseusSchema()
        {
            FieldInfo[] fields = GetType().GetFields();
            foreach (FieldInfo field in fields)
            {
                object[] typeAttributes = field.GetCustomAttributes(typeof(CSAType), true);
                for (int i = 0; i < typeAttributes.Length; i++)
                {
                    CSAType t = (CSAType) typeAttributes[i];
                    fieldsByIndex.Add(t.Index, field.Name);
                    fieldTypes.Add(field.Name, t.FieldType);

                    if (t.ChildPrimitiveType != null)
                    {
                        fieldChildPrimitiveTypes.Add(field.Name, t.ChildPrimitiveType);
                    }

                    if (t.ChildType != null)
                    {
                        fieldChildTypes.Add(field.Name, t.ChildType);
                    }
                }
            }
        }

        /// <summary>
        ///     Allow get and set of property values by its <paramref name="propertyName" />
        /// </summary>
        /// <param name="propertyName">The object's field name</param>
        public object this[string propertyName]
        {
            get { return GetType().GetField(propertyName).GetValue(this); }
            set
            {
                FieldInfo field = GetType().GetField(propertyName);
                field.SetValue(this, value);
            }
        }

        /// <summary>
        ///     <see cref="IColyseusRef" /> implementation - ID with which to reference this <see cref="ColyseusSchema" />
        /// </summary>
        public int __refId { get; set; }

        /// <summary>
        ///     Update this <see cref="ColyseusSchema" />'s EventHandlers
        /// </summary>
        /// <param name="previousInstance">The instance of an <see cref="IColyseusRef" /> from which we will copy the EventHandlers</param>
        public void MoveEventHandlers(IColyseusRef previousInstance)
        {
            OnChange = ((ColyseusSchema) previousInstance).OnChange;
            OnRemove = ((ColyseusSchema) previousInstance).OnRemove;

            foreach (KeyValuePair<int, string> item in ((ColyseusSchema) previousInstance).fieldsByIndex)
            {
                object child = GetByIndex(item.Key);
                if (child is IColyseusRef)
                {
                    ((IColyseusRef) child).MoveEventHandlers((IColyseusRef) previousInstance.GetByIndex(item.Key));
                }
            }
        }

        /// <summary>
        ///     Get a field by it's index
        /// </summary>
        /// <param name="index">Index of the field to get</param>
        /// <returns>The <see cref="object" /> at that index (if it exists)</returns>
        public object GetByIndex(int index)
        {
            string fieldName;
            fieldsByIndex.TryGetValue(index, out fieldName);
            return this[fieldName];
        }

        /// <summary>
        ///     Remove the field by it's index
        /// </summary>
        /// <param name="index">Index of the field to remove</param>
        public void DeleteByIndex(int index)
        {
            string fieldName;
            fieldsByIndex.TryGetValue(index, out fieldName);
            this[fieldName] = null;
        }

        /// <inheritdoc cref="CSAOnChangeEventHandler" />
        public event CSAOnChangeEventHandler OnChange;

        /// <inheritdoc cref="CSAOnRemoveEventHandler" />
        public event CSAOnRemoveEventHandler OnRemove;

        /// <summary>
        ///     Getter function, required for <see cref="ColyseusReferenceTracker.GarbageCollection" />
        /// </summary>
        /// <returns>
        ///     <see cref="fieldChildTypes" />
        /// </returns>
        public Dictionary<string, Type> GetFieldChildTypes()
        {
            // This is required for "garbage collection" inside ReferenceTracker.
            return fieldChildTypes;
        }

        /// <summary>
        ///     Decode incoming data
        /// </summary>
        /// <param name="bytes">The incoming data</param>
        /// <param name="it"><see cref="CSAIterator" /> used to decode. If null, will create a new one</param>
        /// <param name="refs">
        ///     <see cref="ColyseusReferenceTracker" /> for all refs found through the decoding process. If null, will
        ///     create a new one
        /// </param>
        /// <exception cref="Exception">If no decoding fails</exception>
        public void Decode(byte[] bytes, CSAIterator it = null, ColyseusReferenceTracker refs = null)
        {
            ColyseusDecoder decode = ColyseusDecoder.GetInstance();

            if (it == null)
            {
                it = new CSAIterator();
            }

            if (refs == null)
            {
                refs = new ColyseusReferenceTracker();
            }

            int totalBytes = bytes.Length;

            int refId = 0;
            IColyseusRef _ref = this;

            this.refs = refs;
            refs.Add(refId, _ref);

            List<CSADataChange> changes = new List<CSADataChange>();
            OrderedDictionary allChanges = new OrderedDictionary(); // Dictionary<int, List<DataChange>>
            allChanges.Add(refId, changes);

            while (it.Offset < totalBytes)
            {
                byte _byte = bytes[it.Offset++];

                if (_byte == (byte) CSASPEC.SWITCH_TO_STRUCTURE)
                {
                    refId = Convert.ToInt32(decode.DecodeNumber(bytes, it));
                    _ref = refs.Get(refId);

                    //
                    // Trying to access a reference that haven't been decoded yet.
                    //
                    if (_ref == null)
                    {
                        throw new Exception("refId not found: " + refId);
                    }

                    // create empty list of changes for this refId.
                    changes = new List<CSADataChange>();
                    allChanges[(object) refId] = changes;

                    continue;
                }

                bool isSchema = _ref is ColyseusSchema;

                byte operation = (byte) (isSchema
                    ? (_byte >> 6) << 6 // "compressed" index + operation
                    : _byte); // "uncompressed" index + operation (array/map items)

                if (operation == (byte) CSAOPERATION.CLEAR)
                {
                    ((IColyseusSchemaCollection) _ref).Clear(refs);
                    continue;
                }

                int fieldIndex;
                string fieldName = null;
                string fieldType = null;

                Type childType = null;

                if (isSchema)
                {
                    fieldIndex = _byte % (operation == 0 ? 255 : operation); // FIXME: JS allows (0 || 255)
                    ((ColyseusSchema) _ref).fieldsByIndex.TryGetValue(fieldIndex, out fieldName);

                    // fieldType = ((Schema)_ref).fieldTypes[fieldName];
                    ((ColyseusSchema) _ref).fieldTypes.TryGetValue(fieldName ?? "", out fieldType);
                    ((ColyseusSchema) _ref).fieldChildTypes.TryGetValue(fieldName ?? "", out childType);
                }
                else
                {
                    fieldName = ""; // FIXME

                    fieldIndex = Convert.ToInt32(decode.DecodeNumber(bytes, it));
                    if (((IColyseusSchemaCollection) _ref).HasSchemaChild)
                    {
                        fieldType = "ref";
                        childType = ((IColyseusSchemaCollection) _ref).GetChildType();
                    }
                    else
                    {
                        fieldType = ((IColyseusSchemaCollection) _ref).ChildPrimitiveType;
                    }
                }

                object value = null;
                object previousValue = null;
                object dynamicIndex = null;

                if (!isSchema)
                {
                    previousValue = _ref.GetByIndex(fieldIndex);

                    if ((operation & (byte) CSAOPERATION.ADD) == (byte) CSAOPERATION.ADD)
                    {
                        // MapSchema dynamic index.
                        dynamicIndex = ((IColyseusSchemaCollection) _ref).GetItems() is OrderedDictionary
                            ? (object) decode.DecodeString(bytes, it)
                            : fieldIndex;

                        ((IColyseusSchemaCollection) _ref).SetIndex(fieldIndex, dynamicIndex);
                    }
                    else
                    {
                        dynamicIndex = ((IColyseusSchemaCollection) _ref).GetIndex(fieldIndex);
                    }
                }
                else if (fieldName != null) // FIXME: duplicate check
                {
                    previousValue = ((ColyseusSchema) _ref)[fieldName];
                }

                //
                // Delete operations
                //
                if ((operation & (byte) CSAOPERATION.DELETE) == (byte) CSAOPERATION.DELETE)
                {
                    if (operation != (byte) CSAOPERATION.DELETE_AND_ADD)
                    {
                        _ref.DeleteByIndex(fieldIndex);
                    }

                    // Flag `refId` for garbage collection.
                    if (previousValue != null && previousValue is IColyseusRef)
                    {
                        refs.Remove(((IColyseusRef) previousValue).__refId);
                    }

                    value = null;
                }

                if (fieldName == null)
                {
                    //
                    // keep skipping next bytes until reaches a known structure
                    // by local decoder.
                    //
                    CSAIterator nextIterator = new CSAIterator {Offset = it.Offset};

                    while (it.Offset < totalBytes)
                    {
                        if (decode.SwitchStructureCheck(bytes, it))
                        {
                            nextIterator.Offset = it.Offset + 1;
                            if (refs.Has(Convert.ToInt32(decode.DecodeNumber(bytes, nextIterator))))
                            {
                                break;
                            }
                        }

                        it.Offset++;
                    }

                    continue;
                }

                if (operation == (byte) CSAOPERATION.DELETE)
                {
                    //
                    // FIXME: refactor me.
                    // Don't do anything.
                    //
                }
                else if (fieldType == "ref")
                {
                    refId = Convert.ToInt32(decode.DecodeNumber(bytes, it));
                    value = refs.Get(refId);

                    if (operation != (byte) CSAOPERATION.REPLACE)
                    {
                        Type concreteChildType = GetSchemaType(bytes, it, childType);

                        if (value == null)
                        {
                            value = CreateTypeInstance(concreteChildType);

                            if (previousValue != null)
                            {
                                ((ColyseusSchema) value).MoveEventHandlers((ColyseusSchema) previousValue);

                                if (
                                    ((IColyseusRef) previousValue).__refId > 0 &&
                                    refId != ((IColyseusRef) previousValue).__refId
                                )
                                {
                                    refs.Remove(((IColyseusRef) previousValue).__refId);
                                }
                            }
                        }

                        refs.Add(refId, (IColyseusRef) value, value != previousValue);
                    }
                }
                else if (childType == null)
                {
                    // primitive values
                    value = decode.DecodePrimitiveType(fieldType, bytes, it);
                }
                else
                {
                    refId = Convert.ToInt32(decode.DecodeNumber(bytes, it));
                    value = refs.Get(refId);

                    IColyseusSchemaCollection valueRef = refs.Has(refId)
                        ? (IColyseusSchemaCollection) previousValue
                        : (IColyseusSchemaCollection) Activator.CreateInstance(childType);

                    value = valueRef.Clone();

                    // keep reference to nested childPrimitiveType.
                    string childPrimitiveType;
                    ((ColyseusSchema) _ref).fieldChildPrimitiveTypes.TryGetValue(fieldName, out childPrimitiveType);
                    ((IColyseusSchemaCollection) value).ChildPrimitiveType = childPrimitiveType;

                    if (previousValue != null)
                    {
                        ((IColyseusSchemaCollection) value).MoveEventHandlers(
                            (IColyseusSchemaCollection) previousValue);

                        if (
                            ((IColyseusRef) previousValue).__refId > 0 &&
                            refId != ((IColyseusRef) previousValue).__refId
                        )
                        {
                            refs.Remove(((IColyseusRef) previousValue).__refId);

                            List<CSADataChange> deletes = new List<CSADataChange>();
                            IDictionary items = ((IColyseusSchemaCollection) previousValue).GetItems();

                            foreach (object key in items.Keys)
                            {
                                deletes.Add(new CSADataChange
                                {
                                    DynamicIndex = key,
                                    Op = (byte) CSAOPERATION.DELETE,
                                    Value = null,
                                    PreviousValue = items[key]
                                });
                            }

                            allChanges[(object) ((IColyseusRef) previousValue).__refId] = deletes;
                        }
                    }

                    refs.Add(refId, (IColyseusRef) value, valueRef != previousValue);
                }

                bool hasChange = previousValue != value;

                if (value != null)
                {
                    if (value is IColyseusRef)
                    {
                        ((IColyseusRef) value).__refId = refId;
                    }

                    if (_ref is ColyseusSchema)
                    {
                        ((ColyseusSchema) _ref)[fieldName] = value;
                    }
                    else if (_ref is IColyseusSchemaCollection)
                    {
                        ((IColyseusSchemaCollection) _ref).SetByIndex(fieldIndex, dynamicIndex, value);
                    }
                }

                if (hasChange)
                {
                    changes.Add(new CSADataChange
                    {
                        Op = operation,
                        Field = fieldName,
                        DynamicIndex = dynamicIndex,
                        Value = value,
                        PreviousValue = previousValue
                    });
                }
            }

            TriggerChanges(ref allChanges);

            refs.GarbageCollection();
        }

        /// <summary>
        ///     Gets all changes in <see cref="refs" /> and then triggers them
        /// </summary>
        public void TriggerAll()
        {
            //
            // first state not received from the server yet.
            // nothing to trigger.
            //
            if (refs == null)
            {
                return;
            }

            OrderedDictionary allChanges = new OrderedDictionary();
            TriggerAllFillChanges(this, ref allChanges);
            TriggerChanges(ref allChanges);
        }

        /// <summary>
        ///     Gets all fields and generates a change list
        /// </summary>
        /// <param name="currentRef">
        ///     The <see cref="IColyseusRef" /> that changes will be grabbed from. Used for recursive looping through
        ///     a <see cref="ColyseusSchema" /> and all of it's children
        /// </param>
        /// <param name="allChanges">The changes that have been found</param>
        protected void TriggerAllFillChanges(IColyseusRef currentRef, ref OrderedDictionary allChanges)
        {
            // skip recursive structures...
            if (allChanges.Contains(currentRef.__refId))
            {
                return;
            }

            List<CSADataChange> changes = new List<CSADataChange>();
            allChanges[(object) currentRef.__refId] = changes;

            if (currentRef is ColyseusSchema)
            {
                foreach (string fieldName in ((ColyseusSchema) currentRef).fieldsByIndex.Values)
                {
                    object value = ((ColyseusSchema) currentRef)[fieldName];
                    changes.Add(new CSADataChange
                    {
                        Field = fieldName,
                        Op = (byte) CSAOPERATION.ADD,
                        Value = value
                    });

                    if (value is IColyseusRef)
                    {
                        TriggerAllFillChanges((IColyseusRef) value, ref allChanges);
                    }
                }
            }
            else
            {
                IDictionary items = ((IColyseusSchemaCollection) currentRef).GetItems();
                foreach (object key in items.Keys)
                {
                    object child = items[key];

                    changes.Add(new CSADataChange
                    {
                        Field = (string) key,
                        DynamicIndex = key,
                        Op = (byte) CSAOPERATION.ADD,
                        Value = child
                    });

                    if (child is IColyseusRef)
                    {
                        TriggerAllFillChanges((IColyseusRef) child, ref allChanges);
                    }
                }
            }
        }

        /// <summary>
        ///     Take all of the changes that have occurred and apply them in order to the <see cref="ColyseusSchema" />
        /// </summary>
        /// <param name="allChanges">Dictionary of the changes to apply</param>
        protected void TriggerChanges(ref OrderedDictionary allChanges)
        {
            foreach (object refId in allChanges.Keys)
            {
                List<CSADataChange> changes = (List<CSADataChange>) allChanges[refId];

                IColyseusRef _ref = refs.Get((int) refId);
                bool isSchema = _ref is ColyseusSchema;

                foreach (CSADataChange change in changes)
                {
                    //const listener = ref['$listeners'] && ref['$listeners'][change.field];

                    if (!isSchema)
                    {
                        IColyseusSchemaCollection container = (IColyseusSchemaCollection) _ref;

                        if (change.Op == (byte) CSAOPERATION.ADD &&
                            change.PreviousValue == container.GetTypeDefaultValue())
                        {
                            container.InvokeOnAdd(change.Value, change.DynamicIndex);
                        }
                        else if (change.Op == (byte) CSAOPERATION.DELETE)
                        {
                            //
                            // FIXME: `previousValue` should always be avaiiable.
                            // ADD + DELETE operations are still encoding DELETE operation.
                            //
                            if (change.PreviousValue != container.GetTypeDefaultValue())
                            {
                                container.InvokeOnRemove(change.PreviousValue, change.DynamicIndex ?? change.Field);
                            }
                        }
                        else if (change.Op == (byte) CSAOPERATION.DELETE_AND_ADD)
                        {
                            if (change.PreviousValue != container.GetTypeDefaultValue())
                            {
                                container.InvokeOnRemove(change.PreviousValue, change.DynamicIndex);
                            }

                            container.InvokeOnAdd(change.Value, change.DynamicIndex);
                        }
                        else if (
                            change.Op == (byte) CSAOPERATION.REPLACE ||
                            change.Value != change.PreviousValue
                        )
                        {
                            container.InvokeOnChange(change.Value, change.DynamicIndex);
                        }
                    }

                    //
                    // trigger onRemove on child structure.
                    //
                    if (
                        (change.Op & (byte) CSAOPERATION.DELETE) == (byte) CSAOPERATION.DELETE &&
                        change.PreviousValue is ColyseusSchema
                    )
                    {
                        ((ColyseusSchema) change.PreviousValue).OnRemove?.Invoke();
                    }
                }

                if (isSchema)
                {
                    ((ColyseusSchema) _ref).OnChange?.Invoke(changes);
                }
            }
        }

        /// <summary>
        ///     Determine what type of <see cref="ColyseusSchema" /> this is
        /// </summary>
        /// <param name="bytes">Incoming data</param>
        /// <param name="it">
        ///     The <see cref="CSAIterator" /> used to <see cref="ColyseusDecoder.DecodeNumber" /> the <paramref name="bytes" />
        /// </param>
        /// <param name="defaultType">
        ///     The default <see cref="ColyseusSchema" /> type, if one cant be determined from the
        ///     <paramref name="bytes" />
        /// </param>
        /// <returns>The parsed <see cref="System.Type" /> if found, <paramref name="defaultType" /> if not</returns>
        protected Type GetSchemaType(byte[] bytes, CSAIterator it, Type defaultType)
        {
            Type type = defaultType;

            if (bytes[it.Offset] == (byte) CSASPEC.TYPE_ID)
            {
                it.Offset++;
                int typeId = Convert.ToInt32(ColyseusDecoder.GetInstance().DecodeNumber(bytes, it));
                type = ColyseusContext.GetInstance().Get(typeId);
            }

            return type;
        }

        /// <summary>
        ///     Create an instance of the provided <paramref name="type" />
        /// </summary>
        /// <param name="type">The <see cref="System.Type" /> to create an instance of</param>
        /// <returns></returns>
        protected object CreateTypeInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }

        /// <summary>
        ///     Check if this <see cref="ColyseusSchema" /> has a <see cref="ColyseusSchema" /> child
        /// </summary>
        /// <param name="toCheck"><see cref="ColyseusSchema" /> type to check for</param>
        /// <returns>True if found, false otherwise</returns>
        public static bool CheckSchemaChild(Type toCheck)
        {
            Type generic = typeof(ColyseusSchema);

            while (toCheck != null && toCheck != typeof(object))
            {
                Type cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;

                if (generic == cur)
                {
                    return true;
                }

                toCheck = toCheck.BaseType;
            }

            return false;
        }
    }
}