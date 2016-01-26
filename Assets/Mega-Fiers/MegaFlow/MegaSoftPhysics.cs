
using UnityEngine;
using System.Collections.Generic;

#if false
[System.Serializable]
public class MegaSoft
{
	public Vector3		inOffset	= Vector3.zero;
	public Vector3		outOffset	= Vector3.zero;
	public float		radius		= 0.01f;
	public Vector3		windFrc		= Vector3.zero;
	public Transform	start;
	public Transform	end;
	public float		WireLength	= 0.0f;
	//Matrix4x4			wtm;
	public List<MegaSoftMass>			masses		= new List<MegaSoftMass>();
	public List<MegaSoftSpring>			springs		= new List<MegaSoftSpring>();
	public List<MegaSoftPointConstraint>	constraints	= new List<MegaSoftPointConstraint>();
	public List<MegaSoftLengthConstraint>	lenconstraints	= new List<MegaSoftLengthConstraint>();
	public Vector3[]					masspos;

	// Physics/Solver params
	public float						spring				= 10.0f;
	public float						damp				= 1.0f;
	public float						timeStep			= 0.01f;
	public float						Mass				= 1.0f;
	public Vector3						gravity				= new Vector3(0.0f, -9.81f, 0.0f);
	public float						airdrag				= 0.99f;
	public float						massRand			= 0.0f;
	public bool							doCollisions		= false;
	public float						floor				= 0.0f;
	public int							points				= 10;
	public int							iters				= 4;

	public int							frameWait			= 0;
	public int							frameNum			= 0;

	public bool							stiffnessSprings	= false;
	public float						stiffrate			= 10.0f;
	public float						stiffdamp			= 1.0f;
	public bool							lengthConstraints	= false;

	Matrix4x4							wtm;
	public MegaSoftSolver				verletsolver		= new MegaSoftSolverVertlet();

	// Should set the sim wake time so it will update correctly
	public void SetEndConstraintActive(int index, bool active, float time)
	{
		if ( active )
		{
			constraints[index].reactivate = time;
			constraints[index].rtime = time;
			constraints[index].ps = masses[constraints[index].p1].pos;
		}
		else
			constraints[index].active = active;
	}

	public void Update(float timeStep)
	{
		verletsolver.doIntegration1(this, timeStep);
	}

	// This is for cubic interpolation so dont need
	public void MoveMasses(MegaSoft soft)
	{
		return;
		for ( int i = 0; i < masses.Count; i++ )
		{
			masspos[i + 1] = masses[i].pos;
			masses[i].forcec = Vector3.zero;
		}

		masspos[0] = masses[0].pos - (masses[1].pos - masses[0].pos);
		masspos[masspos.Length - 1] = masses[masses.Count - 1].pos + (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos);
	}

	public float DampingRatio = 1.0f;

	public void BuildSoftBody(List<VoxMass> points, List<Link> links)
	{
		if ( masses == null )
			masses = new List<MegaSoftMass>();

		masses.Clear();

		float ms = Mass / (float)(points.Count + 1);

		for ( int i = 0; i < points.Count; i++ )
		{
			MegaSoftMass rm = new MegaSoftMass(ms, points[i].p);
			masses.Add(rm);
		}

		if ( springs == null )
			springs = new List<MegaSoftSpring>();

		springs.Clear();

		if ( constraints == null )
			constraints = new List<MegaSoftPointConstraint>();

		if ( lenconstraints == null )
			lenconstraints = new List<MegaSoftLengthConstraint>();

		constraints.Clear();
		lenconstraints.Clear();

		float damp1 = (DampingRatio * 0.45f) * (2.0f * Mathf.Sqrt(ms * spring));
		Debug.Log("ratio " + DampingRatio + " damp " + damp1);

		for ( int i = 0; i < links.Count - 1; i++ )
		{
			MegaSoftSpring spr = new MegaSoftSpring(links[i].m1, links[i].m2, spring, damp, this, 1.0f);	//stretch);
			springs.Add(spr);

			//if ( lengthConstraints )
			//{
			//MegaSoftLengthConstraint lcon = new MegaSoftLengthConstraint(links[i].m1, links[i].m2, spr.restlen);
			//lenconstraints.Add(lcon);
			//}
		}

		if ( stiffnessSprings )
		{
			//int gap = 2;
			//for ( int i = 0; i < masses.Count - gap; i++ )
			//{
			//	MegaWireSpring spr = new MegaWireSpring(i, i + 2, wire.stiffrate, wire.stiffdamp, this, wire.stretch);
			//	springs.Add(spr);
			//}
		}

		// Apply fixed end constraints
		//MegaWirePointConstraint pcon = new MegaWirePointConstraint(0, start.transform, outOffset);
		//constraints.Add(pcon);

		//pcon = new MegaWirePointConstraint(masses.Count - 1, end.transform, inOffset);
		//constraints.Add(pcon);

		masspos = new Vector3[masses.Count + 2];

		for ( int i = 0; i < masses.Count; i++ )
			masspos[i + 1] = masses[i].pos;

		masspos[0] = masses[0].pos - (masses[1].pos - masses[0].pos);
		masspos[masspos.Length - 1] = masses[masses.Count - 1].pos + (masses[masses.Count - 1].pos - masses[masses.Count - 2].pos);
	}
}

