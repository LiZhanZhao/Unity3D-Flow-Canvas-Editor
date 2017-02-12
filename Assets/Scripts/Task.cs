using UnityEngine;
using System.Collections;
namespace NodeCanvas.Framework
{
    abstract public partial class Task 
    {
        private ITaskSystem _ownerSystem;

        public ITaskSystem ownerSystem
        {
            get { return _ownerSystem; }
            private set { _ownerSystem = value; }
        }

        public void SetOwnerSystem(ITaskSystem newOwnerSystem)
        {

            if (newOwnerSystem == null)
            {
                Debug.LogError("ITaskSystem set in task is null!!");
                return;
            }

            ownerSystem = newOwnerSystem;

            //setting the bb in editor to update bbfields. in build runtime, bbfields are updated when the task init.
#if UNITY_EDITOR && CONVENIENCE_OVER_PERFORMANCE
			blackboard = newOwnerSystem.blackboard;
#endif
        }

        virtual public void OnValidate(ITaskSystem ownerSystem) { }
    }

}
