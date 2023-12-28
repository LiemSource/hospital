using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace VaeHelper
{
    public class EnumHelper
    {
        public static SortedList<int, string> GetItems<T>() where T : struct
        {
            var result = new SortedList<int, string>();
            Type t = typeof(T);
            if (!t.IsEnum) return result;
            Array arrays = Enum.GetValues(t);
            for (int i = 0; i < arrays.LongLength; i++)
            {
                object enumValue = arrays.GetValue(i);
                FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
                object[] attribArray = fieldInfo.GetCustomAttributes(false);
                if (!attribArray.Any(a => a.GetType() == typeof(DescriptionAttribute))) continue;
                DescriptionAttribute attrib = (DescriptionAttribute)attribArray[0];
                result.Add(Convert.ToInt32(enumValue), attrib.Description);
            }
            return result;
        }

        public static SortedList<int, string> GetItems(Type enumType)
        {
            var result = new SortedList<int, string>();
            if (!enumType.IsEnum) return result;
            Array arrays = Enum.GetValues(enumType);
            for (int i = 0; i < arrays.LongLength; i++)
            {
                object enumValue = arrays.GetValue(i);
                FieldInfo fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
                object[] attribArray = fieldInfo.GetCustomAttributes(false);
                if (!attribArray.Any(a => a.GetType() == typeof(DescriptionAttribute))) continue;
                DescriptionAttribute attrib = (DescriptionAttribute)attribArray[0];
                result.Add(Convert.ToInt32(enumValue), attrib.Description);
            }
            return result;
        }
        public static string GetEnumDescription<T>(T enumValue) where T : struct
        {
            Type t = typeof(T);
            if (!t.IsEnum) return "";
            FieldInfo field = t.GetField(enumValue.ToString());
            if (field == null) return enumValue.ToString();
            var obj = field.GetCustomAttribute(typeof(DescriptionAttribute), false);    //获取描述属性
            if (obj == null)    //当描述属性没有时，直接返回名称
                return enumValue.ToString();
            var descriptionAttribute = (DescriptionAttribute)obj;
            return descriptionAttribute.Description;
        }
    }
}
