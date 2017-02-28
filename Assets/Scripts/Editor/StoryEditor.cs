using UnityEngine;
using System.Collections;
using UnityEditor;

using FlowCanvas.Framework;
using FlowCanvas.Nodes;



namespace StoryEditorContext
{
    public class StoryEditor : EditorWindow, ISerializationCallbackReceiver
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

        private string testtest = "000";

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
            Debug.Log("000000000");
            if (_uiGraph == null)
            {
                Debug.Log("111111111");
                //_uiGraph = new UIGraph();
                _uiGraph = ScriptableObject.CreateInstance<UIGraph>();
                Vector2 pos = new Vector2(position.width / 2, position.height / 2);
                //_uiGraph.AddNode<SimplexNodeWrapper<LogValue>>(pos);

                //LuaCommandNode test = _uiGraph.AddNode<LuaCommandNode>(pos);
                //string configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/story_command/get_targets.lua";
                //test.Config(configFile); 

                FinishNode test1 = _uiGraph.AddNode<FinishNode>(pos);
                MouseEvents e = _uiGraph.AddNode<MouseEvents>(pos);
            }

            _zoomPivotPos = new Vector2(position.width / 2, position.height / 2);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            Debug.Log("Editor OnBeforeSerialize");
            //Serialize();
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Debug.Log("Editor OnAfterDeserialize");
            //Deserialize();
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
            RemoveNotification();
        }
        void OnGUI()
        {

            HandleComiling();
            //if (_uiGraph != null)
            //{

            //    Undo.RecordObject(_uiGraph, "xxx");
            //}
            

            DrawCenterWindow();
            //DrawPlayInfoWidnow();
            DrawToolBar();
            DoRepaint();
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

            HandleScrollWindow(e);
            HandleZoomWindow(e);
        }

        void HandleScrollWindow(Event e)
        {
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

        void HandleZoomWindow(Event e)
        {
            // 滚轮
            bool isCanZoom = (e.type == EventType.ScrollWheel);
            if (isCanZoom)
            {
                _uiGraph.zoom += e.delta.y / 50.0f;
                _uiGraph.zoom = Mathf.Clamp(_uiGraph.zoom, 0.5f, 2.0f);
                
                Repaint();
            }

        }

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

                    // 会导致Node Insp 界面出现问题
                    //GUI.BeginGroup(totalRect);
                    GUI.BeginClip(totalRect);

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
                    
                    //GUI.EndGroup();
                    GUI.EndClip();
                     
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

        void DrawToolBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUI.backgroundColor = new Color(1f, 1f, 1f, 0.5f);

            if (GUILayout.Button("File", EditorStyles.toolbarDropDown, GUILayout.Width(50)))
            {
                var menu = new GenericMenu();

                //Import JSON
                menu.AddItem(new GUIContent("Import JSON"), false, () =>
                {
                    if (_uiGraph.allNodes.Count > 0 && !EditorUtility.DisplayDialog("Import Graph", "All current graph information will be lost. Are you sure?", "YES", "NO"))
                        return;

                    var path = EditorUtility.OpenFilePanel(string.Format("Import '{0}' Graph", this.GetType().Name), "Assets", "json");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if(!_uiGraph.Deserialize(System.IO.File.ReadAllText(path),true))
                        {
                            EditorUtility.DisplayDialog("Import Failure", "Please read the logs for more information", "OK", "");
                        }
                    }
                });

                //Expot JSON
                menu.AddItem(new GUIContent("Export JSON"), false, () =>
                {
                    var path = EditorUtility.SaveFilePanelInProject(string.Format("Export '{0}' Graph", this.GetType().Name), "", "json", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        //System.IO.File.WriteAllText(path, this.Serialize(true, null)); //true: pretyJson, null: this._objectReferences
                        System.IO.File.WriteAllText(path, _uiGraph.Serialize(true));
                        AssetDatabase.Refresh();
                    }
                });

                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
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
            return !isHitNode;
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


