﻿using UnityEngine;
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
            //private set { _sourcePortName = value; }
            set { _sourcePortName = value; }
        }

        public string targetPortID
        {
            get { return targetPort != null ? targetPort.ID : _targetPortName; }
            //private set { _targetPortName = value; }
            set { _targetPortName = value; }
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
        

        public void GatherAndValidateSourcePort()
        {
            _sourcePort = null;
            
            if (sourcePort != null && TypeConverter.HasConvertion(sourcePort.type, bindingType))
            {
                sourcePort.connections++;
            }
            else
            {
                Debug.LogWarning("************************************* Remove Connection");
                graph.RemoveConnection(this);
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
                Debug.LogWarning("************************************* Remove Connection");
                graph.RemoveConnection(this);
            }
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
            }
        }

        virtual public void UnBind()
        {
            if (sourcePort is FlowOutput)
            {
                (sourcePort as FlowOutput).UnBind();
            }
        }
    }
}
