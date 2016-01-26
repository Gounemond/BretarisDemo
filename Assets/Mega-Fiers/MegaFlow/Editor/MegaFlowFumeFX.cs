
using UnityEngine;
using System.IO;

public class MegaFlowFumeFX
{
	public static MegaFlowFrame LoadFrame(string filename, int frame, string namesplit)
	{
		MegaFlowFrame flow = null;

		char[]	splits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

		string dir= Path.GetDirectoryName(filename);
		string file = Path.GetFileNameWithoutExtension(filename);

		string[] names;

		if ( namesplit.Length > 0 )
		{
			names = file.Split(namesplit[0]);
			names[0] += namesplit[0];
		}
		else
			names = file.Split(splits);

		if ( names.Length > 0 )
		{
			string newfname = dir + "/" + names[0] + frame.ToString("0000") + ".fxd";
			flow = LoadFrame(newfname);
		}

		return flow;
	}

#if false
	public static MegaFlowFrame LoadFrame(string filename, int frame)
	{
		MegaFlowFrame flow = null;

		char[]	splits = { '_' };

		string fname = filename; // use unity get path

		string[] names = fname.Split(splits);

		if ( names.Length > 0 )
		{
			string newfname = names[0] + "_" + frame.ToString("0000") + ".fxd";
			flow = LoadFrame(newfname);
		}

		return flow;
	}
#endif

	public static MegaFlowFrame LoadFrame(string filename)
	{
		MegaFlowFrame flow = null;

		if ( File.Exists(filename) )
		{
			flow = ScriptableObject.CreateInstance<MegaFlowFrame>();
			Load(flow, filename);
		}

		return flow;
	}

	static public void Load(MegaFlowFrame flow, string filename)
	{
		FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, System.IO.FileShare.Read);

		BinaryReader br = new BinaryReader(fs);

		if ( br != null )
		{
			flow.Init();
			Parse(flow, br);
			br.Close();
		}

