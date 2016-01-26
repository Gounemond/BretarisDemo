
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowRBody))]
public class MegaFlowRBodyEditor : Editor
{
	SerializedProperty _prop_source;
	SerializedProperty _prop_framenum;
	SerializedProperty _prop_area;
	SerializedProperty _prop_scale;
	SerializedProperty _prop_reynolds;
	SerializedProperty _prop_density;

	private void OnEnable()
	{
		_prop_source = serializedObject.FindProperty("source");
		_prop_framenum = serializedObject.FindProperty("framenum");
		_prop_area = serializedObject.FindProperty("Area");
		_prop_reynolds = serializedObject.FindProperty("reynolds");
		_prop_density = serializedObject.FindProperty("density");
		_prop_scale = serializedObject.FindProperty("scale");
	}

	public override void OnInspectorGUI()
	{
		MegaFlowRBody mod = (MegaFlowRBody)target;

		serializedObject.Update();

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(_prop_source, new GUIContent("Source"));

		if ( mod.source && mod.source.frames.Count > 1 )
		{
			EditorGUILayout.IntSlider(_prop_framenum, 0, mod.source.frames.Count - 1);
			mod.SetFrame(mod.framenum);
		}

		EditorGUILayout.PropertyField(_prop_scale, new GUIContent("Scale"));
		EditorGUILayout.PropertyField(_prop_reynolds, new GUIContent("Reynolds"));
		EditorGUILayout.PropertyField(_prop_density, new GUIContent("Density"));
		EditorGUILayout.PropertyField(_prop_area, new GUIContent("Area"));

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}
}
