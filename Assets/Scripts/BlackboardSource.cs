using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace NodeCanvas.Framework.Internal
{
    sealed public class BlackboardSource : IBlackboard
    {
        public event System.Action<Variable> onVariableAdded;
        public event System.Action<Variable> onVariableRemoved;

        private string _name;
        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>(StringComparer.Ordinal);
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public Dictionary<string, Variable> variables
        {
            get { return _variables; }
            set { _variables = value; }
        }

        public void InitializePropertiesBinding(GameObject targetGO, bool callSetter)
        {
            foreach (var data in variables.Values)
            {
                data.InitializePropertyBinding(targetGO, callSetter);
            }
        }

        public Variable GetVariableByID(string ID)
        {
            if (variables != null && ID != null)
            {
                foreach (var pair in variables)
                {
                    if (pair.Value.ID == ID)
                    {
                        return pair.Value;
                    }
                }
            }
            return null;
        }

        public Variable GetVariable(string varName, Type ofType = null)
        {
            if (variables != null && varName != null)
            {
                Variable data;
                if (variables.TryGetValue(varName, out data))
                {
                    if (ofType == null || data.CanConvertTo(ofType))
                    {
                        return data;
                    }
                }
            }
            return null;
        }
    }

}

