using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NodeCanvas.Framework.Internal;
//using ParadoxNotion.Serialization;
using System.Linq;
using ParadoxNotion.Services;

namespace NodeCanvas.Framework
{

    abstract public partial class Graph : ScriptableObject, ITaskSystem//, ISerializationCallbackReceiver
    {
        private List<Object> _objectReferences;
        private bool _deserializationFailed = false;
        private string _serializedGraph;
        private List<Node> _nodes = new List<Node>();
        private Node _primeNode = null;
        private BlackboardSource _localBlackboard = null;
        private bool _isRunning;
        public event System.Action<bool> OnFinish;
        private static List<Graph> runningGraphs = new List<Graph>();
        private float timeStarted;
        abstract public bool requiresAgent { get; }
        abstract public bool requiresPrimeNode { get; }

        private bool hasDeserialized = false;

        abstract public bool useLocalBlackboard { get; }
        private IBlackboard _blackboard;

        public List<Node> allNodes
        {
            get { return _nodes; }
            private set { _nodes = value; }
        }

        // GraphSerializationData->graph
        virtual public void OnDerivedDataDeserialization(object data) { }

        // graph -> GraphSerializationData
        virtual public object OnDerivedDataSerialization() { return null; }

        virtual protected void OnGraphValidate() { }

        virtual protected void OnGraphStarted() { }

        virtual protected void OnGraphUpdate() { }

        virtual protected void OnGraphUnpaused() { }

        virtual protected void OnGraphPaused() { }

        virtual protected void OnGraphStoped() { }

        public bool Deserialize(GraphSerializationData data, bool validate, List<UnityEngine.Object> objectReferences)
        {
            if (data == null)
            {
                return false;
            }

            if (objectReferences == null)
            {
                objectReferences = this._objectReferences;
            }

            try
            {
                //deserialize provided serialized graph into a new GraphSerializationData object and load it
                //var data = JSONSerializer.Deserialize<GraphSerializationData>(serializedGraph, objectReferences);
                if (LoadGraphData(data, validate) == true)
                {
                    this._deserializationFailed = false;
                    //this._serializedGraph = serializedGraph;
                    this._objectReferences = objectReferences;
                    //return data;
                    return true;
                }

                _deserializationFailed = true;
                //return null;
                return false;
            }
            catch (System.Exception e)
            {
                //Debug.LogError(string.Format("<b>(Deserialization Error:)</b> {0}", e.ToString()), this);
                Debug.LogError(string.Format("<b>(Deserialization Error:)</b> {0}", e.ToString()));
                _deserializationFailed = true;
                //return null;
                return false;
            }

        }

        //public GraphSerializationData Deserialize(string serializedGraph, bool validate, List<UnityEngine.Object> objectReferences)
        //{
        //    if (string.IsNullOrEmpty(serializedGraph))
        //    {
        //        return null;
        //    }

        //    //the list to load the references from. If not provided externaly we load from the local list (which is the case most times)
        //    if (objectReferences == null)
        //    {
        //        objectReferences = this._objectReferences;
        //    }

        //    try
        //    {
        //        //deserialize provided serialized graph into a new GraphSerializationData object and load it
        //        var data = JSONSerializer.Deserialize<GraphSerializationData>(serializedGraph, objectReferences);
        //        if (LoadGraphData(data, validate) == true)
        //        {
        //            this._deserializationFailed = false;
        //            this._serializedGraph = serializedGraph;
        //            this._objectReferences = objectReferences;
        //            return data;
        //        }

        //        _deserializationFailed = true;
        //        return null;
        //    }
        //    catch (System.Exception e)
        //    {
        //        //Debug.LogError(string.Format("<b>(Deserialization Error:)</b> {0}", e.ToString()), this);
        //        Debug.LogError(string.Format("<b>(Deserialization Error:)</b> {0}", e.ToString()));
        //        _deserializationFailed = true;
        //        return null;
        //    }
        //}

