using UnityEngine;
using System.Collections;
using FlowCanvas.Framework;

namespace FlowCanvas.Nodes
{
    abstract public class EventNode : FlowNode
    {
        public override string name
        {
            get { return string.Format("➥ {0}", base.name.ToUpper()); }
        }
    }
}

