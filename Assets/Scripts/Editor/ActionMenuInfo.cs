using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace StoryEditorContext
{
    public class ActionMenuElement
    {
        public string path;
        public string actionName;

        public ActionMenuElement(){}
        public ActionMenuElement(string p, string an)
        {
            path = p;
            actionName = an;
        }
        
    }

    public class ActionMenuInfo
    {
        public static List<ActionMenuElement> elementList = new List<ActionMenuElement>(){
            new ActionMenuElement("特效/特效播放", "play_effect"),
            new ActionMenuElement("特效/特效播放1", "play_effect1"),
            new ActionMenuElement("特效/特效播放2", "play_effect2")





        };
        

    }

}
