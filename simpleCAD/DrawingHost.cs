using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using simpleCAD.Geometry;
using System.Diagnostics;

namespace simpleCAD
{
	public class DrawingHost : FrameworkElement, ICoordinateSystem
	{
		#region Constructors

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

			DrawingHost.SelectedGeometryProperty = DependencyProperty.Register(
				"SelectedGeometry",
				typeof(ICadGeometry),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(null, On_SelectedGeometry_Changed));

			DrawingHost.GeometryToCreateProperty = DependencyProperty.Register(
				"GeometryToCreate",
				typeof(ICadGeometry),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(null, On_GeometryToCreate_Changed));

			DrawingHost.ScaleProperty = DependencyProperty.Register(
				"Scale",
				typeof(double),
				typeof(DrawingHost),
				new FrameworkPropertyMetadata(1.0, On_Scale_Changed));
		}

		public DrawingHost()
		{
			OnUpdatePlotHandler += OnUpdatePlot;
		}

		#endregion

		#region Properties

		private List<ICadGeometry> m_geometries = new List<ICadGeometry>();
		private List<cadGrip> m_grips = new List<cadGrip>();
		private cadGrip m_gripToMove = null;
		/// <summary>
		/// Point of mouse middle button pressing.
		/// Need for temporary offset calculation.
		/// </summary>
		private Point m_MiddleBtnPressed_Point = new Point();
		/// <summary>
		/// Offset of left top corner of plot from (0;0) point.
		/// </summary>
		private Vector m_OffsetVector = new Vector(0.0, 0.0);
		/// <summary>
		/// Temporary offset vector.
		/// It shows offset when mouse middle button is pressed and mouse is moved.
		/// In this case dont need to change m_OffsetVector because when mouse will move again
		/// (with middle button pressed) need to deduct "old" temp offset and add "new" temp offset.
		/// But we cant calculate "old" temp offset because mouse position is new.
		/// </summary>
		private Vector m_TempOffsetVector = new Vector(0.0, 0.0);
		private ICadGeometry m_NewGeometry = null;

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
			get { return (double)GetValue(DrawingHost.AxisThicknessProperty); }
			set { SetValue(DrawingHost.AxisThicknessProperty, value); }
		}

		//=============================================================================
		public static readonly DependencyProperty ScaleProperty;

		public double Scale
		{
			get { return (double)GetValue(DrawingHost.ScaleProperty); }
			set { SetValue(DrawingHost.ScaleProperty, value); }
		}
		private static void On_Scale_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UpdatePlot();
		}

		//=============================================================================
		public static readonly DependencyProperty SelectedGeometryProperty;

		public ICadGeometry SelectedGeometry
		{
			get { return (ICadGeometry)GetValue(DrawingHost.SelectedGeometryProperty); }
			set { SetValue(DrawingHost.SelectedGeometryProperty, value); }
		}
		private static void On_SelectedGeometry_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DrawingHost dh = d as DrawingHost;
			if (dh != null)
				dh.ResetGrips();
		}

		//=============================================================================
		public static readonly DependencyProperty GeometryToCreateProperty;
		public ICadGeometry GeometryToCreate
		{
			get { return (ICadGeometry)GetValue(DrawingHost.GeometryToCreateProperty); }
			set { SetValue(DrawingHost.GeometryToCreateProperty, value); }
		}
		private static void On_GeometryToCreate_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DrawingHost dh = d as DrawingHost;
			if (dh != null)
				dh.On_GeometryToCreate_Changed();
		}
		public void On_GeometryToCreate_Changed()
		{
			//
			SelectedGeometry = null;

			// delete last not initialized
			if (m_NewGeometry != null)
			{
				DrawingVisual dc = m_NewGeometry.GetGeometryWrapper();
				RemoveVisualChild(dc);
				RemoveLogicalChild(dc);
				m_geometries.Remove(m_NewGeometry);
			}

			m_gripToMove = null;
			ClearGrips();
		}

		//=============================================================================
		private static event EventHandler OnUpdatePlotHandler;

		#endregion

		#region Overrides

		//=============================================================================
		protected override int VisualChildrenCount
		{
			get { return m_geometries.Count + m_grips.Count; }
		}

		//=============================================================================
		protected override Visual GetVisualChild(int index)
		{
			ICadGeometry geom = null;
			if (index >= 0 && index < m_geometries.Count)
				geom = m_geometries[index];

			int offset = m_geometries.Count;
			if (index >= offset && index - offset < m_grips.Count)
				return m_grips[index - offset];

			return geom.GetGeometryWrapper();
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

			// Clone new geometry and add it as a child
			if(m_NewGeometry == null)
			{
				ICadGeometry geomToCreate = GeometryToCreate;
				if(geomToCreate != null)
				{
					ICadGeometry geomCopy = geomToCreate.Clone();
					if (geomCopy != null)
					{
						m_NewGeometry = new GeometryWraper(this, geomCopy);
						DrawingVisual dv = m_NewGeometry.GetGeometryWrapper();

						m_geometries.Add(m_NewGeometry);
						AddVisualChild(dv);
						AddLogicalChild(dv);
					}
				}
			}

			if(m_NewGeometry != null)
			{
				m_NewGeometry.OnMouseLeftButtonClick(globalPnt);
				if (m_NewGeometry.IsPlaced)
				{
					m_NewGeometry.Draw(this, null);
					m_NewGeometry = null;
				}
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

				SelectedGeometry = res.VisualHit as ICadGeometry;
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
				m_TempOffsetVector = (m_MiddleBtnPressed_Point - pnt);
				UpdatePlot();
				return;
			}

			Point globalPnt = _GetGlobalPoint(e);

			if (m_NewGeometry != null)
				m_NewGeometry.OnMouseMove(globalPnt);
			else if (m_gripToMove != null)
				m_gripToMove.Move(globalPnt);

			//
			// Update properties for selected geometry
			ICadGeometry sg = SelectedGeometry;
			if (sg != null)
			{
				foreach (Property_ViewModel p in sg.Properties)
					p.Update_Value();
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

			//
			// Mouse wheel was spinned.
			// Calculate new scale.
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

				int iIndex = m_ScaleList.Count - 1;
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

			//
			// Need to save cursor position - cursor should been placed over same point in global coordiantes.
			globalPnt_UnderMouse.X *= Scale;
			globalPnt_UnderMouse.Y *= Scale;
			m_OffsetVector = globalPnt_UnderMouse - localPnt_UnderMouse;
			m_TempOffsetVector.X = 0;
			m_TempOffsetVector.Y = 0;

			UpdatePlot();
		}

		#endregion

		//=============================================================================
		public static void UpdatePlot()
		{
			EventHandler handler = OnUpdatePlotHandler;
			if (handler != null)
				handler(null, EventArgs.Empty);
		}
		private void OnUpdatePlot(object sender, EventArgs e)
		{
			if (m_geometries != null)
			{
				foreach (ICadGeometry g in m_geometries)
					g.Draw(this, null);
			}

			if (m_grips != null)
			{
				foreach (cadGrip g in m_grips)
					g.Update();
			}
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
			if(m_NewGeometry != null)
			{
				DrawingVisual dc = m_NewGeometry.GetGeometryWrapper();
				RemoveVisualChild(dc);
				RemoveLogicalChild(dc);
				m_geometries.Remove(m_NewGeometry);
			}

			if (bTurnOn)
				ClearGrips();
		}

		//=============================================================================
		private Vector GetOffset()
		{
			return m_OffsetVector + m_TempOffsetVector;
		}

		//=============================================================================
		public void ResetGrips()
		{
			ClearGrips();

			ICadGeometry selectedGeom = SelectedGeometry;
			if (selectedGeom != null)
			{
				List<Point> pnts = selectedGeom.GetGripPoints();
				if (pnts != null)
				{
					foreach (Point p in pnts)
					{
						cadGrip newGrip = new cadGrip(this, selectedGeom, pnts.IndexOf(p));
						m_grips.Add(newGrip);
						AddVisualChild(newGrip);
						AddLogicalChild(newGrip);
					}
				}
			}
		}

		//=============================================================================
		private Point _GetGlobalPoint(MouseEventArgs e)
		{
			return _GetGlobalPoint(e.GetPosition(this));
		}
		private Point _GetGlobalPoint(Point localPnt)
		{
			Point tempPnt = localPnt + GetOffset();
			tempPnt.X = tempPnt.X / Scale;
			tempPnt.Y = tempPnt.Y / Scale;
			return tempPnt;
		}

		//=============================================================================
		private Point _GetLocalPoint(MouseEventArgs e)
		{
			return e.GetPosition(this);
		}
		public Point GetLocalPoint(Point globalPnt)
		{
			Point tempPnt = globalPnt;
			tempPnt.X = tempPnt.X * Scale;
			tempPnt.Y = tempPnt.Y * Scale;

			tempPnt -= GetOffset();

			return tempPnt;
		}
	}
}
