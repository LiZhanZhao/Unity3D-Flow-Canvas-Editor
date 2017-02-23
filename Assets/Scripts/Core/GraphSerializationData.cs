using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace FlowCanvas.Framework
{
    public class GraphSerializationData
    {
        public float version = 1.0f;
        // 节点列表
        public List<Node> nodes = new List<Node>();
        // 连线列表
        public List<Connection> connections = new List<Connection>();
        public Node primeNode = null;
        public object derivedData;

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

            //serialize derived data
            this.derivedData = graph.OnDerivedDataSerialization();
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

            //re-set the node's owner and ID
            for (var i = 0; i < this.nodes.Count; i++)
            {
                nodes[i].graphBase = graph;
                nodes[i].ID = i + 1;
            }

            //deserialize derived data
            graph.OnDerivedDataDeserialization(derivedData);
        }
    }

}