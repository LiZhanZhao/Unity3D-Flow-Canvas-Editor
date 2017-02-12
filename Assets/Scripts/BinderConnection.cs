using UnityEngine;
using System.Collections;
using NodeCanvas.Framework;
using ParadoxNotion;

namespace FlowCanvas
{
    public class BinderConnection : Connection
    {
        private string _sourcePortName;
        private string _targetPortName;
        private Port _sourcePort;
        private Port _targetPort;

        public string sourcePortID
        {
            get { return sourcePort != null ? sourcePort.ID : _sourcePortName; }
            private set { _sourcePortName = value; }
        }
        public Port sourcePort
        {
            get
            {
                if (_sourcePort == null)
                {
                    if (sourceNode is FlowNode)
                    { //In case it's 'MissingNode'
                        _sourcePort = (sourceNode as FlowNode).GetOutputPort(_sourcePortName);
                    }
                }
                return _sourcePort;
            }
        }

        public void GatherAndValidateSourcePort()
        {
            _sourcePort = null;
            if (sourcePort != null && TypeConverter.HasConvertion(sourcePort.type, bindingType))
            {
                sourcePort.connections++;
            }
            else
            {
                graph.RemoveConnection(this, false);
            }
        }

        private System.Type bindingType
        {
            get { return GetType().RTIsGenericType() ? GetType().RTGetGenericArguments()[0] : typeof(Flow); }
        }

        public void GatherAndValidateTargetPort()
        {
            _targetPort = null;
            if (targetPort != null && targetPort.type == bindingType)
            {
                targetPort.connections++;
            }
            else
            {
                graph.RemoveConnection(this, false);
            }
        }

        public Port targetPort
        {
            get
            {
                if (_targetPort == null)
                {
                    if (targetNode is FlowNode)
                    { //In case it's 'MissingNode'
                        _targetPort = (targetNode as FlowNode).GetInputPort(_targetPortName);
                    }
                }
                return _targetPort;
            }
        }
        public string targetPortID
        {
            get { return targetPort != null ? targetPort.ID : _targetPortName; }
            private set { _targetPortName = value; }
        }

        virtual public void Bind()
        {

            if (!isActive)
            {
                return;
            }

            if (sourcePort is FlowOutput && targetPort is FlowInput)
            {
                (sourcePort as FlowOutput).BindTo((FlowInput)targetPort);

#if UNITY_EDITOR && DO_EDITOR_BINDING
				(sourcePort as FlowOutput).Append( (f)=> {BlinkStatus(f);} );
#endif
            }
        }
    }
}

