using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NodeCanvas.Framework
{
    public class ConditionList : ConditionTask
    {
        public List<ConditionTask> conditions = new List<ConditionTask>();
    }
}

