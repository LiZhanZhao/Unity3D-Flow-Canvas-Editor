using System.Collections;
using UnityEngine;
using FlowCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections.Generic;

namespace FlowCanvas.Nodes{
    //[Name("*** Root *** ")]
    //[Category("Events/Root")]
    //[DoNotList]
    //[Description("11111111111111111111.")]
	public class RootNode : FlowNode {

        private FlowOutput once;
        private bool called = false;

        [SerializeField]
        private List<string> _actorTypes = new List<string>();
        [SerializeField]
        private List<string> _actorNames = new List<string>();

        

        public override void OnGraphStarted()
        {
            if (!called)
            {
                Debug.Log("*** OnAwake ***");
                called = true;
                once.Call(new Flow(1));
                
            }
        }

        /*
        // 作为FlowInput
        [Name("bbb")]
        public void Test(Flow f)
        {

        }
         * */

        protected override void RegisterPorts()
        {
            once = AddFlowOutput("Once");
        }


#if UNITY_EDITOR

        private List<GameObject> _actorGos = new List<GameObject>();

        override protected void OnNodeInspectorGUI()
        {
            if (_actorGos.Count != _actorNames.Count)
            {
                _actorGos.Clear();
                for (int i = 0; i < _actorNames.Count; i++)
                {
                    _actorGos.Add(GameObject.Find(_actorNames[i]));
                }
            }

            UnityEditor.EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                _actorNames.Add("");
                _actorGos.Add(GameObject.Find(""));
                _actorTypes.Add("");
            }

            if (GUILayout.Button("-"))
            {
                int count = _actorTypes.Count;
                if (_actorTypes.Count > 0)
                {
                    _actorNames.RemoveAt(count - 1);
                    _actorGos.RemoveAt(count - 1);
                    _actorTypes.RemoveAt(count - 1);
                }
            }
            UnityEditor.EditorGUILayout.EndHorizontal();
            
            
            for (int i = 0; i < _actorTypes.Count; i++)
            {
                _actorTypes[i] = EditorUtils.GenericField(string.Format("Type {0}", i),
                    _actorTypes[i], typeof(string), null, null) as string;

                _actorGos[i] = EditorUtils.GenericField(string.Format("go {0}", i),_actorGos[i],
                    typeof(GameObject), null, null) as GameObject;

                if (_actorGos[i] != null)
                    _actorNames[i] = _actorGos[i].transform.name;

                EditorUtils.GenericField(string.Format("name {0}", i), _actorNames[i],
                    typeof(string), null, null);

                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.Space();
                UnityEditor.EditorGUILayout.Space();
            }

        }
#endif

	}
}