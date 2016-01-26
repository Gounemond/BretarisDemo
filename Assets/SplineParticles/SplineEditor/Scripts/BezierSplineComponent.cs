using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


namespace PigtailGames
{
		
		[Serializable]
		public class BezierSpline : PigtailGames.BaseSpline
		{
			
		public delegate void SplineRebuild();
		public event SplineRebuild OnSplineRebuild;

			public class EditHelper
			{
				internal EditHelper(BezierSpline spline)
				{
					m_spline = spline;
				}
				
				public bool MoveNext()
				{
					++m_idx;
					if(m_idx < m_spline.m_points.Count)
					{
						return true;
					}
					return false;
				}
				
				public void Reset()
				{
					m_idx = -1;
				}
				
				public void AppendPoint()
				{
					if(m_spline.m_points.Count == 0)
					{
						m_spline.AppendPoint(Vector3.zero, BaseSpline.SplinePointType.Smooth, Vector3.zero, Vector3.zero);
					}
					else
					{
						m_spline.AppendPoint(m_spline.m_points[m_spline.m_points.Count - 1].m_point + Vector3.right, BaseSpline.SplinePointType.Smooth, Vector3.zero, Vector3.zero);
					}
					m_selidx = m_spline.m_points.Count - 1;			
				}
				
				public void InsertBefore()
				{
					if(m_spline.m_points.Count == 1)
					{
						m_spline.InsertPoint(0, m_spline.m_points[m_spline.m_points.Count - 1].m_point + Vector3.right, BaseSpline.SplinePointType.Smooth, Vector3.zero, Vector3.zero);
					}
					else
					{
						int previdx = m_selidx;
						--m_selidx;
						if(m_selidx < 0)
						{
							m_selidx = m_spline.m_points.Count - 1;
						}
						m_spline.InsertPoint(previdx, (m_spline.m_points[m_selidx].m_point + m_spline.m_points[previdx].m_point) * 0.5f, BaseSpline.SplinePointType.Smooth, Vector3.zero, Vector3.zero);
						m_selidx = previdx;
					}
				}
				
				public void InsertAfter()
				{
					if(m_spline.m_points.Count == 1)
					{
						m_spline.InsertPoint(0, m_spline.m_points[m_spline.m_points.Count - 1].m_point + Vector3.right, BaseSpline.SplinePointType.Smooth, Vector3.zero, Vector3.zero);
					}
					else
					{
						int previdx = m_selidx;
						++m_selidx;
						if(m_selidx == m_spline.m_points.Count)
						{
							m_selidx = m_spline.m_points.Count - 1;
						}
						m_spline.InsertPoint(m_selidx, (m_spline.m_points[m_selidx].m_point + m_spline.m_points[previdx].m_point) * 0.5f, BaseSpline.SplinePointType.Smooth, Vector3.zero, Vector3.zero);
					}
				}
				
				public void Remove()
				{
					m_spline.m_points.RemoveAt(m_selidx);
					if(m_selidx >= m_spline.m_points.Count)
					{
						m_selidx = m_spline.m_points.Count - 1;
					}
				}
				
				public void RemoveLast()
				{
					if(m_spline.m_points.Count > 0)
					{
						m_spline.RemoveLastPoint();
					}
					if(m_selidx >= m_spline.m_points.Count)
					{
						m_selidx = m_spline.m_points.Count - 1;
					}
				}
				
				public void SelectFirst()
				{
					if(m_spline.m_points.Count > 0)
					{
						m_selidx = 0;
					}
					else
					{
						m_selidx = -1;
					}
				}
				
				public void SelectNext()
				{
					if(m_selidx < m_spline.m_points.Count - 1)
					{
						++m_selidx;
					}
					else
					{
						m_selidx = 0;
					}
				}
				
				public void SelectPrev()
				{
					if(m_selidx > 0)
					{
						--m_selidx;
					}
					else
					{
						m_selidx = m_spline.m_points.Count - 1;
					}
				}
				
				private BezierSpline m_spline;
				private int m_idx = -1, m_selidx = -1, m_selcpidx = -1;
				
				public BezierSplinePoint Point
				{
					get { return m_spline.m_points[m_idx]; }
					set { m_spline.m_points[m_idx] = value; }
				}
				
