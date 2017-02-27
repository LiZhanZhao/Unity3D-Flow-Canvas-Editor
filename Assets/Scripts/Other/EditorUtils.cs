using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using ParadoxNotion;
using ParadoxNotion.Design;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
public class EditorUtils {

    public static void TextFieldComment(string check, string comment = "Comments...")
    {
        if (string.IsNullOrEmpty(check))
        {
            var lastRect = GUILayoutUtility.GetLastRect();
            GUI.color = new Color(1, 1, 1, 0.3f);
            //GUI.Label(lastRect, " <i>" + comment + "</i>");
            GUI.Label(lastRect, comment);
            GUI.color = Color.white;
        }
    }

    public static void ShowAutoEditorGUI(object o)
    {

        if (o == null)
        {
            return;
        }

        foreach (var field in o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            field.SetValue(o, GenericField(field.Name, field.GetValue(o), field.FieldType, field, o));
            GUI.backgroundColor = Color.white;
        }

        foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (prop.CanRead && prop.CanWrite)
            {
                IEnumerable<Attribute> attributes = new Attribute[0];
                attributes = prop.GetCustomAttributes(true).Cast<Attribute>();
                if (attributes.Any(a => a is ShowGUIPropertyAttribute))
                {
                    prop.SetValue(o, GenericField(prop.Name, prop.GetValue(o, null), prop.PropertyType, prop, o), null);
                }
                
            }
        }
    }

    public static object GenericField(string name, object value, Type t, MemberInfo member = null, object context = null)
    {
        if (t == null)
        {
            GUILayout.Label("NO TYPE PROVIDED!");
            return value;
        }

        // t FieldType
        //Preliminary Hides
        if (typeof(Delegate).IsAssignableFrom(t))
        {
            return value;
        }

        name = name.SplitCamelCase();

        IEnumerable<Attribute> attributes = new Attribute[0];
        attributes = member.GetCustomAttributes(true).Cast<Attribute>();

        if (member != null)
        {
            // Name Attribute
            var nameAtt = attributes.FirstOrDefault(a => a is NameAttribute) as NameAttribute;
            if (nameAtt != null)
            {
                name = nameAtt.name;
            }
        }

        if (member != null && attributes.Any(a => a is LuaRelaPathFieldAttribute))
        {
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.TextField(string.Format("Lua:"), (string)value);
            if (GUILayout.Button("Select"))
            {
                string luaPath = EditorUtility.OpenFilePanel("Select Lua file", "", "");
                EditorGUI.FocusTextInControl("");

                if (!luaPath.EndsWith(".lua"))
                {
                    if (EditorUtility.DisplayDialog("no lua file", "no lua file", "ok"))
                    {
                        return value;
                    }
                }

                return luaPath;
            }
            //EditorGUILayout.EndHorizontal();            
            return value;
            
        }

        if (t == typeof(string))
        {
            return EditorGUILayout.TextField(name, (string)value);
        }

        if (t == typeof(bool))
            return EditorGUILayout.Toggle(name, (bool)value);

        if (t == typeof(int))
        {
            if (member != null)
            {
                var sField = attributes.FirstOrDefault(a => a is SliderFieldAttribute) as SliderFieldAttribute;
                if (sField != null)
                    return (int)EditorGUILayout.Slider(name, (int)value, (int)sField.left, (int)sField.right);
            }

            return EditorGUILayout.IntField(name, (int)value);
        }

        if (t == typeof(float))
        {
            if (member != null)
            {
                var sField = attributes.FirstOrDefault(a => a is SliderFieldAttribute) as SliderFieldAttribute;
                if (sField != null)
                    return EditorGUILayout.Slider(name, (float)value, sField.left, sField.right);
            }
            return EditorGUILayout.FloatField(name, (float)value);
        }

        if (t == typeof(Vector2))
            return EditorGUILayout.Vector2Field(name, (Vector2)value);

        if (t == typeof(Vector3))
            return EditorGUILayout.Vector3Field(name, (Vector3)value);

        if (t == typeof(Vector4))
            return EditorGUILayout.Vector4Field(name, (Vector4)value);

        if (t == typeof(Color))
            return EditorGUILayout.ColorField(name, (Color)value);

        if (t == typeof(AnimationCurve))
            return EditorGUILayout.CurveField(name, (AnimationCurve)value);

        return value;
    }
}
#endif