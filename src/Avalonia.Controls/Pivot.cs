using System.ComponentModel;
using System.Linq;
using Avalonia.Collections;
using Avalonia.Automation.Peers;
using Avalonia.Controls.Generators;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Avalonia.Automation;
using Avalonia.Controls.Metadata;
using Avalonia.Data;
using System;
using Avalonia.Rendering.Composition;
using Avalonia.Controls.Utils;
using System.Collections.Specialized;

namespace Avalonia.Controls
{

    public enum PivotHeaderPlacement
    {
        Top,
        Bottom
    }

    /// <summary>
    /// A tab control that displays a tab strip along with the content of the selected tab.
    /// </summary>
    [TemplatePart("PART_Header", typeof(PivotHeader))]
    [TemplatePart("PART_PivotItemsPresenter", typeof(ItemsPresenter))]
    public class Pivot : SelectingItemsControl
    {
        /// <summary>
        /// Defines the <see cref="PivotHeaderPlacement"/> property.
        /// </summary>
        public static readonly StyledProperty<PivotHeaderPlacement> PivotHeaderPlacementProperty =
            AvaloniaProperty.Register<Pivot, PivotHeaderPlacement>(nameof(PivotHeaderPlacement), defaultValue: PivotHeaderPlacement.Top);

        /// <summary>
        /// Defines the <see cref="HorizontalContentAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<HorizontalAlignment> HorizontalContentAlignmentProperty =
            ContentControl.HorizontalContentAlignmentProperty.AddOwner<Pivot>();

        /// <summary>
        /// Defines the <see cref="VerticalContentAlignment"/> property.
        /// </summary>
        public static readonly StyledProperty<VerticalAlignment> VerticalContentAlignmentProperty =
            ContentControl.VerticalContentAlignmentProperty.AddOwner<Pivot>();
        
        /// <summary>
        /// The default value for the <see cref="ItemsControl.ItemsPanel"/> property.
        /// </summary>
        private static readonly FuncTemplate<Panel> DefaultPanel =
            new FuncTemplate<Panel>(() => new Grid());

        /// <summary>
        /// Defines the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
            AvaloniaProperty.Register<HeaderedContentControl, IDataTemplate?>(nameof(HeaderTemplate));

        /// <summary>
        /// Initializes static members of the <see cref="Pivot"/> class.
        /// </summary>
        static Pivot()
        {
            SelectionModeProperty.OverrideDefaultValue<Pivot>(SelectionMode.AlwaysSelected);
            ItemsPanelProperty.OverrideDefaultValue<Pivot>(DefaultPanel);
            AffectsMeasure<Pivot>(PivotHeaderPlacementProperty);
            SelectedItemProperty.Changed.AddClassHandler<Pivot>((x, e) => x.UpdateSelectedContent());
            AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<Pivot>(AutomationControlType.Tab);
        }

        /// <summary>
        /// Gets or sets the horizontal alignment of the content within the control.
        /// </summary>
        public HorizontalAlignment HorizontalContentAlignment
        {
            get { return GetValue(HorizontalContentAlignmentProperty); }
            set { SetValue(HorizontalContentAlignmentProperty, value); }
        }

        /// <summary>
        /// Gets or sets the vertical alignment of the content within the control.
        /// </summary>
        public VerticalAlignment VerticalContentAlignment
        {
            get { return GetValue(VerticalContentAlignmentProperty); }
            set { SetValue(VerticalContentAlignmentProperty, value); }
        }

        /// <summary>
        /// Gets or sets the PivotHeader placement of the Pivot.
        /// </summary>
        public PivotHeaderPlacement PivotHeaderPlacement
        {
            get { return GetValue(PivotHeaderPlacementProperty); }
            set { SetValue(PivotHeaderPlacementProperty, value); }
        }

        /// <summary>
        /// Gets or sets the data template used to display the header content of the control.
        /// </summary>
        public IDataTemplate? HeaderTemplate
        {
            get => GetValue(HeaderTemplateProperty);
            set => SetValue(HeaderTemplateProperty, value);
        }

        internal PivotHeader? HeaderPart { get; private set; }
        internal ItemsPresenter? ItemsPresenterPart { get; private set; }

        protected internal override Control CreateContainerForItemOverride() => new PivotItem();
        protected internal override bool IsItemItsOwnContainerOverride(Control item) => item is PivotItem;

        protected internal override void PrepareContainerForItemOverride(Control element, object? item, int index)
        {
            base.PrepareContainerForItemOverride(element, item, index);

            if (element is PivotItem pivotItem)
            {
                if (ItemTemplate is { } it)
                    pivotItem.ContentTemplate = it;
            }

            if (index == SelectedIndex && element is ContentControl container)
            {
                UpdateSelectedContent();
            }
        }

        protected internal override void ClearContainerForItemOverride(Control element)
        {
            base.ClearContainerForItemOverride(element);
            UpdateSelectedContent();
        }

        private void UpdateSelectedContent()
        {
            if(HeaderPart != null)
            {
                HeaderPart.SelectedIndex = SelectedIndex;
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            HeaderPart = e.NameScope.Get<PivotHeader>("PART_Header");
            ItemsPresenterPart = e.NameScope.Get<ItemsPresenter>("PART_PivotItemsPresenter");

            if (HeaderPart != null)
            {
                HeaderPart.AddHandler(SelectionChangedEvent, (o, e) => SelectedIndex = HeaderPart.SelectedIndex);
            }

            var border =  e.NameScope.Find<Border>("PART_Border");

            if (border != null)
            {
                border.AddHandler(Gestures.ScrollGestureEvent, OnScrolled);
                border.AddHandler(Gestures.ScrollGestureEndedEvent, OnScrolledEnd);
            }
        }

        protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
        {
            base.ContainerIndexChangedOverride(container, oldIndex, newIndex);

            var selectedIndex = SelectedIndex;

            if (selectedIndex == oldIndex || selectedIndex == newIndex)
                UpdateSelectedContent();
        }

        private void OnScrolledEnd(object? sender, ScrollGestureEndedEventArgs e)
        {
        }

        private void OnScrolled(object? sender, ScrollGestureEventArgs e)
        {
            if(SelectedIndex != -1)
            {
                var container = ItemContainerGenerator.ContainerFromIndex(SelectedIndex);

                if(container != null)
                {
                    var compositor = ElementComposition.GetElementVisual(container);

                    if(compositor != null)
                    {
                        var offset = compositor.Offset;

                        offset.X -= (float)e.Delta.X;

                        compositor.Offset = offset;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);

            if (e.NavigationMethod == NavigationMethod.Directional)
            {
                e.Handled = UpdateSelectionFromEventSource(e.Source);
            }
        }

        /// <inheritdoc/>
        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && e.Pointer.Type == PointerType.Mouse)
            {
                e.Handled = UpdateSelectionFromEventSource(e.Source);
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            if (e.InitialPressMouseButton == MouseButton.Left && e.Pointer.Type != PointerType.Mouse)
            {
                var container = GetContainerFromEventSource(e.Source);
                if (container != null
                    && container.GetVisualsAt(e.GetPosition(container))
                        .Any(c => container == c || container.IsVisualAncestorOf(c)))
                {
                    e.Handled = UpdateSelectionFromEventSource(e.Source);
                }
            }
        }
    }
}
