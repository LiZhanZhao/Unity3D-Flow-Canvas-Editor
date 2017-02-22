using System.Collections;
using ParadoxNotion.Design;
using FlowCanvas.Framework;

namespace FlowCanvas.Nodes{

	public class FinishNode : FlowNode {
		protected override void RegisterPorts(){
			var c = AddValueInput<bool>("Success");
			AddFlowInput("In", (f)=> { graph.Stop(c.value); });
		}
	}
}