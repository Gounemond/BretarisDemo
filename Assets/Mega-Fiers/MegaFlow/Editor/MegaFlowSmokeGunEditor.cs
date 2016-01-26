
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowSmokeGun))]
public class MegaFlowSmokeGunEditor : Editor
{
	SerializedProperty _prop_flowrate;
	SerializedProperty _prop_vel;
	SerializedProperty _prop_lifetime;
	SerializedProperty _prop_width;
	SerializedProperty _prop_height;
	SerializedProperty _prop_source;
	SerializedProperty _prop_mass;
	SerializedProperty _prop_area;
	SerializedProperty _prop_xspeed;
	SerializedProperty _prop_yspeed;
	SerializedProperty _prop_poolsize;
	SerializedProperty _prop_gravity;
	SerializedProperty _prop_count;
	SerializedProperty _prop_scale;
	SerializedProperty _prop_framenum;

	private void OnEnable()
	{
		_prop_source = serializedObject.FindProperty("source");
		_prop_framenum = serializedObject.FindProperty("framenum");
		_prop_flowrate = serializedObject.FindProperty("flowrate");
		_prop_vel = serializedObject.FindProperty("vel");
		_prop_lifetime = serializedObject.FindProperty("lifetime");
		_prop_width = serializedObject.FindProperty("width");
		_prop_height = serializedObject.FindProperty("height");
		_prop_mass = serializedObject.FindProperty("mass");
		_prop_area = serializedObject.FindProperty("area");
		_prop_xspeed = serializedObject.FindProperty("yspeed");
		_prop_yspeed = serializedObject.FindProperty("xspeed");
		_prop_poolsize = serializedObject.FindProperty("poolSize");
		_prop_gravity = serializedObject.FindProperty("Gravity");
		_prop_count = serializedObject.FindProperty("count");
		_prop_scale = serializedObject.FindProperty("scale");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		MegaFlowSmokeGun mod = (MegaFlowSmokeGun)target;

		EditorGUIUtility.LookLikeControls();

		EditorGUILayout.PropertyField(_prop_source, new GUIContent("Source"));
		if ( mod.source && mod.source.frames.Count > 1 )
		{
			EditorGUILayout.IntSlider(_prop_framenum, 0, mod.source.frames.Count - 1);
			//mod.SetFrame(mod.framenum);
		}

		EditorGUILayout.PropertyField(_prop_flowrate, new GUIContent("Flow Rate"));
		EditorGUILayout.PropertyField(_prop_count, new GUIContent("Count"));
		EditorGUILayout.PropertyField(_prop_vel, new GUIContent("Vel"));
		EditorGUILayout.PropertyField(_prop_scale, new GUIContent("Scale"));
		EditorGUILayout.PropertyField(_prop_mass, new GUIContent("Mass"));
		EditorGUILayout.PropertyField(_prop_area, new GUIContent("Area"));
		EditorGUILayout.PropertyField(_prop_gravity, new GUIContent("Gravity"));
		//EditorGUILayout.PropertyField(_prop_flowrate, new GUIContent("Flow Rate"));
		EditorGUILayout.PropertyField(_prop_lifetime, new GUIContent("Lifetime"));
		EditorGUILayout.PropertyField(_prop_width, new GUIContent("Width"));
		EditorGUILayout.PropertyField(_prop_height, new GUIContent("Height"));
		EditorGUILayout.PropertyField(_prop_poolsize, new GUIContent("Pool Size"));
		EditorGUILayout.PropertyField(_prop_xspeed, new GUIContent("X Speed"));
		EditorGUILayout.PropertyField(_prop_yspeed, new GUIContent("Y Speed"));

		if ( GUILayout.Button("Add Object") )
		{
			mod.emitobjects.Add(new MegaFlowSmokeObjDef());
		}

		for ( int i = 0; i < mod.emitobjects.Count; i++ )
		{
			EditorGUILayout.BeginVertical("Box");
			mod.emitobjects[i].obj = (MegaFlowEffect)EditorGUILayout.ObjectField("Object", mod.emitobjects[i].obj, typeof(MegaFlowEffect), true);
			mod.emitobjects[i].weight = EditorGUILayout.Slider("Weight", mod.emitobjects[i].weight, 0.0f, 1.0f);
			mod.emitobjects[i].scalelow = EditorGUILayout.Vector3Field("Scale Low", mod.emitobjects[i].scalelow);
			mod.emitobjects[i].scalehigh = EditorGUILayout.Vector3Field("Scale High", mod.emitobjects[i].scalehigh);
			mod.emitobjects[i].rotlow = EditorGUILayout.Vector3Field("Rot Low", mod.emitobjects[i].rotlow);
			mod.emitobjects[i].rothigh = EditorGUILayout.Vector3Field("Rot High", mod.emitobjects[i].rothigh);
			mod.emitobjects[i].rotspeedlow = EditorGUILayout.Vector3Field("Rot Speed Low", mod.emitobjects[i].rotspeedlow);
			mod.emitobjects[i].rotspeedhigh = EditorGUILayout.Vector3Field("Rot Speed High", mod.emitobjects[i].rotspeedhigh);

			if ( GUILayout.Button("Delete") )
				mod.emitobjects.RemoveAt(i);

			EditorGUILayout.EndVertical();
		}

		if ( GUILayout.Button("Add Color") )
		{
			mod.cols.Add(Color.white);
		}

		EditorGUILayout.LabelField("Colors");
		for ( int i = 0; i < mod.cols.Count; i++ )
		{
			EditorGUILayout.BeginHorizontal("box");
			mod.cols[i] = EditorGUILayout.ColorField("Col " + i, mod.cols[i]);

			if ( GUILayout.Button("-", GUILayout.MaxWidth(18)) )
			{
				mod.cols.RemoveAt(i);
			}
			EditorGUILayout.EndHorizontal();
		}

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}

	public void OnSceneGUI()
	{
		MegaFlowSmokeGun mod = (MegaFlowSmokeGun)target;

		Handles.matrix = mod.transform.localToWorldMatrix;

		Vector3 p = Vector3.zero;	//mod.transform.position;
		Vector3[]	verts = new Vector3[4];
		verts[0] = p;
		verts[1] = p;
		verts[2] = p;
		verts[3] = p;

		verts[0].z += mod.width;
		verts[0].y += mod.height;
		verts[1].z -= mod.width;
		verts[1].y += mod.height;
		verts[2].z -= mod.width;
		verts[2].y -= mod.height;
		verts[3].z += mod.width;
		verts[3].y -= mod.height;

		Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 1, 1, 0.25f), new Color(0, 0, 0, 1));

		Vector3 hp = (verts[0] + verts[1]) * 0.5f;
		Vector3 hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.height + mod.width) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);

		if ( hp1.y != hp.y )
			mod.height += hp1.y - hp.y;

		hp = (verts[1] + verts[2]) * 0.5f;
		hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.height + mod.width) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);

		if ( hp1.z != hp.z )
			mod.width -= hp1.z - hp.z;

		hp = (verts[2] + verts[3]) * 0.5f;
		hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.height + mod.width) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);

		if ( hp1.y != hp.y )
			mod.height -= hp1.y - hp.y;

		hp = (verts[3] + verts[0]) * 0.5f;
		hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.height + mod.width) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);

		if ( hp1.z != hp.z )
			mod.width += hp1.z - hp.z;
	}
}
