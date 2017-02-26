using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ParadoxNotion;
using ParadoxNotion.Design;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif
namespace FlowCanvas.Framework
{
    
    public class UIGraph : GraphBase
    {
        public override System.Type baseNodeType { get { return typeof(FlowNode); } }
        public Vector2 scrollOffset;
        public float zoom = 1.0f;

        #if UNITY_EDITOR
        // base class -> sub class list
        private Dictionary<Type, List<Type>> _cachedSubTypes = new Dictionary<Type, List<Type>>();

        ///Get a list of ScriptInfos of the baseType excluding: the base type, abstract classes, Obsolete classes and those with the DoNotList attribute, categorized as a list of ScriptInfo
        private Dictionary<Type, List<ScriptInfo>> _cachedInfos = new Dictionary<Type, List<ScriptInfo>>();

        private static object _currentSelection;

        private Vector2 _nodeInspectorScrollPos;
        private Rect _inspectorRect = new Rect(15, 55, 0, 0);

        public static object currentSelection
        {
            get{   
                return _currentSelection;
            }
            set{
                _currentSelection = value;
            }
        }

        private static Node selectedNode
        {
            get { return currentSelection as Node; }
        }

        public void DrawNodes()
        {
            int nodeCount = allNodes.Count;
            for (int i = 0; i < nodeCount; i++)
            {
                Node node = allNodes[i];
                node.Draw();
            }
        }

        public bool IsHitNode(Vector2 pos)
        {
            if (GetNodeFromPosition(pos) == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Node GetNodeFromPosition(Vector2 pos)
        {
            //Debug.Log(pos);
            for (int i = 0; i < allNodes.Count; i++)
            {
                Node node = allNodes[i];
                Rect nodeBoundleRect = node.BoundeRect;
                if (nodeBoundleRect.Contains(pos))
                {
                    return node;
                }
            }
            return null;
        }

        
        List<Type> GetAssemblyTypes(Type baseType)
        {
            List<Type> subTypes;
            if (_cachedSubTypes.TryGetValue(baseType, out subTypes))
            {
                return subTypes;
            }

            subTypes = new List<Type>();

            // foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies().Where(asm => !asm.GetName().Name.Contains("Editor"))) {
            foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var t in asm.GetExportedTypes().Where(t => t.IsSubclassOf(baseType)))
                    {
                        subTypes.Add(t);
                    }
                }
                catch
                {
                    Debug.Log(asm.FullName + " will be excluded");
                    continue;
                }
            }

            subTypes = subTypes.OrderBy(t => t.FriendlyName()).ToList();
            subTypes = subTypes.OrderBy(t => t.Namespace).ToList();
            return _cachedSubTypes[baseType] = subTypes;
        }

        private List<Type> defaultTypesList
        {
            get
            {
                return new List<Type>{

					typeof(object),
					typeof(string),
					typeof(float),
					typeof(int),
					typeof(bool),

					//Unity basics
					typeof(Vector2),
					typeof(Vector3),
					typeof(Vector4)
				};
            }
        }

        List<ScriptInfo> GetScriptInfosOfType(Type baseType, Type extraGenericType = null)
        {
            List<ScriptInfo> infos;
            if (_cachedInfos.TryGetValue(baseType, out infos))
            {
                return infos;
            }

            infos = new List<ScriptInfo>();
            // get all sub type
            var subTypes = GetAssemblyTypes(baseType);
            if (baseType.IsGenericTypeDefinition)
            {
                subTypes = new List<Type> { baseType };
            }

            foreach (var subType in subTypes)
            {
                // do not have [DoNotList] and [Obsolete] attribute
                if (subType.GetCustomAttributes(typeof(DoNotListAttribute), false).FirstOrDefault() == null && subType.GetCustomAttributes(typeof(ObsoleteAttribute), false).FirstOrDefault() == null)
                {
                    if (subType.IsAbstract)
                    {
                        continue;
                    }

                    // get [Name] Attribute
                    var scriptName = subType.FriendlyName().SplitCamelCase();
                    var nameAttribute = subType.GetCustomAttributes(typeof(NameAttribute), false).FirstOrDefault() as NameAttribute;
                    if (nameAttribute != null)
                        scriptName = nameAttribute.name;

                    // get [Category] Attribute
                    var scriptCategory = string.Empty;
                    var categoryAttribute = subType.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() as CategoryAttribute;
                    if (categoryAttribute != null)
                        scriptCategory = categoryAttribute.category;


                    //add the generic types based on constrains and prefered types list
                    if (subType.IsGenericTypeDefinition && subType.GetGenericArguments().Length == 1)
                    {

                        //infos.Add(new ScriptInfo(null, scriptName.Replace("<T>", " (T)/" + "Generic Argument"), scriptCategory));

                        var arg1 = subType.GetGenericArguments()[0];
                        var types = defaultTypesList;
                        if (extraGenericType != null)
                        {
                            types.Add(extraGenericType);
                        }

                        foreach (var t in types)
                        {
                            var genericType = subType.MakeGenericType(new System.Type[] { t });
                            var finalCategoryPath = (string.IsNullOrEmpty(scriptCategory) ? "" : (scriptCategory + "/")) + scriptName;
                            finalCategoryPath = finalCategoryPath.Replace("<T>", " (T)");
                            finalCategoryPath += "/" + (string.IsNullOrEmpty(t.Namespace) ? "No Namespace" : t.Namespace.Replace(".", "/"));
                            var finalName = scriptName.Replace("<T>", string.Format(" ({0})", t.FriendlyName()));
                            infos.Add(new ScriptInfo(genericType, finalName, finalCategoryPath));
                        }

                    }
                    else
                    {

                        infos.Add(new ScriptInfo(subType, scriptName, scriptCategory));
                    }
                }
            }

            infos = infos.OrderBy(script => script.name).ToList();
            infos = infos.OrderBy(script => script.category).ToList();

            return _cachedInfos[baseType] = infos;
        }