[System.Serializable]
public class MegaSoftConstraint
{
	public bool active;
	public float	reactivate = 0.0f;
	public float	rtime = 0.0f;
	public virtual void Apply(MegaSoft soft)
	{
	}
}

[System.Serializable]
public class MegaSoftLengthConstraint : MegaSoftConstraint
{
	public int		p1;
	public int		p2;
	public float	length;
	Vector3 moveVector = Vector3.zero;

	public MegaSoftLengthConstraint(int _p1, int _p2, float _len)
	{
		p1 = _p1;
		p2 = _p2;
		length = _len;
		active = true;
	}

	public override void Apply(MegaSoft soft)
	{
		if ( active )
		{
			moveVector.x = soft.masses[p2].pos.x - soft.masses[p1].pos.x;
			moveVector.y = soft.masses[p2].pos.y - soft.masses[p1].pos.y;
			moveVector.z = soft.masses[p2].pos.z - soft.masses[p1].pos.z;

			if ( moveVector.x != 0.0f || moveVector.y != 0.0f || moveVector.z != 0.0f )
			{
				float currentLength = moveVector.magnitude;

				float do1 = 1.0f / currentLength;

				float l = 0.5f * (currentLength - length) * do1;
				moveVector.x *= l;
				moveVector.y *= l;
				moveVector.z *= l;

				soft.masses[p1].pos.x += moveVector.x;
				soft.masses[p1].pos.y += moveVector.y;
				soft.masses[p1].pos.z += moveVector.z;

				soft.masses[p2].pos.x -= moveVector.x;
				soft.masses[p2].pos.y -= moveVector.y;
				soft.masses[p2].pos.z -= moveVector.z;
			}
		}
	}
}

[System.Serializable]
public class MegaSoftPointConstraint : MegaSoftConstraint
{
	public int			p1;
	public Vector3		offset;
	public Transform	obj;
	public Vector3		ps;
	public Vector3		tp;

	public MegaSoftPointConstraint(int _p1, Transform trans, Vector3 off)
	{
		offset = off;
		p1 = _p1;
		obj = trans;
		active = true;
		reactivate = 0.0f;
		rtime = 0.0f;
	}

	Vector3 easeInOutSine(Vector3 start, Vector3 end, float value)
	{
		end -= start;
		return -end / 2.0f * (Mathf.Cos(Mathf.PI * value / 1.0f) - 1.0f) + start;
	}

	public void ReActivate(MegaSoft soft, float t)
	{
		tp = obj.TransformPoint(offset);

		if ( !active )
		{
			if ( reactivate > 0.0f )
			{
				reactivate -= 0.01f;
				//Vector3 delta = tp - soft.masses[p1].pos;

				soft.masses[p1].pos = easeInOutSine(tp, ps, reactivate / rtime);

				if ( reactivate < 0.0f )
				{
					reactivate = 0.0f;
					active = true;
				}
			}
		}
	}

