using System.Collections;
using FlowCanvas;
using UnityEngine;
namespace FlowCanvas.Nodes{
	public class ConstructionEvent : EventNode {

        private FlowOutput once;
        private bool called = false;

        public override void OnGraphStarted()
        {
            if (!called)
            {
                called = true;
                once.Call(new Flow(1));
                Debug.Log("111111111111");
            }
        }

        // 这个方法是在Node的OnValidate中触发的
        protected override void RegisterPorts()
        {
            once = AddFlowOutput("Once");
        }
	}
}