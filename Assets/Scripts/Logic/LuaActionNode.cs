using UnityEngine;
using System.Collections;
using FlowCanvas;
using NodeCanvas.Framework;
using System.Collections.Generic;
using System;
using ParadoxNotion;
using LuaInterface;

namespace FlowCanvas.Nodes
{
    public class LuaActionNode : FlowNode
    {
        private string _luaFilePath = "";
        private List<string> _argNames = new List<string>();

        private FlowInput _in;
        private FlowOutput _out;

        private const string kBeginConfig = "--BEGIN_CONFIG--";
        private const string kEndConfig = "--END_CONFIG--";
        public override string name
        {
            get { return "LuaNode"; }
        }

        public void Update()
        {
            if (!UpdateAction())
            {
                EndAction();
            }
        }
        override public void OnGraphStarted() {
            Debug.Log("LuaActionNode On Graph Started");
            // when Update return true, will call
            //_out.Call(new Flow(1));
            InitAction();
        }

        void InitAction()
        {
            // 生成Key
            List<object> argValue = new List<object>();
            foreach (string argName in _argNames)
            {
                Port p = GetInputPort(argName);
                if (p is ValueInput)
                {
                    ValueInput valueInput = (ValueInput)p;
                    argValue.Add(valueInput.value);
                }
            }

            LuaState state = LuaClient.GetMainState();
            LuaFunction testLuaFunc = state.GetFunction("TestFuncArgsTable");
            string[] keys = _argNames.ToArray();
            object[] args = argValue.ToArray();
            testLuaFunc.BeginPCall();
            testLuaFunc.Push(keys);
            testLuaFunc.Push(args);
            testLuaFunc.PCall();
            testLuaFunc.EndPCall();
        }

        void BeginAction()
        {
            
        }

        bool UpdateAction()
        {
            return false;
        }

        void EndAction()
        {

        }

        override public void OnGraphStoped() { }

        protected override void RegisterPorts()
        {
            _out = AddFlowOutput("Out");
            _in = AddFlowInput("In", (Flow f) => {
                BeginAction();
            });
            AutoGenerateValueInput(_luaFilePath);
            
        }

        private void AutoGenerateValueInput(string luaFilePath)
        {
            if (!string.IsNullOrEmpty(luaFilePath))
            {
                string fileContext = System.IO.File.ReadAllText(luaFilePath);
                int beginIndex = fileContext.IndexOf(kBeginConfig);
                int endIndex = fileContext.IndexOf(kEndConfig);
                string configContext = fileContext.Substring(beginIndex, endIndex - beginIndex);
                beginIndex = configContext.IndexOf("{") + 1;
                endIndex = configContext.IndexOf("}") - 1;
                string argsContext = configContext.Substring(beginIndex, endIndex - beginIndex);
                string[] args = argsContext.Split(',');
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];
                    string[] keyValue = arg.Split('=');
                    string argName = keyValue[0].Trim();
                    string argType = keyValue[1].Trim();
                    argType = argType.Replace("\"", "");
                    Debug.Log(argName + "   " + argType);
                    Type t = Type.GetType(argType);
                    var portType = typeof(ValueInput<>).RTMakeGenericType(new Type[] { t });
                    var port = (ValueInput)Activator.CreateInstance(portType, new object[] { this, argName, argName });
                    AddValueInput(port, argName);
                    _argNames.Add(argName);
                }
            }
        }

        

        public void Config(string luaFilePath)
        {
            _luaFilePath = luaFilePath;
        }

        


    }
}

