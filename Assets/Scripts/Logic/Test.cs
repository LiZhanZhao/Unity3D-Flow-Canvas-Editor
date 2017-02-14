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
        //FlowGraph graph = GetCustomGraphOnce();
        FlowGraph graph = GetCustomGraphUpdate();
        FlowScriptController fsc = gameObject.AddComponent<FlowScriptController>();
        fsc.graph = graph;
        //fsc.StartBehaviour(graph);
        
	}

    FlowScript GetCustomGraphUpdate()
    {
        GraphSerializationData data = new GraphSerializationData();
        data.version = 1.0f;

        ///// Nodes
        MouseEvents meNode = new MouseEvents();

        GetVariable<float> getVarFloat = new GetVariable<float>();
        getVarFloat.SetVariable(10.0f);

        SimplexNodeWrapper<LogValue> logNodeWrapper = new SimplexNodeWrapper<LogValue>();

        FinishNode finish = new FinishNode();

        data.nodes.Add(meNode);
        data.nodes.Add(getVarFloat);
        data.nodes.Add(logNodeWrapper);
        data.nodes.Add(finish);

        ////// Connections
        BinderConnection<object> connection1 = new BinderConnection<object>();
        connection1.sourcePortID = "Value";
        connection1.sourceNode = getVarFloat;
        connection1.targetPortID = "Obj";
        connection1.targetNode = logNodeWrapper;

        BinderConnection connection2 = new BinderConnection();
        connection2.sourcePortID = "Down";
        connection2.sourceNode = meNode;
        connection2.targetPortID = " ";
        connection2.targetNode = logNodeWrapper;

        BinderConnection connection3 = new BinderConnection();
        connection3.sourceNode = meNode;
        connection3.sourcePortID = "Up";
        connection3.targetNode = finish;
        connection3.targetPortID = "In";

        data.connections.Add(connection1);
        data.connections.Add(connection2);
        data.connections.Add(connection3);


        FlowScript graph = new FlowScript();
        if (!graph.Deserialize(data, true))
        {
            Debug.LogError("Can not Deserialize File");
        }
        return graph;
    }

    FlowScript GetCustomGraphOnce()
    {
        GraphSerializationData data = new GraphSerializationData();
        data.version = 1.0f;

        ///// Nodes
        ConstructionEvent onAwakeNode = new ConstructionEvent();
        
        GetVariable<float> getVarFloat = new GetVariable<float>();
        getVarFloat.SetVariable(10.0f);

        SimplexNodeWrapper<LogValue> logNodeWrapper = new SimplexNodeWrapper<LogValue>();

        data.nodes.Add(onAwakeNode);
        data.nodes.Add(getVarFloat);
        data.nodes.Add(logNodeWrapper);

        ////// Connections
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
        if (!graph.Deserialize(data, true))
        {
            Debug.LogError("Can not Deserialize File");
        }
        return graph;
    }

}
