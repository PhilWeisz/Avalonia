using Avalonia.Controls.Generators;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;

namespace Avalonia.Controls.Primitives
{
    public class PivotHeader : SelectingItemsControl
    {
        private static readonly FuncTemplate<Panel> DefaultPanel =
            new FuncTemplate<Panel>(() => new VirtualizingStackPanel { Orientation = Orientation.Horizontal });

        static PivotHeader()
        {
            SelectionModeProperty.OverrideDefaultValue<PivotHeader>(SelectionMode.AlwaysSelected);
            FocusableProperty.OverrideDefaultValue(typeof(PivotHeader), false);
            ItemsPanelProperty.OverrideDefaultValue<PivotHeader>(DefaultPanel);
        }

        protected internal override Control CreateContainerForItemOverride() => new PivotHeaderItem();
        protected internal override bool IsItemItsOwnContainerOverride(Control item) => item is PivotHeaderItem;
        protected internal override void PrepareContainerForItemOverride(Control element, object? item, int index)
        {
            base.PrepareContainerForItemOverride(element, item, index);

            if (element is PivotHeaderItem pivotHeaderItem && item is PivotItem pivotItem)
            {
                if (pivotItem.HeaderTemplate is { } header)
                    pivotHeaderItem.ContentTemplate = header;
                else if(ItemTemplate is { } it)
                    pivotHeaderItem.ContentTemplate = it;

                if (item is IHeadered headered)
                {
                    pivotHeaderItem.Content = headered.Header;
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

            if (e.Source is Visual source)
            {
                var point = e.GetCurrentPoint(source);

                if (point.Properties.IsLeftButtonPressed)
                {
                    e.Handled = UpdateSelectionFromEventSource(e.Source);
                }
            }
        }
    }
}
