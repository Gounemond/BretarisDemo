
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MegaFlowSmokeObj
{
	public float		lifetime;
	public GameObject	obj;
}

[System.Serializable]
public class MegaFlowSmokeObjDef
{
	public MegaFlowEffect	obj;
	public Vector3			rotlow			= Vector3.zero;
	public Vector3			rothigh			= Vector3.zero;
	public Vector3			rotspeedlow		= Vector3.zero;
	public Vector3			rotspeedhigh	= Vector3.zero;
	public Vector3			scalelow		= Vector3.one;
	public Vector3			scalehigh		= Vector3.one;
	public float			weight			= 1.0f;
	public float			tw;
	//public bool				align			= false;
	//public Vector3			rot				= Vector3.zero;
	//public Gradient			gradient;
	//public bool				usegradient		= false;
}

[AddComponentMenu("MegaFlow/Smoke Gun")]
[ExecuteInEditMode]
public class MegaFlowSmokeGun : MonoBehaviour
{
	public MegaFlow					source;
	public int						framenum;
	public Vector3					vel			= Vector3.zero;
	public float					flowrate	= 1.0f;
	public float					lifetime	= 4.0f;
	[Range(0.01f, 2.0f)]
	public float					mass		= 0.02f;
	public float					area		= 0.2f;
	public float					width		= 1.0f;
	public float					height		= 1.0f;
	public int						poolSize	= 100;
	public float					Gravity		= 0.0f;
	public float					xspeed		= 10.0f;
	public float					yspeed		= 10.0f;
	public Vector3					offset		= Vector3.zero;
	public int						count		= 32;
	public float					scale		= 1.0f;
	Vector3							screenPoint;
	Vector3							offset1;
	int								colindex	= 0;
	float							t			= 0.0f;
	Vector3							smokepos;
	public List<MegaFlowSmokeObjDef>	emitobjects = new List<MegaFlowSmokeObjDef>();
	//public List<GameObject>			emitobjects = new List<GameObject>();
	List<GameObject>				pool		= new List<GameObject>();
	List<MegaFlowSmokeObj>			objects		= new List<MegaFlowSmokeObj>();

	public List<Color>				cols = new List<Color>();

	//public Color[]	cols = {
					   //new Color(1.0f, 0.75f, 0.75f, 1.0f),
					   //new Color(0.0f, 1.0f, 0.75f, 1.0f),
					   //new Color(0.75f, 0.75f, 1.0f, 1.0f),
				   //};

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=5902");
	}

	float totalweight;

	void Start()
	{
		smokepos = transform.position;

		if ( poolSize > 0 && emitobjects.Count > 0)
		{
			pool.Clear();
			pool.Capacity = poolSize;

			totalweight = 0.0f;

			for ( int i = 0; i < emitobjects.Count; i++ )
			{
				emitobjects[i].tw = totalweight;
				totalweight += emitobjects[i].weight;
			}

			for ( int i = 0; i < poolSize; i++ )
			{
				GameObject go = CreateObj();
				Push(go);
			}
		}
	}

#if false
	GameObject CreateObj()
	{
		int index = (int)(Random.value * emitobjects.Count);
		index = Mathf.Clamp(index, 0, emitobjects.Count -1);

		GameObject go = (GameObject)GameObject.Instantiate(emitobjects[index], Vector3.zero, Quaternion.identity);
		go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
		go.name = "SmokeObj";

		MegaFlowEffect eff = go.GetComponent<MegaFlowEffect>();

		if ( eff )
		{
			eff.vel = vel;
			eff.source = source;
			eff.mass = mass;
			eff.Area = area;
			eff.scale = scale;
			eff.framenum = framenum;
		}

		return go;
	}
#else
	GameObject CreateObj()
	{
		float w = Random.Range(0.0f, totalweight);
		int index = 0;
		for ( int i = 0; i < emitobjects.Count; i++ )
		{
			if ( w >= emitobjects[i].tw && w <= emitobjects[i].tw + emitobjects[i].weight )
			{
				index = i;
				break;
			}
		}
		//int index = (int)(Random.value * emitobjects.Count);
		index = Mathf.Clamp(index, 0, emitobjects.Count - 1);

		if ( emitobjects[index].obj )
		{
			GameObject go = (GameObject)GameObject.Instantiate(emitobjects[index].obj.gameObject, Vector3.zero, Quaternion.identity);
			go.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
			go.name = "SmokeObj";

			MegaFlowEffect eff = go.GetComponent<MegaFlowEffect>();

			if ( eff )
			{
				eff.vel = vel;
				eff.source = source;
				eff.mass = mass;
				eff.Area = area;
				eff.scale = scale;
				eff.framenum = framenum;
				eff.emitindex = index;
			}

			return go;
		}

		return null;
	}
