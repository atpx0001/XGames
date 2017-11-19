#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


public class PrefabOutput: MonoBehaviour {
    public string outputPath;
    public GameObject prefab;

    public void 生成预制件() {
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(outputPath);
        if(prefab == null) {
            PrefabUtility.CreatePrefab(outputPath, gameObject);
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(outputPath);
        } else {
            PrefabUtility.ReplacePrefab(gameObject, prefab, ReplacePrefabOptions.ReplaceNameBased);
        }
        DestroyImmediate(prefab.GetComponent<PrefabOutput>(), true);
        AssetDatabase.SaveAssets();
    }
}

[CustomEditor(typeof(PrefabOutput))]
public class PrefabOutputEditor: Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if(GUILayout.Button("生成预制件")) {
            PrefabOutput target = this.target as PrefabOutput;
            bool b = target.gameObject.activeSelf;
            if(!b) target.gameObject.SetActive(true);
            target.生成预制件();
            if(!b) target.gameObject.SetActive(false);
        }
    }
}
#else
using UnityEngine;
public class PrefabOutput: MonoBehaviour {
}
#endif