using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CanEditMultipleObjects]
[CustomEditor(typeof(MegaFlow))]
public class MegaFlowEditor : Editor
{
	public string lastpath = "";

	[MenuItem("GameObject/Create Other/MegaFlow/Source")]
	static void CreateMegaFlow()
	{
		Vector3 pos = Vector3.zero;

		if ( UnityEditor.SceneView.lastActiveSceneView )
			pos = UnityEditor.SceneView.lastActiveSceneView.pivot;

		GameObject go = new GameObject("MegaFlow Source");

		go.AddComponent<MegaFlow>();

		go.transform.position = pos;
		Selection.activeObject = go;
	}

	SerializedProperty	_prop_velcols;

	private void OnEnable()
	{
		_prop_velcols = serializedObject.FindProperty("velcols");
	}

	public void LoadFumeSeq(MegaFlow flow, string filename, int first, int last, int step)
	{
		if ( flow.frames.Count > 0 )
		{
			if ( !EditorUtility.DisplayDialog("Add to or Replace", "Add new frames to existing frames, or Replace All", "Add", "Replace") )
				flow.DestroyFrames();
		}

		for ( int i = first; i <= last; i += step )
		{
			float a = (float)(i + 1 - first) / (last - first);
			if ( !EditorUtility.DisplayCancelableProgressBar("Loading Fluid Frames", "Frame " + i, a) )
				flow.AddFrame(MegaFlowFumeFX.LoadFrame(filename, i, flow.namesplit));
			else
				break;
		}

		EditorUtility.ClearProgressBar();
	}

	public void LoadFumeFrame(MegaFlow flow, string filename)
	{
		if ( flow.frames.Count > 0 )
		{
			if ( !EditorUtility.DisplayDialog("Add to or Replace", "Add new frame to existing frames", "Add", "Replace") )
				flow.DestroyFrames();
		}
		flow.AddFrame(MegaFlowFumeFX.LoadFrame(filename));
	}

	public void LoadFGASeq(MegaFlow flow, string filename, int first, int last, int step)
	{
		if ( flow.frames.Count > 0 )
		{
			if ( !EditorUtility.DisplayDialog("Add to or Replace", "Add new frames to existing frames, or Replace All", "Add", "Replace") )
				flow.DestroyFrames();
		}

		for ( int i = first; i <= last; i += step )
		{
			float a = (float)(i + 1 - first) / (last - first);
			if ( !EditorUtility.DisplayCancelableProgressBar("Loading Fluid Frames", "Frame " + i, a) )
				flow.AddFrame(MegaFlowFGA.LoadFrame(filename, i, flow.namesplit, flow.decform));
			else
				break;
		}

		EditorUtility.ClearProgressBar();
	}

	public void LoadFGAFrame(MegaFlow flow, string filename)
	{
		if ( flow.frames.Count > 0 )
		{
			if ( !EditorUtility.DisplayDialog("Add to or Replace", "Add new frame to existing frames", "Add", "Replace") )
				flow.DestroyFrames();
		}

		flow.AddFrame(MegaFlowFGA.LoadFrame(filename));
	}

	public void LoadFLWSeq(MegaFlow flow, string filename, int first, int last, int step)
	{
		if ( flow.frames.Count > 0 )
		{
			if ( !EditorUtility.DisplayDialog("Add to or Replace", "Add new frames to existing frames", "Add", "Replace") )
				flow.DestroyFrames();
		}

		for ( int i = first; i <= last; i += step )
		{
			float a = (float)(i + 1 - first) / (last - first);
			if ( !EditorUtility.DisplayCancelableProgressBar("Loading Fluid Frames", "Frame " + i, a) )
				flow.AddFrame(MegaFlowFLW.LoadFrame(filename, i, flow.namesplit));
			else
				break;
		}

		EditorUtility.ClearProgressBar();
	}

	public void LoadFLWFrame(MegaFlow flow, string filename)
	{
		if ( flow.frames.Count > 0 )
		{
			if ( !EditorUtility.DisplayDialog("Add to or Replace", "Add new frame to existing frames", "Add", "Replace") )
				flow.DestroyFrames();
		}

		flow.AddFrame(MegaFlowFLW.LoadFrame(filename));
	}

