﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a scrollable area that can contain other visible elements.
    /// </summary>
    [TemplatePart(Name = ElementViewportControlName, Type = typeof(ViewportControl))]
    [TemplatePart(Name = ElementContentElementName, Type = typeof(ScrollerContentPresenter))]
    [TemplatePart(Name = ElementVerticalScrollBarName, Type = typeof(ScrollBar))]
    [TemplateVisualState(GroupName = ScrollGroup, Name = ScrollingState)]
    [TemplateVisualState(GroupName = ScrollGroup, Name = NotScrollingState)]
    [TemplateVisualState(GroupName = ScrollHintGroup, Name = ScrollHintVisibleState)]
    [TemplateVisualState(GroupName = ScrollHintGroup, Name = ScrollHintHiddenState)]
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Scroller")]
    public sealed class Scroller : ContentControl
    {
        private const string ElementViewportControlName = "ViewportControl";
        private const string ElementContentElementName = "ContentElement";
        private const string ElementHorizontalScrollBarName = "HorizontalScrollBar";
        private const string ElementVerticalScrollBarName = "VerticalScrollBar";

        private const string ScrollGroup = "ScrollStates";
        private const string ScrollingState = "Scrolling";
        private const string NotScrollingState = "NotScrolling";

        private const string ScrollHintGroup = "ScrollHintStates";
        private const string ScrollHintVisibleState = "ScrollHintVisible";
        private const string ScrollHintHiddenState = "ScrollHintHidden";

        private ViewportControl _container;
        private ScrollerContentPresenter _contentElement;
        private ScrollBar _horizontalScroll;
        private ScrollBar _verticalScroll;

        private VisualStateGroup _scrollHintStates;

        private bool _loaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Microsoft.Phone.Controls.Scroller"/> class.
        /// </summary>
        public Scroller()
        {
            DefaultStyleKey = typeof(Scroller);

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            UpdateComputedHorizontalScrollBarVisibility();
            UpdateComputedVerticalScrollBarVisibility();
        }

        #region public ScrollBarVisibility HorizontalScrollBarVisibility

        /// <summary>
        /// Gets or sets a value that indicates whether a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar"/> should be displayed.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ScrollBarVisibility"/> value that indicates whether a horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar"/> should be displayed. The default value is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Disabled"/>.
        /// </returns>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); }
            set { SetValue(HorizontalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.HorizontalScrollBarVisibility"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.HorizontalScrollBarVisibility"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(
            "HorizontalScrollBarVisibility",
            typeof(ScrollBarVisibility),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnHorizontalScrollBarVisibilityChanged()));

        private void OnHorizontalScrollBarVisibilityChanged()
        {
            UpdateContainerManipulationLockMode();
            UpdateComputedHorizontalScrollBarVisibility();
        }

        #endregion

        #region public ScrollBarVisibility VerticalScrollBarVisibility

        /// <summary>
        /// Gets or sets a value that indicates whether a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar"/> should be displayed.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Windows.Controls.ScrollBarVisibility"/> value that indicates whether a vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar"/> should be displayed. The default value is <see cref="F:System.Windows.Controls.ScrollBarVisibility.Disabled"/>.
        /// </returns>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); }
            set { SetValue(VerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.VerticalScrollBarVisibility"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.VerticalScrollBarVisibility"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(
            "VerticalScrollBarVisibility",
            typeof(ScrollBarVisibility),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnVerticalScrollBarVisibilityChanged()));

        private void OnVerticalScrollBarVisibilityChanged()
        {
            UpdateContainerManipulationLockMode();
            UpdateComputedVerticalScrollBarVisibility();
        }

        #endregion

        #region public Visibility ComputedHorizontalScrollBarVisibility

        /// <summary>
        /// Gets a value that indicates whether the horizontal <see cref="T:System.Windows.Controls.Primitives.ScrollBar"/> is visible.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Windows.Visibility"/> that indicates whether the horizontal scroll bar is visible. The default value is <see cref="F:System.Windows.Visibility.Visible"/>.
        /// </returns>
        public Visibility ComputedHorizontalScrollBarVisibility
        {
            get { return (Visibility)GetValue(ComputedHorizontalScrollBarVisibilityProperty); }
            private set { SetValue(ComputedHorizontalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.ComputedHorizontalScrollBarVisibility"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ComputedHorizontalScrollBarVisibility"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ComputedHorizontalScrollBarVisibilityProperty = DependencyProperty.Register(
            "ComputedHorizontalScrollBarVisibility",
            typeof(Visibility),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnComputedHorizontalScrollBarVisibilityChanged()));

        private void OnComputedHorizontalScrollBarVisibilityChanged()
        {
            if (ComputedHorizontalScrollBarVisibility == Visibility.Visible)
            {
                QueueShowScrollHint();
            }
            else
            {
                HideScrollHint();
            }
        }

        private void UpdateComputedHorizontalScrollBarVisibility()
        {
            ComputedHorizontalScrollBarVisibility =
                ScrollableWidth > 0 && (HorizontalScrollBarVisibility == ScrollBarVisibility.Auto || HorizontalScrollBarVisibility == ScrollBarVisibility.Visible) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region public Visibility ComputedVerticalScrollBarVisibility

        /// <summary>
        /// Gets a value that indicates whether the vertical <see cref="T:System.Windows.Controls.Primitives.ScrollBar"/> is visible.
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.Windows.Visibility"/> that indicates whether the vertical scroll bar is visible. The default value is <see cref="F:System.Windows.Visibility.Visible"/>.
        /// </returns>
        public Visibility ComputedVerticalScrollBarVisibility
        {
            get { return (Visibility)GetValue(ComputedVerticalScrollBarVisibilityProperty); }
            private set { SetValue(ComputedVerticalScrollBarVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.ComputedVerticalScrollBarVisibility"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ComputedVerticalScrollBarVisibility"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ComputedVerticalScrollBarVisibilityProperty = DependencyProperty.Register(
            "ComputedVerticalScrollBarVisibility",
            typeof(Visibility),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnComputedVerticalScrollBarVisibilityChanged()));

        private void OnComputedVerticalScrollBarVisibilityChanged()
        {
            if (ComputedVerticalScrollBarVisibility == Visibility.Visible)
            {
                QueueShowScrollHint();
            }
            else
            {
                HideScrollHint();
            }
        }

        private void UpdateComputedVerticalScrollBarVisibility()
        {
            ComputedVerticalScrollBarVisibility =
                ScrollableHeight > 0 && (VerticalScrollBarVisibility == ScrollBarVisibility.Auto || VerticalScrollBarVisibility == ScrollBarVisibility.Visible) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region public double ExtentWidth

        /// <summary>
        /// Gets the horizontal size of all the content for display in the <see cref="T:Microsoft.Phone.Controls.Scroller"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The horizontal size of all the content for display in the <see cref="T:Microsoft.Phone.Controls.Scroller"/>.
        /// </returns>
        public double ExtentWidth
        {
            get { return (double)GetValue(ExtentWidthProperty); }
            private set { SetValue(ExtentWidthProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ExtentWidth"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ExtentWidth"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ExtentWidthProperty = DependencyProperty.Register(
            "ExtentWidth",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnExtentWidthChanged()));

        private void OnExtentWidthChanged()
        {
            UpdateScrollableWidth();
        }

        #endregion

        #region public double ExtentHeight

        /// <summary>
        /// Gets the vertical size of all the content for display in the <see cref="T:Microsoft.Phone.Controls.Scroller"/>.
        /// </summary>
        /// 
        /// <returns>
        /// The vertical size of all the content for display in the <see cref="T:Microsoft.Phone.Controls.Scroller"/>.
        /// </returns>
        public double ExtentHeight
        {
            get { return (double)GetValue(ExtentHeightProperty); }
            private set { SetValue(ExtentHeightProperty, value); }
        }

        /// <summary>
        /// Identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ExtentHeight"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ExtentHeight"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ExtentHeightProperty = DependencyProperty.Register(
            "ExtentHeight",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnExtentHeightChanged()));

        private void OnExtentHeightChanged()
        {
            UpdateScrollableHeight();
        }

        #endregion

        #region public double ScrollableWidth

        /// <summary>
        /// Gets a value that represents the horizontal size of the area that can be scrolled; the difference between the width of the extent and the width of the viewport.
        /// </summary>
        /// 
        /// <returns>
        /// The horizontal size of the area that can be scrolled. This property has no default value.
        /// </returns>
        public double ScrollableWidth
        {
            get { return (double)GetValue(ScrollableWidthProperty); }
            private set { SetValue(ScrollableWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.ScrollableWidth"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ScrollableWidth"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ScrollableWidthProperty = DependencyProperty.Register(
            "ScrollableWidth",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnScrollableWidthChanged()));

        private void OnScrollableWidthChanged()
        {
            UpdateComputedHorizontalScrollBarVisibility();
        }

        private void UpdateScrollableWidth()
        {
            ScrollableWidth = Math.Max(ExtentWidth - ViewportWidth, 0);
        }

        #endregion

        #region public double ScrollableHeight

        /// <summary>
        /// Gets a value that represents the vertical size of the area that can be scrolled; the difference between the height of the extent and the height of the viewport.
        /// </summary>
        /// 
        /// <returns>
        /// The vertical size of the area that can be scrolled. This property has no default value.
        /// </returns>
        public double ScrollableHeight
        {
            get { return (double)GetValue(ScrollableHeightProperty); }
            private set { SetValue(ScrollableHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.ScrollableHeight"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ScrollableHeight"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ScrollableHeightProperty = DependencyProperty.Register(
            "ScrollableHeight",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnScrollableHeightChanged()));

        private void OnScrollableHeightChanged()
        {
            UpdateComputedVerticalScrollBarVisibility();
        }

        private void UpdateScrollableHeight()
        {
            ScrollableHeight = Math.Max(ExtentHeight - ViewportHeight, 0);
        }

        #endregion

        #region public double ViewportWidth

        /// <summary>
        /// Gets a value that contains the horizontal size of the viewable content.
        /// </summary>
        /// 
        /// <returns>
        /// The horizontal size of the viewable content. The default value is 0.0.
        /// </returns>
        public double ViewportWidth
        {
            get { return (double)GetValue(ViewportWidthProperty); }
            private set { SetValue(ViewportWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.ViewportWidth"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ViewportWidth"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ViewportWidthProperty = DependencyProperty.Register(
            "ViewportWidth",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnViewportWidthChanged()));

        private void OnViewportWidthChanged()
        {
            UpdateScrollableWidth();
        }

        #endregion

        #region public double ViewportHeight

        /// <summary>
        /// Gets a value that contains the vertical size of the viewable content.
        /// </summary>
        /// 
        /// <returns>
        /// The vertical size of the viewable content. This property has no default value.
        /// </returns>
        public double ViewportHeight
        {
            get { return (double)GetValue(ViewportHeightProperty); }
            private set { SetValue(ViewportHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.ViewportHeight"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.ViewportHeight"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty ViewportHeightProperty = DependencyProperty.Register(
            "ViewportHeight",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnViewportHeightChanged()));

        private void OnViewportHeightChanged()
        {
            UpdateScrollableHeight();
        }

        #endregion

        #region public double HorizontalOffset

        /// <summary>
        /// Gets a value that contains the horizontal offset of the scrolled content.
        /// </summary>
        /// 
        /// <returns>
        /// The horizontal offset of the scrolled content. The default value is 0.0.
        /// </returns>
        public double HorizontalOffset
        {
            get { return (double)GetValue(HorizontalOffsetProperty); }
            private set { SetValue(HorizontalOffsetProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.HorizontalOffset"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.HorizontalOffset"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
            "HorizontalOffset",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnHorizontalOffsetChanged(e)));

        private void OnHorizontalOffsetChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_horizontalScroll != null)
            {
                _horizontalScroll.Value = (double)e.NewValue;
            }
        }

        #endregion

        #region public double VerticalOffset

        /// <summary>
        /// Gets a value that contains the vertical offset of the scrolled content.
        /// </summary>
        /// 
        /// <returns>
        /// The vertical offset of the scrolled content. The default value is 0.0.
        /// </returns>
        public double VerticalOffset
        {
            get { return (double)GetValue(VerticalOffsetProperty); }
            private set { SetValue(VerticalOffsetProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="P:Microsoft.Phone.Controls.Scroller.VerticalOffset"/> dependency property.
        /// </summary>
        /// 
        /// <returns>
        /// The identifier for the <see cref="P:Microsoft.Phone.Controls.Scroller.VerticalOffset"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
            "VerticalOffset",
            typeof(double),
            typeof(Scroller),
            new PropertyMetadata((d, e) => ((Scroller)d).OnVerticalOffsetChanged(e)));

        private void OnVerticalOffsetChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_verticalScroll != null)
            {
                _verticalScroll.Value = (double)e.NewValue;
            }
        }

        #endregion

        /// <summary>
        /// Builds the visual tree for the <see cref="T:Microsoft.Phone.Controls.Scroller"/> control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_container != null)
            {
                _container.ManipulationStateChanged -= OnContainerManipulationStateChanged;
                _container.ViewportChanged -= OnContainerViewportChanged;
            }

            if (_contentElement != null)
            {
                _contentElement.SizeChanged -= OnContentElementSizeChanged;
                _contentElement.ScrollOwner = null;
            }

            if (_scrollHintStates != null)
            {
                _scrollHintStates.CurrentStateChanged -= OnScrollHintStateChanged;
            }

            _container = GetTemplateChild(ElementViewportControlName) as ViewportControl;
            _contentElement = GetTemplateChild(ElementContentElementName) as ScrollerContentPresenter;
            _horizontalScroll = GetTemplateChild(ElementHorizontalScrollBarName) as ScrollBar;
            _verticalScroll = GetTemplateChild(ElementVerticalScrollBarName) as ScrollBar;
            _scrollHintStates = GetTemplateChild(ScrollHintGroup) as VisualStateGroup;

            if (_container != null)
            {
                _container.ManipulationStateChanged += OnContainerManipulationStateChanged;
                _container.ViewportChanged += OnContainerViewportChanged;
                _container.SizeChanged += OnContainerSizeChanged;
            }
            else
            {
                ClearValue(ExtentHeightProperty);
                ClearValue(ViewportHeightProperty);
                ClearValue(VerticalOffsetProperty);
            }

            if (_contentElement != null)
            {
                _contentElement.SizeChanged += OnContentElementSizeChanged;
                _contentElement.ScrollOwner = this;
            }

            if (_scrollHintStates != null)
            {
                _scrollHintStates.CurrentStateChanged += OnScrollHintStateChanged;
            }

            UpdateContainerManipulationLockMode();
        }

        /// <summary>
        /// Scrolls the content that is within the <see cref="T:Microsoft.Phone.Controls.Scroller"/> to the specified horizontal offset position.
        /// </summary>
        /// <param name="offset">The position that the content scrolls to.</param>
        public void ScrollToHorizontalOffset(double offset)
        {
            if (_container != null)
            {
                _container.SetViewportOrigin(new Point(offset, _container.Viewport.Y));
            }
        }

        /// <summary>
        /// Scrolls the content that is within the <see cref="T:Microsoft.Phone.Controls.Scroller"/> to the specified vertical offset position.
        /// </summary>
        /// <param name="offset">The position that the content scrolls to.</param>
        public void ScrollToVerticalOffset(double offset)
        {
            if (_container != null)
            {
                _container.SetViewportOrigin(new Point(_container.Viewport.X, offset));
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;

            QueueShowScrollHint();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _loaded = false;
        }

        private void OnContainerManipulationStateChanged(object sender, ManipulationStateChangedEventArgs e)
        {
            if (_container.ManipulationState == ManipulationState.Idle)
            {
                GoToState(NotScrollingState, true);
            }
            else
            {
                GoToState(ScrollingState, true);

                if (_container.ManipulationState == ManipulationState.Manipulating)
                {
                    if (!CaptureMouse())
                    {
                        UIElement focusedElement = FocusManager.GetFocusedElement() as UIElement;
                        if (focusedElement != null)
                        {
                            focusedElement.ReleaseMouseCapture();
                            if (CaptureMouse())
                            {
                                return;
                            }
                        }

                        foreach (FrameworkElement child in _container.GetLogicalChildrenBreadthFirst())
                        {
                            child.ReleaseMouseCapture();
                            if (CaptureMouse())
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void OnContainerViewportChanged(object sender, ViewportChangedEventArgs e)
        {
            Rect viewport = _container.Viewport;

            ViewportWidth = viewport.Width;
            ViewportHeight = viewport.Height;
            HorizontalOffset = viewport.X;
            VerticalOffset = viewport.Y;
        }

        private void OnContainerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateContentElementSize();
        }

        private void OnContentElementSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateContainerBounds();
        }

        private void OnScrollHintStateChanged(object sender, VisualStateChangedEventArgs e)
        {
            if (e.NewState.Name == ScrollHintVisibleState)
            {
                HideScrollHint();
            }
        }

        private void GoToState(string stateName, bool useTransitions)
        {
            VisualStateManager.GoToState(this, stateName, useTransitions);
        }

        private void QueueShowScrollHint()
        {
            Dispatcher.BeginInvoke(() =>
            {
                if (_loaded && (ComputedHorizontalScrollBarVisibility == Visibility.Visible || ComputedVerticalScrollBarVisibility == Visibility.Visible))
                {
                    GoToState(ScrollHintVisibleState, true);
                }
            });
        }

        private void HideScrollHint()
        {
            GoToState(ScrollHintHiddenState, false);
        }

        private void UpdateContainerBounds()
        {
            if (_container != null && _contentElement != null)
            {
                Rect bounds = new Rect(0, 0, _contentElement.ActualWidth, _contentElement.ActualHeight);
                _container.Bounds = bounds;

                ExtentWidth = bounds.Width;
                ExtentHeight = bounds.Height;
            }
        }

        private void UpdateContainerManipulationLockMode()
        {
            if (_container != null)
            {
                bool canHorizontallyScroll = HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled;
                bool canVerticallyScroll = VerticalScrollBarVisibility != ScrollBarVisibility.Disabled;

                if (canHorizontallyScroll && canVerticallyScroll)
                {
                    _container.ManipulationLockMode = ManipulationLockMode.PreferHorizontalOrVertical;
                }
                else if (canHorizontallyScroll)
                {
                    _container.ManipulationLockMode = ManipulationLockMode.Horizontal;
                }
                else if (canVerticallyScroll)
                {
                    _container.ManipulationLockMode = ManipulationLockMode.Vertical;
                }
                else
                {
                    _container.ManipulationLockMode = ManipulationLockMode.Free;
                }
            }
        }

        private void UpdateContentElementSize()
        {
            if (_container != null && _contentElement != null)
            {
                if (HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled)
                {
                    _contentElement.Width = _container.ActualWidth;
                }
                else
                {
                    _contentElement.ClearValue(WidthProperty);
                }

                if (VerticalScrollBarVisibility == ScrollBarVisibility.Disabled)
                {
                    _contentElement.Height = _container.ActualHeight;
                }
                else
                {
                    _contentElement.ClearValue(HeightProperty);
                }
            }
        }
    }
}
