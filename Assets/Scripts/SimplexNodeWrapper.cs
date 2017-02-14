using UnityEngine;
using ParadoxNotion;
using ParadoxNotion.Design;
using NodeCanvas.Framework;

namespace FlowCanvas.Nodes{
	///Wraps a SimplexNode
    ///
	public class SimplexNodeWrapper<T> : FlowNode where T:SimplexNode {

		private T _simplexNode;
		private T simplexNode{
			get
			{
				if (_simplexNode == null){
					_simplexNode = (T)System.Activator.CreateInstance(typeof(T));
					if (_simplexNode != null){
						base.GatherPorts();
					}
				}
				return _simplexNode;
			}
		}

		public override string name{
			get {return simplexNode != null? simplexNode.name : "NULL";}
		}


        override public string description
        {
            get { return simplexNode != null ? simplexNode.description : "NULL"; }
        }

		public override void OnGraphStarted(){
			if (simplexNode != null){
				simplexNode.OnGraphStarted();
			}
		}

		public override void OnGraphPaused(){
			if (simplexNode != null){
				simplexNode.OnGraphPaused();
			}
		}

		public override void OnGraphUnpaused(){
			if (simplexNode != null){
				simplexNode.OnGraphUnpaused();
			}			
		}

		public override void OnGraphStoped(){
			if (simplexNode != null){
				simplexNode.OnGraphStoped();
			}
		}

		protected override void RegisterPorts(){
			if (simplexNode != null){
				simplexNode.RegisterPorts(this);
			}
		}
		
	}
}