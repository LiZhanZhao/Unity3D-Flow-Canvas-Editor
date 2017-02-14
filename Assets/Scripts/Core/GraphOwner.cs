using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NodeCanvas.Framework
{
    abstract public class GraphOwner : MonoBehaviour
    {
        abstract public Graph graph { get; set; }

        private string boundGraphSerialization;

        private List<UnityEngine.Object> boundGraphObjectReferences;

        private Dictionary<Graph, Graph> instances = new Dictionary<Graph, Graph>();

        private static bool isQuiting;
        public DisableAction disableAction = DisableAction.DisableBehaviour;



        public enum EnableAction
        {
            EnableBehaviour,
            DoNothing
        }

        private bool startCalled = false;
        public EnableAction enableAction = EnableAction.EnableBehaviour;
        

        protected void Start()
        {
            startCalled = true;
            if (enableAction == EnableAction.EnableBehaviour)
            {
                // 开始执行主要的流程
                StartBehaviour();
            }
        }

        public void StartBehaviour()
        {
            if (graph != null)
            {
                // blackboard 会调用GraphOwner<T>里面的，顺便初始化了，GraphOwner<T> 的 _blackboard
                graph.StartGraph(this, true);
            }
        }

        

        protected void OnEnable()
        {
            if (startCalled && enableAction == EnableAction.EnableBehaviour)
            {
                StartBehaviour();
            }
        }

        public enum DisableAction
        {
            DisableBehaviour,
            PauseBehaviour,
            DoNothing
        }

        
        protected void OnDisable()
        {

            if (isQuiting)
            {
                return;
            }

            if (disableAction == DisableAction.DisableBehaviour)
            {
                StopBehaviour();
            }

            if (disableAction == DisableAction.PauseBehaviour)
            {
                PauseBehaviour();
            }
        }

        public void PauseBehaviour()
        {
            if (graph != null)
            {
                graph.Pause();
            }
        }

        public void StopBehaviour()
        {
            if (graph != null)
            {
                graph.Stop();
            }
        }

        
    }

    abstract public class GraphOwner<T> : GraphOwner where T : Graph
    {
        private T _graph;
        sealed public override Graph graph
        {
            get { return _graph; }
            set { _graph = (T)value; }
        }

        public void StartBehaviour(T newGraph)
        {
            SwitchBehaviour(newGraph);
        }

        public void SwitchBehaviour(T newGraph)
        {
            SwitchBehaviour(newGraph, null);
        }

        public void SwitchBehaviour(T newGraph, System.Action<bool> callback)
        {
            StopBehaviour();
            graph = newGraph;
            StartBehaviour(callback);
        }

        public void StartBehaviour(System.Action<bool> callback)
        {
            if (graph != null)
            {
                graph.StartGraph(this, true, callback);
            }
        }


    }
}

