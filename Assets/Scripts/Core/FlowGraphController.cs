using System.Collections.Generic;
using UnityEngine;

namespace FlowCanvas.Framework
{

    ///Add this component on a game object to be controlled by a Flow Graph script (a FlowScript)
    public class FlowGraphController : GraphOwner<FlowGraph>
    {
        public void Play(string jsonContext)
        {
            FlowGraph fg = new FlowGraph();
            if (!fg.Deserialize(jsonContext, true))
            {
                Debug.LogError("Can not Deserialize File");
            }
            StartBehaviour(fg);
        }
    }
}