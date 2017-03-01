using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ParadoxNotion.Serialization;
using ParadoxNotion;

namespace FlowCanvas.Framework
{

    abstract public partial class Graph : GraphBase //: ScriptableObject//, ISerializationCallbackReceiver
    {
        private bool _isRunning;
        public event System.Action<bool> OnFinish;
        //private float timeStarted;
        
        

        virtual protected void OnGraphStarted() { }

        virtual protected void OnGraphUpdate() { }

        virtual protected void OnGraphUnpaused() { }

        virtual protected void OnGraphPaused() { }

        virtual protected void OnGraphStoped() { }


        


        public bool isRunning
        {
            get { return _isRunning; }
            private set { _isRunning = value; }
        }

        private bool _isPaused;

        public bool isPaused
        {
            get { return _isPaused; }
            private set { _isPaused = value; }
        }

        public void StartGraph(Component agent, bool autoUpdate, System.Action<bool> callback = null)
        {
            if (isRunning)
            {
                if (callback != null)
                {
                    OnFinish += callback;
                }
                Debug.LogWarning("<b>Graph:</b> Graph is already Active.");
                return;
            }


            // 添加回调
            if (callback != null)
            {
                this.OnFinish = callback;
            }

            isRunning = true;

            if (autoUpdate)
            {
                MonoManager.current.onUpdate += UpdateGraph;
            }

            if (!isPaused)
            {
                //timeStarted = Time.time;
                OnGraphStarted();
            }
            else
            {
                OnGraphUnpaused();
            }

            // all node call OnGraphStarted
            for (var i = 0; i < allNodes.Count; i++)
            {
                if (!isPaused)
                {
                    allNodes[i].OnGraphStarted();
                }
                else
                {
                    allNodes[i].OnGraphUnpaused();
                }
            }

            isPaused = false;
        }

        public void UpdateGraph() { OnGraphUpdate(); }



        public void Pause()
        {

            if (!isRunning)
            {
                return;
            }

            MonoManager.current.onUpdate -= UpdateGraph;

            isRunning = false;
            isPaused = true;

            for (var i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].OnGraphPaused();
            }
            OnGraphPaused();
        }

        public void Stop(bool success = true)
        {
            if (!isRunning && !isPaused)
            {
                return;
            }

            MonoManager.current.onUpdate -= UpdateGraph;

            isRunning = false;
            isPaused = false;

            for (var i = 0; i < allNodes.Count; i++)
            {
                allNodes[i].OnGraphStoped();
            }

            OnGraphStoped();

            if (OnFinish != null)
            {
                OnFinish(success);
                OnFinish = null;
            }
            Debug.Log("*** Stop ***");
        }


    }
}