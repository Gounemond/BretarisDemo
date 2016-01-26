
using UnityEngine;
using System.Collections.Generic;
using System.IO;

// This is also a seq of files for particle system and I presume for mesh stuff if we do it
public class MegaFlowRealFlow
{
	static public Vector3 ReadVector3(BinaryReader br)
	{
		Vector3 p = Vector3.zero;

		p.x = br.ReadSingle();
		p.y = br.ReadSingle();
		p.z = br.ReadSingle();

		return p;
	}

	// We should interpolate the frames
	// Looks like there will be a file per frame, so need to load all in directory etc, then build anim data from there
	public void LoadFrames(string path)
	{
		string[] files = Directory.GetFiles(path);

		frames.Clear();

		for ( int i = 0; i < files.Length; i++ )
		{
			string ext = Path.GetExtension(files[i]);
			if ( ext == ".bin" )
			{
				//string filename = Path.GetFileNameWithoutExtension(files[i]);
				MegaFlowParticleFrame frame = Load(files[i]);
				frames.Add(frame);
			}
		}

	}

	List<MegaFlowParticleFrame>	frames = new List<MegaFlowParticleFrame>();

	MegaFlowParticleFrame Load(string filename)
	{
		FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read);

		BinaryReader br = new BinaryReader(fs);

		if ( br != null )
		{
			//ParticleFrame frame = new ParticleFrame();

			MegaFlowParticleFrame frame = MegaFlowParticleFrame.Parse(br);

			//DebugSD();

			br.Close();
			return frame;
		}

		return null;
	}

#if false
	// Have the ParticleSystem.Particles array in here then it is just a case of setting it
	public class ParticleFrame
	{
		public string	fluidname;
		public ushort	version;
		public float	scale;
		public int		ftype;
		public float	etime;
		public int		fnum;
		public int		fps;
		public int		numparticles;
		public float	radius;
		public Vector3	pressure;
		public Vector3	speed;
		public Vector3	temperature;
		public Vector3	emitpos;
		public Vector3	emitrot;
		public Vector3	emitscl;

		public ParticleSystem.Particle[]	particles1;
		public RealFlowParticleData[]	particles;

		static public ParticleFrame Parse(BinaryReader br)
		{
			uint code = br.ReadUInt32();

			if ( code == 0xFABADA )
			{
				ParticleFrame frame = new ParticleFrame();

				frame.fluidname = br.ReadBytes(250).ToString();

				frame.version = br.ReadUInt16();
				frame.scale = br.ReadSingle();
				frame.ftype = br.ReadInt32();
				frame.etime = br.ReadSingle();
				frame.fnum = br.ReadInt32();
				frame.fps = br.ReadInt32();
				frame.numparticles = br.ReadInt32();
				frame.radius = br.ReadSingle();
				frame.pressure = ReadVector3(br);
				frame.speed = ReadVector3(br);
				frame.temperature = ReadVector3(br);

				if ( frame.version >= 7.0f )
				{
					frame.emitpos = ReadVector3(br);
					frame.emitrot = ReadVector3(br);
					frame.emitscl = ReadVector3(br);
				}

				frame.particles = new RealFlowParticleData[frame.numparticles];

				for ( int i = 0; i < frame.numparticles; i++ )
				{
					RealFlowParticleData p = RealFlowParticleData.Parse(br, frame.version);
					frame.particles[i] = p;
				}

				return frame;
			}

			return null;
		}
	}

	[System.Serializable]
	public class RealFlowParticleData
	{
		public Vector3	pos;
		public Vector3	vel;
		public Vector3	frc;
		public Vector3	vor;
		public Vector3	norm;
		public int		numneigh;
		public Vector3	uvvector;
		public ushort	info;
		public float	elapsedtime;
		public float	isotime;
		public float	viscosity;
		public float	density;
		public float	pressure;
		public float	mass;
		public float	temperature;
		public uint		id;

		public static RealFlowParticleData Parse(BinaryReader br, float version)
		{
			RealFlowParticleData p = new RealFlowParticleData();

			//uint code = br.ReadUInt32();
			p.pos = ReadVector3(br);
			p.vel = ReadVector3(br);
			p.frc = ReadVector3(br);
			p.vor = ReadVector3(br);
			p.norm = ReadVector3(br);
			p.numneigh = br.ReadInt32();
			p.uvvector = ReadVector3(br);
			p.info = br.ReadUInt16();
			p.elapsedtime = br.ReadSingle();
			p.isotime = br.ReadSingle();
			p.viscosity = br.ReadSingle();
			p.density = br.ReadSingle();
			p.pressure = br.ReadSingle();
			p.mass = br.ReadSingle();
			p.temperature = br.ReadSingle();

			if ( version >= 12.0f )
				p.id = (uint)br.ReadUInt64();
			else
				p.id = br.ReadUInt32();

			return p;
		}
	}
#endif
}

