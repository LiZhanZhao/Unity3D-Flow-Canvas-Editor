using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NodeCanvas.Framework
{
    public class ActionList : ActionTask
    {
        public List<ActionTask> actions = new List<ActionTask>();
    }
}

