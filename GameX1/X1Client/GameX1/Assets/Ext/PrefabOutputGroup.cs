#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class PrefabOutputGroup: MonoBehaviour {
}

[CustomEditor(typeof(PrefabOutputGroup))]
public class PrefabOutputGroupEditor: Editor {
    Vector2 vp;
    HashSet<PrefabOutput> checkedItems = new HashSet<PrefabOutput>();
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        PrefabOutputGroup target = this.target as PrefabOutputGroup;
        PrefabOutput[] pos = target.GetComponentsInChildren<PrefabOutput>(true);

        using(new EditorGUILayout.VerticalScope()) {
            using(new EditorGUILayout.HorizontalScope()) {
                if(GUILayout.Button("全选")) {
                    checkedItems.Clear();
                    checkedItems.UnionWith(pos);
                }
                if(GUILayout.Button("全不选")) {
                    checkedItems.Clear();
                }
                if(GUILayout.Button("批量生成")) {
                    foreach(PrefabOutput po in pos) {
                        if(po != null && checkedItems.Contains(po)) {
                            bool b = po.gameObject.activeSelf;
                            if(!b) po.gameObject.SetActive(true);
                            po.生成预制件();
                            if(!b) po.gameObject.SetActive(false);
                        }
                    }
                }
            }
            using(EditorGUILayout.ScrollViewScope sv = new EditorGUILayout.ScrollViewScope(vp, false, false)) {
                vp = sv.scrollPosition;
                using(new EditorGUILayout.VerticalScope()) {
                    foreach(PrefabOutput po in pos) {
                        using(new EditorGUILayout.HorizontalScope()) {
                            if(EditorGUILayout.Toggle(checkedItems.Contains(po), GUILayout.Width(32))) {
                                checkedItems.Add(po);
                            } else {
                                checkedItems.Remove(po);
                            }
                            EditorGUILayout.LabelField(po.name);
                            if(GUILayout.Button("生成", GUILayout.Width(64))) {
                                po.gameObject.SetActive(true);
                                po.生成预制件();
                                po.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif