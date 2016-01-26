
using UnityEngine;

[AddComponentMenu("MegaFlow/Sample")]
[ExecuteInEditMode]
public class MegaFlowSample : MonoBehaviour
{
	public MegaFlow		source;
	public int			framenum;
	public Vector3		velocity;
	bool				inbounds = false;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=6103");
	}

	public Vector3 GetVelocity()
	{
		return GetVelocity(transform.position);
	}

	public Vector3 GetVelocity(Vector3 pos)
	{
		if ( source )
		{
			MegaFlowFrame frame = source.frames[framenum];

			if ( frame && frame.GetGridVel != null )
				return frame.GetGridVelWorld(pos, ref inbounds);
		}

		return Vector3.zero;
	}

	void Update()
	{
		framenum = Mathf.Clamp(framenum, 0, source.frames.Count - 1);
		velocity = GetVelocity();
	}

	void OnDrawGizmos()
	{
		Gizmos.DrawIcon(transform.position, "MegaFlowIcon.png", true);
	}
}