				public BezierSplinePoint SelectedPoint
				{
					get { return m_spline.m_points[m_selidx]; }
					set { m_spline.m_points[m_selidx] = value; }
				}
						
				public bool Selected
				{
					get { return m_idx == m_selidx; }
					set { if(value) { m_selidx = m_idx; } else { m_selidx = -1; } m_selcpidx = -1; }
				}
				
				
				public bool ControlPoint1Selected
				{
					get { return m_selcpidx == 0; }
					set { if(value) { m_selcpidx = 0; } else { m_selcpidx = -1; } }
				}
				
				public bool ControlPoint2Selected
				{
					get { return m_selcpidx == 1; }
					set { if(value) { m_selcpidx = 1; } else { m_selcpidx = -1; } }
				}
				
				public bool ControlPointsVisible
				{
					get
					{
						if(m_selidx == - 1) return false;
						return (m_spline.m_points[m_selidx].m_type == BaseSpline.SplinePointType.Bezier ||
								m_spline.m_points[m_selidx].m_type == BaseSpline.SplinePointType.BezierCorner);
					}
				}
				
				public bool SomethingSelected
				{
					get { return m_selidx != -1; }
				}
				
				public int Index
				{
					get { return m_idx; }
				}
				
				public int SelectedIndex
				{
					get { return m_selidx; }
				}		
			}
			
			[Serializable]
			public class BezierSplineSegment
			{
				public Vector3 m_startpos, m_endpos, m_startctrl, m_endctrl;
				public float m_startlen, m_endlen, m_length;
				public SplineSegmentType m_type;
				public float[] m_params, m_precomps;
			}
			
			[Serializable]
			public class BezierSplinePoint
			{
				public BezierSplinePoint(Vector3 p, Vector3 c1, Vector3 c2, SplinePointType t)
				{
					m_point = p;
					m_prevctrl = c1;
					m_nextctrl = c2;
					m_type = t;
				}
					
				public Vector3 m_point, m_prevctrl, m_nextctrl;
				public SplinePointType m_type;
			}
			
			public List<BezierSplinePoint> m_points = new List<BezierSplinePoint>();
			[SerializeField]
			private BezierSplineSegment[] m_segments = null;
			[SerializeField]
			private float m_precompdiv = 1;
			
			public void AppendPoint(Vector3 pos, SplinePointType type, Vector3 cp1, Vector3 cp2)
			{
				m_points.Add(new BezierSpline.BezierSplinePoint(pos, cp1, cp2, type));
			}
			
			public void RemoveLastPoint()
			{
				m_points.RemoveAt(m_points.Count - 1);
			}
			
			public void RemoveAllPoints()
			{
				m_points.Clear();
			}
			
			public void ReversePoints()
			{
				m_points.Reverse();
				Vector3 swp;
				for(int i = 0; i < m_points.Count; ++i)
				{
					swp = m_points[i].m_nextctrl;
					m_points[i].m_nextctrl = m_points[i].m_prevctrl;
					m_points[i].m_prevctrl = swp;
				}
			}
			
			public void InsertPoint(int idx, Vector3 pos, SplinePointType type, Vector3 cp1, Vector3 cp2)
			{
				if(idx < 0 || idx > m_points.Count)
				{
					throw(new IndexOutOfRangeException());
				}
				m_points.Insert(idx, new BezierSpline.BezierSplinePoint(pos, cp1, cp2, type));
			}
			
