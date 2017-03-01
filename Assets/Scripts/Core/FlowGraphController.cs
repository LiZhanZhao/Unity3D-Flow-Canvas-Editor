using System.Collections.Generic;
using UnityEngine;

namespace FlowCanvas.Framework
{

    ///Add this component on a game object to be controlled by a Flow Graph script (a FlowScript)
    public class FlowGraphController : GraphOwner<FlowGraph>
    {
        private static FlowGraphController _current;
        public static FlowGraphController current
        {
            get
            {
                if (_current == null)
                {
                    _current = FindObjectOfType<FlowGraphController>();
                    if (_current == null)
                    {
                        _current = new GameObject("_FlowGraphController").AddComponent<FlowGraphController>();
                    }
                }
                return _current;
            }
        }

        public static void Play(string jsonContext, bool isRunInEditor = false)
        {
            FlowGraph fg = ScriptableObject.CreateInstance<FlowGraph>();
            fg.isRunEditor = isRunInEditor;
            if (!fg.Deserialize(jsonContext, true))
            {
                Debug.LogError("Can not Deserialize File");
            }
            current.StartBehaviour(fg);
        }

        public static void Stop()
        {
            current.StopBehaviour();
        }

        public static void Pause()
        {
            current.PauseBehaviour();
        }
    }
}