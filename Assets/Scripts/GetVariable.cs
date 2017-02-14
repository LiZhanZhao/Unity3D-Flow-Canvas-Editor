using UnityEngine;
using System.Collections;
using NodeCanvas.Framework;

namespace FlowCanvas.Nodes
{
    public class GetVariable<T> : VariableNode
    {

        public BBParameter<T> value;

        protected override void RegisterPorts()
        {
            AddValueOutput<T>("Value", () => { return value.value; });
        }

        public override void SetVariable(object o)
        {

            if (o is Variable<T>)
            {
                value.name = (o as Variable<T>).name;
                return;
            }

            if (o is T)
            {
                value.value = (T)o;
                return;
            }

            Debug.LogError("Set Variable Error");
        }
    }
}