#endif

	GameObject Pop()
	{
		if ( pool.Count == 0 )
		{
			GameObject go = CreateObj();
			return go;
		}

		GameObject obj = pool[pool.Count - 1];
		obj.SetActive(true);
		pool.RemoveAt(pool.Count - 1);
		return obj;
	}

	void Push(GameObject obj)
	{
		obj.SetActive(false);
		pool.Add(obj);
	}

	void OnMouseDown()
	{
		screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
		offset1 = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
	}
 
	void OnMouseDrag()
	{
		Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
		Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset1;
		transform.position = curPosition;
	}

	void Update()
	{
		if ( Input.GetMouseButton(0) )
		{
			smokepos = transform.position;
			smokepos.z += Input.GetAxis("Mouse X") * xspeed * 0.02f;
			smokepos.y += Input.GetAxis("Mouse Y") * yspeed * 0.02f;
		}

		if ( source && source.frames.Count > 0 && emitobjects.Count > 0 )
		{
			source.Prepare();

			t += Time.deltaTime;

			if ( t > flowrate )
			{
				t -= flowrate;

				for ( int i = 0; i < count; i++ )
				{
					GameObject go = Pop();
					if ( go )
					{
						MegaFlowEffect ef = go.GetComponent<MegaFlowEffect>();
						MegaFlowSmokeObjDef def = emitobjects[ef.emitindex];

						float z = (Random.value - 0.5f) * width * 2.0f;
						float y = (Random.value - 0.5f) * height * 2.0f;
						float x = (Random.value - 0.5f) * 0.95f;

						Vector3 pos = Vector3.zero;
						pos.z = z;
						pos.y = y;
						pos.x = x;
						pos += offset;

						Vector3 scl = Vector3.one;

						scl.x = Random.Range(def.scalelow.x, def.scalehigh.x);	//Random.value + 0.4f) * 2.0f;
						scl.y = Random.Range(def.scalelow.y, def.scalehigh.y);	//Random.value + 0.4f) * 2.0f;
						scl.z = Random.Range(def.scalelow.z, def.scalehigh.z);	//Random.value + 0.4f) * 2.0f;
						//scl.y = (Random.value + 0.4f) * 2.0f;
						//scl.z = (Random.value + 0.4f) * 2.0f;

						Vector3 rotspeed = Vector3.zero;
						rotspeed.x = Random.Range(def.rotspeedlow.x, def.rotspeedhigh.x);
						rotspeed.y = Random.Range(def.rotspeedlow.y, def.rotspeedhigh.y);
						rotspeed.z = Random.Range(def.rotspeedlow.z, def.rotspeedhigh.z);

						Vector3 srot = Vector3.zero;
						srot.x = Random.Range(def.rotlow.x, def.rothigh.x);
						srot.y = Random.Range(def.rotlow.y, def.rothigh.y);
						srot.z = Random.Range(def.rotlow.z, def.rothigh.z);
						Quaternion rot = Quaternion.Euler(srot);	//Random.rotation;

						go.transform.position = transform.localToWorldMatrix.MultiplyPoint3x4(pos);
						go.transform.localScale = scl;

						ef.vel = transform.right * vel.x;
						ef.mass = mass;
						ef.Gravity = Gravity;
						ef.scale = scale;
						ef.framenum = framenum;

						ef.rot = rot.eulerAngles;
						ef.rotspeed = rotspeed;	//Random.insideUnitSphere * 180.0f;

						Renderer rend = go.GetComponent<Renderer>();

						if ( cols.Count > 0 )
						{
							if ( colindex >= cols.Count )
								colindex = 0;
							//Renderer rend = go.GetComponent<Renderer>();

							if ( rend && rend.material )
								rend.material.color = cols[colindex] * 0.5f;

							colindex++;
						}

						rend.enabled = false;

						MegaFlowSmokeObj sobj = new MegaFlowSmokeObj();
						sobj.obj = go;
						sobj.lifetime = lifetime;

						objects.Add(sobj);
					}
				}
			}
		}

		for ( int i = 0; i < objects.Count; i++ )
		{
			objects[i].lifetime -= Time.deltaTime;
			if ( objects[i].lifetime < 0.0f )
			{
				GameObject sobj = objects[i].obj;

				Push(sobj);
				objects.RemoveAt(i);
				i--;
			}
		}
	}
}