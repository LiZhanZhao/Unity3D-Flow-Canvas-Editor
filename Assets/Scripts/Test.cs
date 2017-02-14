using UnityEngine;
using System.Collections;
using FlowCanvas;
using System.IO;
using NodeCanvas.Framework;
using NodeCanvas.Framework.Internal;
using System.Collections.Generic;
using FlowCanvas.Nodes;

public class Test : MonoBehaviour {

    //const string path = Application.dataPath + "/cc.FS";

	// Use this for initialization
	void Start () {
        FlowGraph graph = GetCustomGraph();
        FlowScriptController fsc = gameObject.AddComponent<FlowScriptController>();
        fsc.graph = graph;
        //fsc.StartBehaviour(graph);
        
	}


    FlowScript GetCustomGraph()
    {
        
        GraphSerializationData data = new GraphSerializationData();
        data.version = 2.6f;
        data.type = typeof(FlowCanvas.FlowScript);
        data.name = "FlowCanvas.FlowScript";
        data.localBlackboard = new BlackboardSource();

        ///// Nodes
        ConstructionEvent onAwakeNode = new ConstructionEvent();

        GetVariable<float> getVarFloat = new GetVariable<float>();
        getVarFloat.value = new BBParameter<float>();
        getVarFloat.SetVariable(10.0f);

        SimplexNodeWrapper<LogValue> logNodeWrapper = new SimplexNodeWrapper<LogValue>();

        data.nodes.Add(onAwakeNode);
        data.nodes.Add(getVarFloat);
        data.nodes.Add(logNodeWrapper);

        ////// Connections
        //BinderConnection connection1 = new BinderConnection();
        BinderConnection<object> connection1 = new BinderConnection<object>();
        connection1.sourcePortID = "Value";
        connection1.sourceNode = getVarFloat;
        connection1.targetPortID = "Obj";
        connection1.targetNode = logNodeWrapper;

        BinderConnection connection2 = new BinderConnection();
        connection2.sourcePortID = "Once";
        connection2.sourceNode = onAwakeNode;
        connection2.targetPortID = " ";
        connection2.targetNode = logNodeWrapper;

        data.connections.Add(connection1);
        data.connections.Add(connection2);


        FlowScript graph = new FlowScript();
        if (!graph.Deserialize(data, true, null))
        {
            Debug.LogError("Can not Deserialize File");
        }
        return graph;
    }

}
