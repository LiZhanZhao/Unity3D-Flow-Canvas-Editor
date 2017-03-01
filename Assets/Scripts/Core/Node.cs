using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace FlowCanvas.Framework
{
    abstract public partial class Node
    {
        private GraphBase _graphBase;
        private List<Connection> _inConnections = new List<Connection>();
        private List<Connection> _outConnections = new List<Connection>();
        
        private int _ID;
        
        

        public Graph graph
        {
            get
            {
                if (_graphBase is Graph)
                {
                    return _graphBase as Graph;
                }
                return null;
            }
        }

        public GraphBase graphBase
        {
            get { return _graphBase; }
            set { _graphBase = value; }
        }

        
        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        

        ///All incomming connections to this node
        public List<Connection> inConnections
        {
            get { return _inConnections; }
            protected set { _inConnections = value; }
        }

        ///All outgoing connections from this node
        public List<Connection> outConnections
        {
            get { return _outConnections; }
            protected set { _outConnections = value; }
        }

        public Node() { }

        public static Node Create(GraphBase targetGraph, System.Type nodeType, Rect rect)
        {

            if (targetGraph == null)
            {
                Debug.LogError("Can't Create a Node without providing a Target Graph");
                return null;
            }

            var newNode = (Node)System.Activator.CreateInstance(nodeType);
            newNode.rect = rect;
            newNode.graphBase = targetGraph;
            newNode.OnValidate(targetGraph);
            return newNode;
        }


        virtual protected void OnReset() { }
        
        


        
        virtual public void OnGraphStarted() { }

        // call when create node
        virtual public void OnValidate(GraphBase assignedGraph) { }

        virtual public void OnGraphUnpaused() { }
        virtual public void OnGraphPaused() { }

        virtual public void OnGraphStoped() { }
        
        virtual public void OnParentConnected(int connectionIndex) { }

        virtual public void OnParentDisconnected(int connectionIndex) { }
        
        virtual public void OnChildConnected(int connectionIndex) { }

        virtual public void OnChildDisconnected(int connectionIndex) { }

        virtual public void OnDestroy() { }
    }
}