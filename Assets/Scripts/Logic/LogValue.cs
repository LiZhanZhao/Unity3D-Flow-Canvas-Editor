using UnityEngine;
using System.Collections;

namespace FlowCanvas.Nodes
{
    public class LogValue : CallableActionNode<object>
    {
        public override void Invoke(object obj)
        {
            //Debug.Log(obj);
            UnityEngine.UI.Text text = GameObject.Find("TestText").GetComponent<UnityEngine.UI.Text>();
            text.text = obj.ToString();
        }
    }
}

