
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlowCreateFromSplines))]
public class MegaFlowCreateFromSplinesEditor : Editor
{
	[MenuItem("GameObject/Create Other/MegaFlow/Create Flow")]
	static void CreateMegaFlow()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("Create MegaFlow Source");

		go.AddComponent<MegaFlowCreateFromSplines>();

		go.transform.position = pos;
		Selection.activeObject = go;
	}

	SerializedProperty	_prop_velcols;
	SerializedProperty	_prop_startval;

	private void OnEnable()
	{
		_prop_velcols = serializedObject.FindProperty("velcols");
		_prop_startval = serializedObject.FindProperty("startval");
	}

	void CreatePreview(MegaFlowCreateFromSplines mod)
	{
		if ( !mod.preview )
		{
			GameObject obj = new GameObject();

			obj.transform.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			obj.transform.parent = mod.transform;
			obj.name = "Flow Preview";
			mod.preview = obj.AddComponent<MegaFlow>();
			mod.preview.showVel = true;
			mod.preview.showgrid = true;
			mod.preview.showcells = true;
		}

		mod.preview.transform.localPosition = Vector3.zero;
		mod.preview.transform.localRotation = Quaternion.identity;

		mod.preview.DestroyFrames();
		CreateFlow(mod, mod.preview);
	}

	public void ClearPreview(MegaFlowCreateFromSplines mod)
	{
		if ( mod.preview )
			DestroyImmediate(mod.preview.gameObject);
	}

	public override void OnInspectorGUI()
	{
		MegaFlowCreateFromSplines mod = (MegaFlowCreateFromSplines)target;

		serializedObject.Update();

		if ( GUILayout.Button("Create Flow") )
			CreateFlow(mod, mod.flow);

		EditorGUIUtility.LookLikeControls();

		if ( GUILayout.Button("Preview Flow") )
			CreatePreview(mod);

		if ( mod.preview )
		{
			if ( GUILayout.Button("Clear Preview") )
				ClearPreview(mod);
		}

		mod.size = EditorGUILayout.Vector3Field("Size", mod.size);
		mod.square = EditorGUILayout.Toggle("Square Cells", mod.square);

		mod.gridx = EditorGUILayout.IntField("Grid X", mod.gridx);
		mod.gridy = EditorGUILayout.IntField("Grid Y", mod.gridy);
		mod.gridz = EditorGUILayout.IntField("Grid Z", mod.gridz);

		EditorGUILayout.PropertyField(_prop_startval, new GUIContent("Start Value"));

		mod.velocity = EditorGUILayout.FloatField("Velocity", mod.velocity);
		mod.vellen = EditorGUILayout.FloatField("Vel Len", mod.vellen);
		mod.flow = (MegaFlow)EditorGUILayout.ObjectField("Flow", mod.flow, typeof(MegaFlow), true);
		mod.emtyspacemode = (MegaFlowMode)EditorGUILayout.EnumPopup("Empty Space", mod.emtyspacemode);
		mod.shownotsel = EditorGUILayout.Toggle("Gizmo Always Show", mod.shownotsel);

		//mod.showvelmag = EditorGUILayout.BeginToggleGroup("Show Vel Mag", mod.showvelmag);
		EditorGUILayout.PropertyField(_prop_velcols, new GUIContent("Colors"));
		mod.minvel = EditorGUILayout.FloatField("Min Vel", mod.minvel);
		mod.maxvel = EditorGUILayout.FloatField("Max Vel", mod.maxvel);
		//EditorGUILayout.EndToggleGroup();

		mod.texture = (Texture3D)EditorGUILayout.ObjectField("Texture3D", mod.texture, typeof(Texture3D), true);
		mod.texturescale = EditorGUILayout.FloatField("Texture Scale", mod.texturescale);

		if ( mod.preview )
		{
			//bool changed = GUI.changed;

			MegaFlow modp = mod.preview;
			modp.showdisplayparams = EditorGUILayout.Foldout(modp.showdisplayparams, "Data Display Params");

			if ( modp.showdisplayparams )
			{
				EditorGUILayout.BeginVertical("box");
				modp.Scale = EditorGUILayout.FloatField("Scale Frc", modp.Scale);

				modp.Plane = (MegaFlowAxis)EditorGUILayout.EnumPopup("Plane", modp.Plane);

				if ( modp.flow != null )
				{
					switch ( modp.Plane )
					{
						case MegaFlowAxis.X:
							{
								int pos = EditorGUILayout.IntSlider("Position", modp.Position, 0, modp.flow.gridDim2[0] - 1);
								if ( pos != modp.Position )
								{
									modp.Position = pos;
									modp.handlepos.x = pos * modp.flow.spacing.x;
								}
								modp.Thickness = EditorGUILayout.IntSlider("Thickness", modp.Thickness, 0, modp.flow.gridDim2[0]);
							}
							break;

						case MegaFlowAxis.Y:
							{
								int pos = EditorGUILayout.IntSlider("Position", modp.Position, 0, modp.flow.gridDim2[1] - 1);
								if ( pos != modp.Position )
								{
									modp.Position = pos;
									modp.handlepos.y = pos * modp.flow.spacing.y;
								}
								modp.Thickness = EditorGUILayout.IntSlider("Thickness", modp.Thickness, 0, modp.flow.gridDim2[1]);
							}
							break;

						case MegaFlowAxis.Z:
							{
								int pos = EditorGUILayout.IntSlider("Position", modp.Position, 0, modp.flow.gridDim2[2] - 1);
								if ( pos != modp.Position )
								{
									modp.Position = pos;
									modp.handlepos.z = pos * modp.flow.spacing.z;
								}
								modp.Thickness = EditorGUILayout.IntSlider("Thickness", modp.Thickness, 0, modp.flow.gridDim2[2]);
							}
							break;
					}
				}

				modp.velScl = EditorGUILayout.FloatField("Vel Visual Magnitude", modp.velScl);
				if ( modp.velScl < 0.01f )
					modp.velScl = 0.01f;

				modp.showvelmag = EditorGUILayout.BeginToggleGroup("Show Vel Mag", modp.showvelmag);
				EditorGUILayout.PropertyField(_prop_velcols, new GUIContent("Colors"));
				modp.velcols = mod.velcols;
				//modp.velcols.alphaKeys = mod.velcols.alphaKeys;
				//modp.velcols.colorKeys = mod.velcols.colorKeys;
				modp.minvel = EditorGUILayout.FloatField("Min Vel", modp.minvel);
				modp.maxvel = EditorGUILayout.FloatField("Max Vel", modp.maxvel);
				EditorGUILayout.EndToggleGroup();

				EditorGUILayout.BeginVertical("Box");
				if ( modp.flow && (modp.flow.vel.Count > 0 || modp.flow.optvel.Count > 0) )
				{
					modp.showVel = EditorGUILayout.BeginToggleGroup("Show Vel", modp.showVel);
					modp.velLen = EditorGUILayout.FloatField("Vel Vector Len", modp.velLen);
					if ( modp.velLen < 0.01f )
						modp.velLen = 0.01f;
					modp.skip = EditorGUILayout.IntField("Vel Skip", modp.skip);
					modp.velThreshold = EditorGUILayout.Slider("Vel Threshold", modp.velThreshold, 0.0f, 1.0f);
					modp.velAlpha = EditorGUILayout.Slider("Vel Alpha", modp.velAlpha, 0.0f, 1.0f);
					EditorGUILayout.EndToggleGroup();
				}
				EditorGUILayout.EndVertical();

				modp.showgrid = EditorGUILayout.Toggle("Show Grid", modp.showgrid);
				modp.gridColor = EditorGUILayout.ColorField("Grid Color", modp.gridColor);
				modp.gridColor1 = EditorGUILayout.ColorField("Grid Color1", modp.gridColor1);

				modp.showcells = EditorGUILayout.BeginToggleGroup("Show Cells", modp.showcells);
				modp.showcellmag = EditorGUILayout.Toggle("Show Cell Mag", modp.showcellmag);
				modp.cellalpha = EditorGUILayout.Slider("Cell Alpha", modp.cellalpha, 0.0f, 1.0f);
				EditorGUILayout.EndToggleGroup();

				EditorGUILayout.EndVertical();

				//if ( !changed && GUI.changed )
				//{
					//EditorUtility.SetDirty(modp);
				//}
			}
		}

		mod.showsplines = EditorGUILayout.Foldout(mod.showsplines, "Splines");

		if ( mod.showsplines )
		{
			if ( GUILayout.Button("Add Spline") )
			{
				MegaFlowSpline spl = new MegaFlowSpline();
				mod.splines.Add(spl);
			}
			//mod.splinepos = EditorGUILayout.BeginScrollView(mod.splinepos, GUILayout.Height(200));

			for ( int i = 0; i < mod.splines.Count; i++ )
			{
				MegaFlowSpline spl = mod.splines[i];

				string name = "Empty";
				if ( spl.shape )
					name = spl.shape.name;

				EditorGUILayout.BeginVertical("box");
				spl.show = EditorGUILayout.Foldout(spl.show, name);

				if ( spl.show )
				{
					spl.include = EditorGUILayout.BeginToggleGroup("Include", spl.include);
					spl.shape = (MegaShape)EditorGUILayout.ObjectField("Shape", spl.shape, typeof(MegaShape), true);
					spl.weight = EditorGUILayout.FloatField("Weight", spl.weight);
					spl.velocity = EditorGUILayout.FloatField("Velocity", spl.velocity);
					spl.falloffdist = EditorGUILayout.FloatField("Falloff Dist", spl.falloffdist);
					spl.falloffcrv = EditorGUILayout.CurveField("Falloff Crv", spl.falloffcrv);
					spl.velcrv = EditorGUILayout.CurveField("Vel Crv", spl.velcrv);
					spl.distcrv = EditorGUILayout.CurveField("Dist Crv", spl.distcrv);
					spl.mode = (MegaFlowMode)EditorGUILayout.EnumPopup("Mode", spl.mode);
					spl.visrings = EditorGUILayout.IntSlider("Rings", spl.visrings, 1, 50);
					spl.ringalpha = EditorGUILayout.Slider("Ring Alpha", spl.ringalpha, 0.0f, 1.0f);
					EditorGUILayout.EndToggleGroup();

					if ( GUILayout.Button("Delete") )
						mod.splines.RemoveAt(i);

				}
				EditorGUILayout.EndVertical();
			}
			//EditorGUILayout.EndScrollView();
		}

		mod.showmods = EditorGUILayout.Foldout(mod.showmods, "Modifiers");

		if ( mod.showmods )
		{
			if ( GUILayout.Button("Add Modifier") )
			{
				MegaFlowModifier fmod = new MegaFlowModifier();
				mod.modifiers.Add(fmod);
			}

			//mod.modpos = EditorGUILayout.BeginScrollView(mod.modpos, GUILayout.Height(150));

			for ( int i = 0; i < mod.modifiers.Count; i++ )
			{
				MegaFlowModifier fmod = mod.modifiers[i];

				string name = "Empty";
				if ( fmod.obj )
					name = fmod.obj.name;

				EditorGUILayout.BeginVertical("box");
				fmod.show = EditorGUILayout.Foldout(fmod.show, name);

				if ( fmod.show )
				{
					fmod.include = EditorGUILayout.BeginToggleGroup("Include", fmod.include);
					fmod.type = (MegaFlowModType)EditorGUILayout.EnumPopup("Type", fmod.type);
					fmod.obj = (Collider)EditorGUILayout.ObjectField("Object", fmod.obj, typeof(Collider), true);
					fmod.amount = EditorGUILayout.FloatField("Amount", fmod.amount);
					EditorGUILayout.EndToggleGroup();

					if ( GUILayout.Button("Delete") )
						mod.modifiers.RemoveAt(i);
				}

				EditorGUILayout.EndVertical();
			}
			//EditorGUILayout.EndScrollView();
		}

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			EditorUtility.SetDirty(target);
		}
	}

