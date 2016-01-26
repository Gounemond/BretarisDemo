
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class MegaFlowVector
{
	public byte x;
	public byte y;
	public byte z;
}

public class MegaFlowFrame : ScriptableObject
{
	public int[]			gridDim		= new int[3];
	public int[]			gridDim1	= new int[3];
	public int[]			gridDim2	= new int[3];
	public int				flags;
	public int				framenumber = -1;
	public bool				somebool;
	public float			fval;
	public float			fval1;
	public Vector3			spacing;
	public Vector3			size;
	public Vector3			gsize;
	public Vector3			oos;
	public Vector3			offset		= Vector3.zero;
	public int				memory		= 0;
	public List<int>		grid		= new List<int>();
	public List<Vector3>	vel			= new List<Vector3>();
	public List<Vector3>	force		= new List<Vector3>();
	public List<float>		smoke		= new List<float>();
	public List<Vector3>	pos			= new List<Vector3>();
	public List<float>		psize		= new List<float>();
	public List<Color>		color		= new List<Color>();
	public List<float>		rot			= new List<float>();
	public List<MegaFlowVector>	optvel = new List<MegaFlowVector>();
	public Matrix4x4		tm;
	public Matrix4x4		invtm;

	public GetVel GetGridVel;
	public delegate Vector3 GetVel(Vector3 pos, ref bool inbounds);
	public Sample SampleVel;
	public delegate Vector3 Sample(int x, int y, int z);

	public void DebugFlow(float st, float vt)
	{
		Debug.Log("gridDim " + gridDim[0] + " " + gridDim[1] + " " + gridDim[2]);
		Debug.Log("gridDim1 " + gridDim1[0] + " " + gridDim1[1] + " " + gridDim1[2]);
		Debug.Log("gridDim2 " + gridDim2[0] + " " + gridDim2[1] + " " + gridDim2[2]);
		Debug.Log("flags " + flags);
		Debug.Log("Frame " + framenumber);
		Debug.Log("fvals " + fval + " " + fval1);
		Debug.Log("Spacing " + spacing);
		Debug.Log("Size " + size.x + " " + size.y + " " + size.z);
		Debug.Log("gsize " + gsize.x + " " + gsize.y + " " + gsize.z);

		if ( force.Count > 0 )
			Debug.Log("Have Force Data");

		if ( smoke.Count > 0 )
		{
			Debug.Log("Have Smoke Data");
#if false
			for ( int x = 0; x < gridDim[0]; x++ )
			{
				for ( int y = 0; y < gridDim[1]; y++ )
				{
					for ( int z = 0; z < gridDim[2]; z++ )
					{
						float p = smoke[(z * gridDim[1] * gridDim[0]) + (y * gridDim[0]) + x];
						if ( Mathf.Abs(p) > st )
							Debug.Log("Smoke[" + x + " " + y + " " + z + "] " + (z * gridDim[1] * gridDim[0]) + (y * gridDim[0]) + x + " " + p);
					}
				}
			}
#endif
		}

		// Change arrays to Vectors
		if ( vel.Count > 0 )
		{
			Debug.Log("Have Vel Data");
#if false
			for ( int x = 0; x < gridDim2[0]; x++ )
			{
				for ( int y = 0; y < gridDim2[1]; y++ )
				{
					for ( int z = 0; z < gridDim2[2]; z++ )
					{
						Vector3 p = vel[(z * gridDim2[1] * gridDim2[0]) + (y * gridDim2[0]) + x];

						if ( p.magnitude > vt )
							Debug.Log("Grid[" + x + "," + y + "," + z + "] " + p.x + " " + p.y + " " + p.z);
					}
				}
			}
#endif
		}
	}

	public Vector3 GetGridVelWorld(Vector3 pos, ref bool inbounds)
	{
		return invtm.MultiplyVector(GetGridVel(tm.MultiplyPoint3x4(pos), ref inbounds));
	}

	public void Prepare(Matrix4x4 wltm)
	{
		if ( optimized )
		{
			GetGridVel = GetGridVelOpt;
			SampleVel = SampleVelOpt;
		}
		else
		{
			GetGridVel = GetGridVelFloat;
			SampleVel = SampleVelFloat;
		}

		Matrix4x4 offtm = Matrix4x4.TRS((size * 0.5f) + offset, Quaternion.identity, Vector3.one);
		tm = offtm * wltm;
		invtm = tm.inverse;
	}

