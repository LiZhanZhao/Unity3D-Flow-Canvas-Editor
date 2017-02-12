using UnityEngine;
using System.Collections;
using FlowCanvas;
using System.IO;
using NodeCanvas.Framework;
public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        
        string path = Application.dataPath + "/bb.FS";
        FlowScript graph = new FlowScript();
        //FlowScript graph = ScriptableObject.CreateInstance<FlowScript>();
        if (graph.Deserialize(File.ReadAllText(path), true, null) == null)
        {
            Debug.LogError("Can not Deserialize File");
        }
        Debug.Log(path);
        FlowScriptController fsc = gameObject.AddComponent<FlowScriptController>();
        fsc.graph = graph;
        


	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
