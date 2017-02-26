using UnityEngine;
using System.Collections;
using UnityEditor;

using FlowCanvas.Framework;
using FlowCanvas.Nodes;



namespace StoryEditorContext
{
    public class StoryEditor : EditorWindow
    {
        private const float kNodeInfoWWP = 0.3f;
        private const float kNodeInfoWHP = 0.5f;
        private const float kToolbarHeight = 40;

        private const string kBgTexturePath = "Assets/Scripts/Editor/Resources/EditorTextures/background.png";
        private const string kCustomGUISkin = "Assets/Scripts/Editor/Resources/NodeCanvasSkin.guiskin";
        private const float kTitleHeight = 21;

        private Texture2D _girdTex = null;
        private GUISkin _customGUISkin = null;
        
        private Vector2 _mousePos;
        private bool _isCanScrollWindow = false;

        private Matrix4x4 _noZoomMatrix;
        private Vector2 _zoomPivotPos;

        private Node _curveStartPointNode = null;
        private bool _isCanDrawNodeToMouseLine = false;

        // editor serialize data
        private UIGraph _uiGraph = null;
        private bool _willRepaint = true;

        [MenuItem("Window/Story Editor")]
        static void CreateEditor()
        {
            StoryEditor se= (StoryEditor)EditorWindow.GetWindow(typeof(StoryEditor));
            se.minSize = new Vector2(800, 600);
            se._willRepaint = true;
            
        }
        
        void OnEnable()
        {
            

            InitToolRes();

            _willRepaint = true;

            if (_uiGraph == null)
            {
                _uiGraph = new UIGraph();
                Vector2 pos = new Vector2(position.width / 2, position.height / 2);
                _uiGraph.AddNode<SimplexNodeWrapper<LogValue>>(pos);

                //LuaCommandNode test = _uiGraph.AddNode<LuaCommandNode>(pos);
                //string configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/story_command/get_targets.lua";
                //test.Config(configFile);
            }

            _zoomPivotPos = new Vector2(position.width / 2, position.height / 2);
                
        }

        void InitToolRes()
        {
            if (_girdTex == null)
            {
                _girdTex = AssetDatabase.LoadAssetAtPath(kBgTexturePath, typeof(Texture2D)) as Texture2D;
                _customGUISkin = AssetDatabase.LoadAssetAtPath<GUISkin>(kCustomGUISkin);
            }
            
        }

        void HandleComiling()
        {
            if (EditorApplication.isCompiling)
            {
                ShowNotification(new GUIContent("Compiling Please Wait..."));
                _willRepaint = true;
                return;
            }
        }
        void OnGUI()
        {

            HandleComiling();

            //var willDirty = false;
            //if (Event.current.rawType == EventType.MouseUp)
            //{
            //    willDirty = true;
            //}


            DrawCenterWindow();
            //DrawToolBar();
            //DrawNodeInfoWindow();
            //DrawPlayInfoWidnow();
            DoRepaint();

            //if (willDirty)
            //{
            //    _willRepaint = true;
            //    EditorUtility.SetDirty(_uiGraph);
            //}
        }

        void DoRepaint()
        {
            if (_willRepaint || Event.current.type == EventType.MouseMove)
            {
                Repaint();
            }
        }

        void HandleInputEvents()
        {
            Event e = Event.current;
            _mousePos = e.mousePosition;

            _uiGraph.HandleInputEvent(e, _mousePos);

            //HandleLineWithNode();
            // 需要改
            //HandleScrollWindow();
            HandleZoomWindow();
        }

        void HandleScrollWindow()
        {
            Event e = Event.current;
            _mousePos = e.mousePosition;
            // 空白区域左键
            bool isLeftMouseDown = (e.type == EventType.MouseDown) && (e.button == 0) && IsInBlankArea(_mousePos);
            if (isLeftMouseDown)
            {
                _isCanScrollWindow = true;
                e.delta = Vector2.zero;
            }

            if (_isCanScrollWindow)
            {
                _uiGraph.scrollOffset += e.delta / 2;

                for (int i = 0; i < _uiGraph.allNodes.Count; i++)
                {
                    Node node = _uiGraph.allNodes[i];
                    node.rect.position += e.delta / 2;
                }
                Repaint();
            }

            bool isLeftMouseUp = (e.type == EventType.MouseUp) && (e.button == 0);
            if (isLeftMouseUp)
            {
                _isCanScrollWindow = false;
            }


        }

        void HandleZoomWindow()
        {
            Event e = Event.current;
            _mousePos = e.mousePosition;
            // 滚轮
            bool isCanZoom = (e.type == EventType.ScrollWheel);
            if (isCanZoom)
            {
                _uiGraph.zoom += e.delta.y / 50.0f;
                _uiGraph.zoom = Mathf.Clamp(_uiGraph.zoom, 0.5f, 2.0f);
                
                Repaint();
            }

        }


