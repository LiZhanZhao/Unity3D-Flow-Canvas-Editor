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
        private Status _status = Status.Resting;
        
        
        private int _ID;
        private bool isChecked { get; set; }
        

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

        
        
        public Status status
        {
            get { return _status; }
            protected set { _status = value; }
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
        
        public void Reset(bool recursively = true)
        {

            if (status == Status.Resting || isChecked)
                return;

            OnReset();
            status = Status.Resting;

            isChecked = true;
            for (var i = 0; i < outConnections.Count; i++)
            {
                outConnections[i].Reset(recursively);
            }
            isChecked = false;
        }

        public int AssignIDToGraph(int lastID)
        {

            if (isChecked)
            {
                return lastID;
            }

            isChecked = true;
            ID = lastID;
            lastID++;

            for (var i = 0; i < outConnections.Count; i++)
            {
                lastID = outConnections[i].targetNode.AssignIDToGraph(lastID);
            }

            return lastID;
        }

        public void ResetRecursion()
        {

            if (!isChecked)
            {
                return;
            }

            isChecked = false;
            for (var i = 0; i < outConnections.Count; i++)
            {
                outConnections[i].targetNode.ResetRecursion();
            }
        }


        
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