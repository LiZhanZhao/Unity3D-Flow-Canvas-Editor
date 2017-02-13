{
    "version": 2.6,
    "type": "FlowCanvas.FlowScript",
    "name": "FlowScript",
    "nodes": [
        {
            "value": {
                "_value": 10
            },
            "_inputPortValues": {

            },
            "_position": {
                "x": 5294.0,
                "y": 5292.0
            },
            "$type": "FlowCanvas.Nodes.GetVariable`1[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]",
            "$id": "2"
        },
        {
            "_inputPortValues": {
                "Obj": {

                }
            },
            "_position": {
                "x": 5613.0,
                "y": 5219.0
            },
            "$type": "FlowCanvas.Nodes.SimplexNodeWrapper`1[[FlowCanvas.Nodes.LogValue, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]",
            "$id": "5"
        },
        {
            "_inputPortValues": {

            },
            "_nodeColor": {
                "r": 1.0,
                "g": 0.3607843,
                "b": 0.3607843,
                "a": 1.0
            },
            "_position": {
                "x": 5350.0,
                "y": 5126.0
            },
            "$type": "FlowCanvas.Nodes.ConstructionEvent",
            "$id": "8"
        }
    ],
    "connections": [
        {
            "_sourcePortName": "Value",
            "_targetPortName": "Obj",
            "_sourceNode": {
                "$ref": "2"
            },
            "_targetNode": {
                "$ref": "5"
            },
            "$type": "FlowCanvas.BinderConnection`1[[System.Object, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]"
        },
        {
            "_sourcePortName": "Once",
            "_targetPortName": " ",
            "_sourceNode": {
                "$ref": "8"
            },
            "_targetNode": {
                "$ref": "5"
            },
            "$type": "FlowCanvas.BinderConnection"
        }
    ],
    "localBlackboard": {
        "_name": "Local Blackboard",
        "_variables": {

        }
    }
}