	public void Init()
	{
		GetGridVel = GetGridVelFloat;
		SampleVel = SampleVelFloat;

		smoke.Clear();
		vel.Clear();
		force.Clear();
		grid.Clear();
		pos.Clear();
		psize.Clear();
		color.Clear();
		rot.Clear();
		optvel.Clear();
	}

	public Vector3 GetGridVelFloat(Vector3 pos, ref bool inbounds)
	{
		int xi = (int)(pos.x * oos.x);
		int yi = (int)(pos.y * oos.y);
		int zi = (int)(pos.z * oos.z);

		if ( xi < 0 || xi >= gridDim2[0] - 1 )
		{
			inbounds = false;
			return Vector3.zero;
		}

		if ( yi < 0 || yi >= gridDim2[1] - 1 )
		{
			inbounds = false;
			return Vector3.zero;
		}

		if ( zi < 0 || zi >= gridDim2[2] - 1 )
		{
			inbounds = false;
			return Vector3.zero;
		}

		inbounds = true;

		int xi1 = (xi + 1) * gridDim2[2] * gridDim2[1];
		int yi1 = yi + 1;
		int zi1 = (zi + 1) * gridDim2[1];

		float xr = Mathf.Abs((pos.x - (xi * spacing.x)) * oos.x);
		float yr = Mathf.Abs((pos.y - (yi * spacing.y)) * oos.y);
		float zr = Mathf.Abs((pos.z - (zi * spacing.z)) * oos.z);

		xi *= gridDim2[2] * gridDim2[1];
		zi *= gridDim2[1];

		Vector3 V000 = vel[xi + zi + yi];
		Vector3 V100 = vel[xi1 + zi + yi];
		Vector3 V110 = vel[xi1 + zi + yi1];
		Vector3 V010 = vel[xi + zi + yi1];

		Vector3 V001 = vel[xi + zi1 + yi];
		Vector3 V101 = vel[xi1 + zi1 + yi];
		Vector3 V111 = vel[xi1 + zi1 + yi1];
		Vector3 V011 = vel[xi + zi1 + yi1];

		float omx = 1.0f - xr;
		float omy = 1.0f - yr;
		float omz = 1.0f - zr;

		Vector3 Vxyz;
		float c1 = omx * omy * omz;
		float c2 = xr * omy * omz;
		float c3 = omx * yr * omz;
		float c4 = omx * omy * zr;
		float c5 = xr * omy * zr;
		float c6 = omx * yr * zr;
		float c7 = xr * yr * omz;
		float c8 = xr * yr * zr;

		Vxyz.x = V000.x * c1 + V100.x * c2 + V010.x * c3 + V001.x * c4 + V101.x * c5 + V011.x * c6 + V110.x * c7 + V111.x * c8;
		Vxyz.y = V000.y * c1 + V100.y * c2 + V010.y * c3 + V001.y * c4 + V101.y * c5 + V011.y * c6 + V110.y * c7 + V111.y * c8;
		Vxyz.z = V000.z * c1 + V100.z * c2 + V010.z * c3 + V001.z * c4 + V101.z * c5 + V011.z * c6 + V110.z * c7 + V111.z * c8;

		return Vxyz;
	}

	public const float oadj = 1.0f / 255.0f;

