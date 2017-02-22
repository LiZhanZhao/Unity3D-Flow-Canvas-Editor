using UnityEngine;
using System.Collections;
using System.Reflection;
using ParadoxNotion;
using ParadoxNotion.Design;
using FlowCanvas.Framework;

namespace FlowCanvas.Nodes
{
    abstract public class CallableActionNodeBase : SimplexNode { }

    abstract public class CallableActionNode : CallableActionNodeBase
    {
        abstract public void Invoke();
        sealed protected override void OnRegisterPorts(FlowNode node)
        {
            var o = node.AddFlowOutput(" ");
            node.AddFlowInput(" ", (Flow f) => { Invoke(); o.Call(f); });
        }
    }


    abstract public class CallableActionNode<T1> : CallableActionNodeBase
    {
        abstract public void Invoke(T1 a);
        sealed protected override void OnRegisterPorts(FlowNode node)
        {
            var o = node.AddFlowOutput(" ");
            var p1 = node.AddValueInput<T1>(parameters[0].Name.SplitCamelCase());
            node.AddFlowInput(" ", (Flow f) => { Invoke(p1.value); o.Call(f); });
        }
    }
}
