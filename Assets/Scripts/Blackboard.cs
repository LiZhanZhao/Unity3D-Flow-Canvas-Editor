using UnityEngine;
using System.Collections;
using System;
using NodeCanvas.Framework.Internal;
using System.Collections.Generic;

namespace NodeCanvas.Framework
{
    public class Blackboard : MonoBehaviour,IBlackboard
    {

        private BlackboardSource _blackboard = new BlackboardSource();

        public event System.Action<Variable> onVariableAdded;
        public event System.Action<Variable> onVariableRemoved;

        new public string name
        {
            get { return string.IsNullOrEmpty(_blackboard.name) ? gameObject.name + "_BB" : _blackboard.name; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = gameObject.name + "_BB";
                }
                _blackboard.name = value;
            }
        }

        public Dictionary<string, Variable> variables
        {
            get { return _blackboard.variables; }
            set { _blackboard.variables = value; }
        }

        public Variable GetVariableByID(string ID)
        {
            return _blackboard.GetVariableByID(ID);
        }


        public Variable GetVariable(string name, Type ofType = null)
        {
            return _blackboard.GetVariable(name, ofType);
        }

        public Variable AddVariable(string name, Type type)
        {
            var variable = _blackboard.AddVariable(name, type);
            if (onVariableAdded != null)
            {
                onVariableAdded(variable);
            }
            return variable;
        }

        ///Add a new variable of name and value
        public Variable AddVariable(string name, object value)
        {
            var variable = _blackboard.AddVariable(name, value);
            if (onVariableAdded != null)
            {
                onVariableAdded(variable);
            }
            return variable;
        }
    }
}

