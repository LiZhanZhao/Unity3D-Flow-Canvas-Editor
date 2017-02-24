using UnityEngine;
using System.Collections;
using System.Linq;
using ParadoxNotion;
using System;

namespace FlowCanvas.Framework
{
    abstract public partial class FlowNode
    {
#if UNITY_EDITOR
        private Port[] orderedInputs;
        private Port[] orderedOutputs;
        private static GUIStyle _leftLabelStyle;
        private static GUIStyle _rightLabelStyle;

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
            orderedInputs = _inputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowInput) ? 0 : 1).ToArray();
            orderedOutputs = _outputPorts.Values.OrderBy(p => p.GetType() == typeof(FlowOutput) ? 0 : 1).ToArray();
        }


        protected override void OnShowPortName()
        {

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            if (orderedInputs != null)
            {
                for (var i = 0; i < orderedInputs.Length; i++)
                {
                    var inPort = orderedInputs[i];

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
            if (orderedOutputs != null)
            {
                for (var i = 0; i < orderedOutputs.Length; i++)
                {
                    var outPort = orderedOutputs[i];
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
            if (orderedInputs != null)
            {
                // Input Port
                for (var i = 0; i < orderedInputs.Length; i++)
                {
                    var port = orderedInputs[i];
                    portRect.width = port.isConnected ? 12 : 10;
                    portRect.height = portRect.width;
                    portRect.center = new Vector2(rect.x - 5, port.pos.y);
                    port.pos = portRect.center;

                    
                    //GUI.Box(portRect, string.Empty, port.isConnected ? (GUIStyle)"nodePortConnected" : (GUIStyle)"nodePortEmpty");
                    GUI.Box(portRect, string.Empty, (GUIStyle)"nodePortConnected");
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
                }

                // Output Port
                if (orderedOutputs != null)
                {
                    for (var i = 0; i < orderedOutputs.Length; i++)
                    {
                        var port = orderedOutputs[i];
                        portRect.width = port.isConnected ? 12 : 10;
                        portRect.height = portRect.width;
                        portRect.center = new Vector2(rect.xMax + 5, port.pos.y);
                        port.pos = portRect.center;
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
                    }
                }
            }


        }
    }
#endif
}