			public override void Build()
			{
				int idx, count;
				BezierSplinePoint pp, sp, ep, np;
				//
				if(m_points.Count < 2)
				{
					m_segments = null;
					m_length = 0;
					return;
				}
				if(m_wrapmode == SplineWrapMode.Loop)
				{
					count = m_points.Count;
				}
				else
				{
					count = m_points.Count - 1;
				}
				//
				m_segments = new BezierSplineSegment[count];
				m_length = 0;
				idx = 0;
				if(m_wrapmode == SplineWrapMode.Loop)
				{
					while(idx < count)
					{
						pp = m_points[SplineUtil.WrapIndex(idx - 1, m_points.Count)];
						sp = m_points[SplineUtil.WrapIndex(idx, m_points.Count)];
						ep = m_points[SplineUtil.WrapIndex(idx + 1, m_points.Count)];
						np = m_points[SplineUtil.WrapIndex(idx + 2, m_points.Count)];
						
						m_segments[idx] = new BezierSplineSegment();
						BuildSegment(m_segments[idx], pp, sp, ep, np);
						++idx;
					}
				}
				else
				{
					while(idx < count)
					{
						pp = m_points[SplineUtil.ClampIndex(idx - 1, m_points.Count)];
						sp = m_points[SplineUtil.ClampIndex(idx, m_points.Count)];
						ep = m_points[SplineUtil.ClampIndex(idx + 1, m_points.Count)];
						np = m_points[SplineUtil.ClampIndex(idx + 2, m_points.Count)];
						
						m_segments[idx] = new BezierSplineSegment();
						BuildSegment(m_segments[idx], pp, sp, ep, np);
						++idx;
					}
				}
				
				++m_buildnum;

				if (OnSplineRebuild != null)
					OnSplineRebuild();

			}
			
			private void BuildSegment(BezierSplineSegment ss, BezierSplinePoint pp, BezierSplinePoint sp, BezierSplinePoint ep, BezierSplinePoint np)
			{
				PreparePoint(pp, sp, ep);
				PreparePoint(sp, ep, np);
				
				ss.m_startpos = sp.m_point;
				ss.m_endpos = ep.m_point;
				ss.m_startctrl = ss.m_startpos + sp.m_nextctrl;
				ss.m_endctrl = ss.m_endpos + ep.m_prevctrl;
				
				if(sp.m_type == SplinePointType.Corner && ep.m_type == SplinePointType.Corner)
				{
					ss.m_type = SplineSegmentType.Linear;
				}
				else
				{
					ss.m_type = SplineSegmentType.Curve;
				}
				
				ss.m_startlen = m_length;
				float seglen = GetLength(ss);
				m_length += seglen;
				ss.m_length = seglen;
				ss.m_endlen = m_length;
				
				switch(m_reparam)
				{
				case SplineReparamType.None:			
					ss.m_params = null;
					ss.m_precomps = null;
					break;
					
				case SplineReparamType.Simple:
					{
						m_precompdiv = 1 / (float)m_stepcount;
						float param = 0, length = 0;
					
						Vector3 prev, next;
						
						ss.m_params = new float[m_stepcount + 1];
						ss.m_precomps = new float[m_stepcount + 1];
						for(int i = 1; i < m_stepcount + 1; ++i)
						{
							prev = GetPosition(ss, param);
							param += m_precompdiv;
							next = GetPosition(ss, param);
							length += (next - prev).magnitude;
							ss.m_precomps[i] = length / seglen;
							ss.m_params[i] = param;
						}
						ss.m_params[0] = 0;
						ss.m_params[m_stepcount] = 1;
						ss.m_precomps[0] = 0;
						ss.m_precomps[m_stepcount] = 1;
						m_precompdiv = 1 / (float)m_stepcount;
					}
					break;
					
				}
			}
			
			private void PreparePoint(BezierSplinePoint pp, BezierSplinePoint pt, BezierSplinePoint np)
			{
				switch(pt.m_type)
				{
				case SplinePointType.Bezier:
					pt.m_nextctrl = -pt.m_prevctrl;
					break;
				
				case SplinePointType.Smooth:
					pt.m_prevctrl = -0.25f * (np.m_point - pp.m_point);
					pt.m_nextctrl = -0.25f * (pp.m_point - np.m_point);
					break;
					
				case SplinePointType.Corner:
					pt.m_prevctrl = Vector3.zero;
					pt.m_nextctrl = Vector3.zero;
					break;
					
				case SplinePointType.BezierCorner:
					break;
				}
			}
			
			private float GetLength(BezierSplineSegment ss)
			{
				switch(ss.m_type)
				{
				case SplineSegmentType.Linear:
					return (ss.m_endpos - ss.m_startpos).magnitude;
					
				case SplineSegmentType.Curve:
					{
						float len = 0;
						Vector3 start, end;
						float t = 0, dt = 1 / (float)m_stepcount;
						int idx = 0;
						start = ss.m_startpos;
						while(idx < m_stepcount)
						{
							t += dt;
							end = GetPosition(ss, t);
							len += (end - start).magnitude;
							start = end;
							++idx;
						}
						return len;
					}
					
				default: return 0;
				}
			}
			
