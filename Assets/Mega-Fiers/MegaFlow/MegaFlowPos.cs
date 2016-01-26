
using UnityEngine;

public class MegaFlowPos
{
	public Matrix4x4	tm1;
	public Matrix4x4	invtm1;
	public Vector3		pos;
	public float		radius;
	public int			frame;
	public float		fpos;
	public float		vel;
	public float		time;
	public float		stime;
	public float		alpha = 1.0f;
	public float		falloff;

	public void Set(Vector3 _pos, Vector3 _vel, Matrix4x4 _tm, Matrix4x4 _tm1, float _flowscale, float _ftime)
	{
		pos = _pos;
		fpos = 0.0f;
		vel = _vel.magnitude * _flowscale;
		tm1 = _tm1;
		invtm1 = _tm1.inverse;
		time = _ftime;
		stime = _ftime;
		alpha = 1.0f;
	}
}