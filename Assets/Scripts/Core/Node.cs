using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework.Internal;
using ParadoxNotion;
using ParadoxNotion.Design;
using UnityEngine;


namespace NodeCanvas.Framework{
    abstract public partial class Node
    {
        private Graph _graph;
        private List<Connection> _inConnections = new List<Connection>();
        private List<Connection> _outConnections = new List<Connection>();
        private bool _isBreakpoint = false;
        private Status _status = Status.Resting;
        private string _nodeName;
        private string _name;
        private int _ID;
        private bool isChecked { get; set; }
        abstract public bool allowAsPrime { get; }
        private string _nodeDescription;
        

        public Graph graph
        {
            get { return _graph; }
            set { _graph = value; }
        }

        virtual public string description
        {
            get
            {
                if (string.IsNullOrEmpty(_nodeDescription))
                {
                    var descAtt = this.GetType().RTGetAttribute<DescriptionAttribute>(false);
                    _nodeDescription = descAtt != null ? descAtt.description : "No Description";
                }
                return _nodeDescription;
            }
        }

        public bool isBreakpoint
        {
            get { return _isBreakpoint; }
            set { _isBreakpoint = value; }
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

        

        private string customName
        {
            get { return _name; }
            set { _name = value; }
        }

        public Component graphAgent
        {
            get { return graph != null ? graph.agent : null; }
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

        

        virtual public void OnGraphStarted() { }
        // call when create node
        virtual public void OnValidate(Graph assignedGraph) { }

        virtual public void OnGraphUnpaused() { }
        virtual public void OnGraphPaused() { }

        virtual public void OnGraphStoped() { }
        virtual public void OnChildDisconnected(int connectionIndex) { }
        virtual public void OnParentDisconnected(int connectionIndex) { }
    }
}