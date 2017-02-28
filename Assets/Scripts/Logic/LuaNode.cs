using UnityEngine;
using System.Collections.Generic;
using System;
using ParadoxNotion;
using FlowCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes
{
    [DoNotList]
    public partial class LuaNode : FlowNode
    {
        protected const string kBeginConfigValueInput = "--BEGIN_VALUE_INPUT_CONFIG--";
        protected const string kEndConfigValueInput = "--END_VALUE_INPUT_CONFIG--";

        protected const string kBeginConfigValueOutput = "--BEGIN_VALUE_OUTPUT_CONFIG--";
        protected const string kEndConfigValueOutput = "--END_VALUE_OUTPUT_CONFIG--";

        protected const string kBeginConfigFlowInput = "--BEGIN_FLOW_INPUT_CONFIG--";
        protected const string kEndConfigFlowInput = "--END_FLOW_INPUT_CONFIG--";

        protected const string kBeginConfigFlowOutput = "--BEGIN_FLOW_OUTPUT_CONFIG--";
        protected const string kEndConfigFlowOutput = "--END_FLOW_OUTPUT_CONFIG--";

        protected const int kConfigArgLength = 3;

        [SerializeField]
        protected string _luaFileRelaPath = "";

        [LuaRelaPathField]
        [ShowGUIProperty]
        public string LuaPath
        {
            set {
                if (_luaFileRelaPath != value)
                {
                    Debug.Log("set lua path");
                    _luaFileRelaPath = value;
                    Config(value);
                }
                
            }
            get { return _luaFileRelaPath; }
        }

        protected string _luaFileName = "";
        
        protected List<ValueInput> _autoValueInputs = new List<ValueInput>();
        protected List<FlowOutput> _autoFlowOuts = new List<FlowOutput>();

        protected List<string> _autoValueInputArgNames = new List<string>();
        protected List<object> _autoValueInputArgValues = new List<object>();

        public bool ParseHeadConfig(string context, string beginConfig, string endConfig,
           out List<string> argNames, out List<string> argTypes)
        {
            
            argNames = new List<string>();
            argTypes = new List<string>();
            int beginIndex = context.IndexOf(beginConfig);
            int endIndex = context.IndexOf(endConfig);

            if (beginIndex < 0 || endIndex < 0)
            {
                return true;
            }

            string configContext = context.Substring(beginIndex, endIndex - beginIndex);
            beginIndex = configContext.IndexOf("{") + 1;
            endIndex = configContext.IndexOf("}") - 1;

            if (beginIndex < 0 || endIndex < 0)
            {
                return false;
            }

            string argsContext = configContext.Substring(beginIndex, endIndex - beginIndex);
            string[] args = argsContext.Split(',');
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.Length >= kConfigArgLength)
                {
                    string[] keyValue = arg.Split('=');
                    if (keyValue.Length != 2)
                    {
                        return false;
                    }

                    string argName = keyValue[0].Trim();
                    string argType = keyValue[1].Trim();
                    argType = argType.Replace("\"", "");
                    argNames.Add(argName);
                    argTypes.Add(argType);
                }
                
            }

            return true;
        }

        public override void OnValidate(GraphBase flowGraph)
        {
            base.OnValidate(flowGraph);

            if (!string.IsNullOrEmpty(_luaFileRelaPath))
            {
                _luaFileName = System.IO.Path.GetFileNameWithoutExtension(_luaFileRelaPath);
            }
        }
        

        protected override void RegisterPorts()
        {
            AutoGeneratePort();

        }

        protected void AutoGeneratePort()
        {
            if (!string.IsNullOrEmpty(_luaFileRelaPath))
            {
                string fileContext = System.IO.File.ReadAllText(_luaFileRelaPath);
                AutoGenerateValueInput(fileContext);
                AutoGenerateValueOutput(fileContext);
                AutoGenerateFlowInput(fileContext);
                AutoGenerateFlowOutput(fileContext);
            }

        }

        protected virtual void AutoGenerateValueInput(string fileContext)
        {
            List<string> argNames, argTypes;
            if (!ParseHeadConfig(fileContext, kBeginConfigValueInput, kEndConfigValueInput, out argNames, out argTypes))
            {
                Debug.LogError(string.Format("{0} : {1}, {2} error ", _luaFileRelaPath, kBeginConfigValueInput,
                    kEndConfigValueInput));
            }
            _autoValueInputArgNames.Clear();
            for (int i = 0; i < argNames.Count;i++)
            {
                string argName = argNames[i];
                string argType = argTypes[i];
                Type t = Type.GetType(argType);
                var portType = typeof(ValueInput<>).RTMakeGenericType(new Type[] { t });
                var port = (ValueInput)Activator.CreateInstance(portType, new object[] { this, argName, argName });
                AddValueInput(port, argName);
                _autoValueInputs.Add(port);
                _autoValueInputArgNames.Add(argName);

            }
        }

        protected virtual void AutoGenerateValueOutput(string fileContext)
        {
            //Dictionary<string, string> valueOutputconf;
            //if (!ParseHeadConfig(fileContext, kBeginConfigValueOutput, kEndConfigValueOutput, out valueOutputconf))
            //{
            //    Debug.LogError(string.Format("{0} : {1}, {2} error ", _luaFilePath, kBeginConfigValueOutput,
            //        kEndConfigValueOutput));
            //}

            //foreach (KeyValuePair<string, string> conf in valueOutputconf)
            //{
            //    string argName = conf.Key;
            //    string argType = conf.Value;
            //    Type t = Type.GetType(argType);
            //    var portType = typeof(ValueOutput<>).RTMakeGenericType(new Type[] { t });
            //    var port = (ValueOutput)Activator.CreateInstance(portType, new object[] { this, argName, {} });
            //}

        }

        protected virtual void AutoGenerateFlowInput(string fileContext) {
            //Dictionary<string, string> flowInputconf;
            //if (!ParseHeadConfig(fileContext, kBeginConfigFlowInput, kEndConfigFlowInput, out flowInputconf))
            //{
            //    Debug.LogError(string.Format("{0} : {1}, {2} error ", _luaFilePath, kBeginConfigFlowInput,
            //        kEndConfigFlowInput));
            //}


            //foreach (KeyValuePair<string, string> conf in flowInputconf)
            //{
            //    string argName = conf.Key;
            //    FlowInput flowInput = AddFlowInput(argName, (Flow f) =>{});
            //    _autoFlowIns.Add(flowInput);
            //}

        }
        protected virtual void AutoGenerateFlowOutput(string fileContext)
        {
            List<string> argNames, argTypes;
            if (!ParseHeadConfig(fileContext, kBeginConfigFlowOutput, kEndConfigFlowOutput, out argNames, out argTypes))
            {
                Debug.LogError(string.Format("{0} : {1}, {2} error ", _luaFileRelaPath, kBeginConfigFlowOutput,
                    kEndConfigFlowOutput));
            }

            for (int i = 0; i < argNames.Count; i++)
            {
                string argName = argNames[i];
                FlowOutput flowOut = AddFlowOutput(argName);
                _autoFlowOuts.Add(flowOut);
            }
        }

#if UNITY_EDITOR
        /// UnityEditor
        virtual public void Config(string luaFilePath)
        {
            string luaRelaPath = UnityEditor.FileUtil.GetProjectRelativePath(luaFilePath);
            _luaFileRelaPath = luaRelaPath;
            _luaFileName = System.IO.Path.GetFileNameWithoutExtension(_luaFileRelaPath);
            OnValidate(graphBase);
        }
#endif
    }

}