#if false
	static public Color GetCol(float alpha)
	{
#if false
		int elements = Cols.Length - 1;

		alpha = Mathf.Abs(alpha);
		if ( alpha == 0.0f )
			return Cols[0];

		if ( alpha > 0.99999f )
			return Cols[elements];

		int	index = (int)(alpha * elements);
		float a = alpha * (float)elements;

		a = a - index;

		return Cols[index] + ((Cols[index + 1] - Cols[index]) * a);
#endif
		return velcols.Evaluate(alpha);
	}
#endif

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3
	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
#else
	[DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy)]
#endif
	static void RenderGizmo(MegaFlowCreateFromSplines flow, GizmoType gizmoType)
	{
		if ( !flow.shownotsel && Selection.activeGameObject != flow.gameObject )
			return;

		flow.DrawGizmo();

		float rng = 1.0f / (flow.maxvel - flow.minvel);

		for ( int i = 0; i < flow.splines.Count; i++ )
		{
			if ( flow.splines[i].include )
			{
				MegaShape shape = flow.splines[i].shape;

				if ( shape )
				{
					Handles.matrix = shape.transform.localToWorldMatrix;

					Color col = Color.green;

					for ( int s = 0; s < shape.splines.Count; s++ )
					{
						float step = 1.0f / (float)flow.splines[i].visrings;
						for ( float j = 0.0f; j < 1.0f; j += step )
						{
							Vector3 p = shape.InterpCurve3D(s, j, true);
							Vector3 pt = shape.InterpCurve3D(s, j + 0.01f, true);

							float dist = flow.splines[i].distcrv.Evaluate(j) * flow.splines[i].falloffdist;

							float va = flow.splines[i].velcrv.Evaluate(j);
							col = flow.GetCol(va * flow.splines[i].velocity * rng);	//flow.splines[i].velcrv.Evaluate(j));
							col.a = flow.splines[i].ringalpha;
							//float va = flow.splines[i].velcrv.Evaluate(j);
							Handles.color = col;

							Vector3 dir = (pt - p).normalized;
							Handles.DrawWireDisc(p, dir, dist);

							Quaternion rot = Quaternion.LookRotation(dir);
							Handles.ArrowCap(0, p, rot, flow.splines[i].velocity * flow.vellen * va);
						}
					}
				}
			}
		}

		Handles.matrix = Matrix4x4.identity;
	}

	// TODO: Blocking geom
	public void OnSceneGUI()
	{
		MegaFlowCreateFromSplines mod = (MegaFlowCreateFromSplines)target;

		Handles.matrix = mod.transform.localToWorldMatrix;

		Vector3 p = Vector3.zero;
		Vector3[]	verts = new Vector3[6];
		verts[0] = p;
		verts[1] = p;
		verts[2] = p;
		verts[3] = p;
		verts[4] = p;
		verts[5] = p;

		verts[0].x -= mod.size.x * 0.5f;
		verts[1].x += mod.size.x * 0.5f;
		verts[2].y -= mod.size.y * 0.5f;
		verts[3].y += mod.size.y * 0.5f;
		verts[4].z -= mod.size.z * 0.5f;
		verts[5].z += mod.size.z * 0.5f;

		float sz = mod.size.magnitude * 0.0025f;

		Vector3 hp1 = Handles.FreeMoveHandle(verts[0], Quaternion.identity, sz, Vector3.zero, Handles.DotCap);

		if ( hp1.x != verts[0].x )
			mod.size.x -= hp1.x - verts[0].x;

		hp1 = Handles.FreeMoveHandle(verts[1], Quaternion.identity, sz, Vector3.zero, Handles.DotCap);

		if ( hp1.x != verts[1].x )
			mod.size.x += hp1.x - verts[1].x;

		hp1 = Handles.FreeMoveHandle(verts[2], Quaternion.identity, sz, Vector3.zero, Handles.DotCap);

		if ( hp1.y != verts[2].y )
			mod.size.y -= hp1.y - verts[2].y;

		hp1 = Handles.FreeMoveHandle(verts[3], Quaternion.identity, sz, Vector3.zero, Handles.DotCap);

		if ( hp1.y != verts[3].y )
			mod.size.y += hp1.y - verts[3].y;

		hp1 = Handles.FreeMoveHandle(verts[4], Quaternion.identity, sz, Vector3.zero, Handles.DotCap);

		if ( hp1.z != verts[4].z )
			mod.size.z -= hp1.z - verts[4].z;

		hp1 = Handles.FreeMoveHandle(verts[5], Quaternion.identity, sz, Vector3.zero, Handles.DotCap);

		if ( hp1.z != verts[5].z )
			mod.size.z += hp1.z - verts[5].z;

		if ( mod.size.x < 0.0f )
			mod.size.x = 0.0f;

		if ( mod.size.y < 0.0f )
			mod.size.y = 0.0f;

		if ( mod.size.z < 0.0f )
			mod.size.z = 0.0f;

		if ( mod.preview )
		{
			Matrix4x4 offtm = Matrix4x4.TRS(-mod.preview.flow.size * 0.5f, Quaternion.identity, Vector3.one);

			Handles.matrix = mod.transform.localToWorldMatrix * offtm;

			Handles.color = Color.green;
			switch ( mod.preview.Plane )
			{
				case MegaFlowAxis.X:
					mod.preview.handlepos = Handles.FreeMoveHandle(mod.preview.handlepos, Quaternion.identity, mod.preview.flow.spacing.x * 1.0f, Vector3.zero, Handles.SphereCap);
					mod.preview.Position = Mathf.Clamp((int)(mod.preview.handlepos.x / mod.preview.flow.spacing.x), 0, mod.preview.flow.gridDim2[0] - 1);

					mod.preview.handlepos.y = mod.preview.flow.size.y * 0.5f;
					mod.preview.handlepos.z = mod.preview.flow.size.z * 0.5f;
					break;

				case MegaFlowAxis.Y:
					mod.preview.handlepos = Handles.FreeMoveHandle(mod.preview.handlepos, Quaternion.identity, mod.preview.flow.spacing.y * 1.0f, Vector3.zero, Handles.SphereCap);
					mod.preview.Position = Mathf.Clamp((int)(mod.preview.handlepos.y / mod.preview.flow.spacing.y), 0, mod.preview.flow.gridDim2[1] - 1);
					mod.preview.handlepos.x = mod.preview.flow.size.x * 0.5f;
					mod.preview.handlepos.z = mod.preview.flow.size.z * 0.5f;
					break;

				case MegaFlowAxis.Z:
					mod.preview.handlepos = Handles.FreeMoveHandle(mod.preview.handlepos, Quaternion.identity, mod.preview.flow.spacing.z * 1.0f, Vector3.zero, Handles.SphereCap);
					mod.preview.Position = Mathf.Clamp((int)(mod.preview.handlepos.z / mod.preview.flow.spacing.z), 0, mod.preview.flow.gridDim2[2] - 0);
					mod.preview.handlepos.x = mod.preview.flow.size.x * 0.5f;
					mod.preview.handlepos.y = mod.preview.flow.size.y * 0.5f;
					break;
			}

			mod.preview.handlepos.x = Mathf.Clamp(mod.preview.handlepos.x, 0.0f, mod.preview.flow.size.x);
			mod.preview.handlepos.y = Mathf.Clamp(mod.preview.handlepos.y, 0.0f, mod.preview.flow.size.y);
			mod.preview.handlepos.z = Mathf.Clamp(mod.preview.handlepos.z, 0.0f, mod.preview.flow.size.z);
		}

		Handles.matrix = Matrix4x4.identity;
	}

	public void CreateFlow(MegaFlowCreateFromSplines mod, MegaFlow flow)
	{
		int gx = mod.gridx;
		int gy = mod.gridy;
		int gz = mod.gridz;

		if ( mod.square )
		{
			int axis = 0;
			if ( Mathf.Abs(mod.size.x) > Mathf.Abs(mod.size.y) )
			{
				if ( Mathf.Abs(mod.size.x) > Mathf.Abs(mod.size.z) )
					axis = 0;
				else
					axis = 2;
			}
			else
			{
				if ( Mathf.Abs(mod.size.y) > Mathf.Abs(mod.size.z) )
					axis = 1;
				else
					axis = 2;
			}

			float csize = mod.size[axis] / mod.gridx;

			mod.cellsize = new Vector3(csize, csize, csize);
		}
		else
		{
			mod.cellsize.x = mod.size.x / mod.gridx;
			mod.cellsize.y = mod.size.y / mod.gridy;
			mod.cellsize.z = mod.size.z / mod.gridz;
		}

		gx = (int)(mod.size.x / mod.cellsize.x);
		gy = (int)(mod.size.y / mod.cellsize.y);
		gz = (int)(mod.size.z / mod.cellsize.z);

		Vector3[] cells = new Vector3[gx * gy * gz];

		for ( int i = 0; i < cells.Length; i++ )
			cells[i] = mod.startval;

		Vector3 pos = Vector3.zero;

		Vector3 half = mod.cellsize * 0.5f;
		Vector3 tan = Vector3.zero;
		int kn = 0;
		float alpha = 0.0f;

		List<MegaFlowContrib>	contrib = new List<MegaFlowContrib>();

		Matrix4x4 offtm = Matrix4x4.TRS(-mod.size * 0.5f, Quaternion.identity, Vector3.one);

		Matrix4x4 tm = mod.transform.localToWorldMatrix * offtm;

		if ( mod.texture )
		{
			Vector3 p;
			Color[] cols = mod.texture.GetPixels();

			for ( int z = 0; z < gz; z++ )
			{
				int tz = (int)(((float)z / (float)gz) * mod.texture.depth);	// * f.size.z;

				for ( int y = 0; y < gy; y++ )
				{
					int ty = (int)(((float)y / (float)gy) * mod.texture.height);	// * f.size.y;

					for ( int x = 0; x < gx; x++ )
					{
						int tx = (int)(((float)x / (float)gx) * mod.texture.width);	// * f.size.x;

						Color cvel = cols[(tz * mod.texture.width * mod.texture.height) + (ty * mod.texture.width) + tx];

						p.x = (cvel.r - 0.5f) * 2.0f;
						p.y = (cvel.g - 0.5f) * 2.0f;
						p.z = (cvel.b - 0.5f) * 2.0f;
						cells[(x * gz * gy) + (z * gy) + y] = p * mod.texturescale;
					}
				}
			}
		}

		for ( int z = 0; z < gz; z++ )
		{
			pos.z = (z * mod.cellsize.z) + half.z;

			EditorUtility.DisplayProgressBar("Building Vector Field", "Building", (float)z / (float)gz);

			for ( int y = 0; y < gy; y++ )
			{
				pos.y = (y * mod.cellsize.y) + half.y;

				for ( int x = 0; x < gx; x++ )
				{
					pos.x = (x * mod.cellsize.x) + half.x;

					contrib.Clear();
					float nearest = float.MaxValue;
					Vector3 neardelta = Vector3.zero;

					for ( int i = 0; i < mod.splines.Count; i++ )
					{
						MegaFlowSpline fs = mod.splines[i];

						if ( fs.include )
						{
							Vector3 wp = tm.MultiplyPoint3x4(pos);
							Vector3 np = fs.shape.FindNearestPointWorld(wp, 5, ref kn, ref tan, ref alpha);

							Vector3 delta = np - wp;

							float dist = delta.magnitude;
							float fdist = fs.distcrv.Evaluate(alpha) * fs.falloffdist;

							if ( dist < nearest )
							{
								nearest = dist;
								neardelta = delta;
							}

							if ( dist < fdist )
							{
								MegaFlowContrib con = new MegaFlowContrib();
								con.src = fs;
								con.dist = dist;
								con.vel = (tan - np).normalized * fs.velocity;
								con.delta = delta.normalized;
								con.alpha = alpha;
								con.fdist = fdist;
								contrib.Add(con);
							}
						}
					}

					if ( contrib.Count > 0 )
					{
						float tweight = 0.0f;
						for ( int c = 0; c < contrib.Count; c++ )
							tweight += contrib[c].src.weight;

						Vector3 vel = cells[(x * gz * gy) + (z * gy) + y];	//Vector3.zero;

						for ( int c = 0; c < contrib.Count; c++ )
						{
							float a = contrib[c].dist / contrib[c].fdist;
							float lerp = contrib[c].src.falloffcrv.Evaluate(a);

							float v = mod.velocity * contrib[c].src.velcrv.Evaluate(contrib[c].alpha);

							switch ( contrib[c].src.mode )
							{
								case MegaFlowMode.Attract:
									vel += Vector3.Lerp(contrib[c].delta, contrib[c].vel, lerp) * v * (contrib[c].src.weight / tweight);
									break;

								case MegaFlowMode.Repulse:
									vel += Vector3.Lerp(-contrib[c].delta, contrib[c].vel, lerp) * v * (contrib[c].src.weight / tweight);
									break;

								case MegaFlowMode.Flow:
									vel += Vector3.Lerp(Vector3.zero, contrib[c].vel, lerp) * v * (contrib[c].src.weight / tweight);
									break;
							}
						}

						cells[(x * gz * gy) + (z * gy) + y] = vel;
					}
					else
					{
						Vector3 vl = cells[(x * gz * gy) + (z * gy) + y];	//Vector3.zero;

						switch ( mod.emtyspacemode )
						{
							case MegaFlowMode.Attract:
								vl = (neardelta.normalized * mod.velocity) + mod.startval;
								break;

							case MegaFlowMode.Repulse:
								vl = (-neardelta.normalized * mod.velocity) + mod.startval;
								break;

							case MegaFlowMode.Flow:
								break;

						}

						cells[(x * gz * gy) + (z * gy) + y] = vl;
					}
				}
			}
		}
		EditorUtility.ClearProgressBar();

		for ( int i = 0; i < mod.modifiers.Count; i++ )
		{
			EditorUtility.DisplayProgressBar("Adding Modifiers", "Building", (float)i / (float)mod.modifiers.Count);

			MegaFlowModifier fmod = mod.modifiers[i];

			if ( fmod.include && fmod.obj )
			{
				Ray	ray = new Ray(Vector3.zero, Vector3.zero);

				for ( int z = 0; z < gz; z++ )
				{
					pos.z = (z * mod.cellsize.z) + half.z;

					for ( int y = 0; y < gy; y++ )
					{
						pos.y = (y * mod.cellsize.y) + half.y;

						for ( int x = 0; x < gx; x++ )
						{
							pos.x = (x * mod.cellsize.x) + half.x;

							Vector3 wpos = tm.MultiplyPoint3x4(pos);

							Vector3 origin = wpos;
							origin.y += 1000.0f;

							ray.origin = origin;
							ray.direction = Vector3.down;

							RaycastHit hit;

							if ( fmod.obj.Raycast(ray, out hit, 1000.0f) )
							{
								ray.direction = Vector3.up;
								origin.y -= 2000.0f;
								ray.origin = origin;

								if ( fmod.obj.Raycast(ray, out hit, 1000.0f) )
								{
									switch ( fmod.type )
									{
										case MegaFlowModType.VelChange:
											Vector3 vel = cells[(x * gz * gy) + (z * gy) + y];
											vel *= fmod.amount;
											cells[(x * gz * gy) + (z * gy) + y] = vel;
											break;
									}
								}
							}
						}
					}
				}
			}
		}
		EditorUtility.ClearProgressBar();

		if ( flow )
		{
			MegaFlowFrame newf = ScriptableObject.CreateInstance<MegaFlowFrame>();

			newf.gridDim2[0] = gx;
			newf.gridDim2[1] = gy;
			newf.gridDim2[2] = gz;

			newf.size = mod.size;
			newf.gsize = newf.size;

			newf.spacing.x = newf.size.x / newf.gridDim2[0];
			newf.spacing.y = newf.size.y / newf.gridDim2[1];
			newf.spacing.z = newf.size.z / newf.gridDim2[2];

			newf.oos.x = 1.0f / newf.spacing.x;
			newf.oos.y = 1.0f / newf.spacing.y;
			newf.oos.z = 1.0f / newf.spacing.z;

			newf.vel.AddRange(cells);
			flow.AddFrame(newf);
		}
	}
}
