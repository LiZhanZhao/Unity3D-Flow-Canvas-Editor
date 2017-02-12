using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NodeCanvas.Framework
{
    public class GlobalBlackboard : Blackboard
    {
        public static GlobalBlackboard Find(string name)
        {
            if (!Application.isPlaying)
            {
                return FindObjectsOfType<GlobalBlackboard>().Where(b => b.name == name).FirstOrDefault();
            }
            return allGlobals.Find(b => b.name == name);
        }

        public static List<GlobalBlackboard> allGlobals = new List<GlobalBlackboard>();
    }
}

