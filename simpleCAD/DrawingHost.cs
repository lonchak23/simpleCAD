using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using simpleCAD.Geometry;

namespace simpleCAD
{
    public class DrawingHost : FrameworkElement
	{
		static DrawingHost()
		{
			DrawingHost.AxisBrushProperty = DependencyProperty.Register(
				"AxisBrush",
				typeof(Brush),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(Brushes.Black));

			DrawingHost.AxisThicknessProperty = DependencyProperty.Register(
				"AxisThickness",
				typeof(double),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(2.0));

			DrawingHost.IsDrawingLineProperty = DependencyProperty.Register(
				"IsDrawingLine",
				typeof(bool),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(false, On_IsDrawingLine_Changed));

			DrawingHost.SelectedGeometryProperty = DependencyProperty.Register(
				"SelectedGeometry",
				typeof(cadGeometry),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(null, On_SelectedGeometry_Changed));

			DrawingHost.ScaleProperty = DependencyProperty.Register(
				"Scale",
				typeof(double),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(1.0, On_Scale_Changed));
		}

		public DrawingHost()
		{
			OnRedrawGripsHandler += OnRedrawGrips;
		}

		#region Properties

		private List<cadGeometry> m_geometries = new List<cadGeometry>();
		private List<cadGrip> m_grips = new List<cadGrip>();
		private cadGrip m_gripToMove = null;
		private Point m_MiddleBtnPressed_Point = new Point();
		private Vector m_OffsetVector = new Vector(0.0, 0.0);
		private Vector m_TempOffsetVector = new Vector(0.0, 0.0);

		//=============================================================================
		private static List<double> m_ScaleList = new List<double>()
		{
			0.1,
			0.3,
			0.5,
			0.75,
			0.90,
			1,
			1.1,
			1.2,
			1.5,
			1.75,
			2,
			4
		};

		//=============================================================================
		public static readonly DependencyProperty AxisBrushProperty;
		public Brush AxisBrush
		{
			get { return (Brush)GetValue(DrawingHost.AxisBrushProperty); }
			set { SetValue(DrawingHost.AxisBrushProperty, value); }
		}

		//=============================================================================
		public static readonly DependencyProperty AxisThicknessProperty;
		public double AxisThickness
		{
			get { return (double) GetValue(DrawingHost.AxisThicknessProperty); }
			set { SetValue(DrawingHost.AxisThicknessProperty, value); }
		}

		//=============================================================================
		public static readonly DependencyProperty IsDrawingLineProperty;
		public bool IsDrawingLine
		{
			get { return (bool) GetValue(DrawingHost.IsDrawingLineProperty); }
			set { SetValue(DrawingHost.IsDrawingLineProperty, value);}
		}
		private static void On_IsDrawingLine_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DrawingHost dh = d as DrawingHost;
			if (dh != null)
				dh.On_DrawGeometryCommand((bool)e.NewValue);
		}

		//=============================================================================
		public static readonly DependencyProperty ScaleProperty;