	public override void Apply(MegaSoft soft)
	{
		if ( active )
			soft.masses[p1].pos = tp;
	}
}

[System.Serializable]
public class MegaSoftSolver
{
	public virtual void doIntegration1(MegaSoft soft, float dt) { }
	public virtual void Solve() { }
}

[System.Serializable]
public class MegaSoftSolverVertlet : MegaSoftSolver
{
	void doCalculateForces(MegaSoft soft)
	{
		Vector3 frc = soft.gravity;

		frc.x += soft.windFrc.x;
		frc.y += soft.windFrc.y;
		frc.z += soft.windFrc.z;

		for ( int i = 0; i < soft.masses.Count; i++ )
		{
			soft.masses[i].force.x = (soft.masses[i].mass * frc.x) + soft.masses[i].forcec.x;
			soft.masses[i].force.y = (soft.masses[i].mass * frc.y) + soft.masses[i].forcec.y;
			soft.masses[i].force.z = (soft.masses[i].mass * frc.z) + soft.masses[i].forcec.z;
		}

		for ( int i = 0; i < soft.springs.Count; i++ )
			soft.springs[i].doCalculateSpringForce(soft);
	}

	void DoConstraints(MegaSoft soft)
	{
		for ( int c = 0; c < soft.constraints.Count; c++ )
		{
			soft.constraints[c].ReActivate(soft, soft.timeStep);
		}

		for ( int i = 0; i < soft.iters; i++ )
		{
			for ( int c = 0; c < soft.lenconstraints.Count; c++ )
				soft.lenconstraints[c].Apply(soft);
			for ( int c = 0; c < soft.constraints.Count; c++ )
				soft.constraints[c].Apply(soft);
		}
	}

#if false
	public override void doIntegration1(MegaSoft soft, float dt)
	{
		doCalculateForces(soft);	// Calculate forces, only changes _f

		/*	Then do correction step by integration with central average (Heun) */
		for ( int i = 0; i < soft.masses.Count; i++ )
		{
			soft.masses[i].last = soft.masses[i].pos;
			soft.masses[i].vel += dt * soft.masses[i].force * soft.masses[i].oneovermass;
			soft.masses[i].pos += soft.masses[i].vel * dt;
			soft.masses[i].vel *= soft.airdrag;	//friction;
		}

		DoConstraints(soft);
		//DoCollisions(dt);
		if ( soft.doCollisions )
			DoCollisions(soft);

	}
#else
	public override void doIntegration1(MegaSoft soft, float dt)
	{
		doCalculateForces(soft);	// Calculate forces, only changes _f

		float t2 = dt * dt;
		/*	Then do correction step by integration with central average (Heun) */
		for ( int i = 0; i < soft.masses.Count; i++ )
		{
			Vector3 last = soft.masses[i].pos;
			soft.masses[i].pos += soft.airdrag * (soft.masses[i].pos - soft.masses[i].last) + soft.masses[i].force * soft.masses[i].oneovermass * t2;	// * t;

			soft.masses[i].vel = (soft.masses[i].pos - last) / dt;
			soft.masses[i].last = last;
		}

		DoConstraints(soft);

		if ( soft.doCollisions )
			DoCollisions(soft);
	}
#endif
	void DoCollisions(MegaSoft soft)
	{
		for ( int i = 0; i < soft.masses.Count; i++ )
		{
			if ( soft.masses[i].pos.y < soft.floor )
			{
				soft.masses[i].pos.y = soft.floor;

				float VdotN = Vector3.Dot(Vector3.up, soft.masses[i].vel);
				Vector3 Vn = Vector3.up * VdotN;
				// CALCULATE Vt
				//Vector3 Vt = (rope.masses[i].vel - Vn) * rope.floorfriction;
				// SCALE Vn BY COEFFICIENT OF RESTITUTION
				Vn *= 0.9f;	//rope.bounce;
				// SET THE VELOCITY TO BE THE NEW IMPULSE
				soft.masses[i].vel = Vn;	//Vt - Vn;

				soft.masses[i].last = soft.masses[i].pos;
			}
		}
	}
}

