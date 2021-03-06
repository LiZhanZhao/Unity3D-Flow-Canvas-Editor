﻿using System;
using System.Collections;

#if !UNITY_EDITOR && UNITY_WSA
// For System.Reflection.TypeExtensions
using System.Reflection;
#endif

namespace ParadoxNotion.Serialization.FullSerializer.Internal {
    public class fsReflectedConverter : fsConverter {
        public override bool CanProcess(Type type) {
            if (type.Resolve().IsArray ||
                typeof(ICollection).IsAssignableFrom(type)) {

                return false;
            }

            return true;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType) {
            // 传进来的serialized 直接赋值一个Dict
            serialized = fsData.CreateDictionary();
            var result = fsResult.Success;

            // Serializer.Config 就是一些配置
            fsMetaType metaType = fsMetaType.Get(Serializer.Config, instance.GetType());

            metaType.EmitAotData();

            //PARADOXNOTION ADDITION
            object defaultInstance = null;
            //Dont do this for UnityObject. While there is fsUnityObjectConverter, this converter is also used as override,
            //when serializing a UnityObject directly.
            if ( fsGlobalConfig.SerializeDefaultValues == false && !(instance is UnityEngine.Object) ){
                defaultInstance = metaType.CreateInstance();
            }
            //

            for (int i = 0; i < metaType.Properties.Length; ++i) {
                fsMetaProperty property = metaType.Properties[i];
                // 不可读的跳过
                if (property.CanRead == false) continue;

                // 如果是不存储默认值的情况
                //PARADOXNOTION ADDITION
                if ( fsGlobalConfig.SerializeDefaultValues == false && defaultInstance != null ){
                    if (Equals( property.Read(instance), property.Read(defaultInstance) )){
                        continue;
                    }
                }
                //

                fsData serializedData;

                // property.Read(instance) 理解为从某一个实例读取某一个属性，字段值
                var itemResult = Serializer.TrySerialize(property.StorageType, property.OverrideConverterType,
                                                         property.Read(instance), out serializedData);
                result.AddMessages(itemResult);
                if (itemResult.Failed) {
                    continue;
                }

                serialized.AsDictionary[property.JsonName] = serializedData;
            }

            return result;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType) {
            var result = fsResult.Success;

            // Verify that we actually have an Object
            if ((result += CheckType(data, fsDataType.Object)).Failed) {
                return result;
            }

            if (data.AsDictionary.Count == 0){
                return fsResult.Success;
            }

            //
            fsMetaType metaType = fsMetaType.Get(Serializer.Config, storageType);
            metaType.EmitAotData();

            for (int i = 0; i < metaType.Properties.Length; ++i) {
                fsMetaProperty property = metaType.Properties[i];
                if (property.CanWrite == false) continue;

                fsData propertyData;
                if (data.AsDictionary.TryGetValue(property.JsonName, out propertyData)) {
                    object deserializedValue = null;
/*
                    // We have to read in the existing value, since we need to support partial
                    // deserialization. However, this is bad for perf.
                    // TODO: Find a way to avoid this call when we are not doing a partial deserialization
                    //       Maybe through a new property, ie, Serializer.IsPartialSerialization, which just
                    //       gets set when starting a new serialization? We cannot pipe the information
                    //       through CreateInstance unfortunately.
                    if (property.CanRead) {
                        deserializedValue = property.Read(instance);
                    }
*/
                    // 递归下去
                    var itemResult = Serializer.TryDeserialize(propertyData, property.StorageType,
                                                               property.OverrideConverterType, ref deserializedValue);
                    result.AddMessages(itemResult);
                    if (itemResult.Failed) continue;

                    // 往instance的property写入deserializedValue数据
                    property.Write(instance, deserializedValue);
                }
            }

            return result;
        }

        public override object CreateInstance(fsData data, Type storageType) {
            fsMetaType metaType = fsMetaType.Get(Serializer.Config, storageType);
            return metaType.CreateInstance();
        }
    }
}