using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ParadoxNotion.Serialization;
using System.Linq;
using ParadoxNotion;
using FlowCanvas.Nodes;

namespace FlowCanvas.Framework
{
    abstract public class GraphBase
    {
        protected List<Node> _nodes = new List<Node>();
        protected bool _deserializationFailed = false;
        abstract public System.Type baseNodeType { get; }

        private const int kNodeWidth = 200;
        private const int kNodeHeight = 50;

        public List<Node> allNodes
        {
            get { return _nodes; }
            private set { _nodes = value; }
        }

        //public void UpdateNodeIDs(bool alsoReorderList)
        //{
        //    var lastID = 0;

        //    //set the rest starting from nodes without parent(s)
        //    var tempList = allNodes.OrderBy(n => n.inConnections.Count != 0).ToList();
        //    for (var i = 0; i < tempList.Count; i++)
        //    {
        //        lastID = tempList[i].AssignIDToGraph(lastID);
        //    }

        //    //reset the check
        //    for (var i = 0; i < allNodes.Count; i++)
        //    {
        //        allNodes[i].ResetRecursion();
        //    }

        //    if (alsoReorderList)
        //    {
        //        allNodes = allNodes.OrderBy(node => node.ID).ToList();
        //    }
        //}

        //void ISerializationCallbackReceiver.OnBeforeSerialize()
        //{
        //    Serialize();
        //}
        //void ISerializationCallbackReceiver.OnAfterDeserialize() {
        //    Deserialize();
        //}

        public bool Deserialize(string jsonContext, bool validate)
        {
            if (string.IsNullOrEmpty(jsonContext))
            {
                return false;
            }

            try
            {
                GraphSerializationData data = JSONSerializer.Deserialize<GraphSerializationData>(jsonContext);
                if (LoadGraphData(data, validate) == true)
                {
                    this._deserializationFailed = false;
                    return true;
                }

                _deserializationFailed = true;
                return false;
            }
            catch (System.Exception e)
            {
                Debug.LogError(string.Format("<b>(Deserialization Error:)</b> {0}", e.ToString()));
                _deserializationFailed = true;
                return false;
            }

        }
        virtual public void OnDerivedDataDeserialization(object data) { }

        // graph -> GraphSerializationData
        virtual public object OnDerivedDataSerialization() { return null; }


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

            //IMPORTANT: Validate should be called in all deserialize cases outside of Unity's 'OnAfterDeserialize',
            //like for example when loading from json, or manualy calling this outside of OnAfterDeserialize.
            if (validate)
            {
                Validate();
            }

            return true;
        }


        virtual protected void OnGraphValidate() { }

        public void Validate()
        {
            for (var i = 0; i < allNodes.Count; i++)
            {
                try { allNodes[i].OnValidate(this); } //validation could be critical. we always continue
                catch (System.Exception e) { Debug.LogError(e.ToString()); continue; }
            }

            OnGraphValidate();
        }

        public string Serialize(bool pretyJson)
        {
            
            return JSONSerializer.Serialize(typeof(GraphSerializationData), new GraphSerializationData(this), pretyJson);
        }

        ///Add a new node to this graph
        public T AddNode<T>() where T : Node
        {
            return (T)AddNode(typeof(T));
        }

        public T AddNode<T>(Vector2 pos) where T : Node
        {
            return (T)AddNode(typeof(T), pos);
        }

        public Node AddNode(System.Type nodeType)
        {
            return AddNode(nodeType, new Vector2(50, 50));
        }

        ///Add a new node to this graph
        public Node AddNode(System.Type nodeType, Vector2 pos)
        {
            // 这里baseNodeType是typeof(FlowNode)
            if (!nodeType.RTIsSubclassOf(baseNodeType))
            {
                Debug.LogWarning(nodeType + " can't be added to " + this.GetType().FriendlyName() + " graph");
                return null;
            }
            Rect rect = new Rect(pos, new Vector2(kNodeWidth, kNodeHeight));
            var newNode = Node.Create(this, nodeType, rect);
            newNode.ID = allNodes.Count;
            allNodes.Add(newNode);
            return newNode;
        }

        public void RemoveNode(Node node, bool recordUndo = true)
        {
            if (!allNodes.Contains(node))
            {
                Debug.LogWarning("Node is not part of this graph");
                return;
            }

            //callback
            node.OnDestroy();

            //disconnect parents
            foreach (var inConnection in node.inConnections.ToArray())
            {
                RemoveConnection(inConnection);
            }

            //disconnect children
            foreach (var outConnection in node.outConnections.ToArray())
            {
                RemoveConnection(outConnection);
            }

            allNodes.Remove(node);
        }

        public void RemoveConnection(Connection connection)
        {
            connection.OnDestroy();
            connection.sourceNode.OnChildDisconnected(connection.sourceNode.outConnections.IndexOf(connection));
            connection.targetNode.OnParentDisconnected(connection.targetNode.inConnections.IndexOf(connection));

            connection.sourceNode.outConnections.Remove(connection);
            connection.targetNode.inConnections.Remove(connection);
        }

    }

    
}

