using UnityEditor;
using UnityEngine;
using System;
using System.Collections;


namespace PigtailGames
{
[CustomEditor(typeof(BezierSplineComponent))]
public class BezierSplineEditor : Editor
{
	private const float m_point_size = 0.05f;
	private BezierSplineComponent m_splinecomp = null;
	private BezierSpline m_spline = null;
	private BezierSpline.EditHelper m_edithelper;
	private int m_drawidx = -1;
	
	public virtual void OnEnable()
    {
		if(AssetDatabase.Contains(target))
		{
			m_splinecomp = null;
			m_spline = null;
			m_edithelper = null;
		}
		else
		{
			m_splinecomp = (BezierSplineComponent)target;
			m_spline = m_splinecomp.Spline;
			m_edithelper = m_spline.GetEditHelper();
		}

			Undo.undoRedoPerformed += UndoPerformed;
    }
	
	void UndoPerformed()
	{
		m_splinecomp.FlushDrawCache();
	}

    void OnDisable()
    {
		SplineEditor.ToolsUtil.Hidden = false;
    }
	
	public virtual void OnSceneGUI()
	{
		
		if(m_splinecomp != null && m_splinecomp.enableSplineDraw)
		{
			bool selected = false;
			float hsize, hcsize1, hcsize2;
			
			Undo.RecordObject(m_splinecomp, "BezierSpline Modify");
			
			Handles.matrix = m_splinecomp.transform.localToWorldMatrix;
			
			SplineEditor.ToolsUtil.Hidden = m_edithelper.SomethingSelected;
			
			m_edithelper.Reset();
			while(m_edithelper.MoveNext())
			{
				hsize = HandleUtility.GetHandleSize(m_edithelper.Point.m_point);
				if(Handles.Button(m_edithelper.Point.m_point, Quaternion.identity, hsize * m_point_size, hsize * m_point_size, DrawPoint))
				{
					m_edithelper.Selected = true;
					selected = true;
				}
				if(m_edithelper.Selected)
				{
					if(m_edithelper.ControlPointsVisible)
					{
						Handles.color = new Color(1, 1, 0, 1);
						Handles.DrawLine(m_edithelper.SelectedPoint.m_point, m_edithelper.SelectedPoint.m_point + m_edithelper.SelectedPoint.m_prevctrl);
						Handles.DrawLine(m_edithelper.SelectedPoint.m_point, m_edithelper.SelectedPoint.m_point + m_edithelper.SelectedPoint.m_nextctrl);
						Handles.color = new Color(1, 1, 1, 1);
						hcsize1 = HandleUtility.GetHandleSize(m_edithelper.SelectedPoint.m_prevctrl);
						m_drawidx = 0;
						if(Handles.Button(m_edithelper.SelectedPoint.m_point + m_edithelper.SelectedPoint.m_prevctrl, Quaternion.identity, hcsize1 * m_point_size, hcsize1 * m_point_size, DrawControlPoint))
						{
							m_edithelper.ControlPoint1Selected = true;
							selected = true;
						}
						hcsize2 = HandleUtility.GetHandleSize(m_edithelper.SelectedPoint.m_nextctrl);
						m_drawidx = 1;
						if(Handles.Button(m_edithelper.SelectedPoint.m_point + m_edithelper.SelectedPoint.m_nextctrl, Quaternion.identity, hcsize2 * m_point_size, hcsize1 * m_point_size, DrawControlPoint))
						{
							m_edithelper.ControlPoint2Selected = true;
							selected = true;
						}
					}
					
					if(m_edithelper.ControlPoint1Selected)
					{
						if(Tools.current == Tool.Move)
						{
							m_edithelper.SelectedPoint.m_prevctrl = Handles.PositionHandle(m_edithelper.SelectedPoint.m_point + m_edithelper.SelectedPoint.m_prevctrl, Quaternion.identity) - m_edithelper.SelectedPoint.m_point;
						}
					}
					else if(m_edithelper.ControlPoint2Selected)
					{
						if(m_edithelper.SelectedPoint.m_type == BaseSpline.SplinePointType.Bezier)
						{
							if(Tools.current == Tool.Move)
							{
								m_edithelper.SelectedPoint.m_prevctrl = -(Handles.PositionHandle(m_edithelper.SelectedPoint.m_point + m_edithelper.SelectedPoint.m_nextctrl, Quaternion.identity) - m_edithelper.SelectedPoint.m_point);
							}
						}
						else
						{
							if(Tools.current == Tool.Move)
							{
								m_edithelper.SelectedPoint.m_nextctrl = Handles.PositionHandle(m_edithelper.SelectedPoint.m_point + m_edithelper.SelectedPoint.m_nextctrl, Quaternion.identity) - m_edithelper.SelectedPoint.m_point;
							}
						}
					}
					else
					{
						if(Tools.current == Tool.Move)
						{
							m_edithelper.SelectedPoint.m_point = Handles.PositionHandle(m_edithelper.SelectedPoint.m_point, Quaternion.identity);
						}
					}
				}
			}
			
			if(GUI.changed)
			{
				m_spline.Build();
				EditorUtility.SetDirty(m_splinecomp);
				Repaint();

			}
			if(selected)
			{
				Repaint();
				SceneView.RepaintAll();
				GUIUtility.keyboardControl = 0;
			}
		}
	}
		
