using UnityEngine;
using System.Collections;
using FlowCanvas.Framework;

namespace FlowCanvas.Nodes
{
    abstract public class VariableNode : FlowNode
    {

        ///For setting the default variable
        abstract public void SetVariable(object o);
    }
}
