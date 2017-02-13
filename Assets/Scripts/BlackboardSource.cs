using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ParadoxNotion;
using ParadoxNotion.Design;

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

        public Variable AddVariable(string varName, Type type)
        {

            if (variables.ContainsKey(varName))
            {
                var existing = GetVariable(varName, type);
                if (existing == null)
                {
                    Debug.LogError(string.Format("<b>Blackboard:</b> Variable with name '{0}' already exists in blackboard '{1}', but is of different type! Returning null instead of new.", varName, this.name));
                }
                else
                {
                    Debug.LogWarning(string.Format("<b>Blackboard:</b> Variable with name '{0}' already exists in blackboard '{1}'. Returning existing instead of new.", varName, this.name));
                }
                return existing;
            }

            var dataType = typeof(Variable<>).RTMakeGenericType(new Type[] { type });
            var newData = (Variable)Activator.CreateInstance(dataType);
            newData.name = varName;
            variables[varName] = newData;
            if (onVariableAdded != null)
            {
                onVariableAdded(newData);
            }
            return newData;
        }

        public Variable AddVariable(string varName, object value)
        {

            if (value == null)
            {
                Debug.LogError("<b>Blackboard:</b> You can't use AddVariable with a null value. Use AddVariable(string, Type) to add the new data first");
                return null;
            }

            var newData = AddVariable(varName, value.GetType());
            if (newData != null)
            {
                newData.value = value;
            }

            return newData;
        }
    }

}

