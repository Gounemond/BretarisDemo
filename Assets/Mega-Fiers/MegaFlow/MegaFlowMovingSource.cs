
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaFlowPosFrame
{
	public float	time;
	public int		frame;
}

public class MegaFlowMovingSource : MonoBehaviour
{
	public MegaFlow					source;
	public int						framenum;
	public Transform				target;
	public float					flowtime		= 10.0f;
	public float					flowtimestep	= 0.25f;
	public float					flowscale		= 1.0f;
	public float					mindist			= 1.0f;
	public bool						drawpath		= true;
	List<MegaFlowPos>				flowpositions	= new List<MegaFlowPos>();
	MegaFlowFrame					flow;
	float							ftime			= 0.0f;
	Vector3							lastpos;
	Matrix4x4						framegizmotm;
	Matrix4x4						frametm;
	public bool						usefalloff		= false;
	public AnimationCurve			falloffcrv		= new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public List<MegaFlowPosFrame>	frames			= new List<MegaFlowPosFrame>();

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=6105");
	}

	void Start()
	{
		if ( target )
			lastpos = target.position;
	}

	void Update()
	{
		if ( source && target )
		{
			flow = source.frames[framenum];

			SaveFlowPosition(target.position, target.rotation, (target.position - lastpos) / Time.deltaTime, flowscale);
			lastpos = target.position;
		}
	}

	void ProcessPos()
	{
		for ( int i = 0; i < flowpositions.Count; i++ )
		{
			MegaFlowPos fpos = flowpositions[i];

			fpos.fpos += fpos.vel * Time.deltaTime;
			fpos.time -= Time.deltaTime;

			fpos.alpha = 1.0f - (fpos.time / fpos.stime);
			if ( usefalloff )
				fpos.falloff = falloffcrv.Evaluate(fpos.alpha);
			else
				fpos.falloff = 1.0f;

			if ( fpos.time < 0.0f )
			{
				flowpositions.RemoveAt(i);
				i--;
			}
			else
			{
				if ( frames.Count > 0 )
				{
					fpos.frame = 0;

					for ( int f = frames.Count - 1; f >= 0; f-- )
					{
						if ( fpos.alpha > frames[f].time )
						{
							fpos.frame = f;
							break;
						}
					}
				}
			}
		}
	}

	void AddPos(Vector3 pos, Vector3 vel, Matrix4x4 tm, Matrix4x4 tm1, float flowscale)
	{
		MegaFlowPos fpos = new MegaFlowPos();
		fpos.Set(pos, vel, tm, tm1, flowscale, flowtime);

		flowpositions.Add(fpos);
	}

	void UpdateLast(Vector3 pos, Vector3 vel, Matrix4x4 tm, Matrix4x4 tm1, float flowscale)
	{
		MegaFlowPos fpos = flowpositions[flowpositions.Count - 1];
		fpos.Set(pos, vel, tm, tm1, flowscale, flowtime);
	}

	public void SaveFlowPosition(Vector3 pos, Quaternion rot, Vector3 vel, float flowscale)
	{
		Matrix4x4 offtm = Matrix4x4.TRS(-flow.size * 0.5f, Quaternion.identity, Vector3.one);
		framegizmotm = transform.localToWorldMatrix * offtm;

		Matrix4x4 offtm1 = Matrix4x4.TRS(flow.size * 0.5f, Quaternion.identity, Vector3.one);
		frametm = offtm1 * transform.worldToLocalMatrix;

		ProcessPos();

		if ( flowpositions.Count == 0 )
			AddPos(pos, vel, framegizmotm, frametm, flowscale);

		if ( flowpositions.Count == 1 )
			AddPos(pos, vel, framegizmotm, frametm, flowscale);

		ftime += Time.deltaTime;

		if ( ftime >= flowtimestep )
		{
			if ( (pos - flowpositions[flowpositions.Count - 2].pos).magnitude > mindist )
			{
				ftime -= flowtimestep;
				AddPos(pos, vel, framegizmotm, frametm, flowscale);
			}
			else
			{
				UpdateLast(pos, vel, framegizmotm, frametm, flowscale);
				ftime += Time.deltaTime;
			}
		}
		else
			UpdateLast(pos, vel, framegizmotm, frametm, flowscale);
	}

	public Vector3 FindFlowPos(Vector3 pos, ref bool inbounds, ref Matrix4x4 tm, ref float fvel, ref int frame, ref float falloff)
	{
		Vector3 fpos = Vector3.zero;
		float closest = float.MaxValue;
		int index = -1;

		for ( int i = 0; i < flowpositions.Count; i++ )
		{
			fpos = flowpositions[i].pos;
			fpos.x -= pos.x;
			fpos.y -= pos.y;
			fpos.z -= pos.z;

			float sdist = (fpos.x * fpos.x) + (fpos.y * fpos.y) + (fpos.z * fpos.z);

			if ( sdist < closest )
			{
				closest = sdist;
				index = i;
			}
		}

		if ( index >= 0 )
		{
			inbounds = true;
			tm = flowpositions[index].invtm1;
			fpos = flowpositions[index].tm1.MultiplyPoint3x4(pos);
			fpos.y += flowpositions[index].fpos;
			fvel = flowpositions[index].vel;
			frame = flowpositions[index].frame;
			falloff = flowpositions[index].falloff;
		}
		else
			inbounds = false;

		return fpos;
	}

	public void DrawMoveGizmo()
	{
		if ( source )
			flow = source.frames[framenum];

		if ( flow )
		{
			Matrix4x4 offtm = Matrix4x4.TRS(-flow.size * 0.5f, Quaternion.identity, Vector3.one);
			framegizmotm = transform.localToWorldMatrix * offtm;

			Gizmos.matrix = framegizmotm;

			Vector3 min = Vector3.zero;
			Vector3 max = flow.size;
			MegaFlow.DrawBox(min, max);
			Gizmos.matrix = Matrix4x4.identity;
		}
	}

	void OnDrawGizmos()
	{
		if ( drawpath )
		{
			Gizmos.color = Color.grey;

			if ( flowpositions.Count > 0 )
			{
				Vector3 lpos = Vector3.zero;
				for ( int i = 0; i < flowpositions.Count; i++ )
				{
					MegaFlowPos fpos = flowpositions[i];
					Gizmos.color = Color.white;
					if ( i > 0 )
						Gizmos.DrawLine(lpos, fpos.pos);

					Gizmos.color = Color.green;
					Gizmos.DrawSphere(fpos.pos, 0.1f);

					lpos = fpos.pos;
				}
			}

			DrawMoveGizmo();
		}

		Gizmos.DrawIcon(transform.position, "MegaFlowIcon.png", true);
	}
}