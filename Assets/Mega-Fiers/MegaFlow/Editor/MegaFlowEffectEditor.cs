
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowEffect))]
public class MegaFlowEffectEditor : Editor
{
	SerializedProperty _prop_source;
	SerializedProperty _prop_framenum;
	SerializedProperty _prop_area;
	SerializedProperty _prop_dt;
	SerializedProperty _prop_scale;
	SerializedProperty _prop_reynolds;
	SerializedProperty _prop_density;
	SerializedProperty _prop_mass;
	SerializedProperty _prop_align;
	SerializedProperty _prop_alignrot;
	SerializedProperty _prop_usegradient;
	SerializedProperty _prop_gradient;
	SerializedProperty _prop_speedlow;
	SerializedProperty _prop_speedhigh;

	private void OnEnable()
	{
		_prop_source = serializedObject.FindProperty("source");
		_prop_framenum = serializedObject.FindProperty("framenum");
		_prop_mass = serializedObject.FindProperty("mass");
		_prop_area = serializedObject.FindProperty("Area");
		_prop_reynolds = serializedObject.FindProperty("reynolds");
		_prop_density = serializedObject.FindProperty("density");
		_prop_dt = serializedObject.FindProperty("dt");
		_prop_scale = serializedObject.FindProperty("scale");

		_prop_align = serializedObject.FindProperty("align");
		_prop_alignrot = serializedObject.FindProperty("alignrot");
		_prop_usegradient = serializedObject.FindProperty("usegradient");
		_prop_gradient = serializedObject.FindProperty("gradient");
		_prop_speedlow = serializedObject.FindProperty("speedlow");
		_prop_speedhigh = serializedObject.FindProperty("speedhigh");
	}

	public override void OnInspectorGUI()
	{
		MegaFlowEffect mod = (MegaFlowEffect)target;

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
		EditorGUILayout.PropertyField(_prop_mass, new GUIContent("Mass"));
		EditorGUILayout.PropertyField(_prop_area, new GUIContent("Area"));
		EditorGUILayout.PropertyField(_prop_dt, new GUIContent("dt"));

		EditorGUILayout.PropertyField(_prop_align, new GUIContent("Align"));
		EditorGUILayout.PropertyField(_prop_alignrot, new GUIContent("Aligh Rot"));
		EditorGUILayout.PropertyField(_prop_usegradient, new GUIContent("Use Gradient"));
		EditorGUILayout.PropertyField(_prop_gradient, new GUIContent("Gradient"));
		EditorGUILayout.PropertyField(_prop_speedlow, new GUIContent("Speed Low"));
		EditorGUILayout.PropertyField(_prop_speedhigh, new GUIContent("Speed High"));

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}
}
