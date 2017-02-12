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
    }
}

