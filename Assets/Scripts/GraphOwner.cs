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
        protected void Awake()
        {
            if (graph == null)
            {
                return;
            }


            // 下面的主要是确定graph属性
            //The graph is bound
            if (!string.IsNullOrEmpty(boundGraphSerialization))
            {

                //Case1: The graph is a bound instance in the scene.
                //Use it directly.
                if (graph.hideFlags == HideFlags.HideInInspector)
                {
                    instances[graph] = graph;
                    return;
                }

                //Case2: The graph is a bound asset reference. This takes place when instantiating prefabs.
                //Set object references before GetInstance, so that graph deserialize with correct references.
                // 直接把Graph._objectReferences 赋值给 boundGraphObjectReferences
                graph.SetSerializationObjectReferences(boundGraphObjectReferences);
                graph = GetInstance(graph);
                return;
            }

            //Case3: The graph is just a non-bound asset reference.
            //Instantiate and use it.
            graph = GetInstance(graph);
        }

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
            graph = GetInstance(graph);
            if (graph != null)
            {
                // blackboard 会调用GraphOwner<T>里面的，顺便初始化了，GraphOwner<T> 的 _blackboard
                graph.StartGraph(this, blackboard, true);
            }
        }

        protected Graph GetInstance(Graph originalGraph)
        {

            return originalGraph;


            if (originalGraph == null)
            {
                return null;
            }

            //in editor the instance is always the original
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return originalGraph;
            }
#endif

            //if its already an instance, return the instance
            if (instances.Values.Contains(originalGraph))
            {
                return originalGraph;
            }

            Graph instance = null;

            //if it's not an instance but rather an asset reference which has been instantiated before, return the instance stored,
            //otherwise create and store a new instance.

            if (!instances.TryGetValue(originalGraph, out instance))
            {
                instance = Graph.Clone<Graph>(originalGraph);
                instances[originalGraph] = instance;
            }

            instance.agent = this;
            instance.blackboard = this.blackboard;
            return instance;
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

        private static bool isQuiting;
        public DisableAction disableAction = DisableAction.DisableBehaviour;
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

        abstract public IBlackboard blackboard { get; set; }
    }

    abstract public class GraphOwner<T> : GraphOwner where T : Graph
    {
        private Blackboard _blackboard;
        sealed public override IBlackboard blackboard
        {
            get
            {
                // 因为Graph 是用FlowGraph的，useLocalBlackboard = false
                if (graph != null && graph.useLocalBlackboard)
                {
                    return graph.localBlackboard;
                }

                // 初始化
                if (_blackboard == null)
                {
                    _blackboard = GetComponent<Blackboard>();
                }

                return _blackboard;
            }
            set
            {
                if (!ReferenceEquals(_blackboard, value))
                {
                    _blackboard = (Blackboard)(object)value;
                    if (graph != null && !graph.useLocalBlackboard)
                    {
                        graph.blackboard = value;
                    }
                }
            }
        }

        private T _graph;
        sealed public override Graph graph
        {
            get { return _graph; }
            set { _graph = (T)value; }
        }


    }
}

