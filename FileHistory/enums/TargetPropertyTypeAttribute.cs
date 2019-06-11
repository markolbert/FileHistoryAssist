using System;

namespace J4JSoftware.FileHistory
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class TargetPropertyDataAttribute : Attribute
    {
        public TargetPropertyDataAttribute( TargetPropertyType propType )
        {
            PropertyType = propType;
        }

        public TargetPropertyType PropertyType { get; }
    }

    public class NumericTargetPropertyAttribute : TargetPropertyDataAttribute
    {
        public NumericTargetPropertyAttribute()
            : base( TargetPropertyType.Numeric )
        {
        }
    }

    public class TextTargetPropertyAttribute : TargetPropertyDataAttribute
    {
        public TextTargetPropertyAttribute()
            : base(TargetPropertyType.Text)
        {
        }
    }
}
