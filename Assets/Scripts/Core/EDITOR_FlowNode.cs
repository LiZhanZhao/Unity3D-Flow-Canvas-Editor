using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion;
using System;

namespace FlowCanvas.Framework
{
    abstract public partial class FlowNode
    {
#if UNITY_EDITOR
        private Port[] _orderedInputs;
        private Port[] _orderedOutputs;
        private static GUIStyle _leftLabelStyle;
        private static GUIStyle _rightLabelStyle;

        // all node is share this
        

        private static GUIStyle leftLabelStyle
        {
            get
            {
                if (_leftLabelStyle == null)
                {
                    _leftLabelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
                    _leftLabelStyle.alignment = TextAnchor.UpperLeft;
                }
                return _leftLabelStyle;
            }
        }

        //for output ports
        private static GUIStyle rightLabelStyle
        {
            get
            {
                if (_rightLabelStyle == null)
                {
                    _rightLabelStyle = new GUIStyle(GUI.skin.GetStyle("label"));
                    _rightLabelStyle.alignment = TextAnchor.UpperRight;
                }
                return _rightLabelStyle;
            }
        }

        void OnPortsGatheredInEditor()
        {
            _orderedInputs = _inputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowInput) ? 0 : 1).ToArray();
            _orderedOutputs = _outputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowOutput) ? 0 : 1).ToArray();
        }

        static void ConnectPorts(Port source, Port target)
        {
            BinderConnection.Create(source, target);
        }

        BinderConnection[] GetOutPortConnections(Port port)
        {
            return outConnections.Cast<BinderConnection>().Where(c => c.sourcePort == port).ToArray();
        }

        BinderConnection[] GetInPortConnections(Port port)
        {
            return inConnections.Cast<BinderConnection>().Where(c => c.targetPort == port).ToArray();
        }

        protected override void OnShowPortName()
        {

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            if (_orderedInputs != null)
            {
                for (var i = 0; i < _orderedInputs.Length; i++)
                {
                    var inPort = _orderedInputs[i];

                    if (inPort is FlowInput)
                    {
                        GUILayout.Label(string.Format("<b>► {0}</b>", inPort.name), leftLabelStyle);
                    }
                    else
                    {
                        var enumerableList = typeof(IEnumerable).IsAssignableFrom(inPort.type) && (inPort.type.IsGenericType || inPort.type.IsArray);
                        GUILayout.Label(string.Format("<color={0}>{1}{2}</color>", Color.white, enumerableList ? "#" : string.Empty, inPort.name), leftLabelStyle);
                    }

                    // 初始化Port.pos
                    //inPort.pos = new Vector2(inPort.pos.x, GUILayoutUtility.GetLastRect().center.y + rect.y);
                    
                    if (Event.current.type == EventType.Repaint)
                    {
                        inPort.pos = new Vector2(inPort.pos.x, GUILayoutUtility.GetLastRect().center.y + rect.y);
                    }
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            if (_orderedOutputs != null)
            {
                for (var i = 0; i < _orderedOutputs.Length; i++)
                {
                    var outPort = _orderedOutputs[i];
                    if (outPort is FlowOutput)
                    {
                        GUILayout.Label(string.Format("<b>{0} ►</b>", outPort.name), rightLabelStyle);
                    }
                    else
                    {
                        var enumerableList = typeof(IEnumerable).IsAssignableFrom(outPort.type) && (outPort.type.IsGenericType || outPort.type.IsArray);
                        GUILayout.Label(string.Format("<color={0}>{1}{2}</color>", Color.white, enumerableList ? "#" : string.Empty, outPort.name), rightLabelStyle);
                    }

                    if (Event.current.type == EventType.Repaint)
                    {
                        // 初始化Port.pos
                        outPort.pos = new Vector2(outPort.pos.x, GUILayoutUtility.GetLastRect().center.y + rect.y);
                    }
                    
                    
                    
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        
        }

        override protected void OnDrawPort()
        {
            var e = Event.current;
            GUI.color = Color.white;
            GUI.Box(new Rect(rect.x - 8, rect.y + 2, 10, rect.height), string.Empty, (GUIStyle)"nodePortContainer");
            GUI.Box(new Rect(rect.xMax - 2, rect.y + 2, 10, rect.height), string.Empty, (GUIStyle)"nodePortContainer");

            var portRect = new Rect(0, 0, 10, 10);
            if (_orderedInputs != null)
            {
                // Input Port
                for (var i = 0; i < _orderedInputs.Length; i++)
                {
                    var port = _orderedInputs[i];

                    var canConnect = true;
                    if ((port == UIGraph.clickedPort) ||
                        (UIGraph.clickedPort is FlowInput || UIGraph.clickedPort is ValueInput) ||
                        (port.isConnected && port is ValueInput) ||
                        (UIGraph.clickedPort != null && UIGraph.clickedPort.parent == port.parent) ||
                        (UIGraph.clickedPort != null && !TypeConverter.HasConvertion(UIGraph.clickedPort.type, port.type)))
                    {
                        canConnect = false;
                    }

                    portRect.width = port.isConnected ? 12 : 10;
                    portRect.height = portRect.width;
                    portRect.center = new Vector2(rect.x - 5, port.pos.y);
                    port.pos = portRect.center;
                    //port.rect = portRect;

                    if (UIGraph.clickedPort != null && !canConnect && UIGraph.clickedPort != port)
                    {
                        GUI.color = new Color(1, 1, 1, 0.3f);
                    }

                    GUI.Box(portRect, string.Empty, port.isConnected ? (GUIStyle)"nodePortConnected" : (GUIStyle)"nodePortEmpty");
                    //GUI.Box(portRect, string.Empty, (GUIStyle)"nodePortConnected");
                    GUI.color = Color.white;

                    // Tooltip
                    if (portRect.Contains(e.mousePosition))
                    {
                        //var labelString = (port.isConnected) ? port.type.FriendlyName() : "Can't Connect Here";
                        var labelString = port.type.FriendlyName();
                        var size = GUI.skin.GetStyle("box").CalcSize(new GUIContent(labelString));
                        var r = new Rect(0, 0, size.x + 10, size.y + 5);
                        r.x = portRect.x - size.x - 10;
                        r.y = portRect.y - size.y / 2;
                        GUI.Box(r, labelString);
                    }


                    if (e.type == EventType.MouseDown && e.button == 0 && portRect.Contains(e.mousePosition))
                    {
                        if (port.CanAcceptConnections())
                        {
                            UIGraph.clickedPort = port;
                            e.Use();
                        }
                    }

                    if (e.type == EventType.MouseUp && e.button == 0 && UIGraph.clickedPort != null)
                    {
                        if (portRect.Contains(e.mousePosition) && port.CanAcceptConnections())
                        {
                            ConnectPorts(UIGraph.clickedPort, port);
                            UIGraph.clickedPort = null;
                            e.Use();
                        }   
                    }

                    //input -> delete BinderConnection.targetPort equal to port 
                    if (port.isConnected && e.type == EventType.ContextClick && portRect.Contains(e.mousePosition))
                    {
                        foreach (var c in GetInPortConnections(port))
                        {
                            graphBase.RemoveConnection(c);
                        }
                        e.Use();
                        return;
                    }


                }
            }

                // Output Port
            if (_orderedOutputs != null)
            {
                for (var i = 0; i < _orderedOutputs.Length; i++)
                {
                    var port = _orderedOutputs[i];

                    var canConnect = true;
                    if ((port == UIGraph.clickedPort) ||
                        (UIGraph.clickedPort is FlowOutput || UIGraph.clickedPort is ValueOutput) ||
                        (port.isConnected && port is FlowOutput) ||
                        (UIGraph.clickedPort != null && UIGraph.clickedPort.parent == port.parent) ||
                        (UIGraph.clickedPort != null && !TypeConverter.HasConvertion(port.type, UIGraph.clickedPort.type)))
                    {
                        canConnect = false;
                    }


                    portRect.width = port.isConnected ? 12 : 10;
                    portRect.height = portRect.width;
                    portRect.center = new Vector2(rect.xMax + 5, port.pos.y);
                    port.pos = portRect.center;
                    //port.rect = portRect;

                    if (UIGraph.clickedPort != null && !canConnect && UIGraph.clickedPort != port)
                    {
                        GUI.color = new Color(1, 1, 1, 0.3f);
                    }

                    GUI.Box(portRect, string.Empty, port.isConnected ? (GUIStyle)"nodePortConnected" : (GUIStyle)"nodePortEmpty");
                    GUI.color = Color.white;

                    //Tooltip
                    if (portRect.Contains(e.mousePosition))
                    {
                        var labelString = port.type.FriendlyName();
                        var size = GUI.skin.GetStyle("label").CalcSize(new GUIContent(labelString));
                        var r = new Rect(0, 0, size.x + 10, size.y + 5);
                        r.x = portRect.x + 15;
                        r.y = portRect.y - portRect.height / 2;
                        GUI.Box(r, labelString);
                    }


                    if (e.type == EventType.MouseDown && e.button == 0 && portRect.Contains(e.mousePosition))
                    {
                        if (port.CanAcceptConnections())
                        {
                            UIGraph.clickedPort = port;
                            e.Use();
                        }
                    }

                    if (e.type == EventType.MouseUp && e.button == 0 && UIGraph.clickedPort != null)
                    {
                        if (portRect.Contains(e.mousePosition) && port.CanAcceptConnections())
                        {
                            ConnectPorts(port, UIGraph.clickedPort);
                            UIGraph.clickedPort = null;
                            e.Use();
                        }   
                    }

                    //output -> delete BinderConnection.sourcePort equal to port 
                    if (e.type == EventType.ContextClick && portRect.Contains(e.mousePosition))
                    {
                        foreach (var c in GetOutPortConnections(port))
                        {
                            graphBase.RemoveConnection(c);
                        }
                        e.Use();
                        return;
                    }
                }
            }

            if (UIGraph.clickedPort != null && UIGraph.clickedPort.parent == this)
            {
                DrawCurve(UIGraph.clickedPort.pos, e.mousePosition,  (UIGraph.clickedPort is FlowInput || UIGraph.clickedPort is ValueInput));
            }

            for (var i = 0; i < outConnections.Count; i++)
            {
                var binder = outConnections[i] as BinderConnection;
                if (binder != null)
                {
                    var sourcePort = binder.sourcePort;
                    var targetPort = binder.targetPort;
                    if (sourcePort != null && targetPort != null)
                    {
                        DrawCurve(sourcePort.pos, targetPort.pos);
                    }
                }
            }
        }


        void DrawCurve(Vector2 start, Vector2 end, bool isInput = false)
        {
            var xDiff = (start.x - end.x) * 0.8f;
            xDiff = end.x > start.x ? xDiff : -xDiff;
            var tangA = isInput ? new Vector2(xDiff, 0) : new Vector2(-xDiff, 0);
            var tangB = tangA * -1;
            UnityEditor.Handles.DrawBezier(start, end, start + tangA, end + tangB, new Color(0.5f, 0.5f, 0.8f, 0.8f), null, 3);

            //Vector3 startPos = new Vector3(start.x, start.y);
            //Vector3 endPos = new Vector3(end.x, end.y);
            //Vector3 startTan = startPos;//+ Vector3.right * 50;
            //Vector3 endTan = endPos;//+ Vector3.left * 50;
            //Color shadowColor = new Color(0, 0, 0, 0.1f);

            //for (int i = 0; i < 3; i++) // Draw a shadow with 3 shades
            //    UnityEditor.Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowColor, null, (i + 1) * 4); // increasing width for fading shadow
            //UnityEditor.Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 2);
        }

        protected override void OnHandleInputEvent(Event e) {
        }
    }
#endif
}

