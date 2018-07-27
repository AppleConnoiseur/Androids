using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;

namespace Androids
{
    public static class ReflectionUtility
    {
        public static Object CloneObjectShallowly(this Object sourceObject)
        {
            //Log.Message(": " + sourceObject?.ToString() ?? "null");

            if (sourceObject == null)
                return null;

            Type objectType = sourceObject.GetType();

            if (objectType.IsAbstract)
                return null;

            if (objectType.IsPrimitive || objectType.IsValueType || objectType.IsArray || objectType == typeof(String))
                return sourceObject;

            //Create new object.
            Object cloneObject = Activator.CreateInstance(objectType);

            if(cloneObject == null)
                return null;

            //Go through all fields.
            foreach (FieldInfo field in objectType.GetFields())
            {
                if(!field.IsLiteral)
                {
                    //Log.Message("Field: " + field.Name);
                    //Object value = CloneObject(field.GetValue(sourceObject));
                    Object value = field.GetValue(sourceObject);
                    field.SetValue(cloneObject, value);
                }
            }

            //Go through all properties.
            /*foreach(PropertyInfo property in objectType.GetProperties())
            {
                if(property.GetIndexParameters() == null)
                    property.SetValue(cloneObject, property.GetValue(sourceObject, null), null);
            }*/

            return cloneObject;
        }
    }
}
