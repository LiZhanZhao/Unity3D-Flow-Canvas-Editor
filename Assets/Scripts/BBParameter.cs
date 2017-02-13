using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ParadoxNotion;

namespace NodeCanvas.Framework
{
    abstract public class BBParameter
    {
        private IBlackboard _bb;
        private string _targetVariableID;
        private string _name;
        private Variable _varRef;
        public static void SetBBFields(object o, IBlackboard bb)
        {
            var bbParams = GetObjectBBParameters(o);
            for (var i = 0; i < bbParams.Count; i++)
            {
                bbParams[i].bb = bb;
            }
        }

        public IBlackboard bb
        {
            get { return _bb; }
            set
            {
                if (_bb != value)
                {

#if UNITY_EDITOR //avoid when using Dyanmic Variables
                    if (!Application.isPlaying)
                    {
                        if (_bb != null)
                        {
                            _bb.onVariableAdded -= OnBBVariableAdded;
                            _bb.onVariableRemoved -= OnBBVariableRemoved;
                        }
                        if (value != null)
                        {
                            value.onVariableAdded += OnBBVariableAdded;
                            value.onVariableRemoved += OnBBVariableRemoved;
                        }
                    }
#endif

                    _bb = value;
                    varRef = value != null ? ResolveReference(_bb, true) : null;
                }
            }
        }

#if UNITY_EDITOR
        //Is the new variable added eligable to be used by this param?
        void OnBBVariableAdded(Variable variable)
        {
            if (variable != null && variable.name == this.name && variable.CanConvertTo(this.varType))
            {
                varRef = variable;
            }
        }

        //Is the variable removed this param's reference?
        void OnBBVariableRemoved(Variable variable)
        {
            if (variable == varRef)
            {
                varRef = null;
            }
        }
#endif
        public Variable varRef
        {
            get { return _varRef; }
            set
            {
                if (_varRef != value)
                {

#if UNITY_EDITOR //this avoids lots of allocations when using Dynamic Variables
                    if (!Application.isPlaying)
                    {
                        if (_varRef != null)
                        {
                            _varRef.onNameChanged -= OnRefNameChanged; //remove old one
                        }
                        if (value != null)
                        {
                            value.onNameChanged += OnRefNameChanged; //add new one
                            OnRefNameChanged(value.name); //update name immediately
                        }
                        targetVariableID = value != null ? value.ID : null;
                    }
#endif

                    _varRef = value;
                    Bind(value);
                }
            }
        }

#if UNITY_EDITOR
        //Is the param's variable reference changed name?
        void OnRefNameChanged(string newName)
        {
            if (_name.Contains("/"))
            { //is global
                var bbName = _name.Split('/')[0];
                newName = bbName + "/" + newName;
            }
            _name = newName;
        }
#endif

        public static List<BBParameter> GetObjectBBParameters(object o)
        {
            var bbParams = new List<BBParameter>();
            if (o == null)
            { //should not
                return bbParams;
            }
            var fields = o.GetType().RTGetFields();
            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                if (typeof(BBParameter).RTIsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(o);
                    if (value == null && field.FieldType != typeof(BBParameter))
                    {
                        value = Activator.CreateInstance(field.FieldType);
                        field.SetValue(o, value);
                    }
                    if (value != null)
                    {
                        bbParams.Add((BBParameter)value);
                    }
                    continue;
                }

                if (typeof(IList).RTIsAssignableFrom(field.FieldType) && !field.FieldType.IsArray && typeof(BBParameter).RTIsAssignableFrom(field.FieldType.RTGetGenericArguments()[0]))
                {
                    var list = field.GetValue(o) as IList;
                    if (list != null)
                    {
                        for (var j = 0; j < list.Count; j++)
                        {
                            var value = list[j];
                            if (value == null && field.FieldType != typeof(BBParameter))
                            {
                                value = Activator.CreateInstance(field.FieldType.RTGetGenericArguments()[0]);
                                list[j] = value;
                            }

                            if (value != null)
                            {
                                bbParams.Add((BBParameter)value);
                            }
                        }
                    }
                    continue;
                }

                if (o is ISubParametersContainer)
                {
                    var parameters = (o as ISubParametersContainer).GetIncludeParseParameters();
                    if (parameters != null && parameters.Length > 0)
                    {
                        bbParams.AddRange(parameters);
                    }
                }
            }

            return bbParams;
        }

