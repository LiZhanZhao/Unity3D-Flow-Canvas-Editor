using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowCanvas.Framework
{
    public class CsPlayerAgent : PlayerAgent
    {
        public override void Play(string jsonStr)
        {
            FlowGraphPlayer.Play(jsonStr, true);
        }

        public override void Stop()
        {
            FlowGraphPlayer.Stop();
        }

        public override void Pause()
        {
            FlowGraphPlayer.Pause();
        }

        public override void UnPause() {
            FlowGraphPlayer.UnPause();
        }

        public override bool IsRunning()
        {
            if (FlowGraphPlayer.current.graph != null)
                return FlowGraphPlayer.current.graph.isRunning;
            else
                return false;
        }

        public override bool IsPaused()
        {
            if (FlowGraphPlayer.current.graph != null)
                return FlowGraphPlayer.current.graph.isPaused;
            else
                return false;
        }

        public override List<int> GetRunningNodeIndex()
        {
            List<int> indexList = new List<int>();
            Graph graph = FlowGraphPlayer.current.graph;
            if (FlowGraphPlayer.current.graph != null)
            {
                List<Node> nodes = graph.allNodes;
                for (int i = 0; i < nodes.Count; i++)
                {
                    Node node = nodes[i];
                    if (node.IsRunning)
                    {
                        indexList.Add(node.ID);
                    }
                }
                return indexList;
            }
            else
            {
                return indexList;
            }
        }
    }
}

