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

            if (targetType.RTIsSubclassOf(sourceType))
            {
                return func;
            }

            //if (typeof(IConvertible).RTIsAssignableFrom(targetType) && typeof(IConvertible).RTIsAssignableFrom(sourceType))
            //{
            //    return () => { return Convert.ChangeType(func(), targetType); };
            //}


            ///CUSTOM CONVENIENCE CONVERTIONS

            //from anything to string
            //if (targetType == typeof(string) && sourceType != typeof(Flow))
            //{
            //    return () => { try { return func().ToString(); } catch { return null; } };
            //}

            //DOWNCASTING
            

            //From IList to IList
            //if (typeof(IList).RTIsAssignableFrom(sourceType) && typeof(IList).RTIsAssignableFrom(targetType))
            //{
            //    try
            //    {
            //        var elementFrom = sourceType.IsArray ? sourceType.GetElementType() : sourceType.GetGenericArguments()[0];
            //        var elementTo = targetType.IsArray ? targetType.GetElementType() : targetType.GetGenericArguments()[0];
            //        if (elementTo.RTIsAssignableFrom(elementFrom))
            //        {
            //            return () =>
            //            {
            //                var list = new List<object>();
            //                var target = func() as IList;
            //                for (var i = 0; i < target.Count; i++) { list.Add(target[i]); }
            //                return targetType.RTIsArray() ? (object)list.ToArray() : (object)list.ToList();
            //            };
            //        }
            //    }
            //    catch { return null; }
            //}

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