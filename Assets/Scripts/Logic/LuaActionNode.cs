using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LuaInterface;
using FlowCanvas.Framework;

namespace FlowCanvas.Nodes
{
    public partial class  LuaActionNode : LuaNode, IUpdatable
    {
        public static UInt32 counter = 0;
        
        private FlowInput _flowIn;
        private ValueInput<string> _targetValueIn;
        private FlowOutput _flowOut;

        private string _actionKey = "";
        private bool _isFinish = false;

        [SerializeField]
        private string _targetsArgValues;

        public string TargetArgValues
        {
            set { _targetsArgValues = value; }
            get { return _targetsArgValues; }
        }

        public void Update()
        {
            if (!_isFinish && UpdateAction())
            {
                EndAction();
            }
        }

        public override void OnValidate(GraphBase flowGraph)
        {
            base.OnValidate(flowGraph);
            _actionKey = GenerateLuaKey(_luaFileName);
        }

        void BeginAction()
        {
            _isFinish = false;
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

            string targets = _targetValueIn.value;
            LuaState state = LuaClient.GetMainState();
            LuaFunction addActionFunc = state.GetFunction("SMAddAction");
            object[] args = argValue.ToArray();
            addActionFunc.BeginPCall();
            addActionFunc.Push(_actionKey);
            addActionFunc.Push(_luaFileName);
            addActionFunc.Push(targets);
            addActionFunc.Push(args);
            addActionFunc.PCall();
            addActionFunc.EndPCall();
            addActionFunc.Dispose();
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
            updateActionFunc.Dispose();
            return isFinish;

        }

        void EndAction()
        {
            Debug.Log("**** C# EndAction");
            _isFinish = true;

            LuaState state = LuaClient.GetMainState();
            LuaFunction delActionFunc = state.GetFunction("SMDelAction");
            delActionFunc.BeginPCall();
            delActionFunc.Push(_actionKey);
            delActionFunc.PCall();
            delActionFunc.EndPCall();
            delActionFunc.Dispose();
            CallFlowOutputs();
        }

        void CallFlowOutputs()
        {
            _flowOut.Call(new Flow(1));
        }


        override public void OnGraphStoped() { }

        protected override void RegisterPorts()
        {
            // FlowInput
            _flowIn = AddFlowInput("In", (Flow f) =>
            {
                BeginAction();
            });

            _flowOut = AddFlowOutput("Out");

            // valueInput
            _targetValueIn = AddValueInput<string>("Targets");

            AutoGeneratePort();
            
        }

        static string GenerateLuaKey(string luaName)
        {
            return luaName + (counter++).ToString();
        }
    }
}