        //void HandleLineWithNode()
        //{
        //    Event e = Event.current;
        //    _mousePos = e.mousePosition;
        //    // 左键按到Node
        //    bool isCanDo = (e.type == EventType.MouseDown) && (e.button == 0) &&
        //        IsInNodeArea(_mousePos);

        //    if (isCanDo)
        //    {
        //        Node node = GetNodeFromPosition(_mousePos);
        //        Debug.Assert(node != null);
        //        Rect outputRect = node.OutputKnobRect;
        //        bool isHitOutputKnob = outputRect.Contains(_mousePos);
        //        Rect inputRect = node.InputKnobRect;
        //        bool isHitInputKnob = inputRect.Contains(_mousePos);
        //        if (isHitOutputKnob)
        //        {
        //            _isCanDrawNodeToMouseLine = true;
        //            _curveStartPointNode = node;
        //            //Debug.Log("Hit Output Knob");
        //        }

        //        if (isHitInputKnob)
        //        {
        //            int parentIndex = node.parentId;
        //            if (parentIndex != -1)
        //            {
        //                Node parentNode = _uiGraph.nodeList[parentIndex];
        //                _curveStartPointNode = parentNode;
        //                _isCanDrawNodeToMouseLine = true;
        //                Debug.Assert(_curveStartPointNode != null);
        //                _uiGraph.NodeRemoveChild(_curveStartPointNode, node);
        //            }
                    
                    
        //        }

        //    }

        //    bool isLeftMouseUp = (e.type == EventType.MouseUp) && (e.button == 0);
        //    if (isLeftMouseUp)
        //    {
        //        bool isInNodeArea = IsInNodeArea(_mousePos);
        //        if (isInNodeArea)
        //        {
        //            Node childNode = GetNodeFromPosition(_mousePos);
        //            Rect inputRect = childNode.InputKnobRect;
        //            bool isHitInputKnob = inputRect.Contains(_mousePos);
        //            if (isHitInputKnob)
        //            {
        //                Debug.Assert(_curveStartPointNode != null);
        //                Debug.Assert(childNode != null);
        //                _uiGraph.NodeAddChild(_curveStartPointNode, childNode);
        //            }
                    
        //        }
        //        _isCanDrawNodeToMouseLine = false;
        //        _curveStartPointNode = null;
        //    }
            

        //}

        void BeginUseSkin()
        {
            GUI.skin = _customGUISkin;
        }
        void EndUseSkin()
        {
            GUI.skin = null;
        }

        void DrawCenterWindow()
        {
            BeginUseSkin();
            BeginZoomCenterWindow();
            HandleInputEvents();
            DrawGirdBackground();
            DrawZoomGraph();
            //DrawNodeToMouseLine();
            //DrawNodeConnect();
            EndZoomCenterWidnow();

            DrawNoZoomGraph();


            EndUseSkin();


        }

        void DrawZoomGraph()
        {
            BeginWindows();
            if (_uiGraph != null)
            {
                _uiGraph.DrawNodes();
            }
            EndWindows();
        }

        void DrawNoZoomGraph()
        {
            if (_uiGraph != null)
            {
                _uiGraph.DrawNodeInspector();
            }
        }
        

        void DrawGirdBackground()
        {
            Event e = Event.current;
            // update scrollWindow background
            if (e.type == EventType.Repaint)
            {
                if (_girdTex != null)
                {
                    
                    Rect totalRect = new Rect(0, kTitleHeight, position.width, position.height);
                    totalRect = ScaleRect(totalRect, 1.0f / _uiGraph.zoom, _zoomPivotPos);

                    GUI.BeginGroup(totalRect);

                    Vector2 bgSize = new Vector2(_girdTex.width, _girdTex.height);
                    Vector2 beginPos = new Vector2(_uiGraph.scrollOffset.x % bgSize.x - bgSize.x,
                        _uiGraph.scrollOffset.y % bgSize.y - bgSize.y);

                    Rect unitRect = new Rect(beginPos, bgSize);

                    // todo : must need zero to scaleRect
                    unitRect = ScaleRect(unitRect, _uiGraph.zoom, Vector2.zero);
                    Vector2 totalSize = totalRect.size + new Vector2(Mathf.Abs(unitRect.x), Mathf.Abs(unitRect.y));

                    int tileX = Mathf.CeilToInt(totalSize.x / unitRect.width);
                    int tileY = Mathf.CeilToInt(totalSize.y / unitRect.height);
                    for (int i = 0; i < tileX; i++)
                    {
                        for (int j = 0; j < tileY; j++)
                        {
                            Rect rect = new Rect(unitRect.position + new Vector2(i * unitRect.width, j * unitRect.height), 
                                unitRect.size);
                            GUI.DrawTexture(rect, _girdTex);
                        }
                    }
                    
                    GUI.EndGroup();
                     
                }
            }
        }

