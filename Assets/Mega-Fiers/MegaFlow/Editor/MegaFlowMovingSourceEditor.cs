
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowMovingSource))]
public class MegaFlowMovingSourceEditor : Editor
{
	SerializedProperty _prop_source;
	SerializedProperty _prop_framenum;
	SerializedProperty _prop_flowtime;
	SerializedProperty _prop_flowtimestep;
	SerializedProperty _prop_flowscale;
	SerializedProperty _prop_mindist;
	SerializedProperty _prop_target;
	SerializedProperty _prop_drawpath;
	SerializedProperty _prop_usefalloff;
	SerializedProperty _prop_falloffcrv;

	[MenuItem("GameObject/Create Other/MegaFlow/Moving Source")]
	static void CreateMovingSource()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("MegaFlow Moving Source");

		go.AddComponent<MegaFlowMovingSource>();

		go.transform.position = pos;
		Selection.activeObject = go;
	}

	private void OnEnable()
	{
		_prop_source = serializedObject.FindProperty("source");
		_prop_framenum = serializedObject.FindProperty("framenum");
		_prop_flowtime = serializedObject.FindProperty("flowtime");
		_prop_flowtimestep = serializedObject.FindProperty("flowtimestep");
		_prop_flowscale = serializedObject.FindProperty("flowscale");
		_prop_mindist = serializedObject.FindProperty("mindist");
		_prop_target = serializedObject.FindProperty("target");
		_prop_drawpath = serializedObject.FindProperty("drawpath");
		_prop_usefalloff = serializedObject.FindProperty("usefalloff");
		_prop_falloffcrv = serializedObject.FindProperty("falloffcrv");
	}

	public override void OnInspectorGUI()
	{
		MegaFlowMovingSource mod = (MegaFlowMovingSource)target;

		serializedObject.Update();

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(_prop_source, new GUIContent("Source"));

		if ( mod.source )
		{
			if ( mod.source.frames.Count > 1 )
				EditorGUILayout.IntSlider(_prop_framenum, 0, mod.source.frames.Count - 1);

			if ( GUILayout.Button("Align") )
			{
				mod.transform.position = mod.source.transform.position;
				mod.transform.rotation = mod.source.transform.rotation;
				mod.transform.localScale = mod.source.transform.lossyScale;

				if ( mod.target )
					mod.transform.parent = mod.target.transform;
			}
		}

		EditorGUILayout.PropertyField(_prop_flowscale, new GUIContent("Flow Scale"));
		EditorGUILayout.PropertyField(_prop_target, new GUIContent("Target"));

		EditorGUILayout.PropertyField(_prop_flowtime, new GUIContent("Flow Time"));
		EditorGUILayout.PropertyField(_prop_flowtimestep, new GUIContent("Flow dt"));
		EditorGUILayout.PropertyField(_prop_mindist, new GUIContent("Min Dist"));
		EditorGUILayout.PropertyField(_prop_drawpath, new GUIContent("Draw Path"));
		EditorGUILayout.PropertyField(_prop_usefalloff, new GUIContent("Use Falloff"));
		EditorGUILayout.PropertyField(_prop_falloffcrv, new GUIContent("Falloff Curve"));

#if false
		if ( GUILayout.Button("Add Frame") )
		{
			mod.frames.Add(new MegaFlowPosFrame());
		}

		for ( int i = 0; i < mod.frames.Count; i++ )
		{
			EditorGUILayout.BeginVertical("box");
			mod.frames[i].time = EditorGUILayout.Slider("Alpha", mod.frames[i].time, 0.0f, 1.0f);
			mod.frames[i].frame = EditorGUILayout.IntSlider("Frame", mod.frames[i].frame, 0, mod.source.frames.Count - 1);
			if ( GUILayout.Button("Delete") )
			{
				mod.frames.RemoveAt(i);
			}
			EditorGUILayout.EndVertical();
		}

		for ( int i = 0; i < mod.frames.Count; i++ )
		{
			if ( i == 0 )
				mod.frames[i].time = 0.0f;
			else
			{
				if ( mod.frames[i].time < mod.frames[i - 1].time )
					mod.frames[i].time = mod.frames[i - 1].time;
			}
		}
		if ( mod.frames.Count > 1 )
			mod.frames[mod.frames.Count - 1].time = 1.0f;
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
