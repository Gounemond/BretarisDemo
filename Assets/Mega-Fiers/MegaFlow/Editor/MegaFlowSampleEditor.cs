
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowSample))]
public class MegaFlowSampleEditor : Editor
{
	SerializedProperty _prop_source;
	SerializedProperty _prop_framenum;

	[MenuItem("GameObject/Create Other/MegaFlow/Sample")]
	static void CreateSample()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("MegaFlow Sample");

		go.AddComponent<MegaFlowSample>();

		go.transform.position = pos;
		Selection.activeObject = go;
	}

	private void OnEnable()
	{
		_prop_source = serializedObject.FindProperty("source");
		_prop_framenum = serializedObject.FindProperty("framenum");
	}

	public override void OnInspectorGUI()
	{
		MegaFlowSample mod = (MegaFlowSample)target;

		serializedObject.Update();

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(_prop_source, new GUIContent("Source"));

		if ( mod.source && mod.source.frames.Count > 1 )
			EditorGUILayout.IntSlider(_prop_framenum, 0, mod.source.frames.Count - 1);

		EditorGUILayout.TextArea("Velocity " + mod.velocity.ToString("0.00"));

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}
}