			public Vector3 GetPosition(BezierSplineSegment ss, float t)
			{
				switch(ss.m_type)
				{
				case SplineSegmentType.Linear:
					return ss.m_startpos + (ss.m_endpos - ss.m_startpos) * t;
					
				case SplineSegmentType.Curve:
					{
						// (1 - t) ^ 3 * A + 3 * (1 - t) ^ 2 * t * B + 3 * (1 - t) * t ^ 2 * C + t ^ 3 * D
						float _1mt = 1.0f - t, _1mt2 = _1mt * _1mt, t2 = t * t;
						return	ss.m_startpos *_1mt * _1mt2 +
								ss.m_startctrl * 3 * _1mt2 * t +
								ss.m_endctrl * 3 * _1mt * t2 +
								ss.m_endpos * t2 * t;
					}
					
				default: return Vector3.zero;
				}
			}
			
			public Vector3 GetTangent(BezierSplineSegment ss, float t)
			{
				switch(ss.m_type)
				{
				case SplineSegmentType.Linear:
					return (ss.m_endpos - ss.m_startpos);
					
				case SplineSegmentType.Curve:
					{
						// -3 * (A * (t - 1) ^ 2 + B * (-3 * t ^ 2 + 4 * t - 1) + t * (3 * C * t - 2 * C - D * t))
						float _1mt = 1.0f - t, _1mt2 = _1mt * _1mt, t2 = t * t;
						return	ss.m_startpos * -3 * _1mt2 +
								ss.m_startctrl * (-6 * _1mt * t + 3 * _1mt2) +
								ss.m_endctrl * (6 * _1mt * t - 3 * t2) +
								ss.m_endpos * 3 * t2;
					}
					
				default: return Vector3.zero;
				}
			}
			
			private Vector3 GetNormal(BezierSplineSegment ss, float t)
			{
				switch(ss.m_type)
				{
				case SplineSegmentType.Linear:
					return Vector3.zero;
					
				case SplineSegmentType.Curve:
					{
						// -6 * (A * (t - 1) + B * (2 - 3 * t) + 3 * C * t - C - D * t)
						return	-6 * (ss.m_startpos * (1 - t) +
								ss.m_startctrl * (2 - 3 * t) +
								3 * ss.m_endctrl * t -
								ss.m_endctrl -
								ss.m_endpos * t);
					}
					
				default: return Vector3.zero;
				}
			}
			
			private float GetReparamRungeKutta(BezierSplineSegment ss, float u)
			{
				float t = 0, k1, k2, k3, k4, h = u / (float)m_stepcount, mag;
				for (int i = 1; i <= m_stepcount; i++)
				{
					mag = GetTangent(ss, t).magnitude;
					if(mag == 0)
					{
						k1 = 0;
						k2 = 0;
						k3 = 0;
						k4 = 0;
					}
					else
					{
						k1 = h / GetTangent(ss, t).magnitude;
						k2 = h / GetTangent(ss, t + k1 * 0.5f).magnitude;
						k3 = h / GetTangent(ss, t + k2  * 0.5f).magnitude;
						k4 = h / GetTangent(ss, t + k3).magnitude;
					}
					t += (k1 + 2 * (k2 + k3) + k4) * 0.16666666666666666666666666666667f;
				}
				return t;
			}
			
			private float GetReparam(BezierSplineSegment ss, float u)
			{
				if(u <= 0)
				{
					return 0;
				}
				else if(u >= 1)
				{
					return 1;
				}
				
				switch(m_reparam)
				{
				case SplineReparamType.Simple:
					{
						int ridx = 0;
						for(int i = 1; i < m_stepcount + 1; ++i)
						{
							if(ss.m_precomps[i] > u)
							{
								ridx = i - 1;
								break;
							}
						}
						float uc = (u - ss.m_precomps[ridx]) / (ss.m_precomps[ridx + 1] - ss.m_precomps[ridx]);
						return Mathf.Lerp(ss.m_params[ridx], ss.m_params[ridx + 1], uc);
					}
					
				default:
					return 0;
				}
			}
			
