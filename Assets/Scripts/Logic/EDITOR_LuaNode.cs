using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FlowCanvas.Framework;
using System;

namespace FlowCanvas.Nodes
{
    public partial class LuaNode : FlowNode
    {
        override protected void OnNodeInspectorGUI()
        {
            DrawDefaultInspector();

            AutoGenerateGUI();
        }

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



    }
}
