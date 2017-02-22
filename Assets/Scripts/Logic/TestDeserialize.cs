using UnityEngine;
using System.Collections;
using FlowCanvas.Framework;
using FlowCanvas.Nodes;

public class TestDeserialize : MonoBehaviour {

	// Use this for initialization
	void Start () {
        TestSerializeJson();
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
