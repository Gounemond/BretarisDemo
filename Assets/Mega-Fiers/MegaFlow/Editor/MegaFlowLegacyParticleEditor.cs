
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowLegacyParticle))]
public class MegaFlowLegacyParticleEditor : Editor
{
	SerializedProperty _prop_source;
	SerializedProperty _prop_framenum;
	SerializedProperty _prop_framenum1;
	SerializedProperty _prop_interp;
	SerializedProperty _prop_framealpha;
	SerializedProperty _prop_vel;
	SerializedProperty _prop_mass;
	SerializedProperty _prop_area;
	SerializedProperty _prop_particle;
	SerializedProperty _prop_dt;
	SerializedProperty _prop_scale;
	SerializedProperty _prop_maxparticles;

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	SerializedProperty _prop_usethreading;
#endif

	private void OnEnable()
	{
		_prop_source = serializedObject.FindProperty("source");
		_prop_framenum = serializedObject.FindProperty("framenum");
		_prop_framenum1 = serializedObject.FindProperty("framenum1");
		_prop_interp = serializedObject.FindProperty("interp");
		_prop_framealpha = serializedObject.FindProperty("framealpha");
		_prop_vel = serializedObject.FindProperty("vel");
		_prop_mass = serializedObject.FindProperty("mass");
		_prop_area = serializedObject.FindProperty("area");
		_prop_particle = serializedObject.FindProperty("particle");
		_prop_dt = serializedObject.FindProperty("dt");
		_prop_scale = serializedObject.FindProperty("scale");
		_prop_maxparticles = serializedObject.FindProperty("maxparticles");

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
		_prop_usethreading = serializedObject.FindProperty("UseThreading");
#endif
	}

	public override void OnInspectorGUI()
	{
		MegaFlowLegacyParticle mod = (MegaFlowLegacyParticle)target;

		serializedObject.Update();

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(_prop_source, new GUIContent("Source"));

		if ( mod.source && mod.source.frames.Count > 1 )
		{
			EditorGUILayout.IntSlider(_prop_framenum, 0, mod.source.frames.Count - 1);
			mod.SetFrame(mod.framenum);
		}

		EditorGUILayout.PropertyField(_prop_interp, new GUIContent("Interpolate"));
		if ( mod.interp )
		{
			if ( mod.source && mod.source.frames.Count > 1 )
			{
				EditorGUILayout.IntSlider(_prop_framenum1, 0, mod.source.frames.Count - 1);
				mod.SetFrame1(mod.framenum1);
			}
			
			EditorGUILayout.Slider(_prop_framealpha, 0.0f, 1.0f);
		}

		EditorGUILayout.PropertyField(_prop_particle, new GUIContent("Particles"));
		EditorGUILayout.PropertyField(_prop_maxparticles, new GUIContent("Max Particles"));
		EditorGUILayout.PropertyField(_prop_vel, new GUIContent("Vel"));
		EditorGUILayout.PropertyField(_prop_scale, new GUIContent("Scale"));
		EditorGUILayout.PropertyField(_prop_mass, new GUIContent("Mass"));
		EditorGUILayout.PropertyField(_prop_area, new GUIContent("Area"));
		EditorGUILayout.PropertyField(_prop_dt, new GUIContent("dt"));

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