[System.Serializable]
public class MegaFlowParticleData
{
	public Vector3	pos;
	public Vector3	vel;
	public Vector3	frc;
	public Vector3	vor;
	public Vector3	norm;
	public int		numneigh;
	public Vector3	uvvector;
	public ushort	info;
	public float	elapsedtime;
	public float	isotime;
	public float	viscosity;
	public float	density;
	public float	pressure;
	public float	mass;
	public float	temperature;
	public uint		id;

	static public Vector3 ReadVector3(BinaryReader br)
	{
		Vector3 p = Vector3.zero;

		p.x = br.ReadSingle();
		p.y = br.ReadSingle();
		p.z = br.ReadSingle();

		return p;
	}

	public static MegaFlowParticleData Parse(BinaryReader br, float version)
	{
		MegaFlowParticleData p = new MegaFlowParticleData();

		//uint code = br.ReadUInt32();
		p.pos = ReadVector3(br);
		p.vel = ReadVector3(br);
		p.frc = ReadVector3(br);
		p.vor = ReadVector3(br);
		p.norm = ReadVector3(br);
		p.numneigh = br.ReadInt32();
		p.uvvector = ReadVector3(br);
		p.info = br.ReadUInt16();
		p.elapsedtime = br.ReadSingle();
		p.isotime = br.ReadSingle();
		p.viscosity = br.ReadSingle();
		p.density = br.ReadSingle();
		p.pressure = br.ReadSingle();
		p.mass = br.ReadSingle();
		p.temperature = br.ReadSingle();

		if ( version >= 12.0f )
			p.id = (uint)br.ReadUInt64();
		else
			p.id = br.ReadUInt32();

		return p;
	}

	public static ParticleSystem.Particle LoadParticleData(BinaryReader br, float version)
	{
		ParticleSystem.Particle p = new ParticleSystem.Particle();

		p.position = ReadVector3(br);
		p.velocity = ReadVector3(br);

		ReadVector3(br);		// frc
		ReadVector3(br);		// vor
		ReadVector3(br);		// norm
		br.ReadInt32();		// numneigh
		ReadVector3(br);	// uvvector
		br.ReadUInt16();		// info
		br.ReadSingle();
		br.ReadSingle();	// isotime
		br.ReadSingle();	// viscosity
		br.ReadSingle();	// density
		br.ReadSingle();	// pressure
		br.ReadSingle();	// mass
		br.ReadSingle();	// temperature

		if ( version >= 12.0f )
			br.ReadUInt64();
		else
			br.ReadUInt32();

		return p;
	}
}

// Have the ParticleSystem.Particles array in here then it is just a case of setting it
public class MegaFlowParticleFrame
{
	public string	fluidname;
	public ushort	version;
	public float	scale;
	public int		ftype;
	public float	etime;
	public int		fnum;
	public int		fps;
	public int		numparticles;
	public float	radius;
	public Vector3	pressure;
	public Vector3	speed;
	public Vector3	temperature;
	public Vector3	emitpos;
	public Vector3	emitrot;
	public Vector3	emitscl;

	public ParticleSystem.Particle[]	particles1;
	public MegaFlowParticleData[]	particles;

	static public Vector3 ReadVector3(BinaryReader br)
	{
		Vector3 p = Vector3.zero;

		p.x = br.ReadSingle();
		p.y = br.ReadSingle();
		p.z = br.ReadSingle();

		return p;
	}

	static public MegaFlowParticleFrame Parse(BinaryReader br)
	{
		uint code = br.ReadUInt32();

		if ( code == 0xFABADA )
		{
			MegaFlowParticleFrame frame = new MegaFlowParticleFrame();

			frame.fluidname = br.ReadBytes(250).ToString();

			frame.version = br.ReadUInt16();
			frame.scale = br.ReadSingle();
			frame.ftype = br.ReadInt32();
			frame.etime = br.ReadSingle();
			frame.fnum = br.ReadInt32();
			frame.fps = br.ReadInt32();
			frame.numparticles = br.ReadInt32();
			frame.radius = br.ReadSingle();
			frame.pressure = ReadVector3(br);
			frame.speed = ReadVector3(br);
			frame.temperature = ReadVector3(br);

			if ( frame.version >= 7.0f )
			{
				frame.emitpos = ReadVector3(br);
				frame.emitrot = ReadVector3(br);
				frame.emitscl = ReadVector3(br);
			}

			frame.particles = new MegaFlowParticleData[frame.numparticles];
			frame.particles1 = new ParticleSystem.Particle[frame.numparticles];

			for ( int i = 0; i < frame.numparticles; i++ )
			{
				ParticleSystem.Particle p = MegaFlowParticleData.LoadParticleData(br, frame.version);
				frame.particles1[i] = p;
				//MegaFlowParticleData p = MegaFlowParticleData.Parse(br, frame.version);
				//frame.particles[i] = p;
			}

			return frame;
		}

		return null;
	}
}
