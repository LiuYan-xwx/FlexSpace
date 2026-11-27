using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.VisualTree;
using ClassIsland.Core.Assists;
using System.Runtime.CompilerServices;

namespace FlexSpace;

internal sealed class FlexSpaceRow(StackPanel panel, Control workingRoot, LayoutTransformControl transform, Grid contentRoot)
{
    private const double EditWidth = 96;

    private static readonly ConditionalWeakTable<StackPanel, FlexSpaceRow> Rows = new();
    private readonly Dictionary<ListBoxItem, FlexSpaceComponent> _spacers = new();
    private readonly List<FlexSpaceComponent> _visibleSpacers = new();

    public static FlexSpaceRow? Find(StackPanel panel)
    {
        if (Rows.TryGetValue(panel, out var row))
        {
            return row;
        }

        var ancestors = panel.GetVisualAncestors().OfType<Control>().ToArray();
        var workingRoot = ancestors.OfType<Panel>().FirstOrDefault(p => p.Name == "WorkingRoot");
        var transform = ancestors.OfType<LayoutTransformControl>().FirstOrDefault(p => p.Name == "RootLayoutTransformControl");
        var contentRoot = ancestors.OfType<Grid>().FirstOrDefault(p => p.Name == "GridContentRoot");
        if (workingRoot is null || transform is null || contentRoot is null)
        {
            return null;
        }

        return Rows.GetValue(panel, p => new FlexSpaceRow(p, workingRoot, transform, contentRoot));
    }

    public void Add(ListBoxItem item, FlexSpaceComponent spacer)
    {
        if (_spacers.Count == 0)
        {
            panel.LayoutUpdated += OnLayoutUpdated;
        }

        _spacers.Add(item, spacer);
        InvalidateLayout();
    }

    public void Remove(ListBoxItem item)
    {
        _spacers.Remove(item);
        if (_spacers.Count == 0)
        {
            panel.LayoutUpdated -= OnLayoutUpdated;
            Rows.Remove(panel);
            _visibleSpacers.Clear();
        }
        else
        {
            InvalidateLayout();
        }
    }

    public void InvalidateLayout() => panel.InvalidateArrange();

    private void OnLayoutUpdated(object? sender, EventArgs e)
    {
        if (!panel.IsEffectivelyVisible || panel.Orientation != Orientation.Horizontal)
        {
            return;
        }

        if (MainWindowStylesAssist.GetMainWindowInEditMode(panel))
        {
            foreach (var spacer in _spacers.Values)
            {
                spacer.ApplyWidth(EditWidth);
            }

            return;
        }

        var occupiedWidth = contentRoot.Margin.Left + contentRoot.Margin.Right;
        var targetWidth = 0;
        var totalWeight = 0d;
        var visibleItemCount = 0;
        _visibleSpacers.Clear();

        foreach (var child in panel.Children)
        {
            if (!child.IsVisible)
            {
                continue;
            }

            visibleItemCount++;
            if (child is not ListBoxItem item || !_spacers.TryGetValue(item, out var spacer))
            {
                occupiedWidth += child.DesiredSize.Width;
                continue;
            }

            if (!spacer.IsEffectivelyVisible)
            {
                spacer.ApplyWidth(0);
                continue;
            }

            _visibleSpacers.Add(spacer);
            totalWeight += (double)spacer.Settings.Weight;
            if (targetWidth == 0)
            {
                targetWidth = spacer.Settings.TargetWidth;
            }
        }

        if (_visibleSpacers.Count == 0)
        {
            return;
        }

        occupiedWidth += Math.Max(0, visibleItemCount - 1) * panel.Spacing;
        var scale = transform.LayoutTransform?.Value.M11 ?? 1;
        var rowWidth = targetWidth > 0 ? targetWidth : workingRoot.Width / scale;
        var remainingWidth = double.IsFinite(rowWidth) ? Math.Max(0, rowWidth - occupiedWidth) : 0;

        var useLayoutRounding = panel.UseLayoutRounding;
        var layoutScale = TopLevel.GetTopLevel(panel)?.RenderScaling ?? 1;
        // Round cumulative shares to device pixels so each item's layout rounding cannot widen the row.
        var remainingPixels = Math.Floor(remainingWidth * layoutScale + 1e-7);
        var allocatedPixels = 0d;
        var allocatedWeight = 0d;
        foreach (var spacer in _visibleSpacers)
        {
            allocatedWeight += (double)spacer.Settings.Weight;
            var nextPixels = Math.Round(remainingPixels * (allocatedWeight / totalWeight));
            var width = useLayoutRounding
                ? (nextPixels - allocatedPixels) / layoutScale
                : remainingWidth * ((double)spacer.Settings.Weight / totalWeight);
            spacer.ApplyWidth(width);
            allocatedPixels = nextPixels;
        }
    }
}