	public override void OnInspectorGUI()
	{
		MegaFlow mod = (MegaFlow)target;

		serializedObject.Update();

		if ( GUILayout.Button("Optimize Frame Data") )
			mod.OptimizeData();

		if ( GUILayout.Button("Clear All Frames") )
			mod.DestroyFrames();

		EditorGUIUtility.LookLikeControls();

		mod.datasource = (MegaFlowDataSource)EditorGUILayout.EnumPopup("Data Source", mod.datasource);

		switch ( mod.datasource )
		{
			case MegaFlowDataSource.MegaFlow:
				mod.sequence = EditorGUILayout.BeginToggleGroup("Sequence", mod.sequence);
				mod.firstframe = EditorGUILayout.IntField("First Frame", mod.firstframe);
				mod.lastframe = EditorGUILayout.IntField("Last Frame", mod.lastframe);
				mod.framestep = EditorGUILayout.IntField("Frame Step", mod.framestep);
				if ( mod.framestep < 1 )
					mod.framestep = 1;

				mod.namesplit = EditorGUILayout.TextField("Name Split Char", mod.namesplit);

				EditorGUILayout.EndToggleGroup();

				if ( mod.sequence )
				{
					if ( GUILayout.Button("Load FLW Sequence") )
					{
						string file = EditorUtility.OpenFilePanel("MegaFlow File", lastpath, "flw");

						if ( file != null && file.Length > 1 )
						{
							lastpath = file;
							LoadFLWSeq(mod, file, mod.firstframe, mod.lastframe, mod.framestep);
							mod.SetFrame(mod.frame);
							mod.CalcMemUse();
						}
					}
				}
				else
				{
					if ( GUILayout.Button("Load FLW Frame") )
					{
						string file = EditorUtility.OpenFilePanel("MegaFlow File", lastpath, "flw");

						if ( file != null && file.Length > 1 )
						{
							lastpath = file;
							LoadFLWFrame(mod, file);
							mod.SetFrame(0);
							mod.CalcMemUse();
						}
					}
				}
				break;

			case MegaFlowDataSource.FumeFX:
				mod.sequence = EditorGUILayout.BeginToggleGroup("Sequence", mod.sequence);
				mod.firstframe = EditorGUILayout.IntField("First Frame", mod.firstframe);
				mod.lastframe = EditorGUILayout.IntField("Last Frame", mod.lastframe);
				mod.framestep = EditorGUILayout.IntField("Frame Step", mod.framestep);
				mod.namesplit = EditorGUILayout.TextField("Name Split Char", mod.namesplit);

				if ( mod.framestep < 1 )
					mod.framestep = 1;
				EditorGUILayout.EndToggleGroup();

				if ( mod.sequence )
				{
					if ( GUILayout.Button("Load FumeFX Sequence") )
					{
						string file = EditorUtility.OpenFilePanel("Vector Field File", lastpath, "fxd");

						if ( file != null && file.Length > 1 )
						{
							lastpath = file;
							LoadFumeSeq(mod, file, mod.firstframe, mod.lastframe, mod.framestep);
							mod.SetFrame(mod.frame);
							mod.CalcMemUse();
						}
					}
				}
				else
				{
					if ( GUILayout.Button("Load FumeFX Frame") )
					{
						string file = EditorUtility.OpenFilePanel("Vector Field File", lastpath, "fxd");

						if ( file != null && file.Length > 1 )
						{
							lastpath = file;
							LoadFumeFrame(mod, file);
							mod.SetFrame(0);
							mod.CalcMemUse();
						}
					}
				}
				break;

			case MegaFlowDataSource.FGA:
				mod.sequence = EditorGUILayout.BeginToggleGroup("Sequence", mod.sequence);
				mod.firstframe = EditorGUILayout.IntField("First Frame", mod.firstframe);
				mod.lastframe = EditorGUILayout.IntField("Last Frame", mod.lastframe);
				mod.framestep = EditorGUILayout.IntField("Frame Step", mod.framestep);
				if ( mod.framestep < 1 )
					mod.framestep = 1;
							int val = 0;
				mod.decform = EditorGUILayout.IntSlider("Format name" + val.ToString("D" + mod.decform) + ".fga", mod.decform, 1, 6);
				mod.namesplit = EditorGUILayout.TextField("Name Split Char", mod.namesplit);

				EditorGUILayout.EndToggleGroup();

				if ( mod.sequence )
				{
					if ( GUILayout.Button("Load FGA Sequence") )
					{
						string file = EditorUtility.OpenFilePanel("UE4 Vector Field File", lastpath, "fga");

						if ( file != null && file.Length > 1 )
						{
							lastpath = file;
							LoadFGASeq(mod, file, mod.firstframe, mod.lastframe, mod.framestep);
							mod.SetFrame(mod.frame);
							mod.CalcMemUse();
						}
					}
				}
				else
				{
					if ( GUILayout.Button("Load FGA File") )
					{
						string file = EditorUtility.OpenFilePanel("UE4 Vector Field File", lastpath, "fga");

						if ( file != null && file.Length > 1 )
						{
							lastpath = file;
							LoadFGAFrame(mod, file);
							mod.SetFrame(0);
							mod.CalcMemUse();
						}
					}
				}
				break;

			case MegaFlowDataSource.RealFlow:
				if ( GUILayout.Button("Load RealFlow File") )
				{
					string file = EditorUtility.OpenFilePanel("RealFlow Vector Field File", lastpath, "bin");

					if ( file != null && file.Length > 1 )
					{
						lastpath = file;
						//mod.LoadRealFlowFrame(file);
						mod.SetFrame(0);
						mod.CalcMemUse();
					}
				}
				break;
		}

		mod.CalcMemUse();
#if false
		Transform parent = (Transform)EditorGUILayout.ObjectField("Set Parent", mod.fluidPos, typeof(Transform), true);
		if ( parent != mod.fluidPos )
		{
			mod.fluidPos = parent;
			if ( parent )
			{
				mod.transform.parent = parent;
				mod.transform.position = parent.transform.position - (mod.flow.size * 0.5f);
			}
		}
#endif
		string opt = "Not Optimized";
		if ( mod.flow && mod.flow.optimized )
			opt = "Optimized";

		//mod.CalcMemUse();
		if ( mod.frames != null && mod.frames.Count > 0 )
			EditorGUILayout.HelpBox("Frame Memory: " + ((float)mod.flow.memory / (1024.0f * 1024.0f)).ToString("0.00") +  "MBs\nTotal Memory: " + ((float)mod.memoryuse / (1024.0f * 1024.0f)).ToString("0.00") + "MBs\n" + "Frames: " + mod.frames.Count + "\n" + "Grid: " + mod.flow.gridDim2[0] + "x" + mod.flow.gridDim2[1] + "x" + mod.flow.gridDim2[2] + "\n" + "size: " + mod.flow.size + "\n" + opt, MessageType.None, true);
		else
			EditorGUILayout.HelpBox("No Fluid Data Loaded!", MessageType.Warning, true);

		if ( mod.frames.Count > 1 )
		{
			int frame = EditorGUILayout.IntSlider("Frame", mod.frame, 0, mod.frames.Count - 1);
			if ( frame != mod.frame )
				mod.SetFrame(frame);
		}

		if ( GUILayout.Button("Normalize Frame") )
			mod.NormalizeFrame(mod.frame);

		if ( GUILayout.Button("Delete Frame") )
			mod.DestroyFrame(mod.frame);

		if ( mod.flow )
		{
			mod.flow.offset = EditorGUILayout.Vector3Field("Frame Offset", mod.flow.offset);
		}

		mod.showdisplayparams = EditorGUILayout.Foldout(mod.showdisplayparams, "Data Display Params");

		if ( mod.showdisplayparams )
		{
			EditorGUILayout.BeginVertical("box");
			mod.Scale = EditorGUILayout.FloatField("Scale Frc", mod.Scale);

			mod.shownotselected = EditorGUILayout.Toggle("Gizmo Always On", mod.shownotselected);

			mod.Plane = (MegaFlowAxis)EditorGUILayout.EnumPopup("Plane", mod.Plane);

			if ( mod.flow != null )
			{
				switch ( mod.Plane )
				{
					case MegaFlowAxis.X:
						{
							int pos = EditorGUILayout.IntSlider("Position", mod.Position, 0, mod.flow.gridDim2[0] - 1);
							if ( pos != mod.Position )
							{
								mod.Position = pos;
								mod.handlepos.x = pos * mod.flow.spacing.x;
							}
							mod.Thickness = EditorGUILayout.IntSlider("Thickness", mod.Thickness, 0, mod.flow.gridDim2[0]);
						}
						break;

					case MegaFlowAxis.Y:
						{
							int pos = EditorGUILayout.IntSlider("Position", mod.Position, 0, mod.flow.gridDim2[1] - 1);
							if ( pos != mod.Position )
							{
								mod.Position = pos;
								mod.handlepos.y = pos * mod.flow.spacing.y;
							}
							mod.Thickness = EditorGUILayout.IntSlider("Thickness", mod.Thickness, 0, mod.flow.gridDim2[1]);
						}
						break;

					case MegaFlowAxis.Z:
						{
							int pos = EditorGUILayout.IntSlider("Position", mod.Position, 0, mod.flow.gridDim2[2] - 1);
							if ( pos != mod.Position )
							{
								mod.Position = pos;
								mod.handlepos.z = pos * mod.flow.spacing.z;
							}
							mod.Thickness = EditorGUILayout.IntSlider("Thickness", mod.Thickness, 0, mod.flow.gridDim2[2]);
						}
						break;
				}
			}

			mod.velScl = EditorGUILayout.FloatField("Vel Visual Magnitude", mod.velScl);
			if ( mod.velScl < 0.01f )
				mod.velScl = 0.01f;

			mod.showvelmag = EditorGUILayout.BeginToggleGroup("Show Vel Mag", mod.showvelmag);
			EditorGUILayout.PropertyField(_prop_velcols, new GUIContent("Colors"));
			mod.minvel = EditorGUILayout.FloatField("Min Vel", mod.minvel);
			mod.maxvel = EditorGUILayout.FloatField("Max Vel", mod.maxvel);
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.BeginVertical("Box");
			//if ( mod.flow && mod.flow.vel.Count > 0 )
			if ( mod.flow && (mod.flow.vel.Count > 0 || mod.flow.optvel.Count > 0) )
			{
				mod.showVel = EditorGUILayout.BeginToggleGroup("Show Vel", mod.showVel);
				mod.velLen = EditorGUILayout.FloatField("Vel Vector Len", mod.velLen);
				if ( mod.velLen < 0.01f )
					mod.velLen = 0.01f;
				mod.skip = EditorGUILayout.IntField("Vel Skip", mod.skip);
				mod.velThreshold = EditorGUILayout.Slider("Vel Threshold", mod.velThreshold, 0.0f, 1.0f);
				mod.velAlpha = EditorGUILayout.Slider("Vel Alpha", mod.velAlpha, 0.0f, 1.0f);
				//mod.velLen = EditorGUILayout.FloatField("Vel Length", mod.velLen);
				EditorGUILayout.EndToggleGroup();
			}
			EditorGUILayout.EndVertical();

			if ( mod.flow && mod.flow.smoke.Count > 0 )
			{
				mod.showSmoke = EditorGUILayout.BeginToggleGroup("Show Smoke", mod.showSmoke);
				mod.smokeThreshold = EditorGUILayout.Slider("Smoke Threshold", mod.smokeThreshold, 0.0f, 1.0f);
				mod.smokeAlpha = EditorGUILayout.Slider("Smoke Alpha", mod.smokeAlpha, 0.0f, 1.0f);
				EditorGUILayout.EndToggleGroup();
			}

			mod.showgrid = EditorGUILayout.Toggle("Show Grid", mod.showgrid);
			mod.gridColor = EditorGUILayout.ColorField("Grid Color", mod.gridColor);
			mod.gridColor1 = EditorGUILayout.ColorField("Grid Color1", mod.gridColor1);

			mod.showcells = EditorGUILayout.BeginToggleGroup("Show Cells", mod.showcells);
			mod.showcellmag = EditorGUILayout.Toggle("Show Cell Mag", mod.showcellmag);
			mod.cellalpha = EditorGUILayout.Slider("Cell Alpha", mod.cellalpha, 0.0f, 1.0f);
			EditorGUILayout.EndToggleGroup();

			EditorGUILayout.EndVertical();
		}

		mod.showribbonparams = EditorGUILayout.Foldout(mod.showribbonparams, "Ribbon Params");

		if ( mod.showribbonparams )
		{
			EditorGUILayout.BeginVertical("box");
			mod.showRibbon = EditorGUILayout.BeginToggleGroup("Show Ribbons", mod.showRibbon);
			mod.ribpos = EditorGUILayout.Vector3Field("Ribbon Pos", mod.ribpos);
			mod.Dt = EditorGUILayout.Slider("Dt", mod.Dt, 0.001f, 0.1f);
			mod.Density = EditorGUILayout.Slider("Density", mod.Density, 0.1f, 2.0f);
			mod.Area = EditorGUILayout.Slider("Area", mod.Area, 0.0f, 2.0f);
			mod.Reynolds = EditorGUILayout.FloatField("Reynolds", mod.Reynolds);
			mod.Mass = EditorGUILayout.Slider("Mass", mod.Mass, 0.01f, 1.0f);
			mod.floor = EditorGUILayout.FloatField("Floor", mod.floor);
			mod.LineStep = EditorGUILayout.IntField("LineStep", mod.LineStep);
			mod.SizeZ = EditorGUILayout.FloatField("Size Z", mod.SizeZ);
			mod.StepZ = EditorGUILayout.IntField("Step Z", mod.StepZ);
			mod.SizeY = EditorGUILayout.FloatField("Size Y", mod.SizeY);
			mod.StepY = EditorGUILayout.IntField("Step Y", mod.StepY);
			mod.Gravity = EditorGUILayout.Vector3Field("Gravity", mod.Gravity);
			mod.Duration = EditorGUILayout.FloatField("Duration", mod.Duration);
			mod.ribbonscale = EditorGUILayout.FloatField("Ribbon Color Scale", mod.ribbonscale);
			mod.ribbonAlpha = EditorGUILayout.Slider("Ribbon Alpha", mod.ribbonAlpha, 0.0f, 1.0f);
			EditorGUILayout.EndVertical();

			EditorGUILayout.HelpBox("Calculated: " + mod.calculated + " points in " + (mod.calcTime * 1000.0f) + "ms\nPersample = " + ((mod.calcTime * 1000.0f) / (float)mod.calculated) + "ms", MessageType.None, true);

			EditorGUILayout.EndToggleGroup();
		}

		mod.textureoptions = EditorGUILayout.Foldout(mod.textureoptions, "Texture Options");

		if ( mod.textureoptions )
		{
			mod.texturewidth = EditorGUILayout.IntField("Width", mod.texturewidth);
			mod.textureheight = EditorGUILayout.IntField("Height", mod.textureheight);
			mod.texturedepth = EditorGUILayout.IntField("Depth", mod.texturedepth);

			if ( GUILayout.Button("Create 3D Texture") )
			{
				if ( !Directory.Exists("Assets/MegaFlow Textures") )
					AssetDatabase.CreateFolder("Assets", "MegaFlow Textures");

				Texture3D t3d = mod.Create3DTexture(mod.texturewidth, mod.textureheight, mod.texturedepth, mod.frame);

				if ( t3d )
					AssetDatabase.CreateAsset(t3d, "Assets/MegaFlow Textures/" + mod.name + "_" + mod.frame + "_VolumeTexture.asset");
			}
		}

		mod.showadvparams = EditorGUILayout.Foldout(mod.showadvparams, "Advanced Options");
		if ( mod.showadvparams )
		{
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("New Grid Size");	//, GUILayout.MaxWidth(80.0f));
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("x", GUILayout.Width(16.0f));
			mod.samplex = EditorGUILayout.IntField("", mod.samplex, GUILayout.Width(48.0f));
			EditorGUILayout.LabelField("y", GUILayout.Width(16.0f));
			mod.sampley = EditorGUILayout.IntField("", mod.sampley, GUILayout.Width(48.0f));
			EditorGUILayout.LabelField("z", GUILayout.Width(16.0f));
			mod.samplez = EditorGUILayout.IntField("", mod.samplez, GUILayout.Width(48.0f));
			EditorGUILayout.EndHorizontal();

			if ( GUILayout.Button("Resample Grid") )
			{
				mod.Resample(mod.frames[mod.frame], mod.samplex, mod.sampley, mod.samplez);
			}

			if ( mod.flow )
			{
				if ( GUILayout.Button("Export FGA") )
				{
					string epath = EditorUtility.SaveFilePanel("Export FGA File", "", "fgaexport_" + mod.frame, "fga");

					if ( epath.Length > 0 )
						MegaFlowFGA.SaveFGA(mod.flow, epath); 
				}
			}

			EditorGUILayout.EndVertical();
		}
#if false
		mod.showanimparams = EditorGUILayout.Foldout(mod.showanimparams, "Anim Params");
		if ( mod.showanimparams )
		{
			if ( mod.frames.Count > 1 )
			{
				EditorGUILayout.BeginVertical("box");
				mod.animate = EditorGUILayout.BeginToggleGroup("Animate", mod.animate);
				mod.animlength = EditorGUILayout.FloatField("Length", mod.animlength);
				mod.animspeed = EditorGUILayout.FloatField("Speed", mod.animspeed);
				mod.animtime = EditorGUILayout.FloatField("Time", mod.animtime);
				EditorGUILayout.EndToggleGroup();
				EditorGUILayout.EndVertical();
			}
			else
				EditorGUILayout.HelpBox("Need more Flow Frames loaded to be able to animate between them.", MessageType.None, true);
		}
#endif

		if ( GUI.changed )
		{
			serializedObject.ApplyModifiedProperties();
			RibbonPhysics(mod, 0.0f);
			EditorUtility.SetDirty(target);
		}
	}

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3
	[DrawGizmo(GizmoType.InSelectionHierarchy)]
#else
	[DrawGizmo(GizmoType.InSelectionHierarchy)]
#endif
	static void RenderGizmo(MegaFlow flow, GizmoType gizmoType)
	{
		if ( flow.flow != null )
			DisplayFlow(flow, 0.0f, 0);

		flow.DrawGizmo();
	}

#if UNITY_5_1 || UNITY_5_2 || UNITY_5_3
	[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
#else
	[DrawGizmo(GizmoType.NotInSelectionHierarchy)]
#endif
	static void RenderGizmo1(MegaFlow flow, GizmoType gizmoType)
	{
		if ( flow.shownotselected )
		{
			if ( flow.flow != null )
				DisplayFlow(flow, 0.0f, 0);

			flow.DrawGizmo();
		}
	}

	public void OnSceneGUI()
	{
		MegaFlow mod = (MegaFlow)target;

		if ( mod.flow == null )
			return;

		//Matrix4x4 offtm = Matrix4x4.TRS(-mod.flow.size * 0.5f, Quaternion.identity, Vector3.one);
		Matrix4x4 offtm = Matrix4x4.TRS((-mod.flow.size * 0.5f) + mod.flow.offset, Quaternion.identity, Vector3.one);

		Handles.matrix = mod.transform.localToWorldMatrix * offtm;

		if ( mod.showRibbon )
		{
			Vector3[]	verts = new Vector3[4];
			verts[0] = mod.ribpos;
			verts[1] = mod.ribpos;
			verts[2] = mod.ribpos;
			verts[3] = mod.ribpos;

			verts[0].z += mod.SizeZ;
			verts[0].y += mod.SizeY;
			verts[1].z -= mod.SizeZ;
			verts[1].y += mod.SizeY;
			verts[2].z -= mod.SizeZ;
			verts[2].y -= mod.SizeY;
			verts[3].z += mod.SizeZ;
			verts[3].y -= mod.SizeY;

			Handles.color = Color.white;
			Handles.DrawSolidRectangleWithOutline(verts, new Color(1, 1, 1, 0.25f), new Color(0, 0, 0, 1));

			Vector3 hp = (verts[0] + verts[1]) * 0.5f;
			Vector3 hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.SizeY + mod.SizeZ) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);
		
			if ( hp1.y != hp.y )
			{
				mod.SizeY += hp1.y - hp.y;
				RibbonPhysics(mod, 0.0f);
			}

			hp = (verts[1] + verts[2]) * 0.5f;
			hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.SizeY + mod.SizeZ) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);

			if ( hp1.z != hp.z )
			{
				mod.SizeZ -= hp1.z - hp.z;
				RibbonPhysics(mod, 0.0f);
			}

			hp = (verts[2] + verts[3]) * 0.5f;
			hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.SizeY + mod.SizeZ) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);

			if ( hp1.y != hp.y )
			{
				mod.SizeY -= hp1.y - hp.y;
				RibbonPhysics(mod, 0.0f);
			}

			hp = (verts[3] + verts[0]) * 0.5f;
			hp1 = Handles.FreeMoveHandle(hp, Quaternion.identity, (mod.SizeY + mod.SizeZ) * 0.5f * 0.025f, Vector3.zero, Handles.DotCap);

			if ( hp1.z != hp.z )
			{
				mod.SizeZ += hp1.z - hp.z;
				RibbonPhysics(mod, 0.0f);
			}
		}

		if ( mod.showRibbon )
		{
			Vector3 gpos = mod.ribpos;
			gpos = Handles.PositionHandle(gpos, Quaternion.identity);
			if ( gpos != mod.ribpos )
			{
				mod.ribpos = gpos;
				RibbonPhysics(mod, 0.0f);
			}
		}

		Handles.color = Color.green;
		switch ( mod.Plane )
		{
			case MegaFlowAxis.X:
				mod.handlepos = Handles.FreeMoveHandle(mod.handlepos, Quaternion.identity, mod.flow.spacing.x * 1.0f, Vector3.zero, Handles.SphereCap);
				mod.Position = Mathf.Clamp((int)(mod.handlepos.x / mod.flow.spacing.x), 0, mod.flow.gridDim2[0] - 1);

				mod.handlepos.y = mod.flow.size.y * 0.5f;
				mod.handlepos.z = mod.flow.size.z * 0.5f;
				break;

			case MegaFlowAxis.Y:
				mod.handlepos = Handles.FreeMoveHandle(mod.handlepos, Quaternion.identity, mod.flow.spacing.y * 1.0f, Vector3.zero, Handles.SphereCap);
				mod.Position = Mathf.Clamp((int)(mod.handlepos.y / mod.flow.spacing.y), 0, mod.flow.gridDim2[1] - 1);
				mod.handlepos.x = mod.flow.size.x * 0.5f;
				mod.handlepos.z = mod.flow.size.z * 0.5f;
				break;

			case MegaFlowAxis.Z:
				mod.handlepos = Handles.FreeMoveHandle(mod.handlepos, Quaternion.identity, mod.flow.spacing.z * 1.0f, Vector3.zero, Handles.SphereCap);
				mod.Position = Mathf.Clamp((int)(mod.handlepos.z / mod.flow.spacing.z), 0, mod.flow.gridDim2[2] - 0);
				mod.handlepos.x = mod.flow.size.x * 0.5f;
				mod.handlepos.y = mod.flow.size.y * 0.5f;
				break;
		}

		mod.handlepos.x = Mathf.Clamp(mod.handlepos.x, 0.0f, mod.flow.size.x);
		mod.handlepos.y = Mathf.Clamp(mod.handlepos.y, 0.0f, mod.flow.size.y);
		mod.handlepos.z = Mathf.Clamp(mod.handlepos.z, 0.0f, mod.flow.size.z);

		Handles.matrix = Matrix4x4.identity;
	}


	static public Material cellMaterial;
	public static void CreateCellMaterial()
	{
		if( !cellMaterial )
		{
			cellMaterial = new Material( "Shader \"Lines/Colored Blended\" {" +
			"SubShader { Pass { " +
			"    Blend SrcAlpha OneMinusSrcAlpha " +
			"    ZWrite On Cull Off Fog { Mode Off } " +
			"    BindChannels {" +
			"      Bind \"vertex\", vertex Bind \"color\", color }" +
			"} } }" );
			cellMaterial.hideFlags = HideFlags.HideAndDontSave;
			cellMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
		}
	}

	static float cellrng = 0.0f;

	static void DrawCell(MegaFlow mod, Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Matrix4x4 tm)
	{
		Color c1 = Color.black;
		Color c2 = Color.black;
		Color c3 = Color.black;
		Color c4 = Color.black;

		if ( mod.showcellmag )
		{
			float thresh = mod.velThreshold * mod.velThreshold;
			float v = 0.0f;

			if ( v1.sqrMagnitude > thresh )
			{
				v = v1.magnitude * cellrng;
				c1 = mod.GetCol(v);
				c1.a = mod.cellalpha;
			}
			else
				c1.a = 0.0f;

			if ( v2.sqrMagnitude > thresh )
			{
				v = v2.magnitude * cellrng;
				c2 = mod.GetCol(v);
				c2.a = mod.cellalpha;
			}
			else
				c2.a = 0.0f;

			if ( v3.sqrMagnitude > thresh )
			{
				v = v3.magnitude * cellrng;
				c3 = mod.GetCol(v);
				c3.a = mod.cellalpha;
			}
			else
				c3.a = 0.0f;

			if ( v4.sqrMagnitude > thresh )
			{
				v = v4.magnitude * cellrng;
				c4 = mod.GetCol(v);
				c4.a = mod.cellalpha;
			}
			else
				c4.a = 0.0f;
		}
		else
		{
			c1.a = mod.cellalpha;
			c2.a = mod.cellalpha;
			c3.a = mod.cellalpha;
			c4.a = mod.cellalpha;

			float thresh = mod.velThreshold * mod.velThreshold;
			if ( v1.sqrMagnitude > thresh )
			{
				v1 *= mod.Scale;
				c1.r = Mathf.Abs(v1.x);
				c1.g = Mathf.Abs(v1.y);
				c1.b = Mathf.Abs(v1.z);
			}
			else
				c1.a = 0.0f;

			if ( v2.sqrMagnitude > thresh )
			{
				v2 *= mod.Scale;
				c2.r = Mathf.Abs(v2.x);
				c2.g = Mathf.Abs(v2.y);
				c2.b = Mathf.Abs(v2.z);
			}
			else
				c2.a = 0.0f;

			if ( v3.sqrMagnitude > thresh )
			{
				v3 *= mod.Scale;
				c3.r = Mathf.Abs(v3.x);
				c3.g = Mathf.Abs(v3.y);
				c3.b = Mathf.Abs(v3.z);
			}
			else
				c3.a = 0.0f;

			if ( v4.sqrMagnitude > thresh )
			{
				v4 *= mod.Scale;
				c4.r = Mathf.Abs(v4.x);
				c4.g = Mathf.Abs(v4.y);
				c4.b = Mathf.Abs(v4.z);
			}
			else
				c4.a = 0.0f;
		}

		p1 = tm.MultiplyPoint3x4(p1);
		p2 = tm.MultiplyPoint3x4(p2);
		p3 = tm.MultiplyPoint3x4(p3);
		p4 = tm.MultiplyPoint3x4(p4);

		GL.Begin(GL.TRIANGLES);

		GL.Color(c1);
		GL.Vertex(p1);
		GL.Color(c2);
		GL.Vertex(p2);
		GL.Color(c3);
		GL.Vertex(p3);

		GL.Color(c1);
		GL.Vertex(p1);
		GL.Color(c3);
		GL.Vertex(p3);

		GL.Color(c4);
		GL.Vertex(p4);
		GL.End();
	}

	static public void DisplayFlow(MegaFlow mod, float t, int flagst)
	{
		float smokethreshhold = mod.smokeThreshold;
		float velthreshold = mod.velThreshold;
		velthreshold *= velthreshold;

		bool showvel = mod.showVel;
		bool showsmoke = mod.showSmoke;

		int	xd = mod.flow.gridDim2[0] - 1;
		int	yd = mod.flow.gridDim2[1] - 1;
		int	zd = mod.flow.gridDim2[2] - 1;

		int xs = 0;
		int ys = 0;
		int zs = 0;

		//Matrix4x4 offtm = Matrix4x4.TRS(-mod.flow.size * 0.5f, Quaternion.identity, Vector3.one);
		Matrix4x4 offtm = Matrix4x4.TRS((-mod.flow.size * 0.5f) + mod.flow.offset, Quaternion.identity, Vector3.one);

		Matrix4x4 tm = mod.transform.localToWorldMatrix * offtm;
		Handles.matrix = tm;	//mod.transform.localToWorldMatrix * offtm;

		//Handles.matrix = mod.transform.localToWorldMatrix;

		float adjx = mod.flow.spacing.x * 0.5f;
		float adjy = mod.flow.spacing.y * 0.5f;
		float adjz = mod.flow.spacing.z * 0.5f;

		bool inbounds = false;

		mod.Prepare();

		GL.PushMatrix();
		CreateCellMaterial();
		cellMaterial.SetPass(0);

		cellrng = 1.0f / (mod.maxvel - mod.minvel);

		if ( mod.Slice )
		{
			Vector3[] cp = new Vector3[4];

			Handles.color = mod.gridColor1;

			switch ( mod.Plane )
			{
				case MegaFlowAxis.Y:
					yd = 0;
					ys = mod.Position;
					if ( ys >= mod.flow.gridDim2[1] - 1 )
						ys = mod.flow.gridDim2[1] - 1;

					yd = ys + mod.Thickness;
					if ( yd >= mod.flow.gridDim2[1] - 1 )
						yd = mod.flow.gridDim2[1] - 1;

					if ( mod.showgrid )
					{
						for ( int x = xs; x <= xd + 1; x++ )
						{
							Vector3	p;

							p.x = (x * mod.flow.spacing.x);
							p.y = (ys * mod.flow.spacing.y) + adjy;
							p.z = 0.0f;

							if ( x == xs || x == (xd + 1) )
								Handles.color = Color.black;
							else
								Handles.color = mod.gridColor1;

							Vector3 p1 = p;
							p1.z = p.z + ((zd + 1) * mod.flow.spacing.z);
							DrawLine(p, p1);
						}

						for ( int z = zs; z <= (zd + 1); z++ )
						{
							Vector3	p;

							p.x = 0.0f;
							p.y = (ys * mod.flow.spacing.y) + adjy;
							p.z = (z * mod.flow.spacing.z);

							if ( z == zs || z == (zd + 1) )
								Handles.color = Color.black;
							else
								Handles.color = mod.gridColor1;

							Vector3 p1 = p;
							p1.x = p.x + ((xd + 1) * mod.flow.spacing.x);
							DrawLine(p, p1);
						}
					}

					cp[0] = new Vector3(0.0f, (yd * mod.flow.spacing.y) + adjy, 0.0f);
					cp[1] = new Vector3((xd + 1) * mod.flow.spacing.x, (yd * mod.flow.spacing.y) + adjy, 0.0f);
					cp[2] = new Vector3((xd + 1) * mod.flow.spacing.x, (yd * mod.flow.spacing.y) + adjy, (zd + 1) * mod.flow.spacing.z);
					cp[3] = new Vector3(0.0f, (yd * mod.flow.spacing.y) + adjy, (zd + 1) * mod.flow.spacing.z);

					Handles.color = mod.gridColor;
					DrawLine(cp[0], cp[1]);
					DrawLine(cp[1], cp[2]);
					DrawLine(cp[2], cp[3]);
					DrawLine(cp[3], cp[0]);

					if ( mod.showcells )
					{
						//Matrix4x4 tm = mod.transform.localToWorldMatrix;

						Vector3	p;

						for ( int x = xs; x < xd; x++ )
						{
							p.x = x * mod.flow.spacing.x + adjx;

							for ( int y = ys; y <= yd; y++ )
							{
								p.y = y * mod.flow.spacing.y + adjy;

								for ( int z = zs; z < zd; z++ )
								{
									p.z = z * mod.flow.spacing.z + adjz;

									Vector3 v1 = mod.SampleVel(x, y, z) * mod.velScl;
									Vector3 v2 = mod.SampleVel(x + 1, y, z) * mod.velScl;
									Vector3 v3 = mod.SampleVel(x + 1, y, z + 1) * mod.velScl;
									Vector3 v4 = mod.SampleVel(x, y, z + 1) * mod.velScl;

									Vector3 p1 = new Vector3(p.x, p.y, p.z);
									Vector3 p2 = new Vector3(p.x + mod.flow.spacing.x, p.y, p.z);
									Vector3 p3 = new Vector3(p.x + mod.flow.spacing.x, p.y, p.z + mod.flow.spacing.z);
									Vector3 p4 = new Vector3(p.x, p.y, p.z + mod.flow.spacing.z);

									DrawCell(mod, v1, v2, v3, v4, p1, p2, p3, p4, tm);
								}
							}
						}
					}
					break;

				case MegaFlowAxis.X:
					xd = 0;
					xs = mod.Position;
					if ( xs >= mod.flow.gridDim2[0] - 1 )
						xs = mod.flow.gridDim2[0] - 1;
					xd = xs + mod.Thickness;
					if ( xd >= mod.flow.gridDim2[0] - 1 )
						xd = mod.flow.gridDim2[0] - 1;

					if ( mod.showgrid )
					{
						for ( int z = zs; z <= (zd + 1); z++ )
						{
							Vector3	p;

							p.x = (xs * mod.flow.spacing.x) + adjx;
							p.y = 0.0f;
							p.z = (z * mod.flow.spacing.z);

							if ( z == zs || z == (zd + 1) )
								Handles.color = Color.black;
							else
								Handles.color = mod.gridColor1;

							Vector3 p1 = p;
							p1.y = p.y + ((yd + 1) * mod.flow.spacing.y);
							DrawLine(p, p1);
						}

						for ( int y = ys; y <= (yd + 1); y++ )
						{
							Vector3	p;

							p.x = (xs * mod.flow.spacing.x) + adjx;
							p.y = (y * mod.flow.spacing.y);
							p.z = 0.0f;

							if ( y == ys || y == (yd + 1) )
								Handles.color = Color.black;
							else
								Handles.color = mod.gridColor1;

							Vector3 p1 = p;
							p1.z = p.z + ((zd + 1) * mod.flow.spacing.z);
							DrawLine(p, p1);
						}
					}

					cp[0] = new Vector3((xd * mod.flow.spacing.x) + adjx, 0.0f, 0.0f);
					cp[1] = new Vector3((xd * mod.flow.spacing.x) + adjx, 0.0f, ((zd + 1) * mod.flow.spacing.z));
					cp[2] = new Vector3((xd * mod.flow.spacing.x) + adjx, ((yd + 1) * mod.flow.spacing.y), ((zd + 1) * mod.flow.spacing.z));
					cp[3] = new Vector3((xd * mod.flow.spacing.x) + adjx, ((yd + 1) * mod.flow.spacing.y), 0.0f);

					Handles.color = mod.gridColor;
					DrawLine(cp[0], cp[1]);
					DrawLine(cp[1], cp[2]);
					DrawLine(cp[2], cp[3]);
					DrawLine(cp[3], cp[0]);

					if ( mod.showcells )
					{
						//Matrix4x4 tm = mod.transform.localToWorldMatrix;

						Vector3 p;
						for ( int x = xs; x <= xd; x++ )
						{
							p.x = x * mod.flow.spacing.x + adjx;
							for ( int y = ys; y < yd; y++ )
							{
								p.y = y * mod.flow.spacing.y + adjy;
								for ( int z = zs; z < zd; z++ )
								{
									p.z = z * mod.flow.spacing.z + adjz;

									Vector3 v1 = mod.SampleVel(x, y, z) * mod.velScl;
									Vector3 v2 = mod.SampleVel(x, y + 1, z) * mod.velScl;
									Vector3 v3 = mod.SampleVel(x, y + 1, z + 1) * mod.velScl;
									Vector3 v4 = mod.SampleVel(x, y, z + 1) * mod.velScl;

									Vector3 p1 = new Vector3(p.x, p.y, p.z);
									Vector3 p2 = new Vector3(p.x, p.y + mod.flow.spacing.y, p.z);
									Vector3 p3 = new Vector3(p.x, p.y + mod.flow.spacing.y, p.z + mod.flow.spacing.z);
									Vector3 p4 = new Vector3(p.x, p.y, p.z + mod.flow.spacing.z);

									DrawCell(mod, v1, v2, v3, v4, p1, p2, p3, p4, tm);
								}
							}
						}
					}
					break;

				case MegaFlowAxis.Z:
					zd = 0;
					zs = mod.Position;
					if ( zs >= mod.flow.gridDim2[2] - 1 )
						zs = mod.flow.gridDim2[2] - 1;

					zd = zs + mod.Thickness;
					if ( zd >= mod.flow.gridDim2[2] - 1 )
						zd = mod.flow.gridDim2[2] - 1;

					if ( mod.showgrid )
					{
						for ( int x = xs; x <= (xd + 1); x++ )
						{
							Vector3	p;

							p.x = (x * mod.flow.spacing.x);
							p.y = 0.0f;
							p.z = (zs * mod.flow.spacing.z) + adjz;

							if ( x == xs || x == (xd + 1) )
								Handles.color = Color.black;
							else
								Handles.color = mod.gridColor1;

							Vector3 p1 = p;
							p1.y = p.y + ((yd + 1) * mod.flow.spacing.y);
							DrawLine(p, p1);
						}

						for ( int y = ys; y <= (yd + 1); y++ )
						{
							Vector3	p;

							p.x = 0.0f;
							p.y = (y * mod.flow.spacing.y);
							p.z = (zs * mod.flow.spacing.z) + adjz;

							if ( y == ys || y == (yd + 1) )
								Handles.color = Color.black;
							else
								Handles.color = mod.gridColor1;

							Vector3 p1 = p;
							p1.x = p.x + ((xd + 1) * mod.flow.spacing.x);
							DrawLine(p, p1);
						}
					}

					cp[0] = new Vector3(0.0f, 0.0f, (zd * mod.flow.spacing.z) + adjz);
					cp[1] = new Vector3(((xd + 1) * mod.flow.spacing.x), 0.0f, (zd * mod.flow.spacing.z) + adjz);
					cp[2] = new Vector3(((xd + 1) * mod.flow.spacing.x), ((yd + 1) * mod.flow.spacing.y), (zd * mod.flow.spacing.z) + adjz);
					cp[3] = new Vector3(0.0f, ((yd + 1) * mod.flow.spacing.y), (zd * mod.flow.spacing.z) + adjz);

					Handles.color = mod.gridColor;
					DrawLine(cp[0], cp[1]);
					DrawLine(cp[1], cp[2]);
					DrawLine(cp[2], cp[3]);
					DrawLine(cp[3], cp[0]);

					if ( mod.showcells )
					{
						//Matrix4x4 tm = mod.transform.localToWorldMatrix;

						Vector3 p;

						for ( int x = xs; x < xd; x++ )
						{
							p.x = x * mod.flow.spacing.x + adjx;
							for ( int y = ys; y < yd; y++ )
							{
								p.y = y * mod.flow.spacing.y + adjy;
								for ( int z = zs; z <= zd; z++ )
								{
									p.z = z * mod.flow.spacing.z + adjz;

									Vector3 v1 = mod.SampleVel(x, y, z) * mod.velScl;
									Vector3 v2 = mod.SampleVel(x + 1, y, z) * mod.velScl;
									Vector3 v3 = mod.SampleVel(x + 1, y + 1, z) * mod.velScl;
									Vector3 v4 = mod.SampleVel(x, y + 1, z) * mod.velScl;

									Vector3 p1 = new Vector3(p.x, p.y, p.z);
									Vector3 p2 = new Vector3(p.x + mod.flow.spacing.x, p.y, p.z);
									Vector3 p3 = new Vector3(p.x + mod.flow.spacing.x, p.y + mod.flow.spacing.y, p.z);
									Vector3 p4 = new Vector3(p.x, p.y + mod.flow.spacing.y, p.z);

									DrawCell(mod, v1, v2, v3, v4, p1, p2, p3, p4, tm);
								}
							}
						}
					}
					break;
			}
		}

		GL.PopMatrix();

		if ( showvel && (mod.flow.vel.Count > 0 || mod.flow.optvel.Count > 0) )
		{
			if ( mod.skip < 1 )
				mod.skip = 1;

			Color col = Color.white;	//mod.GetCol(len * mod.velScl);
			col.a = mod.velAlpha;

			if ( !mod.showvelmag )
			{
				// Color shows direction
				for ( int x = xs; x <= xd; x += mod.skip )
				{
					for ( int y = ys; y <= yd; y += mod.skip )
					{
						for ( int z = zs; z <= zd; z += mod.skip )
						{
							Vector3	p;

							p.x = x * mod.flow.spacing.x + adjx;
							p.y = y * mod.flow.spacing.y + adjy;
							p.z = z * mod.flow.spacing.z + adjz;

							Vector3 vel = mod.SampleVel(x, y, z);

							if ( vel.sqrMagnitude > velthreshold )
							{
								//float len = vel.magnitude;

								col.r = Mathf.Abs(vel.x * mod.velScl);
								col.g = Mathf.Abs(vel.y * mod.velScl);
								col.b = Mathf.Abs(vel.z * mod.velScl);

								Handles.color = col;

								Vector3 p1 = p + (vel * mod.velLen);	//scale);
								DrawLine(p, p1);
							}
						}
					}
				}
			}
			else
			{
				float rng = 1.0f / (mod.maxvel - mod.minvel);

				// color shows magnitude
				for ( int x = xs; x <= xd; x += mod.skip )
				{
					for ( int y = ys; y <= yd; y += mod.skip )
					{
						for ( int z = zs; z <= zd; z += mod.skip )
						{
							Vector3	p;

							p.x = x * mod.flow.spacing.x + adjx;
							p.y = y * mod.flow.spacing.y + adjy;
							p.z = z * mod.flow.spacing.z + adjz;

							Vector3 vel = mod.SampleVel(x, y, z);

							if ( vel.sqrMagnitude > velthreshold )
							{
								float len = vel.magnitude * mod.velScl;
								col = mod.GetCol(len * rng);

								Handles.color = col;

								Vector3 p1 = p + (vel * mod.velLen);	//scale);
								DrawLine(p, p1);
							}
						}
					}
				}
			}
		}

		if ( showsmoke && mod.flow.smoke.Count > 0 )
		{
			GUIStyle style = new GUIStyle();

			for ( int x = xs; x < xd; x++ )
			{
				for ( int y = ys; y < yd; y++ )
				{
					for ( int z = zs; z < zd; z++ )
					{
						Vector3	p;

						p.x = x * mod.flow.spacing.x;
						p.y = y * mod.flow.spacing.y;
						p.z = z * mod.flow.spacing.z;

						float	s = mod.flow.smoke[(x * mod.flow.gridDim2[2] * mod.flow.gridDim2[1]) + (z * mod.flow.gridDim2[1]) + y];
						
						if ( s > smokethreshhold )
						{
							Color col = mod.GetCol(s);
							col.a = mod.smokeAlpha;

							style.normal.textColor = col;

							Handles.Label(p, "" + s.ToString("0.00"), style);
						}
					}
				}
			}
		}

		Vector3 pos = mod.ribpos;
		Vector3 vel1 = mod.GetGridVel(pos, ref inbounds);

		if ( inbounds )
		{
			float len = vel1.magnitude;

			Color col = mod.GetCol(len);
			Handles.color = col;

			Vector3 p1 = pos + vel1;	// * scale);
			DrawLine(pos, p1);
		}

		if ( mod.showRibbon )
			DisplayRibbons(mod);

		Handles.matrix = Matrix4x4.identity;
	}

	static public void DrawLine(Vector3 p1, Vector3 p2)
	{
		Handles.DrawLine(p1, p2);
	}

	void DrawLine(Vector3 p1, Vector3 p2, Color c1, Color c2)
	{
		Handles.color = c1;
		Handles.DrawLine(p1, p2);
	}

	void RibbonPhysics(MegaFlow mod, float t)
	{
		if ( !mod.showRibbon || mod.flow == null )
			return;

		if ( mod.Dt < 0.002f )
			mod.Dt = 0.002f;

		float dt = mod.Dt;

		mod.Prepare();
		mod.calculated = 0;
		mod.calcTime = Time.realtimeSinceStartup;
		mod.ribbons.Clear();

		bool inbounds = false;

		float p = mod.Density;
		float A = mod.Area;
		float Re = mod.Reynolds;
		float mass = mod.Mass;
		int linestep = mod.LineStep;
		float zs = mod.SizeZ;
		int zst = mod.StepZ;

		if ( zst == 0 )
			return;

		float ys = mod.SizeY;
		int yst = mod.StepY;

		if ( yst == 0 )
			return;

		float coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f); 

		Vector3	Fgrv = mod.Gravity * mass;

		for ( int zi = 0; zi < zst; zi++ )
		{
			float z;

			if ( zst == 1 )
				z = 0.0f;
			else
				z = (((float)zi / (float)(zst - 1)) - 0.5f) * 2.0f * zs;

			for ( int yi = 0; yi < yst; yi++ )
			{
				Ribbon ribbon = new Ribbon();

				float y;

				if ( yst == 1 )
					y = 0.0f;
				else
					y = (((float)yi / (float)(yst - 1)) - 0.5f) * 2.0f * ys;

				Vector3	pos = mod.ribpos;
				pos.z += z;
				pos.y += y;
				Vector3	vel = mod.GetGridVel(pos, ref inbounds);	// * scale;

				mod.calculated++;

				if ( inbounds )
				{
					float U = 1.0f;

					float	duration = mod.Duration;

					Vector3	Fshape = Vector3.zero;	// Random force due to particle shape

					int linecount = 0;
					while ( duration > 0.0f )
					{
						mod.calculated++;
						Vector3 airvel = mod.GetGridVel(pos, ref inbounds);	// * scale;

						if ( !inbounds )
						{
							airvel = new Vector3(mod.Scale, 0.0f, 0.0f);
							break;
						}

						Vector3 tvel = airvel - vel;
						U = tvel.magnitude;	//tvel.sqrMagnitude;

						float df = coef * U;

						Vector3 dir = tvel.normalized;
						Vector3 Fdrag = dir * df;

						Vector3 Fp = Fdrag + Fshape + Fgrv;
						Vector3	acc = Fp / mass;
						vel += acc * dt;
						pos += vel * dt;

						linecount--;

						if ( linecount < 0 )
						{
							ribbon.points.Add(pos);
							Color col = mod.GetCol(vel.magnitude * mod.ribbonscale);
							col.a = mod.ribbonAlpha;
							ribbon.cols.Add(col);

							linecount = linestep;
						}
						duration -= dt;
					}

					if ( linecount != linestep )
					{
						ribbon.points.Add(pos);
						Color col = mod.GetCol(vel.magnitude * mod.ribbonscale);
						col.a = mod.ribbonAlpha;
						ribbon.cols.Add(col);
					}
				}

				mod.ribbons.Add(ribbon);
			}
		}

		mod.calcTime = Time.realtimeSinceStartup - mod.calcTime;
	}

	static void DisplayRibbons(MegaFlow mod)
	{
		for ( int i = 0; i < mod.ribbons.Count; i++ )
		{
			Ribbon ribbon = mod.ribbons[i];
			for ( int p = 0; p < ribbon.points.Count - 1; p++ )
			{
				Handles.color = ribbon.cols[p];
				DrawLine(ribbon.points[p], ribbon.points[p + 1]);
			}
		}
	}
}