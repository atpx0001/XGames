using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;
using System;
using System.Linq;


[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class TestEditor: Editor {
    bool _showFunc = false;
    static GUIStyle _foldout = null;
    static GUIStyle _button = null;
    public override void OnInspectorGUI() {
        base.DrawDefaultInspector();

        if(_foldout == null) {
            _foldout = new GUIStyle(EditorStyles.radioButton);
            _foldout.richText = true;
        }
        if(_button == null) {
            _button = new GUIStyle(EditorStyles.toolbarButton);
            _button.richText = true;
        }
        _showFunc = EditorGUILayout.Foldout(_showFunc, "<color=green>FastFunc</color>", _foldout);
        if(_showFunc) {
            MonoBehaviour cls = target as MonoBehaviour;
            Type t = cls.GetType();
            MethodInfo[] methods = t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach(MethodInfo m in methods) {
                ParameterInfo[] pms = m.GetParameters();
                if(pms.Length > 0) {
                    string info = m.Name + "(" + string.Join(",", pms.Select(pm => pm.ParameterType.Name).ToArray()) + ")";
                    GUILayout.Label(info);
                } else {
                    if(GUILayout.Button("<color=green>" + m.Name + "</color>", _button)) {
                        m.Invoke(target, null);
                    }
                }
            }
        } else {
            MonoBehaviour cls = target as MonoBehaviour;
            Type t = cls.GetType();
            MethodInfo[] methods = t.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach(MethodInfo m in methods) {
                ParameterInfo[] pms = m.GetParameters();
                string name = m.Name.ToLower();
                if(pms.Length == 0 && name.StartsWith("test")) {
                    if(GUILayout.Button("<color=#05D4F1>" + m.Name + "</color>", _button)) {
                        m.Invoke(target, null);
                    }
                }
            }
        }
    }
}
