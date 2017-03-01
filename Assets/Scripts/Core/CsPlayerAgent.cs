using UnityEngine;
using System.Collections;

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
    }
}

