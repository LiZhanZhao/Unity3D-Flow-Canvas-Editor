using UnityEngine;
using System.Collections.Generic;

namespace FlowCanvas.Framework
{
    public delegate void UIMsgCallBack(object[] args);

    public class UIMsg
    {
        public string id;
        public object[] args;

        public UIMsg() { }
        public UIMsg(string id, object[] args)
        {
            this.id = id;
            this.args = args;
        }
    }

    public class UIMsgMgr : MonoBehaviour
    {
        private static UIMsgMgr _instance = null;
        public static UIMsgMgr GetInstance()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<UIMsgMgr>();
                if (_instance == null)
                {
                    _instance = new GameObject("_UIMsgMgr").AddComponent<UIMsgMgr>();
                }

            }
            return _instance;
        }

        // 消息队列
        private Queue<UIMsg> _msgQueue = new Queue<UIMsg>();

        // 监听消息
        private Dictionary<string, List<UIMsgCallBack>> _msgListener = new Dictionary<string, List<UIMsgCallBack>>();

        public void Register(string msgId, UIMsgCallBack cb)
        {
            if (!_msgListener.ContainsKey(msgId))
            {
                _msgListener.Add(msgId, new List<UIMsgCallBack>());
                _msgListener[msgId].Add(cb);
            }
            _msgListener[msgId].Add(cb);
        }

        
        public void Post(string msgId, object[] msgArgs = null)
        {
            _msgQueue.Enqueue(new UIMsg(msgId, msgArgs));
        }

        public void Clear()
        {
            _msgQueue.Clear();
            _msgListener.Clear();
        }

        public void Update()
        {
            DispatchAllMsg();
        }

        private void DispatchAllMsg()
        {
            // 一次抛出所有的msg
            for (int i = 0; i < _msgQueue.Count; i++)
            {
                UIMsg msg = _msgQueue.Dequeue();
                Dispatch(msg);
            }
            
        }

        private void Dispatch(UIMsg msg)
        {
            string id = msg.id;
            object[] args = msg.args;
            if (_msgListener.ContainsKey(id))
            {
                List<UIMsgCallBack> cbList = _msgListener[id];
                for (int i = 0; i < cbList.Count; i++)
                {
                    UIMsgCallBack cb = cbList[i];
                    cb(args);
                }
            }

        }
    }
}