        bool LoadGraphData(GraphSerializationData data, bool validate)
        {

            if (data == null)
            {
                Debug.LogError("Can't Load graph, cause of null GraphSerializationData provided");
                return false;
            }

            
            // data.connections and data.nodes to graph
            data.Reconstruct(this);

            this._nodes = data.nodes;
            this._primeNode = data.primeNode;

            //this._localBlackboard = data.localBlackboard;

            //IMPORTANT: Validate should be called in all deserialize cases outside of Unity's 'OnAfterDeserialize',
            //like for example when loading from json, or manualy calling this outside of OnAfterDeserialize.
            if (validate)
            {
                Validate();
            }

            return true;
        }

        

        public void Validate()
        {
            for (var i = 0; i < allNodes.Count; i++)
            {
                try { allNodes[i].OnValidate(this); } //validation could be critical. we always continue
                catch (System.Exception e) { Debug.LogError(e.ToString()); continue; }
            }

            var allTasks = GetAllTasksOfType<Task>();
            for (var i = 0; i < allTasks.Count; i++)
            {
                try { allTasks[i].OnValidate(this); } //validation could be critical. we always continue
                catch (System.Exception e) { Debug.LogError(e.ToString()); continue; }
            }

            OnGraphValidate();

            //in runtime and if graph uses local blackboard, initialize property/field binding.
            //'null' target gameObject (since localBlackboard can only have static bindings).
            //'false' so that setter is not called.
            if (Application.isPlaying && useLocalBlackboard)
            {
                localBlackboard.InitializePropertiesBinding(null, false);
            }
        }

        

        public void UpdateReferences()
        {
            //Update all graph node's BBFields for current Blackboard. 
            UpdateNodeBBFields();
            //Sets all graph Tasks' owner (which is this)(Task Node)
            SendTaskOwnerDefaults();
        }

        void UpdateNodeBBFields()
        {
            for (var i = 0; i < allNodes.Count; i++)
            {
                BBParameter.SetBBFields(allNodes[i], blackboard);
            }
        }
        public void SendTaskOwnerDefaults()
        {
            var tasks = GetAllTasksOfType<Task>();
            for (var i = 0; i < tasks.Count; i++)
            {
                tasks[i].SetOwnerSystem(this);
            }
        }

        

        public BlackboardSource localBlackboard
        {
            get
            {
                if (ReferenceEquals(_localBlackboard, null))
                {
                    _localBlackboard = new BlackboardSource();
                    _localBlackboard.name = "Local Blackboard";
                }
                return _localBlackboard;
            }
        }

        public IBlackboard blackboard
        {
            get
            {
                if (useLocalBlackboard) { return localBlackboard; }
                return _blackboard;
            }
            set
            {
                if (_blackboard != value)
                {
                    if (isRunning) { return; }
                    if (useLocalBlackboard) { return; }
                    _blackboard = value;
                }
            }
        }

        public bool isRunning
        {
            get { return _isRunning; }
            private set { _isRunning = value; }
        }

        public List<T> GetAllTasksOfType<T>() where T : Task
        {

            var tasks = new List<Task>();
            var resultTasks = new List<T>();

            // 
            for (var i = 0; i < allNodes.Count; i++)
            {
                var node = allNodes[i];
                if (node is ITaskAssignable && (node as ITaskAssignable).task != null)
                {
                    tasks.Add((node as ITaskAssignable).task);
                }

                if (node is ISubTasksContainer)
                {
                    tasks.AddRange((node as ISubTasksContainer).GetTasks());
                }

                for (var j = 0; j < node.outConnections.Count; j++)
                {
                    var c = node.outConnections[j];
                    if (c is ITaskAssignable && (c as ITaskAssignable).task != null)
                    {
                        tasks.Add((c as ITaskAssignable).task);
                    }
                    if (c is ISubTasksContainer)
                    {
                        tasks.AddRange((c as ISubTasksContainer).GetTasks());
                    }
                }
            }

            for (var i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                if (task is ActionList)
                {
                    resultTasks.AddRange((task as ActionList).actions.OfType<T>());
                }
                if (task is ConditionList)
                {
                    resultTasks.AddRange((task as ConditionList).conditions.OfType<T>());
                }
                if (task is T)
                {
                    resultTasks.Add((T)task);
                }
            }

            return resultTasks;
        }

