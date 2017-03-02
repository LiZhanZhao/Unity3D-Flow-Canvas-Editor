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

        //public int FlowOutCount
        //{
        //    set
        //    {
        //        _flowOutCount = value;
        //    }
        //    get { return _flowOutCount; }
        //}

        override protected void OnNodeInspectorGUI()
        {
            base.OnNodeInspectorGUI();
            OnTargetGUI();
            OnFlowOutputCountGUI();
        }

        void OnTargetGUI() {
            TargetArgValues = EditorUtils.GenericField("targets", TargetArgValues, typeof(string), null, null) as string;

        }

        void OnFlowOutputCountGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(string.Format("Out Count    {0}", _flowOutCount));
            if (GUILayout.Button("+"))
            {
                _flowOutCount++;
                ResetFlowOutList();
                GatherPorts();
            }

            if (GUILayout.Button("-"))
            {
                if (_flowOutCount > 1)
                {
                    _flowOutCount--;
                    _flowOutList.RemoveAt(_flowOutCount);
                    GatherPorts();
                }
                
                
            }
            EditorGUILayout.EndHorizontal();

        }
#endif

    }
}

