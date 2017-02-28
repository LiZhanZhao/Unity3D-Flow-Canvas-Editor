using System.Collections;
using UnityEngine;
using FlowCanvas.Framework;
using ParadoxNotion.Design;

namespace FlowCanvas.Nodes{
    //[Name("*** Root *** ")]
    //[Category("Events/Root")]
    //[DoNotList]
    //[Description("11111111111111111111.")]
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