[System.Serializable]
public class MegaSoftMass
{
	public Vector3		pos;
	public Vector3		last;
	public Vector3		force;
	public Vector3		vel;
	public Vector3		posc;
	public Vector3		velc;
	public Vector3		forcec;
	public float		mass;	// 1.0f normally, set to zero to lock in place
	public float		oneovermass;
	public bool			collide;

	public MegaSoftMass(float m, Vector3 p)
	{
		mass = m;
		oneovermass = 1.0f / mass;
		pos = p;
		last = p;
		force = Vector3.zero;
		vel = Vector3.zero;
	}
}

[System.Serializable]
public class MegaSoftSpring
{
	public int		p1;
	public int		p2;
	public float	restlen;
	public float	initlen;
	public float	ks;
	public float	kd;
	public float	len;

	public MegaSoftSpring(int _p1, int _p2, float _ks, float _kd, MegaSoft soft)
	{
		p1 = _p1;
		p2 = _p2;
		ks = _ks;
		kd = _kd;
		restlen = (soft.masses[p1].pos - soft.masses[p2].pos).magnitude;	// * stretch;
		initlen = restlen;
	}

	public MegaSoftSpring(int _p1, int _p2, float _ks, float _kd, MegaSoft soft, float stretch)
	{
		p1 = _p1;
		p2 = _p2;
		ks = _ks;
		kd = _kd;
		restlen = (soft.masses[p1].pos - soft.masses[p2].pos).magnitude * stretch;
		initlen = restlen;
	}

	public void doCalculateSpringForce(MegaSoft hose)
	{
		Vector3 deltaP = hose.masses[p1].pos - hose.masses[p2].pos;

		float dist = deltaP.magnitude;


		Vector3 dir = deltaP / dist;
		Vector3 springForce = -ks * (dist - restlen) * dir;
		Vector3	deltaV = hose.masses[p1].vel - hose.masses[p2].vel;
		springForce += -kd * Vector3.Dot(deltaV, dir) * dir;
		//float Hterm = (dist - restlen) * ks;

		len = dist;

		//float Dterm = (Vector3.Dot(deltaV, deltaP) * kd) / dist;

		//Vector3 springForce = deltaP * (1.0f / dist);
		//springForce *= -(Hterm + Dterm);

		hose.masses[p1].force.x += springForce.x;
		hose.masses[p1].force.y += springForce.y;
		hose.masses[p1].force.z += springForce.z;

		hose.masses[p2].force.x -= springForce.x;
		hose.masses[p2].force.y -= springForce.y;
		hose.masses[p2].force.z -= springForce.z;
	}

	public void doCalculateSpringForce2(MegaSoft hose)
	{
		Vector3 deltaP = hose.masses[p1].pos - hose.masses[p2].pos;

		float dist = deltaP.magnitude;

		float Hterm = (dist - restlen) * ks;

		Vector3	deltaV = hose.masses[p1].vel - hose.masses[p2].vel;
		float Dterm = (Vector3.Dot(deltaV, deltaP) * kd) / dist;

		Vector3 springForce = deltaP * (1.0f / dist);
		springForce *= -(Hterm + Dterm);

		hose.masses[p1].force.x += springForce.x;
		hose.masses[p1].force.y += springForce.y;
		hose.masses[p1].force.z += springForce.z;

		hose.masses[p2].force.x -= springForce.x;
		hose.masses[p2].force.y -= springForce.y;
		hose.masses[p2].force.z -= springForce.z;
	}

	public void doCalculateSpringForce1(MegaSoft mod)
	{
		Vector3 direction = mod.masses[p1].pos - mod.masses[p2].pos;

		if ( direction != Vector3.zero )
		{
			float currLength = direction.magnitude;
			direction = direction.normalized;
			Vector3 force = -ks * ((currLength - restlen) * direction);

			mod.masses[p1].force.x += force.x;
			mod.masses[p1].force.y += force.y;
			mod.masses[p1].force.z += force.z;

			mod.masses[p2].force.x -= force.x;
			mod.masses[p2].force.y -= force.y;
			mod.masses[p2].force.z -= force.z;

			len = currLength;
		}
	}
}
#endif