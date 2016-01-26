
using UnityEngine;
using System.Collections.Generic;
#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
using System.Threading;
#endif

[AddComponentMenu("MegaFlow/Particle Moving")]
public class MegaFlowParticleMoving : MonoBehaviour
{
	public MegaFlowMovingSource	msource;
	MegaFlowFrame				frame;
	public int					framenum;
	[Range(0.01f, 2.0f)]
	public float				mass			= 0.02f;
	public float				area			= 0.2f;
	public ParticleSystem		particle;
	public float				dt				= 0.01f;
	public float				scale			= 1.0f;
	public int					maxparticles	= 1000;
	ParticleSystem.Particle[]	particles;
	Matrix4x4					tm;
	Matrix4x4					invtm;
	float						tdt;
	float						coef;
	float						oomass;
	int							Cores			= 0;
	float						usedt			= 0.0f;
	public float				speed			= 0.0f;
	public float				gravity			= 0.0f;

	[ContextMenu("Help")]
	public void Help()
	{
		Application.OpenURL("http://www.west-racing.com/mf/?page_id=5896");
	}

	public void SetFrame(int f)
	{
		if ( msource && msource.source )
		{
			if ( f >= 0 && f < msource.source.frames.Count )
			{
				frame = msource.source.frames[f];
				framenum = f;
			}
		}
	}

	// Flow source should be normalized
	void RunSim(int start, int end)
	{
		Vector3 tvel;
		Vector3 acc;
		Vector3 dir;

		float fvel = 0.0f;
		float falloff = 0.0f;
		int mframe = 0;

		Matrix4x4 ftm = invtm;
		Vector3 flowpos = Vector3.zero;

		for ( int i = start; i < end; i++ )
		{
			Vector3 pos = particles[i].position;
			Vector3 vel = particles[i].velocity;

			float duration = tdt;
			bool inbounds = true;

			// Not really correct to do here but it is close enough and will save a lot of cpu time
			//flowpos = msource.FindFlowPos(pos, ref inbounds, ref ftm, ref fvel);
			flowpos = msource.FindFlowPos(pos, ref inbounds, ref ftm, ref fvel, ref mframe, ref falloff);

			while ( duration > 0.0001f )
			{
				if ( inbounds )
				{
					Vector3 airvel = ftm.MultiplyVector(frame.GetGridVel(flowpos, ref inbounds));

					if ( inbounds )
					{
						float scl = scale * fvel * falloff;

						airvel.x *= scl;
						airvel.y *= scl;
						airvel.z *= scl;

						if ( airvel.sqrMagnitude > 0.1f )
						{
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
								acc.y = dir.y * dfm + gravity;
								acc.z = dir.z * dfm;

								vel.x += acc.x * usedt;
								vel.y += acc.y * usedt;
								vel.z += acc.z * usedt;

								pos.x += vel.x * usedt;
								pos.y += vel.y * usedt;
								pos.z += vel.z * usedt;
							}
						}
						else
						{
							vel.y += gravity * usedt;
							pos.y += vel.y * usedt;
						}
					}
					else
					{
						vel.y += gravity * usedt;
						pos.y += vel.y * usedt;
					}
				}
				else
				{
					vel.y += gravity * usedt;
					pos.y += vel.y * usedt;

				}
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
			particle = GetComponent<ParticleSystem>();

		particles = new ParticleSystem.Particle[maxparticles];
	}

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
	public bool			UseThreading = false;

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
		if ( msource && msource.source && particle )
		{
			msource.framenum = framenum;

			framenum = Mathf.Clamp(framenum, 0, msource.source.frames.Count - 1);
			frame = msource.source.frames[framenum];

			msource.source.Prepare();
			tdt = Time.deltaTime;

			Matrix4x4 offtm = Matrix4x4.TRS(frame.size * 0.5f, Quaternion.identity, Vector3.one);
			tm = offtm * msource.source.transform.worldToLocalMatrix;
			invtm = tm.inverse;

			float p = msource.source.Density;
			float A = area;
			float Re = msource.source.Reynolds;
			oomass = 1.0f / mass;

			coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f);

			usedt = dt;
			if ( usedt > tdt )
				usedt = tdt;

			int count = particle.GetParticles(particles);

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

			particle.SetParticles(particles, count);
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
		if ( msource && msource.source && particle )
		{
			msource.framenum = framenum;

			framenum = Mathf.Clamp(framenum, 0, msource.source.frames.Count - 1);
			frame = msource.source.frames[framenum];

			source.Prepare();
			tdt = Time.deltaTime;

			Matrix4x4 offtm = Matrix4x4.TRS(frame.size * 0.5f, Quaternion.identity, Vector3.one);
			tm = offtm * msource.source.transform.worldToLocalMatrix;
			invtm = tm.inverse;

			float p = msource.source.Density;
			float A = area;
			float Re = msource.source.Reynolds;
			oomass = 1.0f / mass;

			coef = 1.0f * p * A * Mathf.Pow(Re, -0.5f);

			usedt = dt;
			if ( usedt > tdt )
				usedt = tdt;

			int count = particle.GetParticles(particles);

			RunSim(0, count);

			particle.SetParticles(particles, count);
		}
	}
#endif
}