
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class MegaFlowFLW
{
	public static MegaFlowFrame LoadFrame(string filename)
	{
		MegaFlowFrame flow = null;

		if ( File.Exists(filename) )
		{
			flow = ScriptableObject.CreateInstance<MegaFlowFrame>();
			flow.Init();
			LoadFLW(flow, filename);
		}

		return flow;
	}

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
			string newfname = dir + "/" + names[0] + frame + ".flw";
			flow = LoadFrame(newfname);
		}

		return flow;
	}

#if false
	public static MegaFlowFrame LoadFrame(string filename, int frame)
	{
		MegaFlowFrame flow = null;

		char[]	splits = { '-' };

		string fname = filename; // use unity get path

		string[] names = fname.Split(splits);

		if ( names.Length > 0 )
		{
			string newfname = names[0] + "-" + frame + ".flw";
			flow = LoadFrame(newfname);
		}

		return flow;
	}
#endif

	static public void LoadFLW(MegaFlowFrame flow, string filename)
	{
		StreamReader streamReader = new StreamReader(filename);
		string data = streamReader.ReadToEnd();
		streamReader.Close();

		MegaFlowXMLReader xml = new MegaFlowXMLReader();
		MegaFlowXMLNode node = xml.read(data);

		ParseXML1(flow, node);

		xml = null;
		data = null;
		GC.Collect();
	}

	static Vector3 ReadV3(string[] vals)
	{
		Vector3 v = Vector3.zero;

		v.x = float.Parse(vals[index++]);
		v.y = float.Parse(vals[index++]);
		v.z = float.Parse(vals[index++]);

		return v;
	}

	static Vector3 ReadV3Adj(string[] vals)
	{
		Vector3 v = ReadV3(vals);

		v.z = -v.z;

		return v;
	}

	static int index = 0;

	static public void ParseXML1(MegaFlowFrame flow, MegaFlowXMLNode node)
	{
		foreach ( MegaFlowXMLNode n in node.children )
		{
			switch ( n.tagName )
			{
				case "Fluid": ParseFluid(flow, n); break;
				default: Debug.Log("Unknown Fluid Node " + n.tagName); break;
			}
		}
	}

	static void ParseFluid(MegaFlowFrame flow, MegaFlowXMLNode node)
	{
		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaFlowXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "grid":
					{
						string[] vals = val.value.Split(',');
						flow.gridDim2[0] = int.Parse(vals[0]);
						flow.gridDim2[1] = int.Parse(vals[1]);
						flow.gridDim2[2] = int.Parse(vals[2]);
						break;
					}

				case "size":
					{
						string[] vals = val.value.Split(',');
						index = 0;
						Vector3 bmin = ReadV3(vals);
						Vector3 bmax = ReadV3(vals);

						flow.size = bmax - bmin;
						flow.gsize = flow.size;

						// griddim should have a name change
						flow.spacing.x = flow.size.x / flow.gridDim2[0];
						flow.spacing.y = flow.size.y / flow.gridDim2[1];
						flow.spacing.z = flow.size.z / flow.gridDim2[2];
						flow.oos.x = 1.0f / flow.spacing.x;
						flow.oos.y = 1.0f / flow.spacing.y;
						flow.oos.z = 1.0f / flow.spacing.z;
						break;
					}

				default: Debug.Log("Unknown Fluid attribute " + val.name); break;
			}
		}

		for ( int i = 0; i < node.children.Count; i++ )
		{
			MegaFlowXMLNode n = (MegaFlowXMLNode)node.children[i];
			switch ( n.tagName )
			{
				case "Vel":
					ParseVel(flow, n);
					break;

				case "Force":
					break;

				case "Density":
					break;

				default: Debug.Log("Unknown Fluid node " + n.tagName); break;
			}
		}
	}

	static void ParseVel(MegaFlowFrame flow, MegaFlowXMLNode node)
	{
		for ( int i = 0; i < node.values.Count; i++ )
		{
			MegaFlowXMLValue val = node.values[i];

			switch ( val.name )
			{
				case "data":
					{
						string[] vals = val.value.Split(',');
						index = 0;

						//int len = vals.Length / 3;

						flow.vel.Clear();

						Vector3[] vels = new Vector3[flow.gridDim2[0] * flow.gridDim2[1] * flow.gridDim2[2]];

						for ( int z = 0; z < flow.gridDim2[2]; z++ )
						{
							for ( int y = 0; y < flow.gridDim2[1]; y++ )
							{
								for ( int x = 0; x < flow.gridDim2[0]; x++ )
									vels[(x * flow.gridDim2[2] * flow.gridDim2[1]) + ((flow.gridDim2[2] - z - 1) * flow.gridDim2[1]) + y] = ReadV3Adj(vals);
							}
						}

						flow.framenumber = 0;
						flow.vel.AddRange(vels);
						break;
					}

				default: Debug.Log("Unknown Vel attribute " + val.name); break;
			}
		}
	}
}