        GenericMenu GetTypeSelectionMenu(Type baseType, Action<Type> callback, GenericMenu menu = null, string subCategory = null)
        {

            if (menu == null)
                menu = new GenericMenu();

            if (subCategory != null)
                subCategory = subCategory + "/";

            GenericMenu.MenuFunction2 Selected = delegate(object selectedType)
            {
                callback((Type)selectedType);
            };

            var scriptInfos = GetScriptInfosOfType(baseType);

            // do not have [Category] Attribute
            foreach (var info in scriptInfos.Where(info => string.IsNullOrEmpty(info.category)))
            {
                menu.AddItem(new GUIContent(subCategory + info.name, info.description), false, info.type != null ? Selected : null, info.type);
            }

            //menu.AddSeparator("/");

            // do have [Category] Attribute
            foreach (var info in scriptInfos.Where(info => !string.IsNullOrEmpty(info.category)))
            {
                menu.AddItem(new GUIContent(subCategory + info.category + "/" + info.name, info.description), false, info.type != null ? Selected : null, info.type);
            }

            return menu;
        }

        //virtual protected GenericMenu OnCanvasContextMenu(GenericMenu menu, Vector2 canvasMousePos)
        //{
        //    return menu;
        //}

        GenericMenu GetAddNodeMenu(Vector2 canvasMousePos)
        {
            System.Action<System.Type> Selected = (type) => { currentSelection = AddNode(type, canvasMousePos); };
            var menu = GetTypeSelectionMenu(baseNodeType, Selected);
            //menu = OnCanvasContextMenu(menu, canvasMousePos);
            return menu;
        }

        public void HandleInputEvent(Event e, Vector2 pos)
        {
            HandleNodesInputEvent(e, pos);

            HandleRightClickMenu(e, pos);
        }

        void HandleNodesInputEvent(Event e, Vector2 pos)
        {
            for (int i = 0; i < allNodes.Count; i++)
            {
                Node node = allNodes[i];
                node.HandleInputEvent(e, pos);
            }
        }

        void HandleRightClickMenu(Event e, Vector2 pos)
        {
            if (e.type == EventType.ContextClick)
            {
                var menu = GetAddNodeMenu(pos);
                menu.ShowAsContext();
                e.Use();
            }

        }


        public void DrawNodeInspector()
        {
            if (selectedNode == null)
            {
                _inspectorRect.height = 0;
                return;
            }
            //EditorGUIUtility.AddCursorRect(new Rect(_inspectorRect.x, _inspectorRect.y, 330, 30), MouseCursor.Text);

            _inspectorRect.width = 330;
            _inspectorRect.x = 10;
            _inspectorRect.y = 30;
            // draw Shadow
            GUI.Box(_inspectorRect, "", (GUIStyle)"windowShadow");

            var title = selectedNode.name;
            var lastSkin = GUI.skin;

            var viewRect = new Rect(_inspectorRect.x, _inspectorRect.y, _inspectorRect.width + 18, Screen.height - _inspectorRect.y - 30);
            _nodeInspectorScrollPos = GUI.BeginScrollView(viewRect, _nodeInspectorScrollPos, _inspectorRect);
            GUILayout.BeginArea(_inspectorRect, title, (GUIStyle)"editorPanel");
            GUILayout.Space(5);

            GUI.skin = null;
            selectedNode.DrawInspector();
            // 分割线
            GUILayout.Box("", GUILayout.Height(5), GUILayout.Width(_inspectorRect.width - 10));
            GUI.skin = lastSkin;
            if (Event.current.type == EventType.Repaint)
            {
                // 主要看Node.DrawInspector最后的rect
                _inspectorRect.height = GUILayoutUtility.GetLastRect().yMax + 5;
            }


            GUILayout.EndArea();
            GUI.EndScrollView();

            //if (GUI.changed)
            //{
            //    EditorUtility.SetDirty(this);
            //}

        }
        #endif
    }
}

