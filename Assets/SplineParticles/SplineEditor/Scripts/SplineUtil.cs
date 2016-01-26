using UnityEngine;
using System.Collections;

namespace PigtailGames
{
	public class SplineUtil
	{
		static public int ClampIndex(int idx, int len)
		{
			if(idx < 0)
			{
				idx = 0;
			}
			else if(idx > len - 1)
			{
				idx = len - 1;
			}
			return idx;
		}
			
		static public int WrapIndex(int idx, int len)
		{
			if(idx < 0)
			{
				idx = len + idx % len;
			}
			else if(idx >= len - 1)
			{
				idx = idx % len;
			}
			return idx;
		}
		
		static public float WrapPosition(BaseSpline.SplineWrapMode wrapmode, float pos, float len)
		{
			switch(wrapmode)
			{
			case BaseSpline.SplineWrapMode.Loop:
				if(pos < 0)
				{
					int tms = (int)(-pos / len) + 1;
					pos += tms * len;
				}
				else if(pos >= len)
				{
					int tms = (int)(pos / len);
					pos -= tms * len;
				}
				break;
				
			case BaseSpline.SplineWrapMode.Once:
				if(pos < 0)
				{
					pos = 0;
				}
				else if(pos > len)
				{
					pos = len;
				}
				break;
			}
			return pos;
		}
	}
}