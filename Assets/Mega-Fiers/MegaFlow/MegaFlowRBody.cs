
using UnityEngine;

[AddComponentMenu("MegaFlow/RigidBody Control")]
[ExecuteInEditMode]
public class MegaFlowRBody : MegaFlowEffect
{
	//Matrix4x4		tm;
	//Matrix4x4		invtm;
	float			coef		= 0.0f;
	Rigidbody		rbody;

	[ContextMenu("Help")]
	public void RbodyHelp()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=5898");
	}

	void Start()
	{
		rbody = gameObject.GetComponent<Rigidbody>();
	}

	void Update()
	{
		if ( source && source.frames.Count > 0 )
		{
			scl = source.Scale * scale;

			source.Prepare();
			framenum = Mathf.Clamp(framenum, 0, source.frames.Count - 1);
			frame = source.frames[framenum];

			//Matrix4x4 offtm = Matrix4x4.TRS(source.flow.size * 0.5f, Quaternion.identity, Vector3.one);
			//Matrix4x4 offtm = Matrix4x4.TRS((frame.size * 0.5f) + frame.offset, Quaternion.identity, Vector3.one);

			//tm = offtm * source.transform.worldToLocalMatrix;
			//invtm = tm.inverse;

			coef = source.Density * Area * Mathf.Pow(source.Reynolds, -0.5f);
		}
	}

	void FixedUpdate()
	{
		if ( rbody )
		{
			bool inbounds = true;

			//Vector3 airvel = tm.MultiplyVector(frame.GetGridVel(invtm.MultiplyPoint3x4(rbody.position), ref inbounds) * scl);
			Vector3 airvel = frame.GetGridVelWorld(rbody.position, ref inbounds) * scl;

			if ( inbounds )
			{
				Vector3 tvel = (airvel * scale) - rbody.velocity;
				rbody.AddForce(tvel.normalized * coef * tvel.magnitude);
			}
		}
	}
}