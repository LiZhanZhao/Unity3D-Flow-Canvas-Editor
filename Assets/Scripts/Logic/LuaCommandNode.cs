using UnityEngine;
using System.Collections;
using LuaInterface;
using System.Collections.Generic;
using ParadoxNotion;
using System;
using FlowCanvas.Framework;

namespace FlowCanvas.Nodes
{
    public class LuaCommandNode : LuaNode
    {

        protected override void AutoGenerateValueOutput(string fileContext)
        {
            List<string> argNames, argTypes;
            if (!ParseHeadConfig(fileContext, kBeginConfigValueOutput, kEndConfigValueOutput, out argNames, out argTypes))
            {
                Debug.LogError(string.Format("{0} : {1}, {2} error ", _luaFileRelaPath, kBeginConfigValueOutput,
                    kEndConfigValueOutput));
            }

            for (int i = 0; i < argNames.Count; i++)
            {
                string argName = argNames[i];
                string argType = argTypes[i];
                Type t = Type.GetType(argType);
                AddValueOutput(argName, () => { return GetOutputData(argName, t); });
            }
        }

        private object GetOutputData(string key, Type t)
        {
            List<object> argValue = new List<object>();
            foreach (string argName in _autoValueInputArgNames)
            {
                Port p = GetInputPort(argName);
                if (p is ValueInput)
                {
                    ValueInput valueInput = (ValueInput)p;
                    argValue.Add(valueInput.value);
                }
            }

            object[] args = argValue.ToArray();
            LuaState state = LuaClient.GetMainState();
            LuaFunction getDataFunc = state.GetFunction("SMGetOutputData");
            getDataFunc.BeginPCall();
            getDataFunc.Push(_luaFileName);
            getDataFunc.Push(args);
            getDataFunc.Push(key);
            getDataFunc.PCall();
            object data = getDataFunc.CheckObject(t);
            getDataFunc.EndPCall();
            getDataFunc.Dispose();
            return data;
        }


    }
}

