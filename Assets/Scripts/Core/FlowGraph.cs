using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowCanvas.Framework
{

    public class FlowGraph : Graph {
        sealed public override bool requiresAgent { get { return false; } }
        sealed public override bool requiresPrimeNode { get { return false; } }
        public override System.Type baseNodeType { get { return typeof(FlowNode); } }

        private bool hasInitialized;

        private List<IUpdatable> updatableNodes;

        protected override void OnGraphStarted()
        {
            // 初始化执行
            if (!hasInitialized)
            {
                updatableNodes = new List<IUpdatable>();
            }

            for (var i = 0; i < allNodes.Count; i++)
            {
                if (!hasInitialized)
                {
                    if (allNodes[i] is IUpdatable)
                    {
                        updatableNodes.Add((IUpdatable)allNodes[i]);
                    }
                }
            }

            // 处理FlowNode 类型的Node
            if (!hasInitialized)
            {
                for (var i = 0; i < allNodes.Count; i++)
                {
                    if (allNodes[i] is FlowNode)
                    {
                        var flowNode = (FlowNode)allNodes[i];
                        //flowNode.AssignSelfInstancePort();
                        flowNode.BindPorts();
                    }
                }
            }

            // 初始化完成
            hasInitialized = true;
        }

        protected override void OnGraphUpdate()
        {
            if (updatableNodes != null && updatableNodes.Count > 0)
            {
                for (var i = 0; i < updatableNodes.Count; i++)
                {
                    updatableNodes[i].Update();
                }
            }
        }

        protected override void OnGraphStoped()
        {

        }
    }
}