        public string name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    varRef = value != null ? ResolveReference(bb, false) : null;

                }
                else
                {

#if UNITY_EDITOR
                    //This is done for editor convenience and it's not really mandatory. (handle other way?)
                    if (!Application.isPlaying)
                    {
                        if (varRef == null && !string.IsNullOrEmpty(value))
                        {
                            varRef = ResolveReference(bb, false);
                        }
                    }
#endif

                }
            }

        }

        private Variable ResolveReference(IBlackboard targetBlackboard, bool useID)
        {
            var targetName = this.name;
            if (targetName != null && targetName.Contains("/"))
            {
                var split = targetName.Split('/');
                targetBlackboard = GlobalBlackboard.Find(split[0]);
                targetName = split[1];
            }

            Variable result = null;
            if (targetBlackboard == null) { return null; }
            if (useID && targetVariableID != null) { result = targetBlackboard.GetVariableByID(targetVariableID); }
            if (result == null && !string.IsNullOrEmpty(targetName)) { result = targetBlackboard.GetVariable(targetName, varType); }
            return result;
        }

        private string targetVariableID
        {
            get { return _targetVariableID; }
            set { _targetVariableID = value; }
        }

        abstract public Type varType { get; }
        abstract protected void Bind(Variable data);
        public bool isNone
        {
            get { return name == string.Empty; }
        }

        public bool useBlackboard
        {
            get { return name != null; }
            set
            {
                if (value == false)
                {
                    name = null;
                }
                if (value == true && name == null)
                {
                    name = string.Empty;
                }
            }
        }

        public Variable PromoteToVariable(IBlackboard targetBB)
        {

            if (string.IsNullOrEmpty(name))
            {
                varRef = null;
                return null;
            }

            var varName = name;
            var bbName = targetBB != null ? targetBB.name : string.Empty;
            if (name.Contains("/"))
            {
                var split = name.Split('/');
                bbName = split[0];
                varName = split[1];
                targetBB = GlobalBlackboard.Find(bbName);
            }

            if (targetBB == null)
            {
                varRef = null;
                Debug.LogError(string.Format("Parameter '{0}' failed to promote to a variable, because Blackboard named '{1}' could not be found.", varName, bbName));
                return null;
            }

            varRef = targetBB.AddVariable(varName, varType);
            if (varRef != null)
            {
                Debug.Log(string.Format("Parameter '{0}' (of type '{1}') promoted to a Variable in Blackboard '{2}'.", varName, varType.FriendlyName(), bbName));
            }
            else
            {
                Debug.LogError(string.Format("Parameter {0} (of type '{1}') failed to promote to a Variable in Blackboard '{2}'.", varName, varType.FriendlyName(), bbName));
            }
            return varRef;
        }


    }

    public class BBParameter<T> : BBParameter
    {

        public BBParameter() { }
        public BBParameter(T value) { _value = value; }

        //delegates for Variable binding
        private Func<T> getter;
        private Action<T> setter;

        protected T _value;
        new public T value
        {
            get
            {
                if (getter != null)
                {
                    return getter();
                }

                //Dynamic?
                if (Application.isPlaying && bb != null && !string.IsNullOrEmpty(name))
                {
                    //setting the varRef property also binds it.
                    varRef = bb.GetVariable(name, typeof(T));
                    return getter != null ? getter() : default(T);
                }

                return _value;
            }
            set
            {
                if (setter != null)
                {
                    setter(value);
                    return;
                }

                if (isNone)
                {
                    return;
                }

                //Dynamic?
                if (bb != null && !string.IsNullOrEmpty(name))
                {
                    Debug.LogWarning(string.Format("Dynamic Parameter Variable '{0}' Encountered...", name));
                    //setting the varRef property also binds it
                    varRef = PromoteToVariable(bb);
                    if (setter != null) { setter(value); }
                    return;
                }

                _value = value;
            }
        }

        protected override void Bind(Variable variable)
        {
            if (variable == null)
            {
                getter = null;
                setter = null;
                _value = default(T);
                return;
            }

            BindGetter(variable);
            BindSetter(variable);
        }

        //Bind the Getter
        bool BindGetter(Variable variable)
        {
            if (variable is Variable<T>)
            {
                getter = (variable as Variable<T>).GetValue;
                return true;
            }

            if (variable.CanConvertTo(varType))
            {
                var func = variable.GetGetConverter(varType);
                getter = () => { return (T)func(); };
                return true;
            }

            return false;
        }

        //Bind the Setter
        bool BindSetter(Variable variable)
        {
            if (variable is Variable<T>)
            {
                setter = (variable as Variable<T>).SetValue;
                return true;
            }

            if (variable.CanConvertFrom(varType))
            {
                var func = variable.GetSetConverter(varType);
                setter = (T value) => { func(value); };
                return true;
            }

            return false;
        }


        public static implicit operator BBParameter<T>(T value)
        {
            return new BBParameter<T> { value = value };
        }

        public override Type varType
        {
            get { return typeof(T); }
        }
        
        
    }

}

