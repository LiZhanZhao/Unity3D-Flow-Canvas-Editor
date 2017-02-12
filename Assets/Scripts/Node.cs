using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using ParadoxNotion.Serialization;
using ParadoxNotion.Serialization.FullSerializer;
using ParadoxNotion.Services;
using UnityEngine;


namespace NodeCanvas.Framework{
    [System.Serializable]
    abstract public partial class Node
    {
        private Graph _graph;

        public Graph graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        private bool _isBreakpoint = false;

        public bool isBreakpoint
        {
            get { return _isBreakpoint; }
            set { _isBreakpoint = value; }
        }

        private Status _status = Status.Resting;
        public Status status
        {
            get { return _status; }
            protected set { _status = value; }
        }

        virtual public string name
        {
            get
            {
                if (!string.IsNullOrEmpty(customName))
                {
                    return customName;
                }

                if (string.IsNullOrEmpty(_nodeName))
                {
                    var nameAtt = this.GetType().RTGetAttribute<NameAttribute>(false);
                    _nodeName = nameAtt != null ? nameAtt.name : GetType().FriendlyName().SplitCamelCase();
                }
                return _nodeName;
            }
            set { customName = value; }
        }

        private string _nodeName;
        private string _name;

        private string customName
        {
            get { return _name; }
            set { _name = value; }
        }

        public Component graphAgent
        {
            get { return graph != null ? graph.agent : null; }
        }

        private List<Connection> _inConnections = new List<Connection>();
        
        private List<Connection> _outConnections = new List<Connection>();

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

        virtual public void OnGraphStarted() { }

        virtual public void OnValidate(Graph assignedGraph) { }

        virtual protected void OnReset() { }
        private bool isChecked { get; set; }
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

        virtual public void OnChildDisconnected(int connectionIndex) { }
        virtual public void OnParentDisconnected(int connectionIndex) { }

        private int _ID;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public int AssignIDToGraph(int lastID)
        {

            if (isChecked)
            {
                return lastID;
            }

            isChecked = true;
            lastID++;
            ID = lastID;

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

        abstract public bool allowAsPrime { get; }

        virtual public void OnGraphUnpaused() { }

        virtual public void OnGraphPaused() { }

        virtual public void OnGraphStoped() { }
    }
}