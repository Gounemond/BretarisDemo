

using UnityEngine;

#if !UNITY_FLASH && !UNITY_PS3 && !UNITY_METRO && !UNITY_WP8
using System.Threading;

public class MegaFlowTaskInfo
{
	public volatile int		start;
	public volatile int		end;
	public AutoResetEvent	pauseevent;
	public Thread			_thread;
}
#endif