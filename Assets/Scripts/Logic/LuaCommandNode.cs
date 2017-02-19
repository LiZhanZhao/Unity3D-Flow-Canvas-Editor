using UnityEngine;
using System.Collections;
using LuaInterface;
using System.Collections.Generic;
using ParadoxNotion;
using System;

namespace FlowCanvas.Nodes
{
    public class LuaCommandNode : LuaNode
    {

        protected override void AutoGenerateValueOutput(string fileContext)
        {
            Dictionary<string, string> valueOutputconf;
            if (!ParseHeadConfig(fileContext, kBeginConfigValueOutput, kEndConfigValueOutput, out valueOutputconf))
            {
                Debug.LogError(string.Format("{0} : {1}, {2} error ", _luaFileRelaPath, kBeginConfigValueOutput,
                    kEndConfigValueOutput));
            }

            foreach (KeyValuePair<string, string> conf in valueOutputconf)
            {
                string argName = conf.Key;
                string argType = conf.Value;
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
            return data;
        }


    }
}

