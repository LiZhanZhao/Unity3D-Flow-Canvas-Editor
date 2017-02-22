using UnityEngine;
using System.Collections;
using FlowCanvas.Framework;
using FlowCanvas.Nodes;

public class TestDeserialize : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //TestSerializeJson();
        TestSerializeJsonLua();
	}

    void TestSerializeJsonLua()
    {
        FlowGraph fs = new FlowGraph();
        ///// Nodes
        
        RootNode onAwakeNode = fs.AddNode<RootNode>();

        GetVariable<float> getVarSpeed = fs.AddNode<GetVariable<float>>();
        getVarSpeed.SetVariable(10.0f);

        GetVariable<float> getVarTime = fs.AddNode<GetVariable<float>>();
        getVarTime.SetVariable(5.0f);

        LuaCommandNode getTargets = fs.AddNode<LuaCommandNode>();
        string configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/story_command/get_targets.lua";
        getTargets.Config(configFile);

        LuaActionNode luaNode = fs.AddNode<LuaActionNode>();
        configFile = Application.dataPath + "/ToLuaPlugins/Lua/logic/ai_action/rotate.lua";
        luaNode.Config(configFile);


        GetVariable<string[]> getActorTypes = fs.AddNode<GetVariable<string[]>>();
        getActorTypes.SetVariable(new string[] {"A1","A2" });

        GetVariable<string[]> getActorGoNames = fs.AddNode <GetVariable<string[]>>();
        getActorGoNames.SetVariable(new string[] { "Go1", "Go2" });

        ////// Connections
        BinderConnection.Create(onAwakeNode.GetOutputPort("Once"), luaNode.GetInputPort("In"));
        BinderConnection.Create(getVarSpeed.GetOutputPort("Value"), luaNode.GetInputPort("speed"));
        BinderConnection.Create(getVarTime.GetOutputPort("Value"), luaNode.GetInputPort("time"));

        BinderConnection.Create(getTargets.GetOutputPort("target1"), luaNode.GetInputPort("Targets"));

        BinderConnection.Create(getActorTypes.GetOutputPort("Value"), getTargets.GetInputPort("actorTypes"));
        BinderConnection.Create(getActorGoNames.GetOutputPort("Value"), getTargets.GetInputPort("actorGoNames"));
        

        string testJsonStr = fs.Serialize(true);
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/cc.txt", testJsonStr);

    }


    void TestSerializeJson()
    {
        FlowGraph fs = new FlowGraph();

        RootNode onAwakeNode = fs.AddNode<RootNode>();

        GetVariable<float> getVarFloat = fs.AddNode<GetVariable<float>>();
        getVarFloat.SetVariable(10.0f);

        SimplexNodeWrapper<LogValue> logNodeWrapper = fs.AddNode<SimplexNodeWrapper<LogValue>>();

        ////// Connections
        BinderConnection.Create(getVarFloat.GetOutputPort("Value"), logNodeWrapper.GetInputPort("Obj"));
        BinderConnection.Create(onAwakeNode.GetOutputPort("Once"), logNodeWrapper.GetInputPort(" "));

        string testJsonStr = fs.Serialize(true);
        System.IO.File.WriteAllText(Application.dataPath + "/Resources/bb.txt", testJsonStr);
    }
}
