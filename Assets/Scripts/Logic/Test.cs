using UnityEngine;
using System.Collections;
using FlowCanvas;
using System.IO;
using System.Collections.Generic;
using FlowCanvas.Framework;
using FlowCanvas.Nodes;


public class Test : MonoBehaviour {
    void InitLuaEnv()
    {
        LuaClient lc = gameObject.GetComponent<LuaClient>();
        if (lc == null)
        {
            lc = gameObject.AddComponent<LuaClient>();
        }

        LuaClient.GetMainState()["__UNITY_EDITOR__"] = true;
        LuaClient.GetMainState().DoFile("editor/EditorAdapter.lua");
        
    }

	// Use this for initialization
    void Start()
    {
        InitLuaEnv();
        //TextAsset asset = Resources.Load<TextAsset>("bb");
        TextAsset asset = Resources.Load<TextAsset>("cc");
        FlowGraphController.Play(asset.text);

    }
    
}
