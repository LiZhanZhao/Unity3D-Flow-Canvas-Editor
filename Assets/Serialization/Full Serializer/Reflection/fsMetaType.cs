﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ParadoxNotion.Serialization.FullSerializer.Internal;

namespace ParadoxNotion.Serialization.FullSerializer {
    /// <summary>
    /// MetaType contains metadata about a type. This is used by the reflection serializer.
    /// </summary>
    public class fsMetaType {
        // 理解为缓存
        private static Dictionary<fsConfig, Dictionary<Type, fsMetaType>> _configMetaTypes =
            new Dictionary<fsConfig, Dictionary<Type, fsMetaType>>();

        public static fsMetaType Get(fsConfig config, Type type) {
            Dictionary<Type, fsMetaType> metaTypes;

            // 一个config对应一个Dictionary<Type, fsMetaType>
            if (_configMetaTypes.TryGetValue(config, out metaTypes) == false)
                metaTypes = _configMetaTypes[config] = new Dictionary<Type, fsMetaType>();

            // 一个type 对应一个 fsMetaType
            fsMetaType metaType;
            if (metaTypes.TryGetValue(type, out metaType) == false) {
                metaType = new fsMetaType(config, type);
                metaTypes[type] = metaType;
            }

            return metaType;
        }

        /// <summary>
        /// Clears out the cached type results. Useful if some prior assumptions become invalid, ie, the default member
        /// serialization mode.
        /// </summary>
        public static void ClearCache() {
            _configMetaTypes = new Dictionary<fsConfig, Dictionary<Type, fsMetaType>>();
        }

