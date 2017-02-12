using UnityEngine;
using System.Collections.Generic;
using System;

namespace NodeCanvas.Framework
{
    public interface IBlackboard
    {
        event Action<Variable> onVariableAdded;
        event Action<Variable> onVariableRemoved;

        string name { get; set; }
        Dictionary<string, Variable> variables { get; set; }

        Variable GetVariableByID(string ID);
        Variable GetVariable(string varName, Type ofType = null);
    }
}

