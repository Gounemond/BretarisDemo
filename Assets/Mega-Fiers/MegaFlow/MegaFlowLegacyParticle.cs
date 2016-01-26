
using UnityEngine;
using System.Collections.Generic;
#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
using System.Threading;
#endif

[AddComponentMenu("MegaFlow/Legacy Particle Control")]
public class MegaFlowLegacyParticle : MonoBehaviour
{
	public MegaFlow				source;
	MegaFlowFrame				frame;
	public int					framenum;
	//public MegaFlow				source1;
	MegaFlowFrame				frame1;
	public int					framenum1;
	public Vector3				vel = Vector3.zero;
	float						t			= 0.0f;
	[Range(0.01f, 2.0f)]
	public float				mass		= 0.02f;
	public float				area		= 0.2f;
	public float				dt			= 0.01f;
	public float				scale		= 1.0f;
	public int					maxparticles = 1000;
	Matrix4x4					tm;
	Matrix4x4					invtm;
	Matrix4x4					tm1;
	Matrix4x4					invtm1;
	float						tdt;
	float						coef;
	float						oomass;
	int							Cores		= 0;
	float						usedt = 0.0f;
	public ParticleEmitter		particle;
	Particle[]					particles;

	[Range(0.0f, 1.0f)]
	public float				framealpha = 0.0f;
	public bool					interp = false;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=5960");
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

	public void SetFrame1(int f)
	{
		if ( source )
		{
			if ( f >= 0 && f < source.frames.Count )
			{
				frame1 = source.frames[f];
				framenum1 = f;
			}
		}
	}

	void RunSim(int start, int end)
	{
		Vector3 tvel;
		Vector3 acc;
		Vector3 dir;

		if ( interp && framealpha != 0.0f )
		{
			RunSimNew(start, end);
			return;
		}

		float scl = source.Scale * scale;

		for ( int i = start; i < end; i++ )
		{
			Vector3 pos = particles[i].position;
			Vector3 vel = particles[i].velocity;

			float duration = tdt;
			bool inbounds = true;

			Vector3 flowpos = tm.MultiplyPoint3x4(pos);

			while ( duration > 0.0001f )
			{
				Vector3 airvel = invtm.MultiplyVector(frame.GetGridVel(flowpos, ref inbounds));

				if ( inbounds )
				{
					airvel.x *= scl;
					airvel.y *= scl;
					airvel.z *= scl;

					tvel.x = airvel.x - vel.x;
					tvel.y = airvel.y - vel.y;
					tvel.z = airvel.z - vel.z;

					float l = (tvel.x * tvel.x) + (tvel.y * tvel.y) + (tvel.z * tvel.z);

					if ( l != 0.0f )
					{
						l = Mathf.Sqrt(l);

						float df = coef * l;
						l = 1.0f / l;

						dir.x = tvel.x * l;
						dir.y = tvel.y * l;
						dir.z = tvel.z * l;

						float dfm = df * oomass;

						acc.x = dir.x * dfm;
						acc.y = dir.y * dfm;
						acc.z = dir.z * dfm;

						vel.x += acc.x * usedt;
						vel.y += acc.y * usedt;
						vel.z += acc.z * usedt;
					}
				}

				pos.x += vel.x * usedt;
				pos.y += vel.y * usedt;
				pos.z += vel.z * usedt;

				duration -= usedt;
			}

			particles[i].position = pos;
			particles[i].velocity = vel;
		}
	}

