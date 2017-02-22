using UnityEngine;
using System.Collections;
using FlowCanvas;
using System.IO;
using System.Collections.Generic;
using FlowCanvas.Framework;
using FlowCanvas.Nodes;


public class Test : MonoBehaviour {
    //void InitLuaEnv()
    //{
    //    LuaClient lc = gameObject.GetComponent<LuaClient>();
    //    if (lc == null)
    //    {
    //        lc = gameObject.AddComponent<LuaClient>();
    //    }
    //}

	// Use this for initialization
    void Start()
    {
        //InitLuaEnv();
        TextAsset asset = Resources.Load<TextAsset>("bb");
        FlowGraphController.Play(asset.text);

    }

    //FlowScript GetCustomGraphLua()
    //{
    //    GraphSerializationData data = new GraphSerializationData();
    //    data.version = 1.0f;

    //    ///// Nodes
    //    RootNode onAwakeNode = new RootNode();

    //    GetVariable<float> getVarSpeed = new GetVariable<float>();
    //    getVarSpeed.SetVariable(10.0f);

    //    GetVariable<float> getVarTime = new GetVariable<float>();
    //    getVarTime.SetVariable(0.0f);

    //    string configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/story_command/get_targets.lua";
    //    LuaCommandNode getTargets = new LuaCommandNode();
    //    getTargets.Config(configFile);

    //    LuaActionNode luaNode = new LuaActionNode();
    //    configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/ai_action/rotate.lua";
    //    luaNode.Config(configFile);

    //    data.nodes.Add(onAwakeNode);
    //    data.nodes.Add(getVarSpeed);
    //    data.nodes.Add(luaNode);
    //    data.nodes.Add(getTargets);
    //    data.nodes.Add(getVarTime);

    //    ////// Connections
    //    BinderConnection<float> connection1 = new BinderConnection<float>();
    //    connection1.sourcePortID = "Value";
    //    connection1.sourceNode = getVarSpeed;
    //    connection1.targetPortID = "speed";
    //    connection1.targetNode = luaNode;

    //    BinderConnection<string[]> connection3 = new BinderConnection<string[]>();
    //    connection3.sourcePortID = "target1";
    //    connection3.sourceNode = getTargets;
    //    connection3.targetPortID = "Targets";
    //    connection3.targetNode = luaNode;

    //    BinderConnection<float> connection4 = new BinderConnection<float>();
    //    connection4.sourcePortID = "Value";
    //    connection4.sourceNode = getVarTime;
    //    connection4.targetPortID = "time";
    //    connection4.targetNode = luaNode;

    //    BinderConnection connection2 = new BinderConnection();
    //    connection2.sourcePortID = "Once";
    //    connection2.sourceNode = onAwakeNode;
    //    connection2.targetPortID = "In";
    //    connection2.targetNode = luaNode;



    //    data.connections.Add(connection1);
    //    data.connections.Add(connection2);
    //    data.connections.Add(connection3);
    //    data.connections.Add(connection4);


    //    FlowScript graph = new FlowScript();
    //    if (!graph.Deserialize(data, true))
    //    {
    //        Debug.LogError("Can not Deserialize File");
    //    }
    //    return graph;
    //}
    
}
