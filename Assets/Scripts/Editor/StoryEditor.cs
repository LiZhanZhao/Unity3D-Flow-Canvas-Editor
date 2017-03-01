using UnityEngine;
using System.Collections;
using UnityEditor;

using FlowCanvas.Framework;
using FlowCanvas.Nodes;



namespace StoryEditorContext
{
    public class StoryEditor : EditorWindow, ISerializationCallbackReceiver
    {
        private const string kBgTexturePath = "Assets/Scripts/Editor/Resources/EditorTextures/background.png";
        private const string kCustomGUISkin = "Assets/Scripts/Editor/Resources/NodeCanvasSkin.guiskin";

        readonly public static Texture2D _playIcon = EditorGUIUtility.FindTexture("d_PlayButton");
        readonly public static Texture2D _pauseIcon = EditorGUIUtility.FindTexture("d_PauseButton");

        private const float kTitleHeight = 21;

        private Texture2D _girdTex = null;
        private GUISkin _customGUISkin = null;
        
        private Vector2 _mousePos;
        private bool _isCanScrollWindow = false;

        private Matrix4x4 _noZoomMatrix;
        private Vector2 _zoomPivotPos;

        private Node _curveStartPointNode = null;
        private bool _isCanDrawNodeToMouseLine = false;

        // this add static is not serialize _uiGraph
        private static UIGraph _uiGraph = null;
        private bool _willRepaint = true;

        private const string kSerializeKey = "__StoryEditorSerializeKey";
        private Rect _debugWindowRect = new Rect();
        private PlayerAgent _playerAgent = null;
        private static bool _isInitLuaEnv = false;

        [MenuItem("Window/Story Editor")]

        static void CreateEditor()
        {
            StoryEditor se= (StoryEditor)EditorWindow.GetWindow(typeof(StoryEditor));
            se.minSize = new Vector2(800, 600);
            se._willRepaint = true;
            
        }

        public UIGraph currentGraph
        {
            get {
                if (_uiGraph == null)
                {
                    
                    string serializeJson = "";
                    if (PlayerPrefs.HasKey(kSerializeKey))
                    {
                        serializeJson = PlayerPrefs.GetString(kSerializeKey);
                    }
                    if (!string.IsNullOrEmpty(serializeJson))
                    {
                        _uiGraph = ScriptableObject.CreateInstance<UIGraph>();
                        _uiGraph.Deserialize(serializeJson, true);
                    }
                }
                return _uiGraph; 
            }


            set
            {
                _uiGraph = value;
            }
        }

        void OnEnable()
        {
            InitToolRes();

            if (currentGraph == null)
            {
                currentGraph = ScriptableObject.CreateInstance<UIGraph>();
                Vector2 pos = new Vector2(position.width / 2, position.height / 2);
                currentGraph.AddNode<RootNode>(pos);
            }

            _willRepaint = true;
            _zoomPivotPos = new Vector2(position.width / 2, position.height / 2);
            
            EditorApplication.playmodeStateChanged += PlayModeChange;
        }

        void OnDisable()
        {
            
            EditorApplication.playmodeStateChanged -= PlayModeChange;
        }

        void OnDestroy()
        {            
            SaveSerializeJson();
        }
         
        void SaveSerializeJson()
        {
            // todo : this use _uiGraph rather than currentGraph, because use currentGraph will crash
            if (_uiGraph != null)
            {
                Debug.Log("Serialize Succees !!!!! ");
                string jsonStr = _uiGraph.Serialize(true);
                PlayerPrefs.SetString(kSerializeKey, jsonStr);
            }
        }

