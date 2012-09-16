using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interactivity;
using System.Windows.Markup;
using System.Windows.Threading;
using System.ComponentModel;
using System.Linq;




namespace SilverlightApplicationFlickBehavior {

	public class FlickBehavior : Behavior<ListBox> {
		
		#region Properties

		//using 'lazy initialization'
		private DispatcherTimer _flickTimer;        
		private Point _currentPoint;
		private Point _previousPoint;
		private Point _scrollStartOffset;
		private Point _scrollStartPoint;
		private Point _scrollTarget;
		private Vector _velocity;   
  
		#endregion

		#region Getters/Setters
		
		private ScrollViewer MyScrollViewer { get; set; }

		private ItemsPresenter MyItemPresenter { get; set; }       

		protected bool IsMouseCaptured { get; set; }

		protected Point CurrentPoint {
			get {
				if (_currentPoint == null) {
					_currentPoint = new Point();
				}
				return _currentPoint;
			}
			set {
				_currentPoint = value;
			}
		}

		protected Point PreviousPoint {
			get {
				if (_previousPoint == null) {
					_previousPoint = new Point();
				}
				return _previousPoint;
			}
			set {
				_previousPoint = value;
			}
		}

		protected Point ScrollStartOffset {
			get {
				if (_scrollStartOffset == null) {
					_scrollStartOffset = new Point();
				}
				return _scrollStartOffset;
			}
			set {
				_scrollStartOffset = value;
			}
		}

		protected Point ScrollStartPoint {
			get {
				if (_scrollStartPoint == null) {
					_scrollStartPoint = new Point();
				}
				return _scrollStartPoint;
			}
			set {
				_scrollStartPoint = value;
			}
		}

		protected Point ScrollTarget {
			get {
				if (_scrollTarget == null) {
					_scrollTarget = new Point();
				}
				return _scrollTarget;
			}
			set {
				_scrollTarget = value;
			}
		}

		protected Vector Velocity {
			get {
				if (_velocity == null) {
					_velocity = new Vector();
				}
				return _velocity;
			}
			set {
				_velocity = value;
			}
		}

		protected DispatcherTimer FlickBehaviorTimer {
			get {
				if (_flickTimer == null) {
					_flickTimer = new DispatcherTimer();
				}
				return _flickTimer;
			}
			set {
				_flickTimer = value;
			}
		}

		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="FlickBehavior"/> class.
		/// </summary>
		public FlickBehavior() {
			//default values for the dependency properties
			TimerInterval = 20;
			Speed = 100;
			Friction = 0.98;
			VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
			HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
		}

		#region Basic Behavior Events

		/// <summary>
		/// Called after the behavior is attached to an AssociatedObject.
		/// </summary>
		/// <remarks>Override this to hook up functionality to the AssociatedObject.</remarks>
		protected override void OnAttached() {
			base.OnAttached();
			
			AssociatedObject.LayoutUpdated += AssociatedObject_LayoutUpdated;           
			AssociatedObject.ReleaseMouseCapture();
		}

		/// <summary>
		/// Called when the behavior is being detached from its AssociatedObject, but before it has actually occurred.
		/// </summary>
		/// <remarks>Override this to unhook functionality from the AssociatedObject.</remarks>
		protected override void OnDetaching() {
			base.OnDetaching();
			
			FlickBehaviorTimer.Stop();
			AssociatedObject.LayoutUpdated -= AssociatedObject_LayoutUpdated;
			AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
			FlickBehaviorTimer.Tick -= FlickBehaviorTimer_Tick;
		}

		#endregion

		#region ListBox Event Handling


		/// <summary>
		/// Handles the LayoutUpdated event of the AssociatedObject control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void AssociatedObject_LayoutUpdated(object sender, EventArgs e) {

			if (MyScrollViewer == null) {  
			 
				MyScrollViewer = FindByType<ScrollViewer>(AssociatedObject).ToList().First() as ScrollViewer;
				MyItemPresenter = FindByType<ItemsPresenter>(AssociatedObject).ToList().First() as ItemsPresenter;
						   

				if (MyScrollViewer != null && MyItemPresenter!= null)
					StartUpAndMouseCapture();
				else
					throw new Exception("I can't find the elements needed on the logical tree");
			}
		}


		/// <summary>
		/// Handles the MouseMove event of the AssociatedObject control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
		private void AssociatedObject_MouseMove(object sender, MouseEventArgs e) {
		   
			if (IsMouseCaptured ) {
				
				CurrentPoint = new Point(e.GetPosition(AssociatedObject).X, e.GetPosition(AssociatedObject).Y);
				
				var delta = new Point(ScrollStartPoint.X - CurrentPoint.X, ScrollStartPoint.Y - CurrentPoint.Y);

				ScrollTarget.X = ScrollStartOffset.X + delta.X / Speed;
				ScrollTarget.Y = ScrollStartOffset.Y + delta.Y / Speed;

				MyScrollViewer.ScrollToHorizontalOffset(ScrollTarget.X);
				MyScrollViewer.ScrollToVerticalOffset(ScrollTarget.Y);
			}
		}



		/// <summary>
		/// Handles the MouseLeftButtonDown event of the ItemPresenter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		private void ItemPresenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
					 
			ScrollStartPoint = new Point(e.GetPosition(AssociatedObject).X, e.GetPosition(AssociatedObject).Y);
			ScrollStartOffset.X = MyScrollViewer.HorizontalOffset;
			ScrollStartOffset.Y = MyScrollViewer.VerticalOffset;

