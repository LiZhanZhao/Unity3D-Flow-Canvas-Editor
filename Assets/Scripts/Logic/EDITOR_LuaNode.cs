using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FlowCanvas.Framework;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FlowCanvas.Nodes
{
    public partial class LuaNode : FlowNode
    {
        #if UNITY_EDITOR
        

        protected void AutoGenerateGUI()
        {
            if (!string.IsNullOrEmpty(_luaFileRelaPath))
            {
                string fileContext = System.IO.File.ReadAllText(_luaFileRelaPath);
                AutoGenerateValueInputGUI(fileContext);

            }
        }

        protected void AutoGenerateValueInputGUI(string fileContext)
        {
            List<string> argNames, argTypes;
            if (!ParseHeadConfig(fileContext, kBeginConfigValueInput, kEndConfigValueInput, out argNames, out argTypes))
            {
                Debug.LogError(string.Format("{0} : {1}, {2} GUI error ", _luaFileRelaPath, kBeginConfigValueInput,
                    kEndConfigValueInput));
            }
            _autoValueInputArgNames.Clear();
            
            for (int i = 0; i < argNames.Count; i++)
            {
                string argName = argNames[i];
                string argType = argTypes[i];
                Type t = Type.GetType(argType);
                _autoValueInputArgNames.Add(argName);

                object sData = _autoValueInputArgValues.Count - 1 >= i ? _autoValueInputArgValues[i] : null;
                // 存在的数据不同类型的情况下
                if (sData != null){
                    Type st = sData.GetType();
                    if (t != st){
                        if (typeof(string) == t){
                            _autoValueInputArgValues[i] = "";
                        }
                        else{
                            _autoValueInputArgValues[i] = Activator.CreateInstance(t);
                        }
                    }
                }
                else{
                    if (typeof(string) == t){
                        _autoValueInputArgValues.Add("");
                    }
                    else{
                        _autoValueInputArgValues.Add(Activator.CreateInstance(t));
                    }
                }

                _autoValueInputArgValues[i] = EditorUtils.GenericField(argName, _autoValueInputArgValues[i], t, null, null);
            }
        }


        virtual public void Config(string luaFilePath)
        {
            string luaRelaPath = UnityEditor.FileUtil.GetProjectRelativePath(luaFilePath);
            _luaFileRelaPath = luaRelaPath;
            _luaFileName = System.IO.Path.GetFileNameWithoutExtension(_luaFileRelaPath);
            OnValidate(graphBase);
        }

        public string LuaPath
        {
            set
            {
                if (_luaFileRelaPath != value)
                {
                    Debug.Log("set lua path");
                    _luaFileRelaPath = value;
                    Config(value);
                }

            }
            get { return _luaFileRelaPath; }
        }

        protected void OnLuaPathGUI() {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(string.Format("Lua:"), (string)LuaPath);
                if (GUILayout.Button("Select"))
                {
                    string path = EditorUtility.OpenFilePanel("Select Lua file", "", "");
                    EditorGUI.FocusTextInControl("");

                    if (!path.EndsWith(".lua")){
                        if (EditorUtility.DisplayDialog("no lua file", "no lua file", "ok")){
                            return;
                        }
                    }
                    else{
                        LuaPath =  path;
                    }
                }
                EditorGUILayout.EndHorizontal();
        }

        override protected void OnNodeInspectorGUI()
        {
            DrawDefaultInspector();
            OnLuaPathGUI();
            AutoGenerateGUI();
        }

        #endif
    }
}
