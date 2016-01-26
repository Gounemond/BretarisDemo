
using UnityEngine;
using System.Collections.Generic;

public enum MegaFlowModType
{
	VelChange,
}

[System.Serializable]
public class MegaFlowModifier
{
	public Collider			obj;
	public MegaFlowModType	type	= MegaFlowModType.VelChange;
	public float			amount	= 0.0f;
	public bool				include	= true;
	public bool				show	= true;
}

[System.Serializable]
public class MegaFlowSpline
{
	public MegaShape		shape;
	public int				curve;
	public float			velocity	= 1.0f;
	public float			weight		= 1.0f;
	public float			falloffdist	= 1.0f;
	public AnimationCurve	falloffcrv	= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 0));
	public AnimationCurve	velcrv		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public AnimationCurve	distcrv		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public MegaFlowMode		mode		= MegaFlowMode.Attract;
	public bool				include		= true;
	public int				visrings	= 20;
	public float			ringalpha	= 0.75f;
	public bool				show		= true;
}

public struct MegaFlowContrib
{
	public float			dist;
	public MegaFlowSpline	src;
	public Vector3			vel;
	public Vector3			delta;
	public float			alpha;
	public float			fdist;
}

public enum MegaFlowMode
{
	Attract,
	Repulse,
	Flow,
}

[AddComponentMenu("MegaFlow/Create From Splines")]
[ExecuteInEditMode]
public class MegaFlowCreateFromSplines : MonoBehaviour
{
	public Vector3				size			= Vector3.one;
	public bool					square			= true;
	public int					gridx			= 32;
	public int					gridy			= 32;
	public int					gridz			= 32;
	public List<MegaFlowSpline>	splines			= new List<MegaFlowSpline>();
	public MegaFlow				flow;
	public int					frame			= 0;
	public Vector3				cellsize		= Vector3.one;
	Vector3[]					cells;
	public float				velocity		= 1.0f;
	public float				vellen			= 1.0f;
	public MegaFlowMode			emtyspacemode	= MegaFlowMode.Attract;
	public bool					shownotsel		= false;
	public Gradient				velcols;
	public float				minvel			= 0.0f;
	public float				maxvel			= 10.0f;
	public Vector3				startval		= Vector3.zero;

	public List<MegaFlowModifier>		modifiers		= new List<MegaFlowModifier>();

	// Visualization
	public MegaFlow				preview;

	public Texture3D			texture;
	public float				texturescale	= 1.0f;

