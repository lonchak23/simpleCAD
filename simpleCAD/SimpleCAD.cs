using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using simpleCAD.Geometry;
using System.Diagnostics;

namespace simpleCAD
{
	public class SimpleCAD : FrameworkElement, ICoordinateSystem
	{
		#region Constructors

		static SimpleCAD()
		{
			SimpleCAD.AxesColorProperty = DependencyProperty.Register(
				"AxesColor",
				typeof(Color),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(Colors.Black, On_AxesColor_Changed));

			SimpleCAD.AxesThicknessProperty = DependencyProperty.Register(
				"AxesThickness",
				typeof(double),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(2.0, On_AxesThickness_Changed));

			SimpleCAD.AxesLengthProperty = DependencyProperty.Register(
				"AxesLength",
				typeof(double),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(50.0, On_AxesLength_Changed));

			SimpleCAD.AxesTextSizeProperty = DependencyProperty.Register(
				"AxesTextSize",
				typeof(double),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(12.0, On_AxesTextSize_Changed));

			SimpleCAD.SelectedGeometryProperty = DependencyProperty.Register(
				"SelectedGeometry",
				typeof(ICadGeometry),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(null, On_SelectedGeometry_Changed));

			SimpleCAD.GeometryToCreateProperty = DependencyProperty.Register(
				"GeometryToCreate",
				typeof(ICadGeometry),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(null, On_GeometryToCreate_Changed));

			SimpleCAD.ScaleProperty = DependencyProperty.Register(
				"Scale",
				typeof(double),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(1.0, On_Scale_Changed));
		}

		public SimpleCAD()
		{
			//
			// Default WPF coordinate system has Y-axis, directed to down.
			// So left top corner has(0, 0) coordinate.
			//
			// All graphics that i have seen use Y-axis, directed to up.
			// So left BOT corner has(0, 0) coordinate.
			//
			// Lets revert Y - axis.
			// So all coordinate properties will show default "human" coordinate system.
			this.RenderTransform = new ScaleTransform(1, -1);
			this.RenderTransformOrigin = new Point(0.5, 0.5);

			OnUpdatePlotHandler += OnUpdatePlot;

			m_axes = new CoordinateAxes(this);
			AddVisualChild(m_axes);
			AddLogicalChild(m_axes);
			m_axes.Draw();
		}

		#endregion

		#region Properties

		private CoordinateAxes m_axes = null;
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
		public static readonly DependencyProperty AxesColorProperty;
		public Color AxesColor
		{
			get { return (Color)GetValue(SimpleCAD.AxesColorProperty); }
			set { SetValue(SimpleCAD.AxesColorProperty, value); }
		}
		private static void On_AxesColor_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
			if (dh != null)
				dh.Update_CoordinateAxes();
		}

		//=============================================================================
		public static readonly DependencyProperty AxesThicknessProperty;
		public double AxesThickness
		{
			get { return (double)GetValue(SimpleCAD.AxesThicknessProperty); }
			set { SetValue(SimpleCAD.AxesThicknessProperty, value); }
		}
		private static void On_AxesThickness_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
			if (dh != null)
				dh.Update_CoordinateAxes();
		}

		//=============================================================================
		public static readonly DependencyProperty AxesLengthProperty;
		public double AxesLength
		{
			get { return (double)GetValue(SimpleCAD.AxesLengthProperty); }
			set { SetValue(SimpleCAD.AxesLengthProperty, value); }
		}
		private static void On_AxesLength_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
			if (dh != null)
				dh.Update_CoordinateAxes();
		}

		//=============================================================================
		public static readonly DependencyProperty AxesTextSizeProperty;
		public double AxesTextSize
		{
			get { return (double)GetValue(SimpleCAD.AxesTextSizeProperty); }
			set { SetValue(SimpleCAD.AxesTextSizeProperty, value); }
		}
		private static void On_AxesTextSize_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
			if (dh != null)
				dh.Update_CoordinateAxes();
		}

		//=============================================================================
		public static readonly DependencyProperty ScaleProperty;

		public double Scale
		{
			get { return (double)GetValue(SimpleCAD.ScaleProperty); }
			set { SetValue(SimpleCAD.ScaleProperty, value); }
		}
		private static void On_Scale_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			UpdatePlot();
		}

		//=============================================================================
		public static readonly DependencyProperty SelectedGeometryProperty;

		public ICadGeometry SelectedGeometry
		{
			get { return (ICadGeometry)GetValue(SimpleCAD.SelectedGeometryProperty); }
			set { SetValue(SimpleCAD.SelectedGeometryProperty, value); }
		}
		private static void On_SelectedGeometry_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
			if (dh != null)
				dh.ResetGrips();
		}

		//=============================================================================
		public static readonly DependencyProperty GeometryToCreateProperty;
		public ICadGeometry GeometryToCreate
		{
			get { return (ICadGeometry)GetValue(SimpleCAD.GeometryToCreateProperty); }
			set { SetValue(SimpleCAD.GeometryToCreateProperty, value); }
		}
		private static void On_GeometryToCreate_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
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
			// 1 for m_axes
			get { return 1 + m_geometries.Count + m_grips.Count; }
		}

		//=============================================================================
		protected override Visual GetVisualChild(int index)
		{
			int offset = 0;

			if (index == 0)
				return m_axes;

			offset += 1;

			ICadGeometry geom = null;
			if (index >= offset && index - offset < m_geometries.Count)
				geom = m_geometries[index - offset];

			offset += m_geometries.Count;
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
		public void Update_CoordinateAxes()
		{
			if (m_axes != null)
				m_axes.Draw();
		}

		//=============================================================================
		public static void UpdatePlot()
		{
			EventHandler handler = OnUpdatePlotHandler;
			if (handler != null)
				handler(null, EventArgs.Empty);
		}
		private void OnUpdatePlot(object sender, EventArgs e)
		{
			if (m_axes != null)
				m_axes.Draw();

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
			return GetLocalPoint(globalPnt, true);
		}
		public Point GetLocalPoint(Point globalPnt, bool bWithScale)
		{
			Point tempPnt = globalPnt;
			if (bWithScale)
			{
				tempPnt.X *= Scale;
				tempPnt.Y *= Scale;
			}

			tempPnt -= GetOffset();

			return tempPnt;
		}

		//=============================================================================
		public double Get_Scale()
		{
			return this.Scale;
		}
	}
}
