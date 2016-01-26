
#if false
using UnityEngine;

public class MegaFlowForce : MonoBehaviour
{
	public virtual Vector3 GetForce(Vector3 p)
	{
		return Vector3.zero;
	}

	public virtual Vector3 GetVel(Vector3 p)
	{
		return Vector3.zero;
	}
}

// Option to bake all the forces into a Vector Field
public class MegaFlowSpline : MegaFlowForce
{
	public MegaShape	shape;
	public int			curve;

	public float		force;

	public float		radius = 1.0f;
	public AnimationCurve radiuscrv = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
	public AnimationCurve forcecrv = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));

	Vector3 tan;
	float alpha;
	int kn;

	[Range(0.001f, 0.1f)]
	public float		mass = 1.0f;
	public Vector3		vel = Vector3.zero;
	public Vector3		pos = Vector3.zero;

	public float		constrainfrc = 1.0f;

	public float drag = 0.1f;
	// Break line up into segments then can do quick segment check
	// Start with spline for proof concept

	public float velfrc = 1.0f;

	public override Vector3 GetForce(Vector3 p)
	{
		Vector3 np = shape.FindNearestPointWorld(p, 4, ref kn, ref tan, ref alpha);

		//Debug.Log("np " + np);
		float dist = Vector3.Distance(np, p);

		if ( dist < radius )
		{
			Vector3 frc = tan.normalized * force * (1.0f - (dist / radius));

			frc += (np - p).normalized * constrainfrc * (dist / radius);

			frc -= vel * drag;
			//Debug.Log("tan " + tan);

			//vel = tan.normalized * velfrc;

			return frc;	//tan.normalized * force * (1.0f - (dist / radius));
		}

		return Vector3.zero;
	}

	public float	flowvel = 1.0f;
	public override Vector3 GetVel(Vector3 p)
	{
		Vector3 np = shape.FindNearestPointWorld(p, 4, ref kn, ref tan, ref alpha);

		float dist = Vector3.Distance(np, p);

		if ( dist < radius )
		{
			Vector3 vel1 = tan.normalized * flowvel;	// * (1.0f - (dist / radius));
			return vel1;	//tan.normalized * force * (1.0f - (dist / radius));
		}

		return vel;	//Vector3.zero;
	}


	void Start()
	{
		pos = transform.position;
	}

	public float gravity = -9.81f;

	void Update()
	{
		UpdateVel();
		return;

		Vector3 frc = GetForce(pos);

		frc.y += gravity;

		vel += (frc / mass) * Time.deltaTime;

		pos += vel * Time.deltaTime;

		if ( pos.y < 0.0f )
			pos.y = 0.0f;

		transform.position = pos;
	}

	void UpdateVel1()
	{
		//UpdateVel();
		//return;

		Vector3 tvel = GetVel(pos);

		Vector3 delta = tvel - vel;

		Vector3 imp = mass * (delta / Time.deltaTime);

		Vector3 frc = imp;

		frc.y += gravity;

		vel += (frc / mass) * Time.deltaTime;

		pos += vel * Time.deltaTime;

		if ( pos.y < 0.0f )
			pos.y = 0.0f;

		transform.position = pos;
	}

	void UpdateVel2()
	{
		//UpdateVel();
		//return;

		Vector3 tvel = GetVel(pos);

		Vector3 delta = tvel - vel;

		Vector3 imp = mass * (delta / Time.deltaTime);

		Vector3 frc = imp;

		frc.y += gravity;

		vel += (frc / mass) * Time.deltaTime;

		pos += vel * Time.deltaTime;

		if ( pos.y < 0.0f )
			pos.y = 0.0f;

		transform.position = pos;
	}

	public float Area = 1.0f;
	public float Reynolds = 20.0f;
	public float Density = 1.22f;

	void UpdateVel()
	{
		//if ( source )
		{
			Vector3	Fshape = Vector3.zero;	// Random force due to particle shape
			Vector3	Fgrv = new Vector3(0.0f, gravity, 0.0f);

			float duration = Time.deltaTime;

			pos = transform.position;

			bool inbounds = true;

			float p = Density;	//->GetFloat(unityflow_density, t);	//1.22f;
			float A = Area;	//pblock->GetFloat(unityflow_area, t);	//0.01f;	//1.0f;
			float Re = Reynolds;	//->GetFloat(unityflow_reynolds, t);	//20.0f;
			//float mass = mass;	//pblock->GetFloat(unityflow_mass, t);	//0.001f;

			float dt = 0.01f;
			float coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f);

			Vector3 flowpos = pos;	//source.transform.worldToLocalMatrix.MultiplyPoint3x4(pos);

			while ( duration > 0.0f )
			{
				//Vector3 flowpos = source.transform.worldToLocalMatrix.MultiplyPoint(pos);

				Vector3 airvel = GetVel(flowpos);	//source.GetGridVel(flowpos, ref inbounds) * source.Scale;

				if ( !inbounds )
				{
					//airvel = new Vector3(scale, 0.0f, 0.0f);
					//flowpos += vel * dt;
				}
				else
				{
					Vector3 tvel = airvel - vel;
					float U = tvel.sqrMagnitude;	//magnitude;

					//float df = 1.0f * p * (U * U) * A * Mathf.Pow(Re, -0.5f);
					float df = coef * U;	//(U * U);

					Vector3 dir = tvel.normalized;
					Vector3 Fdrag = dir * df;

					Vector3 Fp = Fdrag + Fshape + Fgrv;
					Vector3	acc = Fp / mass;
					vel += acc * dt;
					//vel = airvel;
					flowpos += vel * dt;
				}

				duration -= dt;
				//break;
			}

			if ( flowpos.y < 0.0f )
			{
				flowpos.y = 0.0f;
			}

			transform.position = flowpos;	//source.transform.localToWorldMatrix.MultiplyPoint3x4(flowpos);
			//rot += rotspeed * Time.deltaTime;
			//transform.rotation = Quaternion.Euler(rot);

		}
	}
}
#endif