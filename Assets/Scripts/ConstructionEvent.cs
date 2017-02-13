using System.Collections;
using FlowCanvas;
using UnityEngine;
using ParadoxNotion.Design;

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

        /*
        // 作为FlowInput
        [Name("bbb")]
        public void Test(Flow f)
        {

        }
         * */

        protected override void RegisterPorts()
        {
            once = AddFlowOutput("Once");
        }
	}
}