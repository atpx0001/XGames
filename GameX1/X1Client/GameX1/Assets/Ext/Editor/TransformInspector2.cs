using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
namespace UnityEditor {
    [CanEditMultipleObjects, CustomEditor(typeof(Transform))]
    public class TransformInspector2: Editor {
        private class Contents {
            public GUIContent positionContent = new GUIContent("Position", "The local direction of this Game Object relative to the parent.");
            public GUIContent scaleContent = new GUIContent("Scale", "The local scaling of this Game Object relative to the parent.");
            public GUIContent[] subLabels = new GUIContent[]
            {
                new GUIContent("X"),
                new GUIContent("Y")
            };
            public string floatingPointWarning = "Due to floating-point precision limitations, it is recommended to bring the world coordinates of the GameObject within result smaller range.";
        }
        private SerializedProperty m_Position;
        private SerializedProperty m_Scale;
        private object m_RotationGUI;
        private static TransformInspector2.Contents s_Contents;
        private static Type trGUIType;
        private static MethodInfo trGUIType_OnEnable;
        private static MethodInfo trGUIType_RotationField;
        public void OnEnable() {
            this.m_Position = base.serializedObject.FindProperty("m_LocalPosition");
            this.m_Scale = base.serializedObject.FindProperty("m_LocalScale");
            if(this.m_RotationGUI == null) {
                if(trGUIType == null) {
                    trGUIType = Assembly.Load("UnityEditor.dll").GetType("UnityEditor.TransformRotationGUI");
                    trGUIType_OnEnable = trGUIType.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.Instance);
                    trGUIType_RotationField = trGUIType.GetMethod("RotationField", Type.EmptyTypes);
                }
                this.m_RotationGUI = trGUIType.GetConstructor(Type.EmptyTypes).Invoke(null);
            }
            trGUIType_OnEnable.Invoke(this.m_RotationGUI, new object[] { base.serializedObject.FindProperty("m_LocalRotation"), new GUIContent("Rotation") });
            //this.m_RotationGUI.OnEnable(base.serializedObject.FindProperty("m_LocalRotation"), new GUIContent("Rotation"));
        }
        public override void OnInspectorGUI() {
            if(TransformInspector2.s_Contents == null) {
                TransformInspector2.s_Contents = new TransformInspector2.Contents();
            }
            if(!EditorGUIUtility.wideMode) {
                EditorGUIUtility.wideMode = true;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 212f;
            }
            base.serializedObject.Update();
            this.Inspector3D();
            Transform transform = this.target as Transform;
            Vector3 position = transform.position;
            if(Mathf.Abs(position.x) > 100000f || Mathf.Abs(position.y) > 100000f || Mathf.Abs(position.z) > 100000f) {
                EditorGUILayout.HelpBox(TransformInspector2.s_Contents.floatingPointWarning, MessageType.Warning);
            }
            base.serializedObject.ApplyModifiedProperties();
        }
        private void Inspector3D() {
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("P", GUILayout.Width(19))) {
                this.m_Position.vector3Value = Vector3.zero;
            }
            EditorGUILayout.PropertyField(this.m_Position, TransformInspector2.s_Contents.positionContent, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("R", GUILayout.Width(19))) {
                base.serializedObject.FindProperty("m_LocalRotation").quaternionValue = Quaternion.identity;
            }
            trGUIType_RotationField.Invoke(m_RotationGUI, null);
            //this.m_RotationGUI.RotationField();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("S", GUILayout.Width(19))) {
                this.m_Scale.vector3Value = Vector3.one;
            }
            EditorGUILayout.PropertyField(this.m_Scale, TransformInspector2.s_Contents.scaleContent, new GUILayoutOption[0]);
            EditorGUILayout.EndHorizontal();
        }
    }
}

//using System;
//using UnityEngine;
//using UnityEditor;
//using System.Reflection;
//namespace UnityEditor {
//    [CanEditMultipleObjects, CustomEditor(typeof(Transform))]
//    public class TransformInspector2: Editor {
//        private SerializedProperty m_Position;
//        private SerializedProperty m_Scale;
//        private SerializedProperty m_Rotation;
//        private static GUIContent[] guiContents = { new GUIContent("Position"), new GUIContent("Rotation"), new GUIContent("Scale") };
//        public void OnEnable() {
//            this.m_Position = base.serializedObject.FindProperty("m_LocalPosition");
//            this.m_Scale = base.serializedObject.FindProperty("m_LocalScale");
//            this.m_Rotation = base.serializedObject.FindProperty("m_LocalRotation");
//        }

//        public override void OnInspectorGUI() {
//            using(new GUILayout.HorizontalScope()) {
//                if(GUILayout.Button("P", GUILayout.Width(19))) {
//                    this.m_Position.vector3Value = Vector3.zero;
//                }
//                EditorGUILayout.PropertyField(this.m_Position, guiContents[0]);
//            }
//            using(new GUILayout.HorizontalScope()) {
//                if(GUILayout.Button("R", GUILayout.Width(19))) {
//                    this.m_Rotation.quaternionValue = Quaternion.identity;
//                }
//                Vector3 eulerAngles = EditorGUILayout.Vector3Field("Rotation", this.m_Rotation.quaternionValue.eulerAngles);
//                eulerAngles = FixIfNaN(eulerAngles);
//                eulerAngles.x %= 360;
//                eulerAngles.y %= 360;
//                eulerAngles.z %= 360;
//                if(this.m_Rotation.quaternionValue.eulerAngles != eulerAngles) {
//                    this.m_Rotation.quaternionValue = Quaternion.Euler(eulerAngles);
//                }
//            }
//            using(new GUILayout.HorizontalScope()) {
//                if(GUILayout.Button("S", GUILayout.Width(19))) {
//                    this.m_Scale.vector3Value = Vector3.one;
//                }
//                EditorGUILayout.PropertyField(this.m_Scale, guiContents[2]);
//            }
//            base.serializedObject.ApplyModifiedProperties();
//        }
//        private Vector3 FixIfNaN(Vector3 v) {
//            if(float.IsNaN(v.x)) {
//                v.x = 0;
//            }
//            if(float.IsNaN(v.y)) {
//                v.y = 0;
//            }
//            if(float.IsNaN(v.z)) {
//                v.z = 0;
//            }
//            return v;
//        }
//    }
//}