			public override int GetPointCount()
			{
				return m_points.Count;
			}
			
			public override int GetSegmentCount()
			{
				if(m_segments != null)
				{
					return m_segments.Length;
				}
				return 0;
			}
			
			protected override float GetSegmentLength(int segidx)
			{
				return m_segments[segidx].m_length;
			}
			
			protected override float GetSegmentStartLength(int segidx)
			{
				return m_segments[segidx].m_startlen;
			}
			
			protected override float GetSegmentEndLength(int segidx)
			{
				return m_segments[segidx].m_endlen;
			}
			
			protected override int FindSegment(float offset)
			{
				for(int i = 0; i < m_segments.Length; ++i)
				{
					if(m_segments[i].m_startlen <= offset && m_segments[i].m_endlen > offset)
					{
						return i;
					}
				}
				return m_segments.Length - 1;
			}
			
			protected override Vector3 GetDrawPosition(int segidx, float t)
			{
				BezierSplineSegment ss = m_segments[segidx];
				return GetPosition(ss, t);
			}
			
			protected override Vector3 GetPosition(int segidx, float segpos)
			{
				BezierSplineSegment ss = m_segments[segidx];
				if(m_reparam == BaseSpline.SplineReparamType.None)
				{
					return GetPosition(ss, segpos / ss.m_length);
				}
				else
				{
					return GetPosition(ss, GetReparam(ss, segpos / ss.m_length));
				}
			}
			
			protected override Vector3 GetTangent(int segidx, float segpos)
			{
				BezierSplineSegment ss = m_segments[segidx];
				if(m_reparam == BaseSpline.SplineReparamType.None)
				{
					return GetTangent(ss, segpos / ss.m_length);
				}
				else
				{
					return GetTangent(ss, GetReparam(ss, segpos / ss.m_length));
				}
			}
			
			protected override Vector3 GetNormal(int segidx, float segpos)
			{
				BezierSplineSegment ss = m_segments[segidx];
				if(m_reparam == BaseSpline.SplineReparamType.None)
				{
					return GetNormal(ss, segpos / ss.m_length);
				}
				else
				{
					return GetNormal(ss, GetReparam(ss, segpos / ss.m_length));
				}
			}
			
			public EditHelper GetEditHelper()
			{
				return new EditHelper(this);
			}
		}
		
		public class BezierSplineComponent : MonoBehaviour
		{	
			[HideInInspector]
			[SerializeField]
			private BezierSpline m_spline = new BezierSpline();
			
			public BezierSplineComponent	splinePath;
			
			private int m_buildnum = -1;
			private Vector3[] m_drawcache;
			
			public  bool	enableSplineDraw = true;
			
			
			private bool forzeRepaint = false;
			
			[HideInInspector]
			public BezierSpline Spline
			{
				get { return m_spline; }
			}
			
			void OnDrawGizmosSelected()
			{
				DrawGizmos(Color.red);
			}
			
			void OnDrawGizmos()
			{
				DrawGizmos(Color.white);
			}
			
			void DrawGizmos(Color color)
			{
				if (!enableSplineDraw) 
					return;
				
				int curbuild = m_spline.BuildNum;
				if(m_spline.GetSegmentCount() > 0)
				{
					if(m_buildnum != curbuild || forzeRepaint)
					{
						m_drawcache = m_spline.GenerateSplinePoints(16);
						m_buildnum = curbuild;
						forzeRepaint = false;
					}
		
					if(m_drawcache != null)
					{
						Gizmos.matrix = transform.localToWorldMatrix;
						Gizmos.color = color;
					
						Vector3 startpos = Vector3.zero, endpos = Vector3.zero;
				
						for(int i = 0; i < m_drawcache.Length; ++i)
						{
							endpos = m_drawcache[i];
							if(i != 0)
							{
								Gizmos.DrawLine(startpos, endpos);
							}
							startpos = endpos;
						}
					}
				}
			}

		public void FlushDrawCache()
		{
			forzeRepaint = true;
		}
		}
}