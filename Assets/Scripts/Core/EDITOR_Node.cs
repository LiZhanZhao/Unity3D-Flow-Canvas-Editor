using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ParadoxNotion;
using ParadoxNotion.Design;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlowCanvas.Framework
{

    abstract public partial class Node
    {
        public Rect rect;

#if UNITY_EDITOR
        
        private const float KNOB_SIZE = 13;

        private string _customName;
        private string _nodeName;

        private Port[] orderedInputs;
        private Port[] orderedOutputs;

        private static GUIStyle _centerLabel = null;

        public Node(Rect r)
        {
            rect = r;
        }

        private string customName
        {
            get { return _customName; }
            set { _customName = value; }
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


        

        

        public Rect BoundeRect
        {
            get
            {
                return new Rect(
                    rect.x - KNOB_SIZE,
                    rect.y,
                    rect.width + KNOB_SIZE * 2,
                    rect.height
                    );
            }
        }

        
        

        private static GUIStyle centerLabel
        {
            get
            {
                if (_centerLabel == null)
                {
                    _centerLabel = new GUIStyle("label");
                    _centerLabel.alignment = TextAnchor.UpperCenter;
                    _centerLabel.richText = true;
                }
                return _centerLabel;
            }
        }

        public bool isSelected
        {
            get { return UIGraph.currentSelection == this; }
        }

        public void Draw()
        {
            //rect = GUILayout.Window(ID, rect, DrawContext, string.Empty, (GUIStyle)"window");
            rect = GUILayout.Window(ID, rect, DrawContext, string.Empty, (GUIStyle)"window");

            DrawSelectEffct();
            DrawShadow();
            DrawPort();
        }

        void DrawContext(int id)
        {
            ShowHeader();
            ShowPortName();
            GUI.DragWindow();
        }

        void ShowHeader()
        {
            var finalTitle = name;
            GUILayout.Label(string.Format("<b><size=12><color=#{0}>{1}</color></size></b>", Color.white, finalTitle), centerLabel);
        }


        virtual protected void OnShowPortName() { }

        void ShowPortName()
        {
            GUI.skin.label.richText = true;
            OnShowPortName();
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
        }

        void DrawSelectEffct()
        {
            if (isSelected)
            {
                GUI.color = new Color(0.7f, 0.7f, 1f, 0.8f);
                GUI.Box(rect, string.Empty, (GUIStyle)"windowHighlight");
            }
            
        }

        void DrawShadow()
        {
            GUI.Box(rect, string.Empty, (GUIStyle)"windowShadow");
            GUI.color = new Color(1, 1, 1, 0.5f);
            GUI.Box(new Rect(rect.x + 6, rect.y + 6, rect.width, rect.height), string.Empty, (GUIStyle)"windowShadow");
        }

        virtual protected void OnDrawPort(){
        }

        public void DrawPort()
        {
            OnDrawPort();
        }

        public void HandleInputEvent(Event e, Vector2 pos)
        {
            // is select ?
            if (e.type == EventType.MouseDown && e.button != 2 && rect.Contains(pos))
            {
                UIGraph.currentSelection = this;
            }

            if (e.type == EventType.MouseUp && e.button == 1 && rect.Contains(pos))
            {
                ShowSelectedMenu(e, pos);
            }
        }

        void ShowSelectedMenu(Event e, Vector2 pos)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete (DEL)"), false, () =>{ graphBase.RemoveNode(this); });
            menu.ShowAsContext();

            //if (inConnections.Count > 0)
            //    menu.AddItem(new GUIContent(isActive ? "Disable" : "Enable"), false, () => { SetActive(!isActive); });

            e.Use();
        }

        

        #endif

        
    }
}