	void DrawPoint(int controlID, Vector3 position, Quaternion rotation, float size)
	{
		if(m_edithelper.Selected && !m_edithelper.ControlPoint1Selected && !m_edithelper.ControlPoint2Selected)
		{
			Handles.color = new Color(1, 0, 0, 1);
		}
		else
		{
			Handles.color = new Color(1, 1, 1, 1);
		}
		Handles.DotCap(controlID, position, rotation, size);
		Handles.Label(position, m_edithelper.Index.ToString());
	}
	
	void DrawControlPoint(int controlID, Vector3 position, Quaternion rotation, float size)
	{
		if(m_drawidx == 0)
		{
			if(m_edithelper.ControlPoint1Selected)
			{
				Handles.color = new Color(1, 0, 0, 1);
			}
			else
			{
				Handles.color = new Color(0, 1, 0, 1);
			}
			Handles.DotCap(controlID, position, rotation, size);
			Handles.Label(position, "-");
		}
		else
		{
			if(m_edithelper.ControlPoint2Selected)
			{
				Handles.color = new Color(1, 0, 0, 1);
			}
			else
			{
				Handles.color = new Color(0, 1, 0, 1);
			}
			Handles.DotCap(controlID, position, rotation, size);
			Handles.Label(position, "+");
		}
	}
	
	override public void OnInspectorGUI()
    {
		
		if(m_splinecomp != null && m_splinecomp.splinePath == null)
		{
			bool addremove = false, selected = false;
			
			Undo.RecordObject(m_splinecomp, "BezierSpline Modify");
			
			EditorGUILayout.BeginVertical();
			EditorGUILayout.LabelField("Point count", m_spline.m_points.Count.ToString());
			EditorGUILayout.LabelField("Length", m_spline.Length.ToString());
			m_spline.WrapMode = (BaseSpline.SplineWrapMode)EditorGUILayout.EnumPopup("Wrap mode", m_spline.WrapMode);
			m_spline.ReparamType = (BaseSpline.SplineReparamType)EditorGUILayout.EnumPopup("Reparameterization", m_spline.ReparamType);
			if(m_spline.ReparamType != BaseSpline.SplineReparamType.None)
			{
				m_spline.StepCount = EditorGUILayout.IntSlider("Step count", m_spline.StepCount, 1, 64);
			}
			
			EditorGUILayout.EndVertical();
			
			EditorGUILayout.BeginHorizontal();
			if(GUILayout.Button("Append point"))
			{
				m_edithelper.AppendPoint();
				addremove = true;
			}
			if(GUILayout.Button("Remove last"))
			{
				m_edithelper.RemoveLast();
				addremove = true;
			}
			if(GUILayout.Button("Reverse points"))
			{
				m_spline.ReversePoints();
				addremove = true;
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			if(m_spline.m_points.Count > 0)
			{
				if(GUILayout.Button("Select first"))
				{
					m_edithelper.SelectFirst();
					selected = true;
				}
				if(m_edithelper.SomethingSelected)
				{
					if(GUILayout.Button("Select next"))
					{
						m_edithelper.SelectNext();
						selected = true;
					}
					if(GUILayout.Button("Select previous"))
					{
						m_edithelper.SelectPrev();
						selected = true;
					}
				}
			}
			EditorGUILayout.EndHorizontal();
	
			if(m_edithelper.SomethingSelected)
			{
				m_edithelper.SelectedPoint.m_type = (BaseSpline.SplinePointType)EditorGUILayout.EnumPopup("Point type", m_edithelper.SelectedPoint.m_type);
				m_edithelper.SelectedPoint.m_point = EditorGUILayout.Vector3Field("Position", m_edithelper.SelectedPoint.m_point);
				
				if(m_edithelper.ControlPointsVisible)
				{
					m_edithelper.SelectedPoint.m_prevctrl = EditorGUILayout.Vector3Field("Control -", m_edithelper.SelectedPoint.m_prevctrl);
					m_edithelper.SelectedPoint.m_nextctrl = EditorGUILayout.Vector3Field("Control +", m_edithelper.SelectedPoint.m_nextctrl);
				}
				
				EditorGUILayout.BeginHorizontal();
				if(GUILayout.Button("Insert before"))
				{
					m_edithelper.InsertBefore();					
					addremove = true;
				}
				if(GUILayout.Button("Insert after"))
				{
					m_edithelper.InsertAfter();
					addremove = true;
				}
				if(GUILayout.Button("Remove"))
				{
					m_edithelper.Remove();
					addremove = true;
				}
				EditorGUILayout.EndHorizontal();
			}
	
			if(GUI.changed || addremove)
			{
				m_spline.Build();
				EditorUtility.SetDirty(m_splinecomp);
				Repaint();

			}
			if(selected)
			{
				Repaint();
				SceneView.RepaintAll();
				GUIUtility.keyboardControl = 0;
			}
		}
    }	
}
}