        // fsMetaType的构造函数
        private fsMetaType(fsConfig config, Type reflectedType) {
            // 初始化属性ReflectedType
            ReflectedType = reflectedType;

            List<fsMetaProperty> properties = new List<fsMetaProperty>();
            // 收集所有可以序列化的属性与字段，保存到properties中
            CollectProperties(config, properties, reflectedType);
            // 把properties保存到Properties中
            Properties = properties.ToArray();

#if UNITY_EDITOR || UNITY_STANDALONE
            try
            {
                if (!ReflectedType.Resolve().IsValueType && ReflectedType.GetDeclaredConstructor(fsPortableReflection.EmptyTypes) != null ){
                    Generator = Expression.Lambda<Func<object>>(Expression.New(reflectedType)).Compile();
                }
            }
            catch {Generator = null;}
#endif
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        private Func<object> Generator;

#endif

        public Type ReflectedType;

        private static void CollectProperties(fsConfig config, List<fsMetaProperty> properties, Type reflectedType) {
            // do we require a [SerializeField] or [fsProperty] attribute?
            bool requireOptIn = config.DefaultMemberSerialization == fsMemberSerialization.OptIn;
            bool requireOptOut = config.DefaultMemberSerialization == fsMemberSerialization.OptOut;

            // 检查是否有[fsObject] attribute
            fsObjectAttribute attr = fsPortableReflection.GetAttribute<fsObjectAttribute>(reflectedType);
            if (attr != null) {
                requireOptIn = attr.MemberSerialization == fsMemberSerialization.OptIn;
                requireOptOut = attr.MemberSerialization == fsMemberSerialization.OptOut;
            }

            // 获得所有的成员，包含方法与属性字段
            MemberInfo[] members = reflectedType.GetDeclaredMembers();
            foreach (var member in members) {
                // We don't serialize members annotated with any of the ignore serialize attributes
                // 跳过attribute 是 typeof(NonSerializedAttribute) 或者 typeof(fsIgnoreAttribute) 
                if (config.IgnoreSerializeAttributes.Any(t => fsPortableReflection.HasAttribute(member, t))) {
                    continue;
                }

                // 如果不是属性，字段的话，就跳过
                PropertyInfo property = member as PropertyInfo;
                FieldInfo field = member as FieldInfo;

                // Early out if it's neither a field or a property, since we don't serialize anything else.
                if (property == null && field == null) {
                    continue;
                }

                // 如果设置了requireOptIn，跳过那些Attribute不是UnityEngine.SerializeField和fsPropertyAttribute的
                // If an opt-in annotation is required, then skip the property if it doesn't have one
                // of the serialize attributes
                if (requireOptIn &&
                    !config.SerializeAttributes.Any(t => fsPortableReflection.HasAttribute(member, t))) {

                    continue;
                }

                // 如果设置了requireOptOut，跳过Attribute是 typeof(NonSerializedAttribute), typeof(fsIgnoreAttribute)的
                // If an opt-out annotation is required, then skip the property *only if* it has one of
                // the not serialize attributes
                if (requireOptOut &&
                    config.IgnoreSerializeAttributes.Any(t => fsPortableReflection.HasAttribute(member, t))) {

                    continue;
                }

                // 如果是属性的话，
                if (property != null) {
                    // 判断是否可以序列化这个Property
                    if (CanSerializeProperty(config, property, members, requireOptOut)) {
                        properties.Add(new fsMetaProperty(config, property));
                    }
                }
                // 如果是字段的话
                else if (field != null) {
                    // 判断是否可以序列化这个Field
                    if (CanSerializeField(config, field, requireOptOut)) {
                        properties.Add(new fsMetaProperty(config, field));
                    }
                }
            }

            // 处理基类，收集基类的属性
            if (reflectedType.Resolve().BaseType != null) {
                CollectProperties(config, properties, reflectedType.Resolve().BaseType);
            }
        }

        private static bool IsAutoProperty(PropertyInfo property, MemberInfo[] members) {
            if (!property.CanWrite || !property.CanRead) {
                return false;
            }

            string backingFieldName = "<" + property.Name + ">k__BackingField";
            for (int i = 0; i < members.Length; ++i) {
                if (members[i].Name == backingFieldName) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Returns if the given property should be serialized.
        /// </summary>
        /// <param name="annotationFreeValue">Should a property without any annotations be serialized?</param>
        private static bool CanSerializeProperty(fsConfig config, PropertyInfo property, MemberInfo[] members, bool annotationFreeValue) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(property.PropertyType)) {
                return false;
            }

            var publicGetMethod = property.GetGetMethod(/*nonPublic:*/ false);
            var publicSetMethod = property.GetSetMethod(/*nonPublic:*/ false);

            // We do not bother to serialize static fields or private fields.
            if ((publicGetMethod != null && publicGetMethod.IsStatic) ||
                (publicSetMethod != null && publicSetMethod.IsStatic)) {
                return false;
            }

            // Never serialize indexers. I can't think of a sane way to serialize/deserialize them, and they're normally wrappers around other fields anyway...
            if (property.GetIndexParameters().Length > 0) {
                return false;
            }

            // If a property is annotated with one of the serializable attributes, then it should
            // it should definitely be serialized.
            //
            // NOTE: We place this override check *after* the static check, because we *never*
            //       allow statics to be serialized.
            if (config.SerializeAttributes.Any(t => fsPortableReflection.HasAttribute(property, t))) {
                return true;
            }

            // If the property cannot be both read and written to, we are not going to serialize it
            // regardless of the default serialization mode
            if (property.CanRead == false || property.CanWrite == false) {
                return false;
            }

            // Depending on the configuration options, check whether the property is automatic
            // and if it has a public setter to determine whether it should be serialized
            if ((config.SerializeNonAutoProperties || IsAutoProperty(property, members)) &&
                (publicGetMethod != null && (config.SerializeNonPublicSetProperties || publicSetMethod != null))) {
                return true;
            }


            // Otherwise, we don't bother with serialization
            return annotationFreeValue;
        }

        private static bool CanSerializeField(fsConfig config, FieldInfo field, bool annotationFreeValue) {
            // We don't serialize delegates
            if (typeof(Delegate).IsAssignableFrom(field.FieldType)) {
                return false;
            }

            // We don't serialize compiler generated fields.
            if (field.IsDefined(typeof(CompilerGeneratedAttribute), false)) {
                return false;
            }

            // We don't serialize static fields
            if (field.IsStatic) {
                return false;
            }

            // We want to serialize any fields annotated with one of the serialize attributes.
            //
            // NOTE: This occurs *after* the static check, because we *never* want to serialize
            //       static fields.
            if (config.SerializeAttributes.Any(t => fsPortableReflection.HasAttribute(field, t))) {
                return true;
            }

            // We use !IsPublic because that also checks for internal, protected, and private.
            if (!annotationFreeValue && !field.IsPublic) {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Attempt to emit an AOT compiled direct converter for this type.
        /// </summary>
        /// <returns>True if AOT data was emitted, false otherwise.</returns>
        public bool EmitAotData() {
            if (_hasEmittedAotData == false) {
                _hasEmittedAotData = true;

                // NOTE:
                // Even if the type has derived types, we can still generate a direct converter for it.
                // Direct converters are not used for inherited types, so the reflected converter or something
                // similar will be used for the derived type instead of our AOT compiled one.

                for (int i = 0; i < Properties.Length; ++i) {
                    // Cannot AOT compile since we need to public member access.
                    if (Properties[i].IsPublic == false)
                        return false;
                }

                // Cannot AOT compile since we need a default ctor.
                if (HasDefaultConstructor == false)
                    return false;

                //fsAotCompilationManager.AddAotCompilation(ReflectedType, Properties, _isDefaultConstructorPublic);
                return true;
            }

            return false;
        }
        private bool _hasEmittedAotData;

        public fsMetaProperty[] Properties {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if the type represented by this metadata contains a default constructor.
        /// </summary>
        public bool HasDefaultConstructor {
            get {
                if (_hasDefaultConstructorCache.HasValue == false) {
                    // arrays are considered to have a default constructor
                    if (ReflectedType.Resolve().IsArray) {
                        _hasDefaultConstructorCache = true;
                        _isDefaultConstructorPublic = true;
                    }

                    // value types (ie, structs) always have a default constructor
                    else if (ReflectedType.Resolve().IsValueType) {
                        _hasDefaultConstructorCache = true;
                        _isDefaultConstructorPublic = true;
                    }

                    else {
                        // consider private constructors as well
                        var ctor = ReflectedType.GetDeclaredConstructor(fsPortableReflection.EmptyTypes);
                        _hasDefaultConstructorCache = ctor != null;
                        if (ctor != null) {
                            _isDefaultConstructorPublic = ctor.IsPublic;
                        }
                    }
                }

                return _hasDefaultConstructorCache.Value;
            }
        }
        private bool? _hasDefaultConstructorCache;
        private bool _isDefaultConstructorPublic;

        /// <summary>
        /// Creates a new instance of the type that this metadata points back to. If this type has a
        /// default constructor, then Activator.CreateInstance will be used to construct the type
        /// (or Array.CreateInstance if it an array). Otherwise, an uninitialized object created via
        /// FormatterServices.GetSafeUninitializedObject is used to construct the instance.
        /// </summary>
        public object CreateInstance() {
            if (ReflectedType.Resolve().IsInterface || ReflectedType.Resolve().IsAbstract) {
                throw new Exception("Cannot create an instance of an interface or abstract type for " + ReflectedType);
            }

#if !NO_UNITY
            // Unity requires special construction logic for types that derive from
            // ScriptableObject.
            if (typeof(UnityEngine.ScriptableObject).IsAssignableFrom(ReflectedType)) {
                return UnityEngine.ScriptableObject.CreateInstance(ReflectedType);
            }
#endif

            // Strings don't have default constructors but also fail when run through
            // FormatterSerivces.GetSafeUninitializedObject
            if (typeof(string) == ReflectedType) {
                return string.Empty;
            }

            if (HasDefaultConstructor == false) {
#if !UNITY_EDITOR && (UNITY_WEBPLAYER || UNITY_WP8 || UNITY_METRO)
                throw new InvalidOperationException("The selected Unity platform requires " +
                    ReflectedType.FullName + " to have a default constructor. Please add one.");
#else
                return System.Runtime.Serialization.FormatterServices.GetSafeUninitializedObject(ReflectedType);
#endif
            }

            if (ReflectedType.Resolve().IsArray) {
                // we have to start with a size zero array otherwise it will have invalid data
                // inside of it
                return Array.CreateInstance(ReflectedType.GetElementType(), 0);
            }

            try {

#if UNITY_EDITOR || UNITY_STANDALONE
                if (Generator != null){
                    return Generator();
                }
#endif

#if (!UNITY_EDITOR && (UNITY_METRO))
                // In WinRT/WinStore builds, Activator.CreateInstance(..., true) is broken
                return Activator.CreateInstance(ReflectedType);
#else
                return Activator.CreateInstance(ReflectedType, /*nonPublic:*/ true);
#endif
            }
#if (!UNITY_EDITOR && (UNITY_METRO)) == false
            catch (MissingMethodException e) {
                throw new InvalidOperationException("Unable to create instance of " + ReflectedType + "; there is no default constructor", e);
            }
#endif
            catch (TargetInvocationException e) {
                throw new InvalidOperationException("Constructor of " + ReflectedType + " threw an exception when creating an instance", e);
            }
            catch (MemberAccessException e) {
                throw new InvalidOperationException("Unable to access constructor of " + ReflectedType, e);
            }
        }
    }
}