		fs.Close();
	}

	static public void Parse(MegaFlowFrame flow, BinaryReader br)
	{
		int head = br.ReadInt16();	// head val

		flow.framenumber = br.ReadInt32();
		flow.fval = br.ReadSingle();
		flow.spacing.x = br.ReadSingle();
		flow.spacing.y = flow.spacing.x;
		flow.spacing.z = flow.spacing.x;
		flow.oos.x = 1.0f / flow.spacing.x;
		flow.oos.y = 1.0f / flow.spacing.y;
		flow.oos.z = 1.0f / flow.spacing.z;

		flow.size.x = br.ReadSingle();
		flow.size.z = br.ReadSingle();
		flow.size.y = br.ReadSingle();

		flow.gsize.x = br.ReadSingle();
		flow.gsize.z = br.ReadSingle();
		flow.gsize.y = br.ReadSingle();

		flow.gridDim[0] = br.ReadInt32();
		flow.gridDim[2] = br.ReadInt32();
		flow.gridDim[1] = br.ReadInt32();

		flow.gridDim1[0] = br.ReadInt32();
		flow.gridDim1[2] = br.ReadInt32();
		flow.gridDim1[1] = br.ReadInt32();

		flow.gridDim2[0] = br.ReadInt32();
		flow.gridDim2[2] = br.ReadInt32();
		flow.gridDim2[1] = br.ReadInt32();

		flow.somebool = br.ReadBoolean();
		flow.flags = br.ReadInt32();

		if ( head == 31 )
			br.ReadInt32();

		flow.fval1 = br.ReadSingle();

		if ( (flow.flags & 8) != 0 )
			ParseSmoke(flow, br);
		else
			Debug.Log("No Smoke");

		if ( (flow.flags & 0x20) != 0 )
			ParseVelocity(flow, br);
		else
			Debug.Log("No Vel");

		ParseGrid(flow, br);
	}

	static public bool ReadChunk(BinaryReader br)
	{
		uint[]	chunk = new uint[4];

		chunk[0] = br.ReadUInt32();
		chunk[1] = br.ReadUInt32();
		chunk[2] = br.ReadUInt32();
		chunk[3] = br.ReadUInt32();

		if ( chunk[0] == 0xaaaaaaaa && chunk[1] == 0xbbbbbbbb && chunk[2] == 0xcccccccc && chunk[3] == 0xdddddddd )
			return true;

		return false;
	}

	static public void ParseSmoke(MegaFlowFrame flow, BinaryReader br)
	{
		if ( ReadChunk(br) )
		{
			int count = br.ReadInt32();

			while ( count > 0 )
			{
				ushort cw = br.ReadUInt16();

				count--;

				if ( cw == 0xffff )
				{
					ushort zc = br.ReadUInt16();

					count--;
					for ( int i = 0; i < zc; i++ )
						flow.smoke.Add(0.0f);
				}
				else
				{
					float val = (float)cw / 65535.0f;	//32768.0f;	// / (float)cw;
					flow.smoke.Add(val);
				}
			}

			br.ReadSingle();	// val1 float
		}
	}

	// ff = do next byte of zeros then read words till
	// 7fff = do next word of zeros
	// Guess any shortfall in length is padded with zeroes
	static public void ParseVelocity(MegaFlowFrame flow, BinaryReader br)
	{
		int len = flow.gridDim2[0] * flow.gridDim2[1] * flow.gridDim2[2];

		for ( int j = 0; j < 3; j++ )
		{
			if ( ReadChunk(br) )
			{
				int count = br.ReadInt32();
				int index = 0;
				while ( count > 0 )
				{
					ushort data = br.ReadUInt16();
					count--;

					if ( data == 0x7fff )
					{
						ushort zc = br.ReadUInt16();
						count--;

						for ( int z = 0; z < zc; z++ )
						{
							if ( j == 0 )
							{
								flow.vel.Add(Vector3.zero);
								index++;
							}
							else
							{
								Vector3 v = flow.vel[index];
								switch ( j )
								{
									case 0: v.x = 0.0f; break;
									case 1: v.z = 0.0f; break;
									case 2: v.y = 0.0f; break;
								}
								flow.vel[index++] = v;
							}
						}
					}
					else
					{
						short v = (short)data;

						float val = (float)v / 32767.0f;	//32768.0f

						if ( j == 0 )
						{
							flow.vel.Add(new Vector3(val, 0.0f, 0.0f));
							index++;
						}
						else
						{
							Vector3 v1 = flow.vel[index];
							switch ( j )
							{
								case 0: v1.x = val; break;
								case 1: v1.z = val; break;
								case 2: v1.y = val; break;
							}
							flow.vel[index++] = v1;
						}
					}
				}

				for ( int p = index; p < len; p++ )
				{
					if ( j == 0 )
					{
						flow.vel.Add(Vector3.zero);
						index++;
					}
					else
					{
						Vector3 v1 = flow.vel[index];
						switch ( j )
						{
							case 0: v1.x = 0.0f; break;
							case 1: v1.z = 0.0f; break;
							case 2: v1.y = 0.0f; break;
						}
						flow.vel[index++] = v1;
					}
				}
			}
		}
	}

	static public void ParseForce(MegaFlowFrame flow, BinaryReader br)
	{
	}

	static public void ParseGrid(MegaFlowFrame flow, BinaryReader br)
	{
		if ( ReadChunk(br) )
		{
			for ( int i = 0; i < flow.gridDim[0] * flow.gridDim[1] * flow.gridDim[2]; i++ )
				flow.grid.Add(0);

			int length = br.ReadInt32();

			byte[] data = br.ReadBytes(length);

			int index = 0;

			int si = 0;
			while ( si < length )
			{
				byte ch = data[si++];

				if ( si >= length )
					break;

				if ( ch == 255 )
					index += data[si++];
				else
					flow.grid[index] = ch;
			}
		}
	}
}