	void RunSimNew(int start, int end)
	{
		Vector3 tvel;
		Vector3 acc;
		Vector3 dir;

		float scl = source.Scale * scale;

		for ( int i = start; i < end; i++ )
		{
			Vector3 pos = particles[i].position;
			Vector3 vel = particles[i].velocity;

			float duration = tdt;
			bool inbounds = true;
			bool inbounds1 = true;

			Vector3 flowpos = tm.MultiplyPoint3x4(pos);
			Vector3 flowpos1 = tm1.MultiplyPoint3x4(pos);

			while ( duration > 0.0001f )
			{
				Vector3 airvel = invtm.MultiplyVector(frame.GetGridVel(flowpos, ref inbounds));

				if ( inbounds )
				{
					Vector3 airvel1 = invtm1.MultiplyVector(frame1.GetGridVel(flowpos1, ref inbounds1));

					if ( inbounds1 )
					{
						airvel = Vector3.Lerp(airvel, airvel1, framealpha);
						airvel.x *= scl;
						airvel.y *= scl;
						airvel.z *= scl;

						tvel.x = airvel.x - vel.x;
						tvel.y = airvel.y - vel.y;
						tvel.z = airvel.z - vel.z;

						float l = (tvel.x * tvel.x) + (tvel.y * tvel.y) + (tvel.z * tvel.z);

						if ( l != 0.0f )
						{
							l = Mathf.Sqrt(l);

							float df = coef * l;
							l = 1.0f / l;

							dir.x = tvel.x * l;
							dir.y = tvel.y * l;
							dir.z = tvel.z * l;

							float dfm = df * oomass;

							acc.x = dir.x * dfm;
							acc.y = dir.y * dfm;
							acc.z = dir.z * dfm;

							vel.x += acc.x * usedt;
							vel.y += acc.y * usedt;
							vel.z += acc.z * usedt;
						}
					}
				}

				pos.x += vel.x * usedt;
				pos.y += vel.y * usedt;
				pos.z += vel.z * usedt;

				duration -= usedt;
			}

			particles[i].position = pos;
			particles[i].velocity = vel;
		}
	}


	void Start()
	{
		if ( Cores == 0 )
			Cores = SystemInfo.processorCount - 1;

		if ( particle == null )
			particle = GetComponent<ParticleEmitter>();

		particles = new Particle[maxparticles];
	}

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	public bool			UseThreading = false;

	public class MegaFlowTaskInfo
	{
		public volatile int		start;
		public volatile int		end;
		public AutoResetEvent	pauseevent;
		public Thread			_thread;
	}

	public MegaFlowTaskInfo[]	tasks;

	void MakeThreads()
	{
		if ( Cores > 0 )
		{
			isRunning = true;
			tasks = new MegaFlowTaskInfo[Cores];

			for ( int i = 0; i < Cores; i++ )
			{
				tasks[i] = new MegaFlowTaskInfo();

				tasks[i].pauseevent = new AutoResetEvent(false);
				tasks[i]._thread = new Thread(DoWork);
				tasks[i]._thread.Start(tasks[i]);
			}
		}
	}