        private bool _isPaused;

        public bool isPaused
        {
            get { return _isPaused; }
            private set { _isPaused = value; }
        }

        private Component _agent;

        public Component agent
        {
            get { return _agent; }
            set { _agent = value; }
        }

        public void RemoveConnection(Connection connection, bool recordUndo = true)
        {

            //for live editing
            if (Application.isPlaying)
            {
                connection.Reset();
            }

            if (recordUndo)
            {
                RecordUndo("Delete Connection");
            }

            //callbacks
            connection.OnDestroy();
            connection.sourceNode.OnChildDisconnected(connection.sourceNode.outConnections.IndexOf(connection));
            connection.targetNode.OnParentDisconnected(connection.targetNode.inConnections.IndexOf(connection));

            connection.sourceNode.outConnections.Remove(connection);
            connection.targetNode.inConnections.Remove(connection);


            UpdateNodeIDs(false);
        }

        void RecordUndo(string name)
        {

        }

        public Node primeNode
        {
            get { return _primeNode; }
            set
            {
                if (_primeNode != value)
                {

                    if (value != null && value.allowAsPrime == false)
                    {
                        return;
                    }

                    if (isRunning)
                    {
                        if (_primeNode != null) _primeNode.Reset();
                        if (value != null) value.Reset();
                    }

                    RecordUndo("Set Start");

                    _primeNode = value;
                    UpdateNodeIDs(true);
                }
            }
        }

        public void UpdateNodeIDs(bool alsoReorderList)
        {

            var lastID = 0;

            //start from prime
            if (primeNode != null)
            {
                lastID = primeNode.AssignIDToGraph(lastID);
            }

            //set the rest starting from nodes without parent(s)
            var tempList = allNodes.OrderBy(n => n.inConnections.Count != 0).ToList();
            for (var i = 0; i < tempList.Count; i++)
            {
                lastID = tempList[i].AssignIDToGraph(lastID);
            }

            //reset the check
            for (var i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].ResetRecursion();
            }

            if (alsoReorderList)
            {
                allNodes = allNodes.OrderBy(node => node.ID).ToList();
            }
        }
        public void SetSerializationObjectReferences(List<UnityEngine.Object> references)
        {
            this._objectReferences = references;
        }

        public static T Clone<T>(T graph) where T : Graph
        {
            var newGraph = (T)Instantiate(graph);
            newGraph.name = newGraph.name.Replace("(Clone)", "");
            return (T)newGraph;
        }

        

        public void StartGraph(Component agent, IBlackboard blackboard, bool autoUpdate, System.Action<bool> callback = null)
        {
            if (isRunning)
            {
                if (callback != null)
                {
                    OnFinish += callback;
                }
                Debug.LogWarning("<b>Graph:</b> Graph is already Active.");
                return;
            }
            
            if (agent == null && requiresAgent)
            {
                Debug.LogWarning("<b>Graph:</b> You've tried to start a graph with null Agent.");
                return;
            }

            if (primeNode == null && requiresPrimeNode)
            {
                Debug.LogWarning("<b>Graph:</b> You've tried to start graph without 'Start' node");
                return;
            }

            if (blackboard == null)
            {
                if (agent != null)
                {
                    Debug.Log("<b>Graph:</b> Graph started without blackboard. Looking for blackboard on agent '" + agent.gameObject + "'...", agent.gameObject);
                    blackboard = agent.GetComponent(typeof(IBlackboard)) as IBlackboard;
                    if (blackboard != null)
                    {
                        Debug.Log("<b>Graph:</b> Blackboard found");
                    }
                }
                if (blackboard == null)
                {
                    Debug.LogWarning("<b>Graph:</b> Started with null Blackboard. Using Local instead...");
                    blackboard = localBlackboard;
                }
            }

            this.agent = agent;
            this.blackboard = blackboard;

            // deal with Task node and node BBField
            UpdateReferences();

            // 添加回调
            if (callback != null)
            {
                this.OnFinish = callback;
            }

            isRunning = true;
            runningGraphs.Add(this);

            if (autoUpdate)
            {
                MonoManager.current.onUpdate += UpdateGraph;
            }

            if (!isPaused)
            {
                timeStarted = Time.time;
                OnGraphStarted();
            }
            else
            {
                OnGraphUnpaused();
            }

            // all node call OnGraphStarted
            for (var i = 0; i < allNodes.Count; i++)
            {
                if (!isPaused)
                {
                    allNodes[i].OnGraphStarted();
                }
                else
                {
                    allNodes[i].OnGraphUnpaused();
                }
            }

            isPaused = false;
        }

