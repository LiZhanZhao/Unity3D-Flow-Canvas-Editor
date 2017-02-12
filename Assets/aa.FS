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
                "x": 5355.0,
                "y": 5345.0
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
                "x": 5621.0,
                "y": 5309.0
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
                "x": 5336.0,
                "y": 5189.0
            },
            "$type": "FlowCanvas.Nodes.MouseEvents",
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
            "_sourcePortName": "Down",
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