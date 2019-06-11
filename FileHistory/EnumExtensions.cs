using System;

namespace J4JSoftware.FileHistory
{
    // Thanx to Troy Alford for this
    // https://stackoverflow.com/questions/1799370/getting-attributes-of-enums-value
    public static class EnumExtensions
    {
        public static T GetAttribute<T>(this Enum value) where T : Attribute
        {
            var type = value.GetType();
            var memberInfo = type.GetMember(value.ToString());
            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            return attributes.Length > 0
                ? (T)attributes[0]
                : null;
        }

        public static TargetPropertyType GetTargetPropertyType(this Enum value)
        {
            var attribute = value.GetAttribute<TargetPropertyDataAttribute>();
            return attribute?.PropertyType ?? TargetPropertyType.Undefined;
        }
    }
}
