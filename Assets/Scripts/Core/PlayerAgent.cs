using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowCanvas.Framework
{
    // 播放器
    public class PlayerAgent
    {
        public virtual void Play(string jsonStr)
        {

        }

        public virtual void Stop()
        {

        }

        public virtual void Pause()
        {

        }

        public virtual void UnPause() { }

        public virtual bool IsRunning()
        {
            return false;
        }

        public virtual bool IsPaused() 
        {
            return false;
        }

        public virtual List<int> GetRunningNodeIndex()
        {
            return new List<int>();
        }

    }
}

