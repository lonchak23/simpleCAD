﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using simpleCAD.Geometry;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Controls;
using System.Windows.Data;

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

			SimpleCAD.cadToolTipProperty = DependencyProperty.Register(
				"cadToolTip",
				typeof(ITooltip),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(null));

			SimpleCAD.StateProperty = DependencyProperty.Register(
				"State",
				typeof(SimpleCAD_State),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(null, On_State_Changed));

			SimpleCAD.SelectionBrushProperty = DependencyProperty.Register(
				"SelectionBrush",
				typeof(Brush),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(Brushes.Blue, On_SelectionBrush_Changed));

			SimpleCAD.MousePointPropertyKey = DependencyProperty.RegisterReadOnly(
				"MousePoint",
				typeof(Point),
				typeof(SimpleCAD),
				new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.None));
			SimpleCAD.MousePointProperty = MousePointPropertyKey.DependencyProperty;
		}

		public SimpleCAD()
		{
			OnUpdatePlotHandler += OnUpdatePlot;

			m_axes = new CoordinateAxes(this);
			AddVisualChild(m_axes);
			AddLogicalChild(m_axes);
			m_axes.Draw();

			this.Loaded += SimpleCAD_Loaded;
		}

		#endregion

		#region EventHandlers

		//=============================================================================
		private void SimpleCAD_Loaded(object sender, RoutedEventArgs e)
		{
			// place (0, 0) point at center of SimpleCAD
			// You should do it on Loaded event, otherwise ActualHeight and ActualWidth will be 0.
			double rOffset_X = this.ActualWidth / 2;
			double rOffset_Y = this.ActualHeight / 2;

			m_OffsetVector.X = -rOffset_X;
			m_OffsetVector.Y = -rOffset_Y;

			// read comment in GetLocalPoint()
			m_OffsetVector.Y *= -1;

			// redraw children
			UpdatePlot();
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
		// 
		// State property updates every time when simpleCAD state changed.
		// So simpleCAD user can implement Memento pattern for undo\redo.
		// But State is DependencyProperty, so the only way to mark State changed - 
		// is set value to it. Like this:
		// State = _GetState()
		//
		// But there is a problem - when State updated this way it calls On_State_Changed callback.
		// In callback _ClearAll() function is called and then new state is setted. 
		// If user move grip and click mouse left button, _OnInternalCommandEnded is called, State update happens and _ClearAll()
		// remove all grips. But State is not changed really.
		//
		// To solve this problem let set m_bDontUpdateState = true on internal commands(_OnCommandEnded) and ignore
		// State changing.
		private bool m_bDontUpdateState = false;

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
			_Cancel();

			_CloneGeom();

			_OnInternalCommandEnded();
		}

		//=============================================================================
		public static readonly DependencyProperty StateProperty;
		public SimpleCAD_State State
		{
			get { return (SimpleCAD_State)GetValue(SimpleCAD.StateProperty); }
			set { SetValue(SimpleCAD.StateProperty, value); }
		}
		private static void On_State_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
			if (dh != null)
				dh.On_State_Changed(e.NewValue as SimpleCAD_State);
		}
		public void On_State_Changed(SimpleCAD_State newState)
		{
			if (m_bDontUpdateState)
				return;

			// Deep clone state.
			// If you dont clone state then SetState will set existing geometry objects as SimpleCAD children geometry.
			//
			// Bug:
			// 1. Create line and save this drawing
			// 2. Restart SimpleCAD and open the drawing.
			// 3. Select line and move grip point 2 times, so you will have 2 changes.
			// 4. Click Undo.
			// 5. Click Undo again - the drawing doesnt return in original state.
			// Instead its the last state - when grip point was moved the second time.
			//
			// Its result of using the same line instance in SimpleCAD as in Document.States collection.
			// When grip was moved changes are applied to Document.States collection.
			// Correct way - make copy of State and use it as a new state. So SimpleCAD and Document.States will
			// have independent copies of the same state.
			SimpleCAD_State clonedState = null;
			if(newState != null)
				clonedState = Utils.DeepClone<SimpleCAD_State>(newState);

			this.SetState(clonedState);
		}

		//=============================================================================
		private static event EventHandler OnUpdatePlotHandler;

		//=============================================================================
		public static readonly DependencyProperty cadToolTipProperty;
		public ITooltip cadToolTip
		{
			get { return (ITooltip)GetValue(SimpleCAD.cadToolTipProperty); }
			set { SetValue(SimpleCAD.cadToolTipProperty, value); }
		}

		//=============================================================================
		/// <summary>
		/// DependencyProperty for <see cref="Background" /> property.
		/// </summary>
		public static readonly DependencyProperty BackgroundProperty =
				DependencyProperty.Register("Background",
						typeof(Brush),
						typeof(SimpleCAD),
						new FrameworkPropertyMetadata((Brush)null,
								FrameworkPropertyMetadataOptions.AffectsRender |
								FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		/// <summary>
		/// The Background property defines the brush used to fill the area between borders.
		/// </summary>
		public Brush Background
		{
			get { return (Brush)GetValue(BackgroundProperty); }
			set { SetValue(BackgroundProperty, value); }
		}

		//=============================================================================
		public static readonly DependencyProperty DisabledBackgroundProperty =
				DependencyProperty.Register("DisabledBackground",
						typeof(Brush),
						typeof(SimpleCAD),
						new FrameworkPropertyMetadata( new SolidColorBrush(Color.FromRgb(190, 190, 190)),
								FrameworkPropertyMetadataOptions.AffectsRender |
								FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
		/// <summary>
		/// The Background property defines the brush used to fill the area between borders.
		/// </summary>
		public Brush DisabledBackground
		{
			get { return (Brush)GetValue(DisabledBackgroundProperty); }
			set { SetValue(DisabledBackgroundProperty, value); }
		}

		//=============================================================================
		public static readonly DependencyProperty SelectionBrushProperty;
		public Brush SelectionBrush
		{
			get { return (Brush)GetValue(SimpleCAD.SelectionBrushProperty); }
			set { SetValue(SimpleCAD.SelectionBrushProperty, value); }
		}
		private static void On_SelectionBrush_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			SimpleCAD dh = d as SimpleCAD;
			if (dh != null)
				UpdatePlot();
		}

		//=============================================================================
		public static readonly DependencyPropertyKey MousePointPropertyKey;
		public static readonly DependencyProperty MousePointProperty;
		public Point MousePoint
		{
			get { return (Point)GetValue(SimpleCAD.MousePointProperty); }
			protected set { SetValue(SimpleCAD.MousePointPropertyKey, value); }
		}

		#endregion

		#region Overrides

		//=============================================================================
		/// <summary>
		///     Fills in the background based on the Background property.
		/// </summary>
		protected override void OnRender(DrawingContext dc)
		{
			Brush background = Background;
			if (!this.IsEnabled)
				background = DisabledBackground;

			//
			// This code is copied from panel source.
			//
			if (background != null)
			{
				// Using the Background brush, draw a rectangle that fills the
				// render bounds of the panel.
				Size renderSize = RenderSize;
				dc.DrawRectangle(background,
								 null,
								 new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));
			}
		}

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

			if (!this.IsEnabled)
				return;

			Point globalPnt = _GetGlobalPoint(e);

			if(m_NewGeometry != null)
			{
				m_NewGeometry.OnMouseLeftButtonClick(globalPnt);
				this.cadToolTip = m_NewGeometry.Tooltip;
				if (m_NewGeometry.IsPlaced)
				{
					m_NewGeometry.Draw(this, null);
					m_NewGeometry = null;

					_OnInternalCommandEnded();

					// start new geom
					_CloneGeom();
				}
			}
			else
			{
				Point localPnt = _GetLocalPoint(e);
				HitTestResult res = VisualTreeHelper.HitTest(this, localPnt);

				if (m_gripToMove != null)
				{
					m_gripToMove = null;

					//
					// End of grip dragging.
					// Geometry property was changed.
					_OnInternalCommandEnded();

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

			if (!this.IsEnabled)
				return;

			Point globalPnt = _GetGlobalPoint(e);
			MousePoint = globalPnt;

			// move plot
			if (e.MiddleButton == MouseButtonState.Pressed)
			{
				Point pnt = _GetLocalPoint(e);
				m_TempOffsetVector = (m_MiddleBtnPressed_Point - pnt);

				// read comment in GetLocalPoint()
				m_TempOffsetVector.Y *= -1;

				UpdatePlot();
				return;
			}

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

			if (!this.IsEnabled)
				return;

			//
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Pressed)
				m_MiddleBtnPressed_Point = _GetLocalPoint(e);
		}

		//=============================================================================
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			base.OnMouseUp(e);

			if (!this.IsEnabled)
				return;

			//
			if (e.ChangedButton == MouseButton.Middle && e.ButtonState == MouseButtonState.Released)
			{
				m_OffsetVector = m_OffsetVector + m_TempOffsetVector;
				m_TempOffsetVector.X = 0.0;
				m_TempOffsetVector.Y = 0.0;

				_OnInternalCommandEnded();
			}
		}

		//=============================================================================
		protected override void OnMouseWheel(MouseWheelEventArgs e)
		{
			base.OnMouseWheel(e);

			if (!this.IsEnabled)
				return;

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

			// reverse Y
			// read comment in GetLocalPoint()
			globalPnt_UnderMouse.Y *= -1;

			m_OffsetVector = globalPnt_UnderMouse - localPnt_UnderMouse;
			
			// reverse Y
			m_OffsetVector.Y *= -1;

			m_TempOffsetVector.X = 0;
			m_TempOffsetVector.Y = 0;

			_OnInternalCommandEnded();

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

			m_gripToMove = null;
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
			Point tempPnt = localPnt;

			// reverse Y
			// read comment in GetLocalPoint()
			tempPnt.Y *= -1;

			tempPnt += GetOffset();

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

			//
			// Default WPF coordinate system has Y-axis, directed to down.
			// So left top corner has(0, 0) coordinate.
			//
			// All graphics that i have seen use Y-axis, directed to up.
			// So left BOT corner has(0, 0) coordinate.
			//
			// Lets revert Y - axis.
			// So all coordinate properties will show default "human" coordinate system.
			//
			// ---------------------------------------------------------------------------------------
			// Another way to solve this problem is applly (Y=-1) render transform to SimpleCAD control.
			// Something like this:
			//
			// this.RenderTransform = new ScaleTransform(1, -1);
			// this.RenderTransformOrigin = new Point(0.5, 0.5);
			//
			// It is much simplier but incorrect. Because all text will have correct (x,y)-point at which he should be drawn,
			// but it will be drawn with (Y=-1) scaling.
			tempPnt.Y *= -1;

			return tempPnt;
		}

		//=============================================================================
		public double Get_Scale()
		{
			return this.Scale;
		}

		//=============================================================================
		public void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (!this.IsEnabled)
				return;

			if (Key.Escape == e.Key)
				_Cancel();
			else if (SelectedGeometry != null)
				SelectedGeometry.OnKeyDown(e);
			else if (m_NewGeometry != null)
			{
				m_NewGeometry.OnKeyDown(e);

				if (m_NewGeometry.IsPlaced)
				{
					m_NewGeometry.Draw(this, null);
					m_NewGeometry = null;

					// start new geom
					_CloneGeom();
				}
			}
		}

		//=============================================================================
		private void _Cancel()
		{
			this.cadToolTip = null;

			//
			SelectedGeometry = null;

			// delete last not initialized
			if (m_NewGeometry != null)
			{
				DrawingVisual dc = m_NewGeometry.GetGeometryWrapper();
				RemoveVisualChild(dc);
				RemoveLogicalChild(dc);
				m_geometries.Remove(m_NewGeometry);

				m_NewGeometry = null;
			}

			m_gripToMove = null;
			ClearGrips();
		}

		//=============================================================================
		private void _CloneGeom()
		{
			// Clone new geometry and add it as a child
			if (m_NewGeometry == null)
			{
				ICadGeometry geomToCreate = GeometryToCreate;
				if (geomToCreate != null)
				{
					ICadGeometry geomCopy = geomToCreate.Clone();
					if (geomCopy != null)
					{
						m_NewGeometry = new GeometryWraper(this, geomCopy);
						DrawingVisual dv = m_NewGeometry.GetGeometryWrapper();

						m_geometries.Add(m_NewGeometry);
						AddVisualChild(dv);
						AddLogicalChild(dv);

						this.cadToolTip = m_NewGeometry.Tooltip;
					}
				}
			}
		}

		//=============================================================================
		private void _ClearAll()
		{
			this.cadToolTip = null;

			//
			foreach (ICadGeometry geom in m_geometries)
			{
				DrawingVisual dc = geom.GetGeometryWrapper();
				RemoveVisualChild(dc);
				RemoveLogicalChild(dc);
			}
			m_geometries.Clear();

			//
			SelectedGeometry = null;

			// delete last not initialized
			if (m_NewGeometry != null)
			{
				DrawingVisual dc = m_NewGeometry.GetGeometryWrapper();
				RemoveVisualChild(dc);
				RemoveLogicalChild(dc);
				m_geometries.Remove(m_NewGeometry);

				m_NewGeometry = null;
			}

			//
			ClearGrips();

			//
			m_MiddleBtnPressed_Point = new Point();
			m_TempOffsetVector.X = 0;
			m_TempOffsetVector.Y = 0;

			//
			AxesColor = Colors.Black;
			AxesThickness = 2.0;
			AxesLength = 50.0;
			AxesTextSize = 12.0;

			//
			GeometryToCreate = null;
			Scale = 1.0;
		}

		//=============================================================================
		private void _OnInternalCommandEnded()
		{
			//
			// Update State
			// Set m_bDontUpdateState = true to ignore State changing because
			// it is not changed really.
			m_bDontUpdateState = true;
			//
			State = GetState();
			//
			m_bDontUpdateState = false;
		}

		//=============================================================================
		private SimpleCAD_State GetState()
		{
			SimpleCAD_State curentState = new SimpleCAD_State(
				m_geometries,
				m_OffsetVector,
				GeometryToCreate,
				AxesColor,
				AxesThickness,
				AxesLength,
				AxesTextSize,
				Scale);

			// return copy of state
			// dont return curent state, because when call method SimpleCAD._ClearAll m_geometries array will be empty in returned value
			return Utils.DeepClone<SimpleCAD_State>(curentState);
		}

		//=============================================================================
		private bool SetState(SimpleCAD_State state)
		{
			_ClearAll();

			bool bInvalidate = false;
			if (state == null)
			{
				if (this.IsEnabled)
				{
					this.IsEnabled = false;
					bInvalidate = true;
				}
			}
			else if (!this.IsEnabled)
			{
				this.IsEnabled = true;
				bInvalidate = true;
			}

			//
			// call InvalidateVisual, it will call OnRender() and redraw background
			if (bInvalidate)
				InvalidateVisual();

			if (state == null)
				return false;

			m_OffsetVector = state.OffsetVector;

			if (state.Geometries != null)
			{
				foreach (ICadGeometry geom in state.Geometries)
				{
					GeometryWraper gw = geom.GetGeometryWrapper() as GeometryWraper;
					if (gw != null)
						gw.Owner = this;
					else
						gw = new GeometryWraper(this, geom);

					if (gw != null)
					{
						AddVisualChild(gw);
						AddLogicalChild(gw);

						m_geometries.Add(gw);
					}
				}
			}

			AxesColor = state.AxesColor;
			AxesThickness = state.AxesThickness;
			AxesLength = state.AxesLength;
			AxesTextSize = state.AxesTextSize;

			Scale = state.Scale;

			GeometryToCreate = state.GeometryToCreate;

			UpdatePlot();

			return true;
		}
	}
}
