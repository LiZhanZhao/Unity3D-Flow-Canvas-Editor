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
    public class LuaActionNode : FlowNode, IUpdatable
    {
        public static UInt32 counter = 0;
        private string _luaFilePath = "";
        private string _luaFileName = "";
        private List<string> _argNames = new List<string>();

        private FlowInput _in;
        private FlowOutput _out;
        private ValueInput<string[]> _targetIn;

        private string _actionKey = "";
        private bool _isFinish = false;

        private const string kBeginConfig = "--BEGIN_CONFIG--";
        private const string kEndConfig = "--END_CONFIG--";
        public override string name
        {
            get { return "LuaNode"; }
        }

        public void Update()
        {
            if (!_isFinish && UpdateAction())
            {
                EndAction();
            }
        }
        //override public void OnGraphStarted() {
        //}

        void BeginAction()
        {
            _isFinish = false;
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

            string[] targets = null ;
            if (_targetIn.value == null)
            {
                targets = new string[] { };
            }
            else
            {
                targets = _targetIn.value;
            }
            

            LuaState state = LuaClient.GetMainState();
            LuaFunction addActionFunc = state.GetFunction("SMAddAction");
            string[] keys = _argNames.ToArray();
            object[] args = argValue.ToArray();
            addActionFunc.BeginPCall();
            addActionFunc.Push(_actionKey);
            addActionFunc.Push(_luaFileName);
            addActionFunc.Push(targets);
            addActionFunc.Push(args);
            addActionFunc.PCall();
            addActionFunc.EndPCall();
        }

        bool UpdateAction()
        {
            LuaState state = LuaClient.GetMainState();
            LuaFunction updateActionFunc = state.GetFunction("SMUpdateAction");
            updateActionFunc.BeginPCall();
            updateActionFunc.Push(_actionKey);
            updateActionFunc.Push(Time.deltaTime);
            updateActionFunc.PCall();
            bool isFinish = updateActionFunc.CheckBoolean();  
            updateActionFunc.EndPCall();
            return isFinish;

        }

        void EndAction()
        {
            Debug.Log("**** C# EndAction");
            _isFinish = true;
            _out.Call(new Flow(1));
        }

        override public void OnGraphStoped() { }

        protected override void RegisterPorts()
        {
            _out = AddFlowOutput("Out");
            _in = AddFlowInput("In", (Flow f) => {
                BeginAction();
            });

            _targetIn = AddValueInput<string[]>("Targets");

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
            _luaFileName = System.IO.Path.GetFileNameWithoutExtension(_luaFilePath);
            _actionKey = _luaFileName + (counter++).ToString();

        }

        


    }
}