			IsMouseCaptured = !IsMouseCaptured;
		 
		}

		/// <summary>
		/// Handles the MouseLeftButtonUp event of the ItemPresenter control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
		private void ItemPresenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {

			IsMouseCaptured = !IsMouseCaptured; 			
		}


		/// <summary>
		/// Handles the Tick event of the FlickBehaviorTimer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void FlickBehaviorTimer_Tick(object sender, EventArgs e) {

			if (IsMouseCaptured) {
				Velocity.X = PreviousPoint.X - CurrentPoint.X;
				Velocity.Y = PreviousPoint.Y - CurrentPoint.Y;
				PreviousPoint = CurrentPoint;
			}
			else if (Velocity.Length > 1) {
				MyScrollViewer.ScrollToHorizontalOffset(ScrollTarget.X);
				MyScrollViewer.ScrollToVerticalOffset(ScrollTarget.Y);
				ScrollTarget.X += Velocity.X / Speed;
				ScrollTarget.Y += Velocity.Y / Speed;
				Velocity *= Friction;
			}
		}

	  
		#endregion

		#region Actions and Helpers

		/// <summary>
		/// Starts up and mouse capture.
		/// </summary>
		private void StartUpAndMouseCapture() {

			MyScrollViewer.VerticalScrollBarVisibility = VerticalScrollBarVisibility;
			MyScrollViewer.HorizontalScrollBarVisibility = HorizontalScrollBarVisibility;

			AssociatedObject.MouseMove += AssociatedObject_MouseMove;

			MyItemPresenter.AddHandler(UIElement.MouseLeftButtonDownEvent, new MouseButtonEventHandler(ItemPresenter_MouseLeftButtonDown), true);
			MyItemPresenter.AddHandler(UIElement.MouseLeftButtonUpEvent, new MouseButtonEventHandler(ItemPresenter_MouseLeftButtonUp), true);

			FlickBehaviorTimer.Interval = new TimeSpan(0, 0, 0, 0, TimerInterval);
			FlickBehaviorTimer.Tick += FlickBehaviorTimer_Tick;
			FlickBehaviorTimer.Start();

		}

		/// <summary>
		/// Finds a object inside the VisualTree guiven a type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="control">The control.</param>
		/// <returns></returns>
		protected IEnumerable<DependencyObject> FindByType<T>(DependencyObject control) where T : class {

			var childrenCount = VisualTreeHelper.GetChildrenCount(control);

			for (var index = 0; index < childrenCount; index++) {

				var child = VisualTreeHelper.GetChild(control, index);
				if (child is T) yield return child;

				foreach (var desc in FindByType<T>(child)) {
					if (desc is T) yield return desc;
				}
			}
		}

		#endregion

		#region Dependency properties

		#region ScrollBar Properties
		
		public static readonly DependencyProperty VerticalScrollBarVisibilityProperty =
		  DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(FlickBehavior), null);

		[Category("ScrollBar Properties")]
		public ScrollBarVisibility VerticalScrollBarVisibility {
			get { return (ScrollBarVisibility)this.GetValue(FlickBehavior.VerticalScrollBarVisibilityProperty); }
			set { this.SetValue(FlickBehavior.VerticalScrollBarVisibilityProperty, value); }
		}

		public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty =
		 DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), typeof(FlickBehavior), null);

		[Category("ScrollBar Properties")]
		public ScrollBarVisibility HorizontalScrollBarVisibility {
			get { return (ScrollBarVisibility)this.GetValue(FlickBehavior.HorizontalScrollBarVisibilityProperty); }
			set { this.SetValue(FlickBehavior.HorizontalScrollBarVisibilityProperty, value); }
		}

		#endregion

		#region Flick Properties
	   
		public static readonly DependencyProperty SpeedProperty =
			DependencyProperty.Register("Speed", typeof(double), typeof(FlickBehavior), null);

		[Category("Flick Properties"), CustomPropertyValueEditor(CustomPropertyValueEditor.PropertyBinding)]
		public double Speed {
			get { return (double)this.GetValue(FlickBehavior.SpeedProperty); }
			set { this.SetValue(FlickBehavior.SpeedProperty, value); }
		}

		public static readonly DependencyProperty FrictionProperty =
		   DependencyProperty.Register("Friction", typeof(double), typeof(FlickBehavior), null);

		[Category("Flick Properties"), CustomPropertyValueEditor(CustomPropertyValueEditor.PropertyBinding)]
		public double Friction {
			get { return (double)this.GetValue(FlickBehavior.FrictionProperty); }
			set { this.SetValue(FlickBehavior.FrictionProperty, value); }
		}


		public static readonly DependencyProperty TimerIntervalProperty =
		   DependencyProperty.Register("TimerInterval", typeof(int), typeof(FlickBehavior), null);

		[Category("Flick Properties"), CustomPropertyValueEditor(CustomPropertyValueEditor.PropertyBinding)]
		public int TimerInterval {
			get { return (int)this.GetValue(FlickBehavior.TimerIntervalProperty); }
			set { this.SetValue(FlickBehavior.TimerIntervalProperty, value); }
		}
		#endregion

		#endregion
	}

	#region Suport Clases

	public class Point {
		
		public Point() { X = 0; Y = 0; }

		public Point(double x, double y) { X = x; Y = y; }

		public double X { get; set; }

		public double Y { get; set; }
	}

	/// <summary>
	/// It's a very tinny vector class with just the
	/// basic to provide support to this behavior
	/// </summary>
	public class Vector : Point {

		public Vector() { X = 0; Y = 0; }

		public Vector(double x, double y) { X = x;  Y = y;  }

		public double Length {
			get {
				return Math.Sqrt(LengthSquared);
			}
		}

		public double LengthSquared {
			get {
				return (this.X * this.X) + (this.Y * this.Y);
			}
		}

		public static Vector operator *(Vector Point1, double scalar) {
			return new Vector(Point1.X * scalar, Point1.Y * scalar);
		}
	}

	
	#endregion
}

