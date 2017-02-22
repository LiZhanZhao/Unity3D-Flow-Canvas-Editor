using UnityEngine;
using System.Collections;
using ParadoxNotion;

namespace FlowCanvas.Framework
{
    public class BinderConnection : Connection
    {
        [SerializeField]
        private string _sourcePortName;
        [SerializeField]
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
                Debug.LogWarning(string.Format("<color=red>RemoveConnection Source : </color> sourcePort : {0}  bindingType : {1}",
                    sourcePort.type.ToString(), bindingType.ToString()));
                graph.RemoveConnection(this);
            }
        }

        // 获得如果BInderConnection 是泛型的话，就获得泛型类型，不是就获得Flow
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
                Debug.LogWarning(string.Format("<color=red>RemoveConnection Target : </color> targetPort:{0}  bindingType:{1}",
                    targetPort.type.ToString(), bindingType.ToString()));
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

        public static BinderConnection Create(Port source, Port target)
        {

            if (source == null || target == null)
            {
                Debug.LogError("Source Port or Target Port is null when making a new Binder Connection");
                return null;
            }

            //if (!source.CanAcceptConnections())
            //{
            //    Debug.LogWarning("Source port can accept no more connections");
            //    return null;
            //}

            //if (!target.CanAcceptConnections())
            //{
            //    Debug.LogWarning("Target port can accept no more connections");
            //    return null;
            //}

            if (source.parent == target.parent)
            {
                Debug.LogWarning("Can't connect ports on the same parent node");
                return null;
            }


            if (source is FlowOutput && !(target is FlowInput))
            {
                Debug.LogWarning("Flow ports can only be connected to other Flow ports");
                return null;
            }

            if ((source is FlowInput && target is FlowInput) || (source is ValueInput && target is ValueInput))
            {
                Debug.LogWarning("Can't connect input to input");
                return null;
            }

            if ((source is FlowOutput && target is FlowOutput) || (source is ValueOutput && target is ValueOutput))
            {
                Debug.LogWarning("Can't connect output to output");
                return null;
            }

            if (!TypeConverter.HasConvertion(source.type, target.type))
            {
                Debug.LogWarning(string.Format("Can't connect ports. Type '{0}' is not assignable from Type '{1}' and there exists no internal convertion for those types.", target.type.FriendlyName(), source.type.FriendlyName()));
                return null;
            }

            if (source is FlowOutput && target is FlowInput)
            {
                var flowBind = new BinderConnection();
                flowBind.OnCreate(source, target);
                return flowBind;
            }

            if (source is ValueOutput && target is ValueInput)
            {
                var valueBind = (BinderConnection)System.Activator.CreateInstance(typeof(BinderConnection<>).RTMakeGenericType(new System.Type[] { target.type }));
                valueBind.OnCreate(source, target);
                return valueBind;
            }

            return null;
        }

        void OnCreate(Port source, Port target)
        {
            sourceNode = source.parent;
            targetNode = target.parent;
            sourcePortID = source.ID;
            targetPortID = target.ID;
            sourceNode.outConnections.Add(this);
            targetNode.inConnections.Add(this);

            source.connections++;
            target.connections++;
            
            //Bind();
            
        }

        public override void OnDestroy()
        {
            if (sourcePort != null) //check null for cases like the SwitchInt, where basicaly the source port is null since pin is removed first
                sourcePort.connections--;
            if (targetPort != null)
                targetPort.connections--;

            UnBind();
            
        }
    }
}

