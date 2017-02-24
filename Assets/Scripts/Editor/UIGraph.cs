using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace FlowCanvas.Framework
{
    
    public class UIGraph : GraphBase
    {
        public override System.Type baseNodeType { get { return typeof(FlowNode); } }

        public Vector2 scrollOffset;
        public float zoom = 1.0f;

        #if UNITY_EDITOR
        public void DrawNodes()
        {
            int nodeCount = allNodes.Count;
            for (int i = 0; i < nodeCount; i++)
            {
                Node node = allNodes[i];
                node.Draw();
            }
        }

        #endif
    }
}

