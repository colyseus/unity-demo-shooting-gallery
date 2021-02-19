using System;
using System.Reflection;
using Colyseus.Schema;

namespace Colyseus
{
    /// <summary>
    ///     An instance of ISerializer specifically for <see cref="ColyseusSchema" /> based serialization
    /// </summary>
    /// <typeparam name="T">A child of <see cref="ColyseusSchema" /></typeparam>
    public class ColyseusSchemaSerializer<T> : IColyseusSerializer<T>
    {
        /// <summary>
        ///     A reference to the <see cref="CSAIterator" />
        /// </summary>
        protected CSAIterator it = new CSAIterator();

        /// <summary>
        ///     Used for tracking all references
        /// </summary>
        protected ColyseusReferenceTracker refs = new ColyseusReferenceTracker();

        /// <summary>
        ///     The current state of this Serializer
        /// </summary>
        protected T state;

        public ColyseusSchemaSerializer()
        {
            state = Activator.CreateInstance<T>();
        }

        /// <inheritdoc />
        public void SetState(byte[] data, int offset = 0)
        {
            it.Offset = offset;
            (state as ColyseusSchema)?.Decode(data, it, refs);
        }

        /// <inheritdoc />
        public T GetState()
        {
            return state;
        }

        /// <inheritdoc />
        public void Patch(byte[] data, int offset = 0)
        {
            it.Offset = offset;
            (state as ColyseusSchema)?.Decode(data, it, refs);
        }

        /// <inheritdoc />
        public void Teardown()
        {
            // Clear all stored references.
            refs.Clear();
        }

        /// <inheritdoc />
        public void Handshake(byte[] bytes, int offset)
        {
            Type targetType = typeof(T);

            Type[] allTypes = targetType.Assembly.GetTypes();
            Type[] namespaceSchemaTypes = Array.FindAll(allTypes, t => t.Namespace == targetType.Namespace &&
                                                                       typeof(ColyseusSchema).IsAssignableFrom(
                                                                           targetType));

            ColyseusReflection reflection = new ColyseusReflection();
            CSAIterator it = new CSAIterator {Offset = offset};

            reflection.Decode(bytes, it);

            for (int i = 0; i < reflection.types.Count; i++)
            {
                Type schemaType = Array.Find(namespaceSchemaTypes, t => CompareTypes(t, reflection.types[i]));

                if (schemaType == null)
                {
                    throw new Exception(
                        "Local schema mismatch from server. Use \"schema-codegen\" to generate up-to-date local definitions.");
                }

                ColyseusContext.GetInstance().SetTypeId(schemaType, reflection.types[i].id);
            }
        }

        private static bool CompareTypes(Type schemaType, CSAReflectionType reflectionType)
        {
            FieldInfo[] fields = schemaType.GetFields();
            int typedFieldCount = 0;

            string fieldNames = "";
            for (int i = 0; i < fields.Length; i++)
            {
                fieldNames += fields[i].Name + ", ";
            }

            foreach (FieldInfo field in fields)
            {
                object[] typeAttributes = field.GetCustomAttributes(typeof(CSAType), true);

                if (typeAttributes.Length == 1)
                {
                    CSAType typedField = (CSAType) typeAttributes[0];
                    CSAReflectionField reflectionField = reflectionType.fields[typedField.Index];

                    if (
                        reflectionField == null ||
                        reflectionField.type.IndexOf(typedField.FieldType) != 0 ||
                        reflectionField.name != field.Name
                    )
                    {
                        return false;
                    }

                    typedFieldCount++;
                }
            }

            // skip if number of Type'd fields doesn't match
            if (typedFieldCount != reflectionType.fields.Count)
            {
                return false;
            }

            return true;
        }
    }
}