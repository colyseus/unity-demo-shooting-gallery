namespace Colyseus.Schema
{
    /// <summary>
    ///     <see cref="ColyseusSchema" /> used for the purposes of reflection
    /// </summary>
    public class CSAReflectionField : ColyseusSchema
    {
        /// <summary>
        ///     The field name
        /// </summary>
        [CSAType(0, "string")]
        public string name;

        [CSAType(2, "number")]
        public float referencedType; //TODO: remove unused attribute?

        /// <summary>
        ///     The type of the field
        /// </summary>
        [CSAType(1, "string")]
        public string type;
    }

    /// <summary>
    ///     Mid level reflection container of an <see cref="ColyseusArraySchema{T}" />
    /// </summary>
    public class CSAReflectionType : ColyseusSchema
    {
        /// <summary>
        ///     An <see cref="ColyseusArraySchema{T}" /> of <see cref="CSAReflectionField" />
        /// </summary>
        [CSAType(1, "array", typeof(ColyseusArraySchema<CSAReflectionField>))]
        public ColyseusArraySchema<CSAReflectionField> fields = new ColyseusArraySchema<CSAReflectionField>();

        /// <summary>
        ///     The ID of this <see cref="ColyseusSchema" />
        /// </summary>
        [CSAType(0, "number")]
        public float id;
    }

    /// <summary>
    ///     Top level reflection container for an <see cref="ColyseusArraySchema{T}" />
    /// </summary>
    public class ColyseusReflection : ColyseusSchema
    {
        [CSAType(1, "number")]
        public float rootType;  //TODO: remove unused attribute?

        /// <summary>
        ///     An <see cref="ColyseusArraySchema{T}" /> of <see cref="CSAReflectionType" />
        /// </summary>
        [CSAType(0, "array", typeof(ColyseusArraySchema<CSAReflectionType>))]
        public ColyseusArraySchema<CSAReflectionType> types = new ColyseusArraySchema<CSAReflectionType>();
    }
}