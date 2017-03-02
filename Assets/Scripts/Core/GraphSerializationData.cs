using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace FlowCanvas.Framework
{
    public class GraphSerializationData
    {
        public float version = 1.0f;
        public List<Node> nodes = new List<Node>();
        public List<Connection> connections = new List<Connection>();
        public float zoom;
        public Vector2 scrollOffset;


        //required
        public GraphSerializationData() { }

        //Construct
        public GraphSerializationData(GraphBase graph)
        {
            //connections are serialized seperately and not part of their parent node
            this.nodes = graph.allNodes;
            var structConnections = new List<Connection>();

            for (var i = 0; i < nodes.Count; i++)
            {
                for (var j = 0; j < nodes[i].outConnections.Count; j++)
                {
                    structConnections.Add(nodes[i].outConnections[j]);
                }
            }

            this.connections = structConnections;
            graph.OnSerialize(this);
        }

        ///MUST reconstruct before using the data
        public void Reconstruct(GraphBase graph)
        {

            //check serialization versions here in the future if needed

            //re-link connections for deserialization
            for (var i = 0; i < this.connections.Count; i++)
            {
                connections[i].sourceNode.outConnections.Add(connections[i]);
                connections[i].targetNode.inConnections.Add(connections[i]);
            }

            //re-set the node's owner
            for (var i = 0; i < this.nodes.Count; i++)
            {
                nodes[i].graphBase = graph;
            }

            graph.OnDeserialize(this);
        }
    }

}