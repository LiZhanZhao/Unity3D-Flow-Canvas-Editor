using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using ParadoxNotion;
using ParadoxNotion.Design;

namespace FlowCanvas.Framework
{
    public class ScriptInfo
    {
        public Type type;
        public string name;
        public string category;
        public string description;


        public ScriptInfo(Type type, string name, string category)
        {
            this.type = type;
            this.name = name;
            this.category = category;
            if (type != null)
            {
                var descAtt = type.RTGetAttribute<DescriptionAttribute>(true);
                description = descAtt != null ? descAtt.description : description;
            }
        }
    }
}