	public Vector3 GetGridVelOpt(Vector3 pos, ref bool inbounds)
	{
		int xi = (int)(pos.x * oos.x);
		int yi = (int)(pos.y * oos.y);
		int zi = (int)(pos.z * oos.z);

		if ( xi < 0 || xi >= gridDim2[0] - 1 )
		{
			inbounds = false;
			return Vector3.zero;
		}

		if ( yi < 0 || yi >= gridDim2[1] - 1 )
		{
			inbounds = false;
			return Vector3.zero;
		}

		if ( zi < 0 || zi >= gridDim2[2] - 1 )
		{
			inbounds = false;
			return Vector3.zero;
		}

		inbounds = true;

		float xr = Mathf.Abs((pos.x - (xi * spacing.x)) * oos.x);
		float yr = Mathf.Abs((pos.y - (yi * spacing.y)) * oos.y);
		float zr = Mathf.Abs((pos.z - (zi * spacing.z)) * oos.z);

		int xi1 = (xi + 1) * gridDim2[2] * gridDim2[1];
		int yi1 = yi + 1;
		int zi1 = (zi + 1) * gridDim2[1];

		xi *= gridDim2[2] * gridDim2[1];
		zi *= gridDim2[1];

		MegaFlowVector mv = optvel[xi + zi + yi];

		Vector3 V000;
		V000.x = (float)mv.x;
		V000.y = (float)mv.y;
		V000.z = (float)mv.z;

		mv = optvel[xi1 + zi + yi];

		Vector3 V100;
		V100.x = (float)mv.x;
		V100.y = (float)mv.y;
		V100.z = (float)mv.z;

		mv = optvel[xi1 + zi + yi1];

		Vector3 V110;
		V110.x = (float)mv.x;
		V110.y = (float)mv.y;
		V110.z = (float)mv.z;

		mv = optvel[xi + zi + yi1];

		Vector3 V010;
		V010.x = (float)mv.x;
		V010.y = (float)mv.y;
		V010.z = (float)mv.z;
		
		mv = optvel[xi + zi1 + yi];

		Vector3 V001;
		V001.x = (float)mv.x;
		V001.y = (float)mv.y;
		V001.z = (float)mv.z;

		mv = optvel[xi1 + zi1 + yi];

		Vector3 V101;
		V101.x = (float)mv.x;
		V101.y = (float)mv.y;
		V101.z = (float)mv.z;

		mv = optvel[xi1 + zi1 + yi1];

		Vector3 V111;
		V111.x = (float)mv.x;
		V111.y = (float)mv.y;
		V111.z = (float)mv.z;

		mv = optvel[xi + zi1 + yi1];

		Vector3 V011;
		V011.x = (float)mv.x;
		V011.y = (float)mv.y;
		V011.z = (float)mv.z;

		float omx = 1.0f - xr;
		float omy = 1.0f - yr;
		float omz = 1.0f - zr;

		Vector3 Vxyz = V000 * omx * omy * omz +
			V100 * xr * omy * omz +
			V010 * omx * yr * omz +
			V001 * omx * omy * zr +
			V101 * xr * omy * zr +
			V011 * omx * yr * zr +
			V110 * xr * yr * omz +
			V111 * xr * yr * zr;

		Vxyz.x = ((Vxyz.x * oadj) - 0.5f) * maxval.x;
		Vxyz.y = ((Vxyz.y * oadj) - 0.5f) * maxval.y;
		Vxyz.z = ((Vxyz.z * oadj) - 0.5f) * maxval.z;

		return Vxyz;
	}

	public Vector3	maxval;
	public bool		optimized = false;

	public Vector3 SampleVelFloat(int x, int y, int z)
	{
		return vel[(x * gridDim2[2] * gridDim2[1]) + (z * gridDim2[1]) + y];
	}

	public Vector3 SampleVelOpt(int x, int y, int z)
	{
		MegaFlowVector mv = optvel[(x * gridDim2[2] * gridDim2[1]) + (z * gridDim2[1]) + y];

		Vector3 v;

		v.x = (((float)mv.x * oadj) - 0.5f) * maxval.x;
		v.y = (((float)mv.y * oadj) - 0.5f) * maxval.y;
		v.z = (((float)mv.z * oadj) - 0.5f) * maxval.z;
		return v;
	}

	public void Optimize()
	{
		Vector3 max = Vector3.zero;

		if ( optvel == null )
			optvel = new List<MegaFlowVector>();

		optvel.Clear();

		for ( int i = 0; i < vel.Count; i++ )
		{
			Vector3 v = vel[i];

			if ( Mathf.Abs(v.x) > max.x  )
				max.x = Mathf.Abs(v.x);

			if ( Mathf.Abs(v.y) > max.y )
				max.y = Mathf.Abs(v.y);

			if ( Mathf.Abs(v.z) > max.z )
				max.z = Mathf.Abs(v.z);
		}

		maxval = max;

		for ( int i = 0; i < vel.Count; i++ )
		{
			MegaFlowVector mv = new MegaFlowVector();

			Vector3 v = vel[i];

			mv.x = (byte)(((v.x / maxval.x) + 1.0f) * 0.5f * 255.0f);
			mv.y = (byte)(((v.y / maxval.y) + 1.0f) * 0.5f * 255.0f);
			mv.z = (byte)(((v.z / maxval.z) + 1.0f) * 0.5f * 255.0f);

			optvel.Add(mv);
		}

		optimized = true;

		GetGridVel = GetGridVelOpt;
		SampleVel = SampleVelOpt;
		maxval *= 2.0f;

		vel.Clear();

		GC.Collect();
	}
}