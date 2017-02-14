using UnityEngine;
using System.Collections;
using NodeCanvas.Framework;

namespace FlowCanvas.Nodes
{
    public class GetVariable<T> : VariableNode
    {
        public T value;

        protected override void RegisterPorts()
        {
            AddValueOutput<T>("Value", () => { return value; });
        }

        public override void SetVariable(object o)
        {
            if (o is T)
            {
                value = (T)o;
                return;
            }

            Debug.LogError("Set Variable Error");
        }
    }
}