        void PlayModeChange()
        {
            SaveSerializeJson();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            SaveSerializeJson();
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize(){
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

            currentGraph.HandleInputEvent(e, _mousePos);

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
                currentGraph.scrollOffset += e.delta / 2;

                for (int i = 0; i < currentGraph.allNodes.Count; i++)
                {
                    Node node = currentGraph.allNodes[i];
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
                currentGraph.zoom += e.delta.y / 50.0f;
                currentGraph.zoom = Mathf.Clamp(currentGraph.zoom, 0.5f, 2.0f);
                
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
            if (currentGraph != null)
            {
                currentGraph.DrawNodes();
            }
            EndWindows();
        }

        void DrawNoZoomGraph()
        {
            if (currentGraph != null)
            {
                currentGraph.DrawNodeInspector();
            }

            DrawDebugWidnow();
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
                    totalRect = ScaleRect(totalRect, 1.0f / currentGraph.zoom, _zoomPivotPos);

                    // 会导致Node Insp 界面出现问题
                    //GUI.BeginGroup(totalRect);
                    GUI.BeginClip(totalRect);

                    Vector2 bgSize = new Vector2(_girdTex.width, _girdTex.height);
                    Vector2 beginPos = new Vector2(currentGraph.scrollOffset.x % bgSize.x - bgSize.x,
                        currentGraph.scrollOffset.y % bgSize.y - bgSize.y);

                    Rect unitRect = new Rect(beginPos, bgSize);

                    // todo : must need zero to scaleRect
                    unitRect = ScaleRect(unitRect, currentGraph.zoom, Vector2.zero);
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
            Vector2 scale = new Vector2(currentGraph.zoom, currentGraph.zoom);
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
                    if (currentGraph.allNodes.Count > 0 && !EditorUtility.DisplayDialog("Import Graph", "All current graph information will be lost. Are you sure?", "YES", "NO"))
                        return;

                    var path = EditorUtility.OpenFilePanel(string.Format("Import '{0}' Graph", this.GetType().Name), "Assets", "json");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if(!currentGraph.Deserialize(System.IO.File.ReadAllText(path),true))
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
                        System.IO.File.WriteAllText(path, currentGraph.Serialize(true));
                        AssetDatabase.Refresh();
                    }
                });

                menu.ShowAsContext();
            }

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;
        }

        void InitLuaEnv()
        {
            GameObject lcGo = new GameObject("__LuaClient");
            LuaClient lc = lcGo.GetComponent<LuaClient>();
            if (lc == null)
            {
                lc = lcGo.AddComponent<LuaClient>();
            }

            LuaClient.GetMainState()["__UNITY_EDITOR__"] = true;
            LuaClient.GetMainState().DoFile("editor/EditorAdapter.lua");

        }

        void DrawDebugWidnow()
        {
            if (!Application.isPlaying) { return; }

            if (_playerAgent == null)
            {
                _playerAgent = new CsPlayerAgent();
            }

            _debugWindowRect.x = position.width / 3;
            _debugWindowRect.height = 35;
            _debugWindowRect.width = position.width / 3;
            _debugWindowRect.y = position.height - _debugWindowRect.height;

            var pressed = new GUIStyle(GUI.skin.GetStyle("button"));
            pressed.normal.background = pressed.active.background;

            GUILayout.BeginArea(_debugWindowRect, (GUIStyle)"windowShadow");

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            //  play
            if (GUILayout.Button(_playIcon, _playerAgent.IsRunning()? pressed : (GUIStyle)"button"))
            {
                if(!_isInitLuaEnv){
                    InitLuaEnv();
                    _isInitLuaEnv = true;
                }
                if (!_playerAgent.IsRunning())
                {
                    string jsonStr = _uiGraph.Serialize(true);
                    _playerAgent.Play(jsonStr);
                }
                else
                {
                    _playerAgent.Stop();
                }
            }

            // play and pause
            if (GUILayout.Button(_pauseIcon, _playerAgent.IsPaused() ? pressed : (GUIStyle)"button"))
            {
                if (_playerAgent.IsPaused())
                {
                    _playerAgent.UnPause();
                }
                else
                {
                    _playerAgent.Pause();
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();

        }

        public bool IsInBlankArea(Vector2 mousePos)
        {
            bool isHitNode = currentGraph.IsHitNode(mousePos);
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


