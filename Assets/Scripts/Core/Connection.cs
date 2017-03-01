using UnityEngine;
using System.Collections;

namespace FlowCanvas.Framework
{
    // 连线类
    abstract public partial class Connection
    {
        [SerializeField]
        private Node _sourceNode;
        [SerializeField]
        private Node _targetNode;
        
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
        protected GraphBase graphBase
        {
            get { return sourceNode.graphBase; }
        }

        virtual public void OnDestroy() { }

        
    }
}

