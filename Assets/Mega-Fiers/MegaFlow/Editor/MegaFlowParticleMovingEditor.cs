
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowParticleMoving))]
public class MegaFlowParticleMovingEditor : Editor
{
	SerializedProperty _prop_msource;
	SerializedProperty _prop_framenum;
	//SerializedProperty _prop_vel;
	SerializedProperty _prop_mass;
	SerializedProperty _prop_area;
	SerializedProperty _prop_particle;
	SerializedProperty _prop_dt;
	SerializedProperty _prop_scale;
	SerializedProperty _prop_maxparticles;
	SerializedProperty _prop_speed;
	SerializedProperty _prop_gravity;

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	SerializedProperty _prop_usethreading;
#endif

	private void OnEnable()
	{
		_prop_msource = serializedObject.FindProperty("msource");
		_prop_framenum = serializedObject.FindProperty("framenum");
		//_prop_vel = serializedObject.FindProperty("vel");
		_prop_mass = serializedObject.FindProperty("mass");
		_prop_area = serializedObject.FindProperty("area");
		_prop_particle = serializedObject.FindProperty("particle");
		_prop_dt = serializedObject.FindProperty("dt");
		_prop_scale = serializedObject.FindProperty("scale");
		_prop_maxparticles = serializedObject.FindProperty("maxparticles");
		_prop_speed = serializedObject.FindProperty("speed");
		_prop_gravity = serializedObject.FindProperty("gravity");

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
		_prop_usethreading = serializedObject.FindProperty("UseThreading");
#endif
	}

	public override void OnInspectorGUI()
	{
		MegaFlowParticleMoving mod = (MegaFlowParticleMoving)target;

		serializedObject.Update();

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(_prop_msource, new GUIContent("MSource"));

		if ( mod.msource && mod.msource.source && mod.msource.source.frames.Count > 1 )
		{
			EditorGUILayout.IntSlider(_prop_framenum, 0, mod.msource.source.frames.Count - 1);
			mod.SetFrame(mod.framenum);
		}

		EditorGUILayout.PropertyField(_prop_particle, new GUIContent("Particles"));
		EditorGUILayout.PropertyField(_prop_maxparticles, new GUIContent("Max Particles"));
		//EditorGUILayout.PropertyField(_prop_vel, new GUIContent("Vel"));
		EditorGUILayout.PropertyField(_prop_scale, new GUIContent("Scale"));
		EditorGUILayout.PropertyField(_prop_mass, new GUIContent("Mass"));
		EditorGUILayout.PropertyField(_prop_area, new GUIContent("Area"));
		EditorGUILayout.PropertyField(_prop_dt, new GUIContent("dt"));
		EditorGUILayout.PropertyField(_prop_speed, new GUIContent("Speed"));
		EditorGUILayout.PropertyField(_prop_gravity, new GUIContent("Gravity"));

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
		EditorGUILayout.PropertyField(_prop_usethreading, new GUIContent("Use Threading"));
#endif

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}

	void OnSceneGUI()
	{
	}
}
