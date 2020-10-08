// -----------------------------------------------------------------------
//   Copyright (C) 2018 Adam Hancock
//    
//   Viewport.cs Licensed under the MIT License. See LICENSE file in
//   the project root for full license information.  
// -----------------------------------------------------------------------

namespace Han.Wpf.ViewportControl
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    public class Viewport : ContentControl
    {
        public static readonly DependencyProperty MaxZoomProperty =
            DependencyProperty.Register(
                nameof(MaxZoom),
                typeof(double),
                typeof(Viewport),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty MinZoomProperty =
            DependencyProperty.Register(
                nameof(MinZoom),
                typeof(double),
                typeof(Viewport),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty ZoomSpeedProperty =
            DependencyProperty.Register(
                nameof(ZoomSpeed),
                typeof(float),
                typeof(Viewport),
                new PropertyMetadata(0f));

        public static readonly DependencyProperty ZoomXProperty =
            DependencyProperty.Register(
                nameof(ZoomX),
                typeof(double),
                typeof(Viewport),
                new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ZoomYProperty =
            DependencyProperty.Register(
                nameof(ZoomY),
                typeof(double),
                typeof(Viewport),
                new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty OffsetXProperty =
            DependencyProperty.Register(
                nameof(OffsetX),
                typeof(double),
                typeof(Viewport),
                new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty OffsetYProperty =
            DependencyProperty.Register(
                nameof(OffsetY),
                typeof(double),
                typeof(Viewport),
                new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BoundsProperty =
            DependencyProperty.Register(
                nameof(Bounds),
                typeof(Rect),
                typeof(Viewport),
                new FrameworkPropertyMetadata(default(Rect), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private bool _capture;
        private FrameworkElement _content;
        private Matrix _matrix;
        private Point _lastMousePosition; // needed for translate delta calculation

        public Viewport()
        {
            DefaultStyleKey = typeof(Viewport);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        public Rect Bounds
        {
            get => (Rect) GetValue(BoundsProperty);
            set => SetValue(BoundsProperty, value);
        }

        public double MaxZoom
        {
            get => (double) GetValue(MaxZoomProperty);
            set => SetValue(MaxZoomProperty, value);
        }

        public double MinZoom
        {
            get => (double) GetValue(MinZoomProperty);
            set => SetValue(MinZoomProperty, value);
        }

        public double OffsetX
        {
            get => (double) GetValue(OffsetXProperty);
            set => SetValue(OffsetXProperty, value);
        }

        public double OffsetY
        {
            get => (double) GetValue(OffsetYProperty);
            set => SetValue(OffsetYProperty, value);
        }

        public float ZoomSpeed
        {
            get => (float) GetValue(ZoomSpeedProperty);
            set => SetValue(ZoomSpeedProperty, value);
        }

        public double ZoomX
        {
            get => (double) GetValue(ZoomXProperty);
            set => SetValue(ZoomXProperty, value);
        }

        public double ZoomY
        {
            get => (double) GetValue(ZoomYProperty);
            set => SetValue(ZoomYProperty, value);
        }

        private void Arrange(Size desired, Size render)
        {
            _matrix = Matrix.Identity;

            var zx = desired.Width / render.Width;
            var zy = desired.Height / render.Height;
            var cx = render.Width < desired.Width ? render.Width / 2.0 : 0.0;
            var cy = render.Height < desired.Height ? render.Height / 2.0 : 0.0;

            var zoom = Math.Min(zx, zy);

            if (render.Width > desired.Width &&
                render.Height > desired.Height)
            {
                cx = (desired.Width - render.Width * zoom) / 2.0;
                cy = (desired.Height - render.Height * zoom) / 2.0;

                _matrix = new Matrix(zoom, 0d, 0d, zoom, cx, cy);
            }
            else
            {
                _matrix.ScaleAt(zoom, zoom, cx, cy);
            }
        }

        private void ChangeContent(FrameworkElement content, bool force)
        {
            if (force || !Equals(content, _content))
            {
                if (_content != null)
                {
                    Detach();
                    _content = null;
                }

                if (content != null)
                {
                    Attach(content);
                    _content = content;
                }
            }
        }

        private void Attach(FrameworkElement content)
        {
            content.MouseMove += OnMouseMove;
            content.MouseLeave += OnMouseLeave;
            content.MouseWheel += OnMouseWheel;
            content.MouseLeftButtonDown += OnMouseLeftButtonDown;
            content.MouseLeftButtonUp += OnMouseLeftButtonUp;
            content.MouseRightButtonDown += OnMouseRightButtonDown;
            content.SizeChanged += OnSizeChanged;
            content.IsManipulationEnabled = true;
            content.ManipulationDelta += OnManipulationDelta;
        }

        private void Detach()
        {
            _content.MouseMove -= OnMouseMove;
            _content.MouseLeave -= OnMouseLeave;
            _content.MouseWheel -= OnMouseWheel;
            _content.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            _content.MouseLeftButtonUp -= OnMouseLeftButtonUp;
            _content.MouseRightButtonDown -= OnMouseRightButtonDown;
            _content.SizeChanged -= OnSizeChanged;
            _content.IsManipulationEnabled = false;
            _content.ManipulationDelta -= OnManipulationDelta;
        }


        private double Constrain(double value, double min, double max)
        {
            if (min > max)
            {
                min = max;
            }

            if (value <= min)
            {
                return min;
            }

            if (value >= max)
            {
                return max;
            }

            return value;
        }

        private void Constrain()
        {
            var x = Constrain(_matrix.OffsetX, _content.ActualWidth - _content.ActualWidth * _matrix.M11, 0);
            var y = Constrain(_matrix.OffsetY, _content.ActualHeight - _content.ActualHeight * _matrix.M22, 0);

            _matrix = new Matrix(_matrix.M11, 0d, 0d, _matrix.M22, x, y);
        }

        private void Invalidate()
        {
            if (_content != null)
            {
                Constrain();

                _content.RenderTransformOrigin = new Point(0, 0);
                _content.RenderTransform = new MatrixTransform(_matrix);
                _content.InvalidateVisual();

                ZoomX = _matrix.M11;
                ZoomY = _matrix.M22;

                OffsetX = _matrix.OffsetX;
                OffsetY = _matrix.OffsetY;

                var rect = new Rect
                {
                    X = OffsetX * -1,
                    Y = OffsetY * -1,
                    Width = ActualWidth,
                    Height = ActualHeight
                };

                Bounds = rect;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _matrix = Matrix.Identity;
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            if (Content is FrameworkElement element)
            {
                ChangeContent(element, false);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Content is FrameworkElement element)
            {
                ChangeContent(element, true);
            }

            SizeChanged += OnSizeChanged;
            Loaded -= OnLoaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            ChangeContent(null, true);

            SizeChanged -= OnSizeChanged;
            Unloaded -= OnUnloaded;
            Loaded += OnLoaded;
        }


        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (_capture && !IsTouchInducedEvent(e))
            {
                Released();
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled && !_capture && !IsTouchInducedEvent(e))
            {
                Pressed(e.GetPosition(this));
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled && _capture && !IsTouchInducedEvent(e))
            {
                Released();
            }
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (IsEnabled && _capture && !IsTouchInducedEvent(e))
            {
                var position = e.GetPosition(this);
                var delta = position - _lastMousePosition;

                _lastMousePosition = position;
                ApplyTranslate(delta);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsEnabled && e.StylusDevice == null && !IsTouchInducedEvent(e))
            {
                ApplyScale(e.GetPosition(_content), e.Delta > 0 ? ZoomSpeed : 1 / ZoomSpeed);
            }
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsEnabled)
            {
                Reset();
            }
        }

        protected override void OnManipulationStarting(ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;

            e.Handled = true;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (IsEnabled)
            {
                ApplyTranslate(e.DeltaManipulation.Translation);
                ApplyScale(this.TranslatePoint(e.ManipulationOrigin, (UIElement)Content), e.DeltaManipulation.Scale.X);
            }

            e.Handled = true;
        }

        private void ApplyTranslate(Vector deltaInViewport)
        {
            _matrix.Translate(deltaInViewport.X, deltaInViewport.Y);

            Invalidate();
        }

        private void ApplyScale(Point positionInContent, double scale)
        {
            var x = Constrain(scale, MinZoom / _matrix.M11, MaxZoom / _matrix.M11);
            var y = Constrain(scale, MinZoom / _matrix.M22, MaxZoom / _matrix.M22);

            _matrix.ScaleAtPrepend(x, y, positionInContent.X, positionInContent.Y);

            ZoomX = _matrix.M11;
            ZoomY = _matrix.M22;

            Invalidate();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_content?.IsMeasureValid ?? false)
            {
                Arrange(_content.DesiredSize, _content.RenderSize);

                Invalidate();
            }
        }


        private void Pressed(Point position)
        {
            if (IsEnabled)
            {
                _content.Cursor = Cursors.Hand;
                _lastMousePosition = position;
                _capture = true;
            }
        }

        private void Released()
        {
            if (IsEnabled)
            {
                _content.Cursor = null;
                _capture = false;
            }
        }

        public void Reset()
        {
            _matrix = Matrix.Identity;

            if (_content != null)
            {
                Arrange(_content.DesiredSize, _content.RenderSize);
            }

            Invalidate();
        }

        private static bool IsTouchInducedEvent(MouseEventArgs e)
        {
            // Ideas from:
            // - https://social.msdn.microsoft.com/Forums/en-US/9b05e550-19c0-46a2-b19c-40f40c8bf0ec/prevent-a-wpf-application-to-interpret-touch-events-as-mouse-events?forum=wpf
            // - https://stackoverflow.com/questions/29857587/detect-if-wm-mousemove-is-caused-by-touch-pen

            return e.StylusDevice != null;
        }
    }
}
