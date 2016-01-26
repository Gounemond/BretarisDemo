using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;

namespace SplineEditor
{
	public class ToolsUtil
	{
	    public static bool Hidden
	    {
	        get
	        {
	            return (bool)typeof(Tools).GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
	        }
	        set
	        {
	            typeof(Tools).GetField("s_Hidden", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
	        }
	    }
	}
}