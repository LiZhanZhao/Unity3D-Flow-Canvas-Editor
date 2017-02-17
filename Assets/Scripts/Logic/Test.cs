﻿using UnityEngine;
using System.Collections;
using FlowCanvas;
using System.IO;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using System.Collections.Generic;
using FlowCanvas.Nodes;
using LuaInterface;

public class Test : MonoBehaviour {

    //void Start()
    //{
    //    InitLuaEnv();
    //}

    void InitLuaEnv()
    {
        LuaClient lc = gameObject.GetComponent<LuaClient>();
        if (lc == null)
        {
            lc = gameObject.AddComponent<LuaClient>();
        }
        
        //LuaState state = LuaClient.GetMainState();
        //LuaFunction testLuaFunc = state.GetFunction("TestFuncArgsTable");

        //object keys = new object[] { "111", "222", "333" };
        //object args = new object[] { "aaa", "bbb", "ccc" };
        
        //testLuaFunc.BeginPCall();
        //testLuaFunc.Push(keys);
        //testLuaFunc.Push(args);
        //testLuaFunc.PCall();
        //testLuaFunc.EndPCall();

        //LuaActionNode node = new LuaActionNode();
        //node.Config(Application.dataPath + "/Lua/ActionConfig.lua");

    }

	// Use this for initialization
    void Start()
    {
        Debug.Log(typeof(List<>).ToString());
        //System.Type t = System.Type.GetType("System.Float");
        InitLuaEnv();
        FlowGraph graph = GetCustomGraphLua();
        //FlowGraph graph = GetCustomGraphUpdate();
        FlowScriptController fsc = gameObject.AddComponent<FlowScriptController>();
        fsc.graph = graph;
        //fsc.StartBehaviour(graph);

    }

    //FlowScript GetCustomGraphUpdate()
    //{
    //    GraphSerializationData data = new GraphSerializationData();
    //    data.version = 1.0f;

    //    ///// Nodes
    //    MouseEvents meNode = new MouseEvents();

    //    GetVariable<float> getVarFloat = new GetVariable<float>();
    //    getVarFloat.SetVariable(10.0f);

    //    SimplexNodeWrapper<LogValue> logNodeWrapper = new SimplexNodeWrapper<LogValue>();

    //    FinishNode finish = new FinishNode();

    //    data.nodes.Add(meNode);
    //    data.nodes.Add(getVarFloat);
    //    data.nodes.Add(logNodeWrapper);
    //    data.nodes.Add(finish);

    //    ////// Connections
    //    BinderConnection<object> connection1 = new BinderConnection<object>();
    //    connection1.sourcePortID = "Value";
    //    connection1.sourceNode = getVarFloat;
    //    connection1.targetPortID = "Obj";
    //    connection1.targetNode = logNodeWrapper;

    //    BinderConnection connection2 = new BinderConnection();
    //    connection2.sourcePortID = "Down";
    //    connection2.sourceNode = meNode;
    //    connection2.targetPortID = " ";
    //    connection2.targetNode = logNodeWrapper;

    //    BinderConnection connection3 = new BinderConnection();
    //    connection3.sourceNode = meNode;
    //    connection3.sourcePortID = "Up";
    //    connection3.targetNode = finish;
    //    connection3.targetPortID = "In";

    //    data.connections.Add(connection1);
    //    data.connections.Add(connection2);
    //    data.connections.Add(connection3);


    //    FlowScript graph = new FlowScript();
    //    if (!graph.Deserialize(data, true))
    //    {
    //        Debug.LogError("Can not Deserialize File");
    //    }
    //    return graph;
    //}

    //FlowScript GetCustomGraphOnce()
    //{
    //    GraphSerializationData data = new GraphSerializationData();
    //    data.version = 1.0f;

    //    ///// Nodes
    //    ConstructionEvent onAwakeNode = new ConstructionEvent();
        
    //    GetVariable<float> getVarFloat = new GetVariable<float>();
    //    getVarFloat.SetVariable(10.0f);

    //    SimplexNodeWrapper<LogValue> logNodeWrapper = new SimplexNodeWrapper<LogValue>();

    //    data.nodes.Add(onAwakeNode);
    //    data.nodes.Add(getVarFloat);
    //    data.nodes.Add(logNodeWrapper);

    //    ////// Connections
    //    BinderConnection<object> connection1 = new BinderConnection<object>();
    //    connection1.sourcePortID = "Value";
    //    connection1.sourceNode = getVarFloat;
    //    connection1.targetPortID = "Obj";
    //    connection1.targetNode = logNodeWrapper;

    //    BinderConnection connection2 = new BinderConnection();
    //    connection2.sourcePortID = "Once";
    //    connection2.sourceNode = onAwakeNode;
    //    connection2.targetPortID = " ";
    //    connection2.targetNode = logNodeWrapper;

    //    data.connections.Add(connection1);
    //    data.connections.Add(connection2);


    //    FlowScript graph = new FlowScript();
    //    if (!graph.Deserialize(data, true))
    //    {
    //        Debug.LogError("Can not Deserialize File");
    //    }
    //    return graph;
    //}


    FlowScript GetCustomGraphLua()
    {
        GraphSerializationData data = new GraphSerializationData();
        data.version = 1.0f;

        ///// Nodes
        RootNode onAwakeNode = new RootNode();

        GetVariable<float> getVarSpeed = new GetVariable<float>();
        getVarSpeed.SetVariable(10.0f);

        GetVariable<float> getVarTime = new GetVariable<float>();
        getVarTime.SetVariable(3.0f);

        string configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/story_command/get_targets.lua";
        LuaCommandNode getTargets = new LuaCommandNode();
        getTargets.Config(configFile);

        LuaActionNode luaNode = new LuaActionNode();
        configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/ai_action/rotate.lua";
        luaNode.Config(configFile);

        data.nodes.Add(onAwakeNode);
        data.nodes.Add(getVarSpeed);
        data.nodes.Add(luaNode);
        data.nodes.Add(getTargets);
        data.nodes.Add(getVarTime);

        ////// Connections
        BinderConnection<float> connection1 = new BinderConnection<float>();
        connection1.sourcePortID = "Value";
        connection1.sourceNode = getVarSpeed;
        connection1.targetPortID = "speed";
        connection1.targetNode = luaNode;

        BinderConnection<string[]> connection3 = new BinderConnection<string[]>();
        connection3.sourcePortID = "target1";
        connection3.sourceNode = getTargets;
        connection3.targetPortID = "Targets";
        connection3.targetNode = luaNode;

        BinderConnection<float> connection4 = new BinderConnection<float>();
        connection4.sourcePortID = "Value";
        connection4.sourceNode = getVarTime;
        connection4.targetPortID = "time";
        connection4.targetNode = luaNode;

        BinderConnection connection2 = new BinderConnection();
        connection2.sourcePortID = "Once";
        connection2.sourceNode = onAwakeNode;
        connection2.targetPortID = "In";
        connection2.targetNode = luaNode;



        data.connections.Add(connection1);
        data.connections.Add(connection2);
        data.connections.Add(connection3);
        data.connections.Add(connection4);


        FlowScript graph = new FlowScript();
        if (!graph.Deserialize(data, true))
        {
            Debug.LogError("Can not Deserialize File");
        }
        return graph;
    }

}
