using UnityEngine;
using ParadoxNotion;
using NodeCanvas;

namespace FlowCanvas{

	///Value bindings use the generic version of FlowBinderConnection.
	///T is always the same at the 'target' ValueInput type.
	public class BinderConnection<T> : BinderConnection{

		///Binds source and target value ports
		public override void Bind(){

			if (!isActive){
				return;
			}

			DoNormalBinding(sourcePort, targetPort);
			
		}

		///Unbinds source and target value ports
		public override void UnBind(){
			if (targetPort is ValueInput){
				(targetPort as ValueInput).UnBind();
			}
		}

		//Normal binder from source Output, to target Input
		void DoNormalBinding(Port source, Port target){
			(target as ValueInput<T>).BindTo( (ValueOutput)source );
		}


		
	}
}