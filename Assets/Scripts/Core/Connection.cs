using UnityEngine;
using System.Collections;

namespace NodeCanvas.Framework
{
    // 连线类
    abstract public partial class Connection
    {
        [SerializeField]
        private Node _sourceNode;
        [SerializeField]
        private Node _targetNode;
        private Status _status = Status.Resting;
        private bool _isDisabled;
        ///The source node of the connection
        public Node sourceNode
        {
            get { return _sourceNode; }
            //protected set { _sourceNode = value; }
            set { _sourceNode = value; }
        }

        ///The target node of the connection
        public Node targetNode
        {
            get { return _targetNode; }
            //protected set { _targetNode = value; }
            set { _targetNode = value; }
        }
        protected Graph graph
        {
            get { return sourceNode.graph; }
        }

        public Status status
        {
            get { return _status; }
            set { _status = value; }
        }

        public void Reset(bool recursively = true)
        {
            if (status == Status.Resting)
            {
                return;
            }
            status = Status.Resting;
            if (recursively)
            {
                targetNode.Reset(recursively);
            }
        }

        virtual public void OnDestroy() { }

        public bool isActive
        {
            get { return !_isDisabled; }
            set
            {
                if (!_isDisabled && value == false)
                {
                    Reset();
                }
                _isDisabled = !value;
            }
        }
    }
}