	void Update()
	{
		if ( source && particle && source.frames.Count > 0 )
		{
			framenum = Mathf.Clamp(framenum, 0, source.frames.Count - 1);
			frame = source.frames[framenum];

			framenum1 = Mathf.Clamp(framenum1, 0, source.frames.Count - 1);
			frame1 = source.frames[framenum1];

			source.Prepare();
			tdt = Time.deltaTime;

			//Matrix4x4 offtm = Matrix4x4.TRS(frame.size * 0.5f, Quaternion.identity, Vector3.one);
			Matrix4x4 offtm = Matrix4x4.TRS((frame.size * 0.5f) + frame.offset, Quaternion.identity, Vector3.one);

			tm = offtm * source.transform.worldToLocalMatrix;
			invtm = tm.inverse;

			offtm = Matrix4x4.TRS((frame1.size * 0.5f) + frame1.offset, Quaternion.identity, Vector3.one);

			tm1 = offtm * source.transform.worldToLocalMatrix;
			invtm1 = tm1.inverse;

			float p = source.Density;
			float A = area;
			float Re = source.Reynolds;
			oomass = 1.0f / mass;

			coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f);

			t += Time.deltaTime;
			usedt = dt;
			if ( usedt > tdt )
				usedt = tdt;

			int count = particle.particleCount;
			particles = particle.particles;

			if ( !UseThreading || Cores < 1 || !Application.isPlaying )
				RunSim(0, count);
			else
			{
				if ( Cores == 0 )
					Cores = SystemInfo.processorCount - 1;

				if ( tasks == null )
					MakeThreads();

				int step = count / (Cores + 1);

				if ( Cores > 0 )
				{
					int index = step;
					for ( int i = 0; i < tasks.Length; i++ )
					{
						tasks[i].start = index;
						tasks[i].end = index + step;
						index += step;
					}

					tasks[Cores - 1].end = count;

					for ( int i = 0; i < tasks.Length; i++ )
						tasks[i].pauseevent.Set();
				}

				RunSim(0, step);
				WaitJobs();
			}

			particle.particles = particles;
		}
	}

	void WaitJobs()
	{
		if ( Cores > 0 )
		{
			int	count = 0;
			bool wait = false;
			do
			{
				wait = false;
				for ( int i = 0; i < tasks.Length; i++ )
				{
					if ( tasks[i].end > 0 )
					{
						wait = true;
						break;
					}
				}

				if ( wait )
				{
					count++;
					Thread.Sleep(0);
				}
			} while ( wait );
		}
	}

	void OnApplicationQuit()
	{
		if ( Application.isPlaying )
		{
			isRunning = false;

			if ( tasks != null )
			{
				for ( int i = 0; i < tasks.Length; i++ )
				{
					tasks[i].pauseevent.Set();

					while ( tasks[i]._thread.IsAlive )
					{
					}
				}
			}
			tasks = null;
		}
	}

	static bool isRunning = true;

	void DoWork(object info)
	{
		MegaFlowTaskInfo inf = (MegaFlowTaskInfo)info;

		while ( isRunning )
		{
			inf.pauseevent.WaitOne(Timeout.Infinite, false);

			RunSim(inf.start, inf.end);
			inf.end = 0;
		}
	}
#else
	void Update()
	{
		if ( source && particle && source.frames.Count > 0 )
		{
			//tdt = Time.deltaTime;
			//tm = source.transform.worldToLocalMatrix;
			//invtm = source.transform.localToWorldMatrix;

			framenum = Mathf.Clamp(framenum, 0, source.frames.Count - 1);
			frame = source.frames[framenum];

			framenum1 = Mathf.Clamp(framenum1, 0, source.frames.Count - 1);
			frame1 = source.frames[framenum1];

			source.Prepare();
			tdt = Time.deltaTime;

			//Matrix4x4 offtm = Matrix4x4.TRS(frame.size * 0.5f, Quaternion.identity, Vector3.one);
			Matrix4x4 offtm = Matrix4x4.TRS((frame.size * 0.5f) + frame.offset, Quaternion.identity, Vector3.one);

			tm = offtm * source.transform.worldToLocalMatrix;
			invtm = tm.inverse;

			offtm = Matrix4x4.TRS((frame1.size * 0.5f) + frame1.offset, Quaternion.identity, Vector3.one);

			tm1 = offtm * source.transform.worldToLocalMatrix;
			invtm1 = tm1.inverse;

			float p = source.Density;
			float A = Area;
			float Re = source.Reynolds;

			coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f);

			t += Time.deltaTime;

			int count = particle.particleCount;
			particles = particle.particles;

			RunSim(0, count);

			particle.particles = particles;
		}
	}
#endif

	public void UpdateSim()
	{
		if ( source && particle && source.frames.Count > 0 )
		{
			frame = source.frames[framenum];

			tdt = Time.deltaTime;
			//Matrix4x4 offtm = Matrix4x4.TRS(frame.size * 0.5f, Quaternion.identity, Vector3.one);
			Matrix4x4 offtm = Matrix4x4.TRS((frame.size * 0.5f) + frame.offset, Quaternion.identity, Vector3.one);

			tm = offtm * source.transform.worldToLocalMatrix;
			invtm = tm.inverse;

			float p = source.Density;
			float A = area;
			float Re = source.Reynolds;

			coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f);

			t += Time.deltaTime;

			int count = particle.particleCount;
			particles = particle.particles;

			RunSim(0, count);

			particle.particles = particles;
		}
	}
}
