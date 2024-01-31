using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Inspectors
{
    [CustomPropertyDrawer(typeof(HookAttribute))]
    public class HookPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            // TODO: range support
            // if (property.propertyType == SerializedPropertyType.Float)
            // {
            //     var floatField = property.serializedObject.targetObject.GetType().GetField(property.name);
            //     var rangeFloat = floatField.GetCustomAttribute<HookRangeFloatAttribute>();
            //     if (rangeFloat != null)
            //     {
            //         var rangeField = new SliderInt()
            //         return null;
            //     }
            // }

            var field = new PropertyField(property);

            var target = property.serializedObject.targetObject;
            var targetType = target.GetType();
            if (attribute is not HookAttribute attr)
            {
                return field;
            }

            var flags = BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.NonPublic;

            #region Method

            var hookMethod = targetType.GetMethod(attr.MethodName, flags);
            if (hookMethod == null)
            {
                hookMethod = GetFlattenedMethods(targetType, attr.MethodName).FirstOrDefault();
            }

            if (hookMethod != null)
            {
                field.RegisterValueChangeCallback(e => { hookMethod.Invoke(target, null); });
                return field;
            }

            #endregion

            #region Property

            var hookProperty = targetType.GetProperty(attr.MethodName, flags);
            if (hookProperty == null)
            {
                hookProperty = GetFlattenedProperties(targetType, attr.MethodName).FirstOrDefault();
            }

            if (hookProperty != null)
            {
                field.RegisterValueChangeCallback(e => hookProperty.SetValue(target, e.changedProperty.boxedValue));
                return field;
            }

            #endregion

            return field;
        }

        private static IEnumerable<MethodInfo> GetFlattenedMethods(Type type, string methodName)
        {
            while (type != null)
            {
                MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                                       BindingFlags.Static | BindingFlags.Public |
                                                       BindingFlags.NonPublic);
                for (int i = 0; i < methods.Length; ++i)
                {
                    if (methods[i].Name == methodName)
                        yield return methods[i];
                }

                type = type.BaseType;
            }
        }

        private static IEnumerable<PropertyInfo> GetFlattenedProperties(Type type, string propertyName)
        {
            while (type != null)
            {
                var properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                                    BindingFlags.Static | BindingFlags.Public |
                                                    BindingFlags.NonPublic);
                for (int i = 0; i < properties.Length; ++i)
                {
                    if (properties[i].Name == propertyName)
                        yield return properties[i];
                }

                type = type.BaseType;
            }
        }
    }
}