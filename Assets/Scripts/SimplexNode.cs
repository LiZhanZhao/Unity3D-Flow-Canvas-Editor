using UnityEngine;
using System.Collections;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;


namespace FlowCanvas.Nodes
{

    ///SimplexNodes are used within a SimplexNodeWrapper node.
    ///Derive CallableActionNode, CallableFunctionNode, LatentActionNode, PureFunctionNode or ExtractorNode, for creating simple nodes easily and type cast safe.
    abstract public class SimplexNode
    {
        private string _name;
        private string _description;

        virtual public string name
        {
            get
            {
                if (string.IsNullOrEmpty(_name))
                {
                    var nameAtt = this.GetType().RTGetAttribute<NameAttribute>(false);
                    _name = nameAtt != null ? nameAtt.name : this.GetType().FriendlyName().SplitCamelCase();
                }
                return _name;
            }
        }

        virtual public string description
        {
            get
            {
                if (string.IsNullOrEmpty(_description))
                {
                    var descAtt = this.GetType().RTGetAttribute<DescriptionAttribute>(false);
                    _description = descAtt != null ? descAtt.description : "No Description";
                }
                return _description;
            }
        }

        // get Invoke method parameters
        protected ParameterInfo[] parameters
        {
            get { return this.GetType().GetMethod("Invoke", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).GetParameters(); }
        }

        public void RegisterPorts(FlowNode node)
        {

            OnRegisterPorts(node);

            //out parameters are done through public properties
            var type = this.GetType();
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (prop.CanRead && !prop.GetGetMethod().IsVirtual)
                {
                    node.AddPropertyOutput(prop, this);
                }
            }
        }

        abstract protected void OnRegisterPorts(FlowNode node);

        virtual public void OnGraphStarted() { }
        virtual public void OnGraphPaused() { }
        virtual public void OnGraphUnpaused() { }
        virtual public void OnGraphStoped() { }
    }
}
