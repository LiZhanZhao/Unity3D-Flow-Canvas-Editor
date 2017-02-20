using System;
using System.Collections.Generic;
using ParadoxNotion.Serialization.FullSerializer;
using ParadoxNotion.Serialization.FullSerializer.Internal;
using UnityEngine;

namespace ParadoxNotion.Serialization{

    ///Serializes/Deserializes to/from JSON with 'FullSerializer'
    public static class JSONSerializer {
        
#if UNITY_EDITOR //this is used to avoid calling Unity API in serialization for the editor
        [UnityEditor.InitializeOnLoad]
        class StartUp{
            static StartUp(){
                //set to false since this is always called in editor start where application is not playing.
                JSONSerializer.applicationPlaying = false;
                UnityEditor.EditorApplication.playmodeStateChanged += ()=>
                {
                    JSONSerializer.applicationPlaying = Application.isPlaying && UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
                };
            }
        }
#endif

        private static Dictionary<string, fsData> cache = new Dictionary<string, fsData>();
        private static object serializerLock = new object();
        private static fsSerializer serializer = new fsSerializer();
        private static bool init = false;

        //initialize to true since StartUp is editor only and in runtime application is of course always playing.
        public static bool applicationPlaying = true;

        ///Serialize to json
        public static string Serialize(Type type, object value, bool pretyJson = false, List<UnityEngine.Object> objectReferences = null) {

            lock (serializerLock)
            {
                // 初始化的时候，就增加fsUnityObjectConverter
                if (!init){
                    serializer.AddConverter(new fsUnityObjectConverter());
                    init = true;
                }

                //set the objectReferences context
                if (objectReferences != null){
                    objectReferences.Clear(); //we clear the list since it will be populated by the converter.
                    serializer.Context.Set<List<UnityEngine.Object>>(objectReferences);
                }

                //serialize the data
                fsData data;
                //如果我们准备要序列化一个UnityObject的话，就直接用fsReflectedConverter
                //We override the UnityObject converter if we serialize a UnityObject directly.
                //UnityObject converter will still be used for every serialized property found within the object though.
                var overrideConverterType = typeof(UnityEngine.Object).RTIsAssignableFrom(type)? typeof(fsReflectedConverter) : null;
                serializer.TrySerialize(type, overrideConverterType, value, out data).AssertSuccess();

                //print data to json
                if (pretyJson){
                    return fsJsonPrinter.PrettyJson(data);
                }
                return fsJsonPrinter.CompressedJson(data);
            }
        }

        ///Deserialize generic
        public static T Deserialize<T>(string serializedState, List<UnityEngine.Object> objectReferences = null, T deserialized = default(T)){
            return (T)Deserialize(typeof(T), serializedState, objectReferences, deserialized);
        }

        ///Deserialize from json
        public static object Deserialize(Type type, string serializedState, List<UnityEngine.Object> objectReferences = null, object deserialized = null) {

            lock (serializerLock)
            {
                if (!init){
                    serializer.AddConverter(new fsUnityObjectConverter());
                    init = true;
                }

                //set the objectReferences context
                if (objectReferences != null){
                    serializer.Context.Set<List<UnityEngine.Object>>(objectReferences);
                }

                fsData data = null;
                //先检查缓冲，看看是不是已经反序列化过了
                cache.TryGetValue(serializedState, out data);
                // 之前没有反序列化过的情况下
                if (data == null){
                    // 把一个string 转为一个fsData
                    data = fsJsonParser.Parse(serializedState);
                    cache[serializedState] = data;                
                }

                //deserialize the data
                //We override the UnityObject converter if we deserialize a UnityObject directly.
                //UnityObject converter will still be used for every serialized property found within the object though.
                var overrideConverterType = typeof(UnityEngine.Object).RTIsAssignableFrom(type)? typeof(fsReflectedConverter) : null;
                serializer.TryDeserialize(data, type, overrideConverterType, ref deserialized).AssertSuccess();

                return deserialized;
            }
        }
    }
}