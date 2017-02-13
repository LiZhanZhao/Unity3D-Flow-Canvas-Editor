using UnityEngine;
using System.Collections;
using System;
using ParadoxNotion;

namespace NodeCanvas.Framework {
    abstract public class Variable
    {
        private string _name;
        private string _id;

        public event Action<string> onNameChanged;
        public event Action<string, object> onValueChanged;

        abstract public bool hasBinding { get; }

        public string name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    if (onNameChanged != null)
                    {
                        onNameChanged(value);
                    }
                }
            }
        }

        abstract public Type varType { get; }
        abstract protected object objectValue { get; set; }
        public object value
        {
            get { return objectValue; }
            set { objectValue = value; }
        }

        public bool CanConvertTo(Type toType) { return GetGetConverter(toType) != null; }

        public Func<object> GetGetConverter(Type toType)
        {

            //normal assignment
            if (toType.RTIsAssignableFrom(varType))
            {
                return () => { return value; };
            }

            //convertible to convertible
            if (typeof(IConvertible).RTIsAssignableFrom(toType) && typeof(IConvertible).RTIsAssignableFrom(varType))
            {
                return () => { try { return Convert.ChangeType(value, toType); } catch { return Activator.CreateInstance(toType); } };
            }

            //gameobject to transform
            if (toType == typeof(Transform) && varType == typeof(GameObject))
            {
                return () => { return value != null ? (value as GameObject).transform : null; };
            }

            //component to gameobject
            if (toType == typeof(GameObject) && typeof(Component).RTIsAssignableFrom(varType))
            {
                return () => { return value != null ? (value as Component).gameObject : null; };
            }

            //gameobject to vector3
            if (toType == typeof(Vector3) && varType == typeof(GameObject))
            {
                return () => { return value != null ? (value as GameObject).transform.position : Vector3.zero; };
            }

            //transform to vector3
            if (toType == typeof(Vector3) && varType == typeof(Transform))
            {
                return () => { return value != null ? (value as Transform).position : Vector3.zero; };
            }

            //quaternion to vector3
            if (toType == typeof(Vector3) && varType == typeof(Quaternion))
            {
                return () => { return (Vector3)((Quaternion)value).eulerAngles; };
            }

            //vector3 to quaternion
            if (toType == typeof(Quaternion) && varType == typeof(Vector3))
            {
                return () => { return (Quaternion)Quaternion.Euler((Vector3)value); };
            }

            return null;
        }

        public string ID
        {
            get
            {
                if (string.IsNullOrEmpty(_id))
                {
                    _id = Guid.NewGuid().ToString();
                }
                return _id;
            }
        }

        abstract public void InitializePropertyBinding(GameObject go, bool callSetter = false);

        protected bool HasValueChangeEvent()
        {
            return onValueChanged != null;
        }
        protected void OnValueChanged(string name, object value)
        {
            onValueChanged(name, value);
        }

        abstract public string propertyPath { get; set; }

        public bool CanConvertFrom(Type fromType) { return GetSetConverter(fromType) != null; }

        public Action<object> GetSetConverter(Type fromType)
        {

            //normal assignment
            if (varType.RTIsAssignableFrom(fromType))
            {
                return (o) => { value = o; };
            }

            //convertible to convertible
            if (typeof(IConvertible).RTIsAssignableFrom(varType) && typeof(IConvertible).RTIsAssignableFrom(fromType))
            {
                return (o) => { try { value = Convert.ChangeType(o, varType); } catch { value = Activator.CreateInstance(varType); } };
            }

            //gameobject to transform
            if (varType == typeof(Transform) && fromType == typeof(GameObject))
            {
                return (o) => { value = o != null ? (o as GameObject).transform : null; };
            }

            //component to gameobject
            if (varType == typeof(GameObject) && typeof(Component).RTIsAssignableFrom(fromType))
            {
                return (o) => { value = o != null ? (o as Component).gameObject : null; };
            }

            //Vector3 to gameobject
            if (varType == typeof(GameObject) && fromType == typeof(Vector3))
            {
                return (o) => { if (value != null) (value as GameObject).transform.position = (Vector3)o; };
            }

            //Vector3 to transform
            if (varType == typeof(Transform) && fromType == typeof(Vector3))
            {
                return (o) => { if (value != null) (value as Transform).position = (Vector3)o; };
            }

            //quaternion to vector3
            if (varType == typeof(Vector3) && fromType == typeof(Quaternion))
            {
                return (o) => { value = ((Quaternion)o).eulerAngles; };
            }

            //vector3 to quaternion
            if (varType == typeof(Quaternion) && fromType == typeof(Vector3))
            {
                return (o) => { value = Quaternion.Euler((Vector3)o); };
            }

            return null;
        }

    }

    public class Variable<T> : Variable
    {
        private T _value;
   
        private string _propertyPath;

        //required
        public Variable() { }

        //delegates for property binding
        private Func<T> getter;
        private Action<T> setter;
        //

        public override string propertyPath
        {
            get { return _propertyPath; }
            set { _propertyPath = value; }
        }

        public T GetValue() { return value; }

        new public T value
        {
            get { return getter != null ? getter() : _value; }
            set
            {
                if (base.HasValueChangeEvent())
                { //check this first to avoid possible unescessary value boxing
                    if (!object.Equals(_value, value))
                    {
                        this._value = value;
                        if (setter != null) setter(value);
                        base.OnValueChanged(name, value);
                    }
                    return;
                }

                if (setter != null)
                {
                    setter(value);
                    return;
                }

                this._value = value;
            }
        }

        public void SetValue(T newValue) { value = newValue; }

        public override bool hasBinding
        {
            get { return !string.IsNullOrEmpty(_propertyPath); }
        }

        public override void InitializePropertyBinding(GameObject go, bool callSetter = false)
        {

            if (!hasBinding || !Application.isPlaying)
            {
                return;
            }

            getter = null;
            setter = null;

            var idx = _propertyPath.LastIndexOf('.');
            var typeString = _propertyPath.Substring(0, idx);
            var memberString = _propertyPath.Substring(idx + 1);
            var type = ReflectionTools.GetType(typeString, /*fallback?*/ false);

            if (type == null)
            {
                Debug.LogError(string.Format("Type '{0}' not found for Blackboard Variable '{1}' Binding", typeString, name), go);
                return;
            }

            var prop = type.RTGetProperty(memberString);
            if (prop != null)
            {
                var getMethod = prop.RTGetGetMethod();
                var setMethod = prop.RTGetSetMethod();
                var isStatic = (getMethod != null && getMethod.IsStatic) || (setMethod != null && setMethod.IsStatic);
                var instance = isStatic ? null : go.GetComponent(type);
                if (instance == null && !isStatic)
                {
                    Debug.LogError(string.Format("A Blackboard Variable '{0}' is due to bind to a Component type that is missing '{1}'. Binding ignored", name, typeString));
                    return;
                }

                if (prop.CanRead)
                {
                    try { getter = getMethod.RTCreateDelegate<Func<T>>(instance); } //JIT
                    catch { getter = () => { return (T)getMethod.Invoke(instance, null); }; } //AOT
                }
                else
                {
                    getter = () => { Debug.LogError(string.Format("You tried to Get a Property Bound Variable '{0}', but the Bound Property '{1}' is Write Only!", name, _propertyPath)); return default(T); };
                }

                if (prop.CanWrite)
                {
                    try { setter = setMethod.RTCreateDelegate<Action<T>>(instance); } //JIT
                    catch { setter = (o) => { setMethod.Invoke(instance, new object[] { o }); }; } //AOT

                    if (callSetter)
                    {
                        setter(_value);
                    }

                }
                else
                {
                    setter = (o) => { Debug.LogError(string.Format("You tried to Set a Property Bound Variable '{0}', but the Bound Property '{1}' is Read Only!", name, _propertyPath)); };
                }

                return;
            }

            var field = type.RTGetField(memberString);
            if (field != null)
            {
                var instance = field.IsStatic ? null : go.GetComponent(type);
                if (instance == null && !field.IsStatic)
                {
                    Debug.LogError(string.Format("A Blackboard Variable '{0}' is due to bind to a Component type that is missing '{1}'. Binding ignored", name, typeString));
                    return;
                }
                if (field.IsReadOnly())
                {
                    T value = (T)field.GetValue(instance);
                    getter = () => { return value; };
                }
                else
                {
                    getter = () => { return (T)field.GetValue(instance); };
                    setter = (o) => { field.SetValue(instance, o); };
                }
                return;
            }

            Debug.LogError(string.Format("A Blackboard Variable '{0}' is due to bind to a property/field named '{1}' that does not exist on type '{2}'. Binding ignored", name, memberString, type.FullName));
        }

        public override Type varType
        {
            get { return typeof(T); }
        }

        protected override object objectValue
        {
            get { return value; }
            set { this.value = (T)value; }
        }
        
    }
}

