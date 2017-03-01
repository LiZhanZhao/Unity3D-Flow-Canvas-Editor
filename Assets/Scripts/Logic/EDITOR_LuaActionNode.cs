using UnityEngine;
using System.Collections;
using FlowCanvas.Framework;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace FlowCanvas.Nodes
{
    public partial class LuaActionNode : LuaNode, IUpdatable
    {
#if UNITY_EDITOR
        override protected void OnNodeInspectorGUI()
        {
            base.OnNodeInspectorGUI();
            OnTargetGUI();
        }

        void OnTargetGUI() {
            TargetArgValues = EditorUtils.GenericField("targets", TargetArgValues, typeof(string), null, null) as string;
        }
#endif

    }
}

