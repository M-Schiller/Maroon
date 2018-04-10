using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AssessmentWatchValue)), CanEditMultipleObjects]
public class TextAreaEditor : Editor
{

    public SerializedProperty longStringProp;
    void OnEnable()
    {
        longStringProp = serializedObject.FindProperty("Attributes");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        longStringProp.stringValue = EditorGUILayout.TextArea(longStringProp.stringValue, GUILayout.MaxHeight(75));
        serializedObject.ApplyModifiedProperties();
    }
}