		public double Scale
		{
			get { return (double) GetValue(DrawingHost.ScaleProperty); }
			set { SetValue(DrawingHost.ScaleProperty, value);}
		}
		private static void On_Scale_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DrawingHost dh = d as DrawingHost;
			if (dh != null)
			{
				dh.UpdateGeometry();
				dh.UpdateGrips();
			}
		}

		//=============================================================================
		public static readonly DependencyProperty SelectedGeometryProperty;

		public cadGeometry SelectedGeometry
		{
			get { return (cadGeometry) GetValue(DrawingHost.SelectedGeometryProperty); }
			set { SetValue(DrawingHost.SelectedGeometryProperty, value); }
		}
		private static void On_SelectedGeometry_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DrawingHost dh = d as DrawingHost;
			if (dh != null)
				dh.ResetGrips();
		}

		//=============================================================================
		private static event EventHandler OnRedrawGripsHandler;

		#endregion

		//=============================================================================
		public static void RedrawGrips()
		{
			EventHandler handler = OnRedrawGripsHandler;
			if(handler != null)
				handler(null, EventArgs.Empty);
		}

		private void OnRedrawGrips(object sender, EventArgs e)
		{
			if(m_grips == null)
				return;

			foreach (cadGrip g in m_grips)
				g.Update();
		}

		//=============================================================================
		protected override int VisualChildrenCount
		{
			get { return m_geometries.Count + m_grips.Count; }
		}

		//=============================================================================
		protected override Visual GetVisualChild(int index)
		{
			if (index >= 0 && index < m_geometries.Count)
				return m_geometries[index];

			int offset = m_geometries.Count;
			if (index >= offset && index - offset < m_grips.Count)
				return m_grips[index - offset];

			return null;
		}

		//=============================================================================
		protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
		{
			Point pt = hitTestParameters.HitPoint;

			// Perform custom actions during the hit test processing,
			// which may include verifying that the point actually
			// falls within the rendered content of the visual.

			// Return hit on bounding rectangle of visual object.
			return new PointHitTestResult(this, pt);
		}

		//=============================================================================
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);

			Point globalPnt = _GetGlobalPoint(e);

			if (IsDrawingLine)
			{
				cadGeometry currentGeom = null;
				if (m_geometries.Count > 0)
				{
					cadGeometry lastGeom = m_geometries[m_geometries.Count - 1];
					if (!lastGeom.IsInitialized())
						currentGeom = lastGeom as cadLine;
				}

				// create new one
				if (currentGeom == null)
				{
					if (IsDrawingLine)
						currentGeom = new cadLine(this);

					m_geometries.Add(currentGeom);
					AddVisualChild(currentGeom);
					AddLogicalChild(currentGeom);
				}

				if (currentGeom != null)
					currentGeom.SetPoint(globalPnt, true);
			}
			else
			{
				Point localPnt = _GetLocalPoint(e);
				HitTestResult res = VisualTreeHelper.HitTest(this, localPnt);

				if (m_gripToMove != null)
				{
					m_gripToMove = null;
					return;
				}

				// moving grip
				cadGrip _grip = res.VisualHit as cadGrip;
				if (_grip != null)
				{
					m_gripToMove = _grip;
					return;
				}

				SelectedGeometry = res.VisualHit as cadGeometry;
			}
		}

		//=============================================================================
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			// move plot
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				Point pnt = _GetLocalPoint(e);
				m_TempOffsetVector = (m_MiddleBtnPressed_Point - pnt)/Scale;//pnt - m_MiddleBtnPressed_Point;
				UpdateGeometry();
				UpdateGrips();
				return;
			}

			if (m_geometries.Count > 0)
			{
				Point globalPnt = _GetGlobalPoint(e);

				cadGeometry lastGeom = m_geometries[m_geometries.Count - 1];
				if (lastGeom.IsInitialized())
				{
					if (m_gripToMove != null)
						m_gripToMove.Move(globalPnt);
				}
				else
					lastGeom.SetPoint(globalPnt, false);
			}
		}

		//=============================================================================
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			//
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
				m_MiddleBtnPressed_Point = _GetLocalPoint(e);
		}

		//=============================================================================
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			//
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
			{
				m_OffsetVector = m_OffsetVector + m_TempOffsetVector;

				m_TempOffsetVector.X = 0.0;
				m_TempOffsetVector.Y = 0.0;
			}
		}

		//=============================================================================
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			base.OnMouseWheel(e);

			Point globalPnt_UnderMouse = _GetGlobalPoint(e);
			Point localPnt_UnderMouse = _GetLocalPoint(e);

			double currentScale = Scale;
			//
			// Delta < 0 - scrolling to user
			// Delta > 0 - from user
			if (e.Delta < 0)
			{
				// zoom in
				if (currentScale >= m_ScaleList[m_ScaleList.Count - 1])
					return;

				foreach (double rVal in m_ScaleList)
				{
					if (currentScale < rVal)
					{
						Scale = rVal;
						break;
					}
				}
			}
			else
			{
				// zoom out
				if (currentScale <= m_ScaleList[0])
					return;

				int iIndex = m_ScaleList.Count-1;
				while (iIndex >= 0)
				{
					if (currentScale > m_ScaleList[iIndex])
					{
						Scale = m_ScaleList[iIndex];
						break;
					}
					--iIndex;
				}
			}

			// Need to save cursor position - cursor should been placed over same point in global coordiantes.
			// Imagine that cursor is over Local_PntA, his Global_PntA = (Local_PntA + offset)/Scale.
			// After scaling we need to save point under cursor - Global_PntA. Other, plot will be jumping and
			// when you will try to scale while dragging gripp Point, gripp point will move away from cursor.
			//
			// All we need is to change offset after scaling:
			// offset = Global_PntA*Scale - Local_PntA
			//
			//localPnt_UnderMouse.X /= Scale;
			//localPnt_UnderMouse.Y /= Scale;

			//
			//globalPnt_UnderMouse.X *= Scale;
			//globalPnt_UnderMouse.Y *= Scale;
			//m_OffsetVector = globalPnt_UnderMouse - localPnt_UnderMouse;

			Point globalPnt_UnderMouse2 = _GetGlobalPoint(e);
			m_OffsetVector += globalPnt_UnderMouse - globalPnt_UnderMouse2;

			UpdateGeometry();
			UpdateGrips();
		}

		//=============================================================================
		private void ClearGrips()
		{
			foreach (cadGrip g in m_grips)
			{
				RemoveVisualChild(g);
				RemoveLogicalChild(g);
			}
			m_grips.Clear();
		}

		//=============================================================================
		public void On_DrawGeometryCommand(bool bTurnOn)
		{
			//
			SelectedGeometry = null;

			// delete last not initialized
			if (m_geometries.Count > 0)
			{
				cadGeometry lastGeom = m_geometries[m_geometries.Count - 1];
				if (!lastGeom.IsInitialized())
				{
					RemoveVisualChild(lastGeom);
					RemoveLogicalChild(lastGeom);
					m_geometries.Remove(lastGeom);
				}
			}

			if(bTurnOn)
				ClearGrips();
		}

		//=============================================================================
		public Vector GetOffset()
		{
			return m_OffsetVector + m_TempOffsetVector;
		}

		//=============================================================================
		public void ResetGrips()
		{
			ClearGrips();

			cadGeometry selectedGeom = SelectedGeometry;
			if (selectedGeom != null)
			{
				List<Point> pnts = selectedGeom.GetGripPoints();
				if (pnts != null)
				{
					foreach (Point p in pnts)
					{
						cadGrip newGrip = new cadGrip(selectedGeom, pnts.IndexOf(p));
						m_grips.Add(newGrip);
						AddVisualChild(newGrip);
						AddLogicalChild(newGrip);
					}
				}
			}
		}

		//=============================================================================
		public void UpdateGeometry()
		{
			foreach (cadGeometry g in m_geometries)
				g.Draw();
		}

		//=============================================================================
		public void UpdateGrips()
		{
			foreach (cadGrip g in m_grips)
				g.Update();
		}

		//=============================================================================
		private Point _GetGlobalPoint(MouseEventArgs e)
		{
			return _GetGlobalPoint(e.GetPosition(this));
		}
		private Point _GetGlobalPoint(Point localPnt)
		{
			Point tempPnt = localPnt + GetOffset();
			tempPnt.X = tempPnt.X/Scale;
			tempPnt.Y = tempPnt.Y/Scale;
			return tempPnt;
		}

		//=============================================================================
		private Point _GetLocalPoint(MouseEventArgs e)
		{
			return e.GetPosition(this);
		}
		//=============================================================================
		public Point GetLocalPoint(Point globalPnt)
		{
			Point tempPnt = globalPnt - GetOffset();
			tempPnt.X = tempPnt.X*Scale;
			tempPnt.Y = tempPnt.Y*Scale;
			return tempPnt;
		}
	}
}
