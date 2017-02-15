using UnityEngine;
using System.Collections;
//using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace FlowCanvas.Nodes{

	public class MouseEvents : EventNode, IUpdatable {

		public enum ButtonKeys
		{
			Left = 0,
			Right = 1,
			Middle = 2
		}

		public ButtonKeys buttonKey;

		private FlowOutput down;
		private FlowOutput pressed;
		private FlowOutput up;

		public override string name{
			get {return string.Format("<color=#ff5c5c>➥ Mouse Button '{0}'</color>", buttonKey.ToString() ).ToUpper();}
		}

		protected override void RegisterPorts(){
			down = AddFlowOutput("Down");
			pressed = AddFlowOutput("Pressed");
			up = AddFlowOutput("Up");
		}

		public void Update(){
			if (Input.GetMouseButtonDown((int)buttonKey)){
				down.Call(new Flow(1));
			}

			if (Input.GetMouseButton((int)buttonKey)){
				pressed.Call(new Flow(1));
			}

			if (Input.GetMouseButtonUp((int)buttonKey)){
				up.Call(new Flow(1));
			}
		}
	}
}