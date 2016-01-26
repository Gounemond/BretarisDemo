using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class Ribbon
{
	public List<Vector3>	points = new List<Vector3>();
	public List<Color>		cols = new List<Color>();
}

// I dont think real flow has support for vector fields so may not need here, only in particle cache systems
public enum MegaFlowDataSource
{
	FumeFX,
	MegaFlow,
	FGA,
	RealFlow,
}

public enum MegaFlowParticleFileType
{
	RealFlow,
}

public enum MegaFlowAxis
{
	X,
	Y,
	Z,
}

[AddComponentMenu("MegaFlow/Source")]
[ExecuteInEditMode]
public class MegaFlow : MonoBehaviour
{
	static public float AXIS_SIZE = 1.0f;

	static Color[] Cols = {
		new Color(0.0f, 0.0f, 1.0f, 1.0f),
		new Color(0.0f, 1.0f, 1.0f),
		new Color(0.0f, 1.0f, 0.0f),
		new Color(1.0f, 1.0f, 0.0f),
		new Color(1.0f, 0.0f, 0.0f),
	};

	public Gradient				velcols;	// = new Gradient();
	public bool					showvelmag		= true;
	public bool					showcellmag		= false;
	public float				minvel			= 0.0f;
	public float				maxvel			= 1.0f;
	public string				file			= "";
	public MegaFlowFrame		flow;
	public int					frame			= 0;
	public float				smokeThreshold	= 1.0f;
	public float				velThreshold	= 0.0f;
	public bool					showVel			= true;
	public bool					showSmoke		= true;
	public bool					shownotselected	= true;
	public bool					Slice			= true;
	public bool					showgrid		= false;
	public int					Position		= 0;
	public int					Thickness		= 1;
	public Vector3				ribpos			= Vector3.zero;
	public float				Dt				= 0.01f;
	public float				Density			= 1.22f;
	public float				Area			= 0.01f;
	public float				Reynolds		= 20.0f;
	public float				Mass			= 0.001f;
	public int					LineStep		= 1;
	public float				SizeZ			= 1.0f;
	public int					StepZ			= 1;
	public float				SizeY			= 1.0f;
	public int					StepY			= 1;
	public Vector3				Gravity			= Vector3.zero;
	public float				Duration		= 12.0f;
	public MegaFlowAxis			Plane			= MegaFlowAxis.Y;
	public float				Scale			= 1.0f;
	public bool					showRibbon		= false;
	public Color				gridColor		= Color.white;
	public Color				gridColor1		= Color.white;
	public float				ribbonAlpha		= 1.0f;
	public float				velAlpha		= 1.0f;
	public float				velLen			= 1.0f;
	public float				velScl			= 1.0f;
	public float				smokeAlpha		= 1.0f;
	public int					calculated		= 0;
	public float				calcTime		= 0.0f;
	public List<Ribbon>			ribbons			= new List<Ribbon>();
	public float				floor			= 0.0f;
	public bool					showcells		= false;
	public List<MegaFlowFrame>	frames			= new List<MegaFlowFrame>();
	public MegaFlowDataSource	datasource		= MegaFlowDataSource.MegaFlow;
	public bool					sequence		= false;
	public int					firstframe		= 0;
	public int					lastframe		= 1;
	public int					framestep		= 1;
	public int					skip			= 0;
	public bool					animate			= false;
	public float				animspeed		= 1.0f;
	public float				animtime		= 1.0f;
	public float				animlength		= 5.0f;
	public float				ribbonscale		= 1.0f;
	public float				cellalpha		= 0.5f;
	public Vector3				handlepos		= Vector3.zero;
	public Transform			fluidPos;	
	public int					memoryuse		= 0;
	public bool					optimized		= false;
	public bool					showdisplayparams = false;
	public bool					showribbonparams = false;
	public bool					showanimparams	= false;
	public bool					textureoptions	= false;
	public int					texturewidth	= 128;
	public int					textureheight	= 128;
	public int					texturedepth	= 128;
	public bool					showadvparams	= false;
	public int					samplex			= 128;
	public int					sampley			= 128;
	public int					samplez			= 128;
	public int					decform			= 0;
	public string				namesplit		= "";

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=5894");
	}

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

	void Start()
	{
		Prepare();
	}

	public void OptimizeData()
	{
		for ( int i = 0; i < frames.Count; i++ )
		{
			MegaFlowFrame frame = frames[i];

			if ( frame.optvel == null || frame.optvel.Count == 0 )
				frame.Optimize();
		}

		CalcMemUse();
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.grey;
		Gizmos.DrawIcon(transform.position, "MegaFlowIcon.png", true);
	}

	public void Animate()
	{
		if ( animate && frames.Count > 1 )
		{
			animtime += Time.deltaTime * animspeed;

			if ( animtime > animlength )
				animtime -= animlength;

			if ( animtime < 0.0f )
				animtime += animlength;

			frame = (int)((animtime / animlength) * frames.Count);
		}
	}

	void Update()
	{
		if ( frames.Count > 0 )
		{
			Animate();
			frame = Mathf.Clamp(frame, 0, frames.Count - 1);
			flow = frames[frame];

			if ( flow.SampleVel == null )
			{
				if ( flow.optimized )
				{
					flow.SampleVel = flow.SampleVelOpt;
					flow.GetGridVel = flow.GetGridVelOpt;
				}
				else
				{
					flow.SampleVel = flow.SampleVelFloat;
					flow.GetGridVel = flow.GetGridVelFloat;
				}
			}
		}
	}

	public void SetFrame(int f)
	{
		if ( frames.Count > 0 )
		{
			f = Mathf.Clamp(f, 0, frames.Count - 1);
			flow = frames[f];
			frame = f;
		}
		else
		{
			frame = 0;
			flow = null;
		}
	}

	public void AddFrame(MegaFlowFrame frame)
	{
		if ( frame )
		{
			if ( frames == null )
				frames = new List<MegaFlowFrame>();

			frames.Add(frame);
			if ( flow == null )
				flow = frame;
		}
	}

	public void DestroyFrame(int f)
	{
		if ( f >= 0 && f < frames.Count )
		{
			MegaFlowFrame frame = frames[f];
			frames.RemoveAt(f);

			if ( flow == frame )
			{
				if ( frames.Count > 0 )
					flow = frames[0];
				else
					flow = null;
			}

			if ( Application.isEditor )
				DestroyImmediate(frame);
			else
				Destroy(frame);
		}
	}

	public void DestroyFrames()
	{
		flow = null;

		for ( int i = 0; i < frames.Count; i++ )
		{
			MegaFlowFrame frame = frames[i];

			if ( Application.isEditor )
				DestroyImmediate(frame);
			else
				Destroy(frame);
		}

		frames.Clear();
		GC.Collect();
	}

	public void Prepare()
	{
		Matrix4x4 wltm = transform.worldToLocalMatrix;
		for ( int i = 0; i < frames.Count; i++ )
			frames[i].Prepare(wltm);
	}

	public void CalcMemUse()
	{
		memoryuse = 0;

		for ( int i = 0; i < frames.Count; i++ )
		{
			MegaFlowFrame frame = frames[i];

			if ( frame.optimized )
				frame.memory = frame.gridDim2[0] * frame.gridDim2[1] * frame.gridDim2[2] * 3;
			else
				frame.memory = frame.gridDim2[0] * frame.gridDim2[1] * frame.gridDim2[2] * 12;

			memoryuse += frame.memory;
		}
	}

	Matrix4x4	gettm = Matrix4x4.identity;
	Matrix4x4	invgettm = Matrix4x4.identity;

	public Color GetCol(float alpha)
	{
		return velcols.Evaluate(alpha);
	}

	public void SetMatrix()
	{
		Matrix4x4 offtm = Matrix4x4.TRS((flow.size * 0.5f) + flow.offset, Quaternion.identity, Vector3.one);
		gettm = offtm * transform.worldToLocalMatrix;
		invgettm = gettm.inverse;
	}

	public Vector3 GetGridVelWorld(Vector3 pos, ref bool inbounds)
	{
		return invgettm.MultiplyVector(flow.GetGridVel(gettm.MultiplyPoint3x4(pos), ref inbounds) * Scale);
	}

	public Vector3 GetGridVel(Vector3 pos, ref bool inbounds)
	{
		return flow.GetGridVel(pos, ref inbounds) * Scale;
	}

	public Vector3 SampleVel(int x, int y, int z)
	{
		return flow.SampleVel(x, y, z) * Scale;
	}

	static public void DrawEdge(Vector3 p1, Vector3 p2)
	{
		Vector3 last = p1;
		Vector3 pos = Vector3.zero;
		for ( int i = 1; i <= 10; i++ )
		{
			pos = p1 + ((p2 - p1) * ((float)i / (float)10));

			if ( (i & 1) == 0 )
				Gizmos.color = Color.green;
			else
				Gizmos.color = Color.yellow;

			Gizmos.DrawLine(last, pos);
			last = pos;
		}
		Gizmos.color = Color.white;
	}

	static public void DrawBox(Vector3 min, Vector3 max)
	{
		Vector3[] corners = new Vector3[8];
		corners[0] = new Vector3(min.x, min.y, min.z);
		corners[1] = new Vector3(min.x, max.y, min.z);
		corners[2] = new Vector3(max.x, max.y, min.z);
		corners[3] = new Vector3(max.x, min.y, min.z);

		corners[4] = new Vector3(min.x, min.y, max.z);
		corners[5] = new Vector3(min.x, max.y, max.z);
		corners[6] = new Vector3(max.x, max.y, max.z);
		corners[7] = new Vector3(max.x, min.y, max.z);

		DrawEdge(corners[0], corners[1]);
		DrawEdge(corners[1], corners[2]);
		DrawEdge(corners[2], corners[3]);
		DrawEdge(corners[3], corners[0]);

		DrawEdge(corners[4], corners[5]);
		DrawEdge(corners[5], corners[6]);
		DrawEdge(corners[6], corners[7]);
		DrawEdge(corners[7], corners[4]);

		DrawEdge(corners[0], corners[4]);
		DrawEdge(corners[1], corners[5]);
		DrawEdge(corners[2], corners[6]);
		DrawEdge(corners[3], corners[7]);
	}

	public void DrawGizmo()
	{
		if ( flow )
		{
			//Matrix4x4 offtm = Matrix4x4.TRS(-flow.size * 0.5f, Quaternion.identity, Vector3.one);
			Matrix4x4 offtm = Matrix4x4.TRS((-flow.size * 0.5f) + flow.offset, Quaternion.identity, Vector3.one);

			Gizmos.matrix = transform.localToWorldMatrix * offtm;

			Vector3 min = Vector3.zero;
			Vector3 max = flow.size;
			DrawBox(min, max);
			
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	public Texture3D Create3DTexture(int width, int height, int depth, int frame)
	{
		MegaFlowFrame f = frames[frame];

		if ( width == 0 || height == 0 || depth == 0 )
			return null;

		width = Mathf.ClosestPowerOfTwo(width);
		height = Mathf.ClosestPowerOfTwo(height);
		depth = Mathf.ClosestPowerOfTwo(depth);

		Texture3D tex = new Texture3D(width, height, depth, TextureFormat.RGB24, false);
		tex.wrapMode = TextureWrapMode.Repeat;
		tex.anisoLevel = 0;

		Color[] cols = new Color[width * height * depth];

		float max = 0.0f;

		for ( int i = 0; i < f.vel.Count; i++ )
		{
			float m = f.vel[i].sqrMagnitude;

			if ( m > max )
				max = m;
		}

		float len = Mathf.Sqrt(max);
		Vector3 p;
		Color c = Color.white;
		bool inbounds = false;
#if false
		for ( int x = 0; x < width; x++ )
		{
			p.x = ((float)x / (float)width) * f.size.x;

			for ( int y = 0; y < height; y++ )
			{
				p.y = ((float)y / (float)height) * f.size.y;

				for ( int z = 0; z < depth; z++ )
				{
					p.z = ((float)z / (float)depth) * f.size.z;

					Vector3 vel = f.GetGridVel(p, ref inbounds);
					vel /= len;
					c.r = (vel.x * 0.5f) + 0.5f;
					c.g = (vel.y * 0.5f) + 0.5f;
					c.b = (vel.z * 0.5f) + 0.5f;
					cols[(x * depth * height) + (z * height) + y] = c;
				}
			}
		}
#endif

		int ix = 0;
		for ( int z = 0; z < depth; z++ )
		{
			p.z = ((float)z / (float)depth) * f.size.z;

			for ( int y = 0; y < height; y++ )
			{
				p.y = ((float)y / (float)height) * f.size.y;

				for ( int x = 0; x < width; x++ )
				{
					p.x = ((float)x / (float)width) * f.size.x;

					Vector3 vel = f.GetGridVel(p, ref inbounds);
					vel /= len;
					c.r = (vel.x * 0.5f) + 0.5f;
					c.g = (vel.y * 0.5f) + 0.5f;
					c.b = (vel.z * 0.5f) + 0.5f;
					cols[ix++] = c;
				}
			}
		}

		tex.SetPixels(cols);
		tex.Apply();
		return tex;
	}

	public void NormalizeFrame(int f)
	{
		MegaFlowFrame	fr = frames[f];

		float max = 0.0f;

		for ( int i = 0; i < fr.vel.Count; i++ )
		{
			float vs = fr.vel[i].magnitude;

			if ( vs > max )
				max = vs;
		}

		for ( int i = 0; i < fr.vel.Count; i++ )
			fr.vel[i] = fr.vel[i] / max;
	}

	public void Resample(MegaFlowFrame sf, int gx, int gy, int gz)
	{
		MegaFlowFrame newf = ScriptableObject.CreateInstance<MegaFlowFrame>();

		newf.gridDim2[0] = gx;
		newf.gridDim2[1] = gy;
		newf.gridDim2[2] = gz;

		newf.size = sf.size;
		newf.gsize = newf.size;

		// griddim should have a name change
		newf.spacing.x = newf.size.x / newf.gridDim2[0];
		newf.spacing.y = newf.size.y / newf.gridDim2[1];
		newf.spacing.z = newf.size.z / newf.gridDim2[2];
		newf.oos.x = 1.0f / newf.spacing.x;
		newf.oos.y = 1.0f / newf.spacing.y;
		newf.oos.z = 1.0f / newf.spacing.z;

		float adjx = sf.spacing.x * 0.5f;
		float adjy = sf.spacing.y * 0.5f;
		float adjz = sf.spacing.z * 0.5f;

		bool inbounds = false;

		Prepare();

		Vector3[] cells = new Vector3[gx * gy * gz];

		Vector3 p;
		for ( int x = 0; x < gx; x++ )
		{
			p.x = (((float)x / (float)gx) * sf.size.x) + adjx;
			for ( int y = 0; y < gy; y++ )
			{
				p.y = (((float)y / (float)gy) * sf.size.y) + adjy;
				for ( int z = 0; z < gz; z++ )
				{
					p.z = (((float)z / (float)gz) * sf.size.z) + adjz;

					cells[(x * gz * gy) + (z * gy) + y] = sf.GetGridVel(p, ref inbounds);
				}
			}
		}

		newf.vel.AddRange(cells);

		AddFrame(newf);
	}
}
