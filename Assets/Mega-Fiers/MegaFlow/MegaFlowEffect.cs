
using UnityEngine;

public enum MegaFlowAlign
{
	None,
	Flow,
	Object,
}

[AddComponentMenu("MegaFlow/Effect Control")]
//[ExecuteInEditMode]
public class MegaFlowEffect : MonoBehaviour
{
	public float			mass		= 1.0f;
	public MegaFlow			source;
	public Vector3			pos			= Vector3.zero;
	public Vector3			vel			= Vector3.zero;
	public float			dt			= 0.01f;
	public float			scale		= 1.0f;
	public float			Area		= 0.1f;
	public float			Gravity		= 0.0f;
	public Vector3			rot			= Vector3.zero;
	public Vector3			rotspeed	= Vector3.zero;
	public float			reynolds	= 20.0f;
	public float			density		= 1.22f;
	public int				framenum	= 0;
	public MegaFlowFrame	frame;
	public float			scl;
	public int				emitindex;

	public MegaFlowAlign	align		= MegaFlowAlign.None;
	public Vector3			alignrot	= Vector3.zero;
	public Gradient			gradient;
	public bool				usegradient	= false;
	public float			speedlow	= 0.0f;
	public float			speedhigh	= 1.0f;
	Quaternion				lastalign	= Quaternion.identity;
	Material				mat;
	Renderer				rend1;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=5972");
	}

	public void SetFrame(int f)
	{
		if ( source )
		{
			if ( f >= 0 && f < source.frames.Count )
			{
				frame = source.frames[f];
				framenum = f;
			}
		}
	}

	void Update()
	{
		if ( source && source.frames.Count > 0 )
		{
			framenum = Mathf.Clamp(framenum, 0, source.frames.Count - 1);
			frame = source.frames[framenum];

			float scl = source.Scale * scale;

			Vector3	Fshape = Vector3.zero;	// Random force due to particle shape
			Vector3	Fgrv = new Vector3(0.0f, -Gravity, 0.0f);

			float duration = Time.deltaTime;

			pos = transform.position;

			bool inbounds = true;

			float p = density;
			float A = Area;
			float Re = reynolds;

			float coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f);

			Vector3 flowpos = pos;	//source.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);

			// This should be in source already
			//Matrix4x4 offtm = Matrix4x4.TRS((frame.size * 0.5f) + frame.offset, Quaternion.identity, Vector3.one);
			//Matrix4x4 tm = offtm * source.transform.worldToLocalMatrix;
			//Matrix4x4 invtm = tm.inverse;

			Vector3 airvel = Vector3.zero;

			while ( duration > 0.0f )
			{
				//Vector3 airvel = invtm.MultiplyVector(frame.GetGridVel(tm.MultiplyPoint3x4(flowpos), ref inbounds) * scl);
				airvel = frame.GetGridVelWorld(flowpos, ref inbounds) * scl;	//invtm.MultiplyVector(frame.GetGridVel(tm.MultiplyPoint3x4(flowpos), ref inbounds) * scl);

				if ( !inbounds )
				{
					airvel = new Vector3(scale, 0.0f, 0.0f);
					flowpos += vel * dt;
				}
				else
				{
					Vector3 tvel = airvel - vel;
					float U = tvel.magnitude;
					float df = coef * U;

					Vector3 dir = tvel.normalized;
					Vector3 Fdrag = dir * df;

					Vector3 Fp = Fdrag + Fshape + Fgrv;
					Vector3	acc = Fp / mass;
					vel += acc * dt;
					flowpos += vel * dt;
				}

				duration -= dt;
			}

			if ( flowpos.y < source.floor  )
				flowpos.y = source.floor;

			transform.position = flowpos;	//source.transform.localToWorldMatrix.MultiplyPoint3x4(flowpos);
			rot += rotspeed * Time.deltaTime;

			Quaternion r = Quaternion.Euler(rot);

			Vector3 fdir = flowpos - pos;

			switch ( align )
			{
				case MegaFlowAlign.Flow:
					{
						r = Quaternion.identity;

						Quaternion ar = lastalign;

						if ( airvel != Vector3.zero )
							ar = Quaternion.LookRotation(airvel) * Quaternion.Euler(alignrot);

						r = r * ar;
						lastalign = ar;
					}
					break;

				case MegaFlowAlign.Object:
					{
						r = Quaternion.identity;

						Quaternion ar = lastalign;

						if ( fdir != Vector3.zero )
							ar = Quaternion.LookRotation(fdir) * Quaternion.Euler(alignrot);

						r = r * ar;
						lastalign = ar;
					}
					break;
			}
#if false
			if ( align )
			{
				r = Quaternion.identity;

				Quaternion ar = lastalign;

				if ( fdir != Vector3.zero )
					ar = Quaternion.LookRotation(flowpos - pos) * Quaternion.Euler(alignrot);

				r = r * ar;
				lastalign = ar;
			}
#endif

			transform.rotation = r;	//Quaternion.Euler(r);	//ot);

			if ( usegradient )
			{
				if ( !mat )
				{
					Renderer rend = GetComponent<Renderer>();

					if ( rend && rend.material )
						mat = rend.material;
				}

				if ( mat )
				{
					float spd = airvel.magnitude;
					float a = Mathf.Clamp01((spd - speedlow) / (speedhigh - speedlow));
					mat.color = gradient.Evaluate(a);
				}
			}
		}

		if ( rend1 == null )
			rend1 = GetComponent<Renderer>();

		if ( rend1 && !rend1.enabled )
			rend1.enabled = true;
	}
}