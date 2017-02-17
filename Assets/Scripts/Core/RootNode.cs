using System.Collections;
using FlowCanvas;
using UnityEngine;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{
	public class RootNode : FlowNode {

        private FlowOutput once;
        private bool called = false;
        public override void OnGraphStarted()
        {
            if (!called)
            {
                Debug.Log("*** OnAwake ***");
                called = true;
                once.Call(new Flow(1));
                
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