	public bool					showsplines		= false;
	public bool					showmods		= false;
	public Vector2				splinepos		= Vector2.zero;
	public Vector2				modpos			= Vector2.zero;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=5904");
	}

	static Color[] Cols = {
		new Color(0.0f, 0.0f, 1.0f, 1.0f),
		new Color(0.0f, 1.0f, 1.0f),
		new Color(0.0f, 1.0f, 0.0f),
		new Color(1.0f, 1.0f, 0.0f),
		new Color(1.0f, 0.0f, 0.0f),
	};

	void Awake()
	{
		if ( velcols == null )
		{
			velcols = new Gradient();

			GradientColorKey[] keys = new GradientColorKey[5];

			for ( int i = 0; i < 5; i++ )
			{
				keys[i].color = Cols[i];
				keys[i].time = (float)i / 4.0f;
			}

			GradientAlphaKey[] akeys = new GradientAlphaKey[2];
			akeys[0].alpha = 1.0f;
			akeys[0].time = 0.0f;
			akeys[1].alpha = 1.0f;
			akeys[1].time = 0.0f;

			velcols.SetKeys(keys, akeys);
		}
	}

	public void CreateFlow()
	{
		int gx = gridx;
		int gy = gridy;
		int gz = gridz;

		if ( square )
		{
			// find major axis
			int axis = 0;
			if ( Mathf.Abs(size.x) > Mathf.Abs(size.y) )
			{
				if ( Mathf.Abs(size.x) > Mathf.Abs(size.z) )
					axis = 0;
				else
					axis = 2;
			}
			else
			{
				if ( Mathf.Abs(size.y) > Mathf.Abs(size.z) )
					axis = 1;
				else
					axis = 2;
			}

			float csize = size[axis] / gridx;

			cellsize = new Vector3(csize, csize, csize);
		}
		else
		{
			cellsize.x = size.x / gridx;
			cellsize.y = size.y / gridy;
			cellsize.z = size.z / gridz;
		}

		gx = (int)(size.x / cellsize.x);
		gy = (int)(size.y / cellsize.y);
		gz = (int)(size.z / cellsize.z);

		cells = new Vector3[gx * gy * gz];

		for ( int i = 0; i < cells.Length; i++ )
			cells[i] = startval;

		Vector3 pos = Vector3.zero;

		Vector3 half = cellsize * 0.5f;
		Vector3 tan = Vector3.zero;
		int kn = 0;
		float alpha = 0.0f;

		List<MegaFlowContrib>	contrib = new List<MegaFlowContrib>();

		for ( int z = 0; z < gz; z++ )
		{
			pos.z = (z * cellsize.z) + half.z;

			for ( int y = 0; y < gy; y++ )
			{
				pos.y = (y * cellsize.y) + half.y;

				for ( int x = 0; x < gx; x++ )
				{
					pos.x = (x * cellsize.x) + half.x;

					contrib.Clear();
					float nearest = float.MaxValue;
					Vector3 neardelta = Vector3.zero;

					for ( int i = 0; i < splines.Count; i++ )
					{
						MegaFlowSpline fs = splines[i];

						if ( fs.include )
						{
							Vector3 np = fs.shape.FindNearestPointWorld(pos, 5, ref kn, ref tan, ref alpha);

							Vector3 delta = np - pos;

							float dist = delta.magnitude;

							if ( dist < nearest )
							{
								nearest = dist;
								neardelta = delta;
							}

							if ( dist < fs.falloffdist )
							{
								MegaFlowContrib con = new MegaFlowContrib();
								con.src = fs;
								con.dist = dist;
								con.vel = tan * fs.velocity;
								con.delta = delta.normalized;
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
							float a = contrib[c].dist / contrib[c].src.falloffdist;
							float lerp = contrib[c].src.falloffcrv.Evaluate(a);

							switch ( contrib[c].src.mode )
							{
								case MegaFlowMode.Attract:
									vel += Vector3.Lerp(contrib[c].delta, contrib[c].vel, lerp) * velocity * (contrib[c].src.weight / tweight);
									break;

								case MegaFlowMode.Repulse:
									vel += Vector3.Lerp(-contrib[c].delta, contrib[c].vel, lerp) * velocity * (contrib[c].src.weight / tweight);
									break;

								case MegaFlowMode.Flow:
									vel += Vector3.Lerp(Vector3.zero, contrib[c].vel, lerp) * velocity * (contrib[c].src.weight / tweight);
									break;
							}
						}

						cells[(x * gz * gy) + (z * gy) + y] = vel;
					}
					else
					{
						cells[(x * gz * gy) + (z * gy) + y] = neardelta.normalized * velocity;
					}
				}
			}
		}

		if ( flow )
		{
			MegaFlowFrame newf = ScriptableObject.CreateInstance<MegaFlowFrame>();

			newf.gridDim2[0] = gx;
			newf.gridDim2[1] = gy;
			newf.gridDim2[2] = gz;

			newf.size = size;
			newf.gsize = newf.size;

			// griddim should have a name change
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

	public void DrawEdge(Vector3 p1, Vector3 p2, int steps)
	{
		Vector3 last = p1;
		Vector3 pos = Vector3.zero;
		for ( int i = 1; i <= steps; i++ )
		{
			pos = p1 + ((p2 - p1) * ((float)i / (float)steps));

			if ( (i & 1) == 0 )
				Gizmos.color = Color.white;
			else
				Gizmos.color = Color.black;

			Gizmos.DrawLine(last, pos);
			last = pos;
		}
		Gizmos.color = Color.white;
	}

	public void DrawGizmo()
	{
		Matrix4x4 offtm = Matrix4x4.TRS(-size * 0.5f, Quaternion.identity, Vector3.one);

		Gizmos.matrix = transform.localToWorldMatrix * offtm;

		int gx = gridx;
		int gy = gridy;
		int gz = gridz;

		if ( square )
		{
			// find major axis
			int axis = 0;
			if ( Mathf.Abs(size.x) > Mathf.Abs(size.y) )
			{
				if ( Mathf.Abs(size.x) > Mathf.Abs(size.z) )
					axis = 0;
				else
					axis = 2;
			}
			else
			{
				if ( Mathf.Abs(size.y) > Mathf.Abs(size.z) )
					axis = 1;
				else
					axis = 2;
			}

			float csize = size[axis] / gridx;

			cellsize = new Vector3(csize, csize, csize);
		}
		else
		{
			cellsize.x = size.x / gridx;
			cellsize.y = size.y / gridy;
			cellsize.z = size.z / gridz;
		}

		gx = (int)(size.x / cellsize.x);
		gy = (int)(size.y / cellsize.y);
		gz = (int)(size.z / cellsize.z);

		Vector3 min = Vector3.zero;
		Vector3 max = size;

		Vector3[] corners = new Vector3[8];
		corners[0] = new Vector3(min.x, min.y, min.z);
		corners[1] = new Vector3(min.x, max.y, min.z);
		corners[2] = new Vector3(max.x, max.y, min.z);
		corners[3] = new Vector3(max.x, min.y, min.z);

		corners[4] = new Vector3(min.x, min.y, max.z);
		corners[5] = new Vector3(min.x, max.y, max.z);
		corners[6] = new Vector3(max.x, max.y, max.z);
		corners[7] = new Vector3(max.x, min.y, max.z);

		DrawEdge(corners[2], corners[3], gy);
		DrawEdge(corners[3], corners[0], gx);

		DrawEdge(corners[4], corners[5], gy);
		DrawEdge(corners[5], corners[6], gx);
		DrawEdge(corners[6], corners[7], gy);
		DrawEdge(corners[7], corners[4], gx);

		DrawEdge(corners[0], corners[4], gz);
		DrawEdge(corners[0], corners[1], gy);
		DrawEdge(corners[1], corners[5], gz);
		DrawEdge(corners[2], corners[6], gz);
		DrawEdge(corners[3], corners[7], gz);
		DrawEdge(corners[1], corners[2], gx);

		Gizmos.matrix = Matrix4x4.identity;
	}

	public Color GetCol(float alpha)
	{
		return velcols.Evaluate(alpha);
	}
}