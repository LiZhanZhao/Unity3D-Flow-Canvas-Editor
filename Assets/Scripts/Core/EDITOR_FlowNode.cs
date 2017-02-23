using UnityEngine;
using System.Collections;
using System.Linq;

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


        sealed protected override void OnNodeGUI()
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
                }
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        
        }
    }
#endif
}