        void BeginZoomCenterWindow()
        {
            GUI.EndGroup();
            _noZoomMatrix = GUI.matrix;
            Vector2 scale = new Vector2(_uiGraph.zoom, _uiGraph.zoom);
            GUIUtility.ScaleAroundPivot(scale, _zoomPivotPos);
            
        }
        void EndZoomCenterWidnow()
        {
            GUI.matrix = _noZoomMatrix;
            
            GUI.BeginGroup(new Rect(0, kTitleHeight, Screen.width, Screen.height));
        }


        void DrawCurve(Vector2 start, Vector2 end)
        {
            Vector3 startPos = new Vector3(start.x, start.y);
            Vector3 endPos = new Vector3(end.x, end.y);
            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;
            Color shadowColor = new Color(0, 0, 0, 0.1f);

            for (int i = 0; i < 3; i++) // Draw a shadow with 3 shades
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowColor, null, (i + 1) * 4); // increasing width for fading shadow
            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.black, null, 2);
        }

        void DrawNodeToMouseLine()
        {
            if (_isCanDrawNodeToMouseLine)
            {
                //Rect outputRect = _curveStartPointNode.OutputKnobRect;
                //Vector2 knobPos = outputRect.center;
                //Vector2 mousePos = _mousePos;
                //DrawCurve(knobPos, mousePos);
                //Repaint();
            }
        }

        //void DrawNodeConnect()
        //{
        //    if (_uiGraph != null)
        //    {
        //        int nodeCount = _uiGraph.allNodes.Count;
        //        for (int i = 0; i < nodeCount; i++)
        //        {
        //            Node parentNode = _uiGraph.allNodes[i];
        //            for (int ci = 0; ci < parentNode.childList.Count; ci++)
        //            {
        //                int childIndex = parentNode.childList[ci];
        //                Node childNode = _uiGraph.allNodes[childIndex];
        //                Debug.Assert(childNode != null);
        //                DrawCurve(parentNode.OutputKnobRect.center, 
        //                    childNode.InputKnobRect.center);
        //            }
        //        }
        //    }
        //}

        void DrawToolBar()
        {
            GUILayout.BeginArea(ToolbarRect, GUI.skin.button);
            GUILayout.EndArea();
        }

        void DrawNodeInfoWindow(){
            GUILayout.BeginArea(NodeInfoWindowRect, GUI.skin.button);
            GUILayout.EndArea();
        }

        void DrawPlayInfoWidnow()
        {
            GUILayout.BeginArea(PlayInfoWindowRect, GUI.skin.button);
            GUILayout.EndArea();
        }



        public Rect NodeInfoWindowRect
        {
            // position 就是editorWindow的位置与大小
            get
            {
                return new Rect(position.width * (1 - kNodeInfoWWP),
                kToolbarHeight,
                position.width * kNodeInfoWWP,
                position.height * kNodeInfoWHP);
            }
        }

        public Rect PlayInfoWindowRect
        {
            get
            {
                return new Rect(
                    position.width * (1 - kNodeInfoWWP),
                    kToolbarHeight + position.height * kNodeInfoWHP,
                    position.width * kNodeInfoWWP,
                    position.height - (kToolbarHeight + position.height * kNodeInfoWHP)
                    );
            }
        }
        public Rect ToolbarRect
        {
            get { return new Rect(0, 0, position.width, kToolbarHeight); }
        }

        public bool IsInBlankArea(Vector2 mousePos)
        {
            bool isHitNode = _uiGraph.IsHitNode(mousePos);
            return !NodeInfoWindowRect.Contains(mousePos) &&
                !PlayInfoWindowRect.Contains(mousePos) && !ToolbarRect.Contains(mousePos) && !isHitNode;
        }

        public bool IsInNodeArea(Vector2 mousePos)
        {
            bool isHitNode = _uiGraph.IsHitNode(mousePos);
            return !NodeInfoWindowRect.Contains(mousePos) &&
                !PlayInfoWindowRect.Contains(mousePos) && !ToolbarRect.Contains(mousePos) && isHitNode;
        }

        public Rect ScaleRect(Rect rect, float scale, Vector2 pivotPoint)
        {
            Rect result = rect;
            // 以锚点为中心
            result.x -= pivotPoint.x;
            result.y -= pivotPoint.y;
            result.xMin *= scale;
            result.xMax *= scale;
            result.yMin *= scale;
            result.yMax *= scale;
            result.x += pivotPoint.x;
            result.y += pivotPoint.y;
            return result;
        }
        
    }


}


