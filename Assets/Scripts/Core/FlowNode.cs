using UnityEngine;
using System.Collections.Generic;
using NodeCanvas.Framework;
using NodeCanvas;
using System.Linq;
using System;
using ParadoxNotion;
using System.Reflection;
using ParadoxNotion.Design;

namespace FlowCanvas
{
    abstract public class FlowNode : Node
    {
        private Dictionary<string, Port> _inputPorts = new Dictionary<string, Port>(StringComparer.Ordinal);
        private Dictionary<string, Port> _outputPorts = new Dictionary<string, Port>(StringComparer.Ordinal);
        sealed public override bool allowAsPrime { get { return false; } }

        private Dictionary<string, object> _inputPortValues;

        // be critical, this is will init inputPorts and outputPorts
        public override void OnValidate(Graph flowGraph)
        {
            GatherPorts();
        }

        public BinderConnection GetOutputConnectionForPortID(string ID)
        {
            return outConnections.OfType<BinderConnection>().FirstOrDefault(c => c.sourcePortID == ID);
        }

        

        public Port GetOutputPort(string ID)
        {
            Port output = null;
            _outputPorts.TryGetValue(ID, out output);
            return output;
        }

        
        public void GatherPorts()
        {

            _inputPorts.Clear();
            _outputPorts.Clear();
            RegisterPorts();
            ValidateConnections();
        }

        // Validate
        void ValidateConnections()
        {

            foreach (var cOut in outConnections.ToArray())
            { //ToArray because connection might remove itself if invalid
                if (cOut is BinderConnection)
                {
                    (cOut as BinderConnection).GatherAndValidateSourcePort();
                }
            }

            foreach (var cIn in inConnections.ToArray())
            {
                if (cIn is BinderConnection)
                {
                    (cIn as BinderConnection).GatherAndValidateTargetPort();
                }
            }
        }

        public Port GetInputPort(string ID)
        {
            Port input = null;
            _inputPorts.TryGetValue(ID, out input);
            return input;
        }

        // notice : this is a virtual function
        virtual protected void RegisterPorts()
        {
            DoReflectionBasedRegistration();
        }

        
        void DoReflectionBasedRegistration()
        {
            //FlowInputs. All void methods with one Flow parameter.
            //example
            /*
            [Name("bbb")]
            public void Test(Flow f){} ----> flowInput
            * */
            foreach (var method in this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var parameters = method.GetParameters();
                if (method.ReturnType == typeof(void) && parameters.Length == 1 && parameters[0].ParameterType == typeof(Flow))
                {
                    var nameAtt = method.RTGetAttribute<NameAttribute>(false);
                    var name = nameAtt != null ? nameAtt.name : method.Name.SplitCamelCase();
                    var pointer = method.RTCreateDelegate<FlowHandler>(this);
                    AddFlowInput(name, pointer);
                }
            }


            //ValueOutputs. All readable public properties.
            foreach (var prop in this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (prop.CanRead)
                {
                    AddPropertyOutput(prop, this);
                }
            }


            //Search for delegates fields
            foreach (var field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {

                if (typeof(Delegate).RTIsAssignableFrom(field.FieldType))
                {
                    //[Name("xxx")]
                    var nameAtt = field.RTGetAttribute<NameAttribute>(false);
                    var name = nameAtt != null ? nameAtt.name : field.Name.SplitCamelCase();

                    var invokeMethod = field.FieldType.GetMethod("Invoke");
                    var parameters = invokeMethod.GetParameters();

                    //FlowOutputs. All FlowHandler fields.
                    if (field.FieldType == typeof(FlowHandler))
                    {
                        var flowOut = AddFlowOutput(name);
                        field.SetValue(this, (FlowHandler)flowOut.Call);
                    }

                    //ValueInputs. All ValueHandler<T> fields.
                    if (invokeMethod.ReturnType != typeof(void) && parameters.Length == 0)
                    {
                        var delType = invokeMethod.ReturnType;
                        var portType = typeof(ValueInput<>).RTMakeGenericType(new Type[] { delType });
                        var port = (ValueInput)Activator.CreateInstance(portType, new object[] { this, name, name });

                        var getterType = typeof(ValueHandler<>).RTMakeGenericType(new Type[] { delType });
                        var getter = port.GetType().GetMethod("get_value").RTCreateDelegate(getterType, port);
                        field.SetValue(this, getter);
                        _inputPorts[name] = port;
                    }
                }
            }
        }

        public FlowOutput AddFlowOutput(string name, string ID = "")
        {
            if (string.IsNullOrEmpty(ID)) ID = name;
            return (FlowOutput)(_outputPorts[ID] = new FlowOutput(this, name, ID));
        }

        public FlowInput AddFlowInput(string name, FlowHandler pointer, string ID = "")
        {
            if (string.IsNullOrEmpty(ID)) ID = name;
            return (FlowInput)(_inputPorts[ID] = new FlowInput(this, name, ID, pointer));
        }

        public ValueInput<T> AddValueInput<T>(string name, string ID = "")
        {
            if (string.IsNullOrEmpty(ID)) ID = name;
            return (ValueInput<T>)(_inputPorts[ID] = new ValueInput<T>(this, name, ID));
        }

        public ValueInput AddValueInput(Port port, string name, string ID = "")
        {
            if (string.IsNullOrEmpty(ID)) ID = name;
            return (ValueInput)(_inputPorts[ID] = port);
        }

        public ValueOutput<T> AddValueOutput<T>(string name, ValueHandler<T> getter, string ID = "")
        {
            if (string.IsNullOrEmpty(ID)) ID = name;
            return (ValueOutput<T>)(_outputPorts[ID] = new ValueOutput<T>(this, name, ID, getter));
        }

        // only read property will become ValueOutput,ValueOutput's getter is only get {return value;}
        public ValueOutput AddPropertyOutput(PropertyInfo prop, object instance)
        {

            if (!prop.CanRead)
            {
                Debug.LogError("Property is write only");
                return null;
            }

            var nameAtt = prop.RTGetAttribute<NameAttribute>(false);
            var name = nameAtt != null ? nameAtt.name : prop.Name.SplitCamelCase();

            var getterType = typeof(ValueHandler<>).RTMakeGenericType(new Type[] { prop.PropertyType });
            // RTGetGetMethod : 当在派生类中重写时，返回此属性的公共或非公共 get 访问器
            var getter = prop.RTGetGetMethod().RTCreateDelegate(getterType, instance);
            var portType = typeof(ValueOutput<>).RTMakeGenericType(new Type[] { prop.PropertyType });
            var port = (ValueOutput)Activator.CreateInstance(portType, new object[] { this, name, name, getter });
            return (ValueOutput)(_outputPorts[name] = port);
        }

        

        public BinderConnection GetInputConnectionForPortID(string ID)
        {
            return inConnections.OfType<BinderConnection>().FirstOrDefault(c => c.targetPortID == ID);
        }

        public void BindPorts()
        {
            for (var i = 0; i < outConnections.Count; i++)
            {
                (outConnections[i] as BinderConnection).Bind();
            }
        }

    }
}