        public void UpdateGraph() { OnGraphUpdate(); }

        

        public void Pause()
        {

            if (!isRunning)
            {
                return;
            }

            runningGraphs.Remove(this);
            MonoManager.current.onUpdate -= UpdateGraph;

            isRunning = false;
            isPaused = true;

            for (var i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].OnGraphPaused();
            }
            OnGraphPaused();
        }

        public void Stop(bool success = true)
        {

            if (!isRunning && !isPaused)
            {
                return;
            }

            runningGraphs.Remove(this);
            MonoManager.current.onUpdate -= UpdateGraph;

            isRunning = false;
            isPaused = false;

            for (var i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].Reset(false);
                allNodes[i].OnGraphStoped();
            }

            OnGraphStoped();

            if (OnFinish != null)
            {
                OnFinish(success);
                OnFinish = null;
            }
        }

        

        //void ISerializationCallbackReceiver.OnBeforeSerialize()
        //{
        //    Serialize();
        //}
        //void ISerializationCallbackReceiver.OnAfterDeserialize() {
        //    Deserialize();
        //}

        

//        public void Serialize()
//        {
//            if (_objectReferences != null && _objectReferences.Count > 0 && _objectReferences.Any(o => o != null))
//            { //Unity requires double deserialize for UnityObject refs.
//                hasDeserialized = false;
//            }

//#if UNITY_EDITOR //we only serialize in the editor
//            if (JSONSerializer.applicationPlaying)
//            {
//                return;
//            }

//            ///Serialize the graph and returns the serialized json string
//            /// _serializedGraph 保存的就是Json String，注意_objectReferences也参与序列化
//            _serializedGraph = this.Serialize(false, _objectReferences);

//            //notify owner. This is used for bound graphs
//            var owner = agent != null && agent is GraphOwner ? (GraphOwner)agent : null;
//            //if (owner != null)
//            //{
//            //    owner.OnGraphSerialized(this);
//            //}
//#endif
//        }

        //public string Serialize(bool pretyJson, List<UnityEngine.Object> objectReferences)
        //{
        //    //if something went wrong on deserialization, dont serialize back, but rather keep what we had
        //    // 反序列化失败的情况
        //    if (_deserializationFailed)
        //    {
        //        _deserializationFailed = false;
        //        return _serializedGraph;
        //    }

        //    //the list to save the references in. If not provided externaly we save into the local list
        //    if (objectReferences == null)
        //    {
        //        objectReferences = this._objectReferences = new List<Object>();
        //    }

        //    UpdateNodeIDs(true);
        //    // 这里就把一个Graph的数据填充到GraphSerializationData结构中，再变成Json String
        //    // 注意这里，objectReferences也传进去的，理解为，也序列化objectReferences
        //    return JSONSerializer.Serialize(typeof(GraphSerializationData), new GraphSerializationData(this), pretyJson, objectReferences);
        //}

        //public void Deserialize()
        //{
        //    if (hasDeserialized && JSONSerializer.applicationPlaying)
        //    { //avoid double call if not needed (UnityObject refs).
        //        return;
        //    }
        //    hasDeserialized = true;
        //    this.Deserialize(_serializedGraph, false, _objectReferences);
        //}

    }
}