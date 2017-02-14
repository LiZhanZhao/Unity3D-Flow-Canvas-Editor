using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using ParadoxNotion;

namespace FlowCanvas
{

    ///Responsible for internal -connection level- value conversions
    public static class TypeConverter
    {

        public static ValueHandler<object> GetConverterFuncFromTo(Type sourceType, Type targetType, ValueHandler<object> func)
        {

            if (targetType.RTIsAssignableFrom(sourceType))
            {
                return func;
            }

            if (typeof(IConvertible).RTIsAssignableFrom(targetType) && typeof(IConvertible).RTIsAssignableFrom(sourceType))
            {
                return () => { return Convert.ChangeType(func(), targetType); };
            }


            ///CUSTOM CONVENIENCE CONVERTIONS

            //from anything to string
            if (targetType == typeof(string) && sourceType != typeof(Flow))
            {
                return () => { try { return func().ToString(); } catch { return null; } };
            }

            //from component to Vector3 (position)
            if (targetType == typeof(Vector3) && typeof(Component).RTIsAssignableFrom(sourceType))
            {
                return () => { try { return (func() as Component).transform.position; } catch { return Vector3.zero; } };
            }

            //from gameobject to Vector3 (position)
            if (targetType == typeof(Vector3) && sourceType == typeof(GameObject))
            {
                return () => { try { return (func() as GameObject).transform.position; } catch { return Vector3.zero; } };
            }

            //from component to component
            if (typeof(Component).RTIsAssignableFrom(targetType) && typeof(Component).RTIsAssignableFrom(sourceType))
            {
                return () => { try { return (func() as Component).GetComponent(targetType); } catch { return null; } };
            }

            //from gameobject to component
            if (typeof(Component).RTIsAssignableFrom(targetType) && sourceType == typeof(GameObject))
            {
                return () => { try { return (func() as GameObject).GetComponent(targetType); } catch { return null; } };
            }

            //from component to gameobject
            if (targetType == typeof(GameObject) && typeof(Component).RTIsAssignableFrom(sourceType))
            {
                return () => { try { return (func() as Component).gameObject; } catch { return null; } };
            }

            //Object to bool. Similar to unity checks if it's null
            if (targetType == typeof(bool) && typeof(UnityEngine.Object).RTIsAssignableFrom(sourceType))
            {
                return () => { return (func() as UnityEngine.Object) != null; };
            }

            //DOWNCASTING
            if (targetType.RTIsSubclassOf(sourceType))
            {
                return func;
            }

            //From IList to IList
            if (typeof(IList).RTIsAssignableFrom(sourceType) && typeof(IList).RTIsAssignableFrom(targetType))
            {
                try
                {
                    var elementFrom = sourceType.IsArray ? sourceType.GetElementType() : sourceType.GetGenericArguments()[0];
                    var elementTo = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];
                    if (elementTo.RTIsAssignableFrom(elementFrom))
                    {
                        return () =>
                        {
                            var list = new List<object>();
                            var target = func() as IList;
                            for (var i = 0; i < target.Count; i++) { list.Add(target[i]); }
                            return targetType.RTIsArray() ? (object)list.ToArray() : (object)list.ToList();
                        };
                    }
                }
                catch { return null; }
            }

            ///This is slooow...Check last.
            if (sourceType.RTGetMethods().Any(m => m.ReturnType == targetType && (m.Name == "op_Implicit" || m.Name == "op_Explicit")))
            {
                return func;
            }

            return null;
        }


        ///Is there a convertion available from source type and to target type?
        //This is done only in editor.
        public static bool HasConvertion(Type sourceType, Type targetType)
        {
            ValueHandler<object> dummy = () => { return null; };
            var func = GetConverterFuncFromTo(sourceType, targetType, dummy);
            return func != null;

        }
    }
}