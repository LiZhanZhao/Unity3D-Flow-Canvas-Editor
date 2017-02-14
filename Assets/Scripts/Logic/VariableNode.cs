using UnityEngine;
using System.Collections;

namespace FlowCanvas.Nodes
{
    abstract public class VariableNode : FlowNode
    {

        ///For setting the default variable
        abstract public void SetVariable(object o);
    }
}
