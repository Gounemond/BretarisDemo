
using UnityEngine;

[ExecuteInEditMode]
public class MegaFlowTest : MonoBehaviour
{
	public float	size = 1.0f;

	Vector3[]	points = new Vector3[8];
	Vector3[]	dirs = new Vector3[8];

	[Range(0.0f, 1.0f)]
	public float	x = 0.0f;

	[Range(0.0f, 1.0f)]
	public float	y = 0.0f;

	[Range(0.0f, 1.0f)]
	public float	z = 0.0f;

	void Start()
	{
		MakeCube();
	}

	public void MakeCube()
	{
		Vector3	min = new Vector3(-size, -size, -size);
		Vector3	max = new Vector3(size, size, size);

		points[0] = new Vector3(min.x, min.y, min.z);
		points[1] = new Vector3(max.x, min.y, min.z);
		points[2] = new Vector3(max.x, max.y, min.z);
		points[3] = new Vector3(min.x, max.y, min.z);

		points[4] = new Vector3(min.x, min.y, max.z);
		points[5] = new Vector3(max.x, min.y, max.z);
		points[6] = new Vector3(max.x, max.y, max.z);
		points[7] = new Vector3(min.x, max.y, max.z);

		for ( int i = 0; i < 8; i++ )
		{
			dirs[i] = Random.onUnitSphere;
		}
	}

	void Update()
	{
		//MakeCube();
	}

	public Vector3 GetDir()
	{
		float xr = x;
		float yr = y;
		float zr = z;

		Vector3 V000 = dirs[0];
		Vector3 V100 = dirs[1];
		Vector3 V110 = dirs[2];
		Vector3 V010 = dirs[3];

		Vector3 V001 = dirs[4];
		Vector3 V101 = dirs[5];
		Vector3 V111 = dirs[6];
		Vector3 V011 = dirs[7];

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

	public Vector3 GetPos()
	{
		float xr = x;
		float yr = y;
		float zr = z;

		Vector3 V000 = points[0];
		Vector3 V100 = points[1];
		Vector3 V110 = points[2];
		Vector3 V010 = points[3];

		Vector3 V001 = points[4];
		Vector3 V101 = points[5];
		Vector3 V111 = points[6];
		Vector3 V011 = points[7];

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


	public void DrawGizmo()
	{
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.DrawLine(points[0], points[1]);
		Gizmos.DrawLine(points[1], points[2]);
		Gizmos.DrawLine(points[2], points[3]);
		Gizmos.DrawLine(points[3], points[0]);

		Gizmos.DrawLine(points[4], points[5]);
		Gizmos.DrawLine(points[5], points[6]);
		Gizmos.DrawLine(points[6], points[7]);
		Gizmos.DrawLine(points[7], points[4]);

		Gizmos.DrawLine(points[0], points[4]);
		Gizmos.DrawLine(points[1], points[5]);
		Gizmos.DrawLine(points[2], points[6]);
		Gizmos.DrawLine(points[3], points[7]);

		for ( int i = 0; i < 8; i++ )
		{
			Gizmos.DrawRay(points[i], dirs[i] * 4.0f);
		}

		Vector3 pos = GetPos();
		Vector3 dir = GetDir();

		Gizmos.DrawRay(pos, dir * 4.0f);
		Gizmos.matrix = Matrix4x4.identity;
	}
}