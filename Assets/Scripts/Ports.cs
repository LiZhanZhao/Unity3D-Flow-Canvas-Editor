using System;
using ParadoxNotion;
using UnityEngine;
using System.Reflection;

namespace FlowCanvas{

	///Delegate for Flow
	public delegate void FlowHandler(Flow f);
    public delegate T ValueHandler<T>();
    public struct Flow
    {
        public float value;
        public int ticks;
        public static Flow zero { get { return new Flow(0); } }
        public static Flow one { get { return new Flow(1); } }
        public Flow(float f)
        {
            ticks = 0;
            value = f;
        }
    }

    abstract public class Port {
        public Port() { }
        public Port(FlowNode parent, string name, string ID)
        {
            this.parent = parent;
            this.name = name;
            this.ID = ID;
        }

        ///The FlowNode parent of the port
        public FlowNode parent { get; private set; }

        ///The ID name of the port. Usualy is the same as the display name
        public string ID { get; private set; }

        ///The display name of the port
        public string name { get; private set; }

        ///The position of the port in the context of it's parent (editor use)
        public Vector2 pos { get; set; }

        ///The number of connections the port currently has
        public int connections { get; set; }

        ///Is the port connected?
        public bool isConnected
        {
            get { return connections > 0; }
        }

        ///The type of the port
        abstract public Type type { get; }

        ///Helper method to determine if a port can accept further connections
        public bool CanAcceptConnections()
        {
            if (this is ValueOutput || (this is FlowOutput && !this.isConnected))
            {
                return true;
            }
            if (this is FlowInput || (this is ValueInput && !this.isConnected))
            {
                return true;
            }
            return false;
        }
    }
    public class FlowOutput : Port
    {
        public FlowOutput(FlowNode parent, string name, string ID) : base(parent, name, ID) { }

        ///Used for port binding
        public FlowHandler pointer { get; private set; }
        ///The type of the port which is always type of Flow
        public override Type type { get { return typeof(Flow); } }

        ///Calls the target bound pointer
        public void Call(Flow f)
        {
            if (pointer != null && !parent.graph.isPaused)
            {
                f.ticks++;

#if UNITY_EDITOR

                if (parent.isBreakpoint)
                {
                    ParadoxNotion.Services.MonoManager.current.StartCoroutine(BreakWait(f));
                    return;
                }

                Continue(f);

#else

				pointer(f);

#endif
            }
        }

#if UNITY_EDITOR //Breakpoint helpers
        System.Collections.IEnumerator BreakWait(Flow f)
        {
            UnityEngine.Debug.LogWarning(string.Format("FlowScript Breakpoint Reached: Node '{0}'", parent.name), parent.graphAgent);
            UnityEngine.Debug.Break();
            parent.SetStatus(NodeCanvas.Status.Running);
            yield return null;
            parent.SetStatus(NodeCanvas.Status.Resting);
            Continue(f);
        }

        void Continue(Flow f)
        {
            try { pointer(f); }
            catch (Exception e)
            {
                var connection = parent.GetOutputConnectionForPortID(ID);
                var targetNode = (FlowNode)connection.targetNode;
                targetNode.Fail(string.Format("{0}\n{1}", e.Message, e.StackTrace));
            }
        }
#endif


        ///Bind the port to the target FlowInput
        public void BindTo(FlowInput target)
        {
            this.pointer = target.pointer;
        }

        ///Binds the port to a delegate directly
        public void BindTo(FlowHandler call)
        {
            this.pointer = call;
        }

        ///Unbinds the port
        public void UnBind()
        {
            this.pointer = null;
        }

        ///Appends a delegate when port is called
        public void Append(FlowHandler action)
        {
            this.pointer += action;
        }
    }

    abstract public class ValueInput : Port
    {
        public ValueInput() { }
        public ValueInput(FlowNode parent, string name, string ID) : base(parent, name, ID) { }
        public object value
        {
            get { return GetValue(); }
        }

        abstract public void BindTo(ValueOutput target);
        abstract public void UnBind();
        abstract public object GetValue();
        abstract public object serializedValue { get; set; }
        abstract public bool isDefaultValue { get; }
        abstract public override Type type { get; }
    }

    public class FlowInput : Port
    {
        public FlowInput(FlowNode parent, string name, string ID, FlowHandler pointer)
            : base(parent, name, ID)
        {
            this.pointer = pointer;
        }

        ///Used for port binding
        public FlowHandler pointer { get; private set; }
        ///The type of the port which is always type of Flow
        public override Type type { get { return typeof(Flow); } }
    }

    public class ValueInput<T> : ValueInput
    {
        public ValueInput() { }
        public ValueInput(FlowNode parent, string name, string ID) : base(parent, name, ID) { }

        ///Used for port binding
        public ValueHandler<T> getter { get; private set; }

        private T _value;

        new public T value
        {
            get
            {
                if (getter != null)
                {

#if UNITY_EDITOR

                    try { return getter(); }
                    catch (Exception e)
                    {
                        var connection = parent.GetInputConnectionForPortID(ID);
                        var targetNode = (FlowNode)connection.sourceNode;
                        targetNode.Fail(string.Format("{0}\n{1}", e.Message, e.StackTrace));
                    }

#else

					return getter();

#endif
                }

                return _value;
            }
        }

        public override object GetValue()
        {
            return value;
        }

        ///Basicaly return if the serializedValue is equal to default(T)
        public override bool isDefaultValue
        {
            get { return Equals(_value, default(T)); }
        }

        ///Used to get/set the serialized default value used when port is not connected
        public override object serializedValue
        {
            get { return _value; }
            set { _value = (T)value; }
        }

        ///The port value type which is always of type T
        public override Type type { get { return typeof(T); } }

        ///Binds the port to the target source ValueOutput port
        public override void BindTo(ValueOutput source)
        {
            if (source is ValueOutput<T>)
            {
                this.getter = (source as ValueOutput<T>).getter;
                return;
            }

            var func = (ValueHandler<object>)TypeConverter.GetConverterFuncFromTo(source.type, typeof(T), source.GetValue);
            this.getter = () => { return (T)func(); };
        }

        ///Binds the port to a delegate directly
        public void BindTo(ValueHandler<T> getter)
        {
            this.getter = getter;
        }

        ///Unbinds the port
        public override void UnBind()
        {
            this.getter = null;
        }
    }

    abstract public class ValueOutput : Port
    {
        public ValueOutput() { }
        public ValueOutput(FlowNode parent, string name, string ID) : base(parent, name, ID) { }

        ///Creates a generic instance of ValueOutput
        public static ValueOutput CreateInstance(Type t, FlowNode parent, string name, string ID, ValueHandler<object> getter)
        {
            if (getter == null)
            { //add a dummy to avoid ambigeus contructors
                getter = () => { return null; };
            }
            return (ValueOutput)Activator.CreateInstance(typeof(ValueOutput<>).RTMakeGenericType(new Type[] { t }), new object[] { parent, name, ID, getter });
        }

        ///Used only in case that a binder required casting cause of different port types
        abstract public object GetValue();
    }

    public class ValueOutput<T> : ValueOutput 
    {
        public ValueOutput() { }

        //normal
        public ValueOutput(FlowNode parent, string name, string ID, ValueHandler<T> getter)
            : base(parent, name, ID)
        {
            this.getter = getter;
        }

        //casted
        public ValueOutput(FlowNode parent, string name, string ID, ValueHandler<object> getter)
            : base(parent, name, ID)
        {
            this.getter = () => { return (T)getter(); };
        }

        ///Used for port binding
        public ValueHandler<T> getter { get; private set; }

        ///Used only in case that a binder required casting cause of different port types
        public override object GetValue() { return (object)getter(); }
        ///The type of the port
        public override Type type { get { return typeof(T); } }
    }

}