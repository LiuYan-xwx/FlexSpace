using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Core.Assists;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Controls;
using ClassIsland.Core.Models.Components;

namespace FlexSpace;

[ComponentInfo("48C3220F-3D82-4F3C-8984-D19D97AA9E2E", "弹性空白", "\uf023", "按权重填充同一行的剩余空间。")]
public sealed partial class FlexSpaceComponent : ComponentBase<FlexSpaceComponentSettings>
{
    private FlexSpaceRow? _row;
    private ListBoxItem? _item;
    private IDisposable? _widthOverride;
    private IDisposable? _clipOverride;
    private IDisposable? _opacityOverride;

    public FlexSpaceComponent()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        var presenter = this.FindAncestorOfType<ComponentPresenter>();
        var list = this.FindAncestorOfType<ListBox>();
        var item = this.FindAncestorOfType<ListBoxItem>();
        if (presenter is not { IsOnMainWindow: true, IsRootComponent: true } ||
            list?.Tag is not MainWindowLineSettings ||
            item?.GetVisualParent() is not StackPanel panel || !panel.Classes.Contains("components-panel"))
        {
            return;
        }

        _row = FlexSpaceRow.Find(panel);
        if (_row is not null)
        {
            _item = item;
            _row.Add(item, this);
            Settings.PropertyChanged += SettingsChanged;
            UpdateOpacityOverride();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (_item is not null)
        {
            Settings.PropertyChanged -= SettingsChanged;
            _row?.Remove(_item);
            _row = null;
            _item = null;
            _widthOverride?.Dispose();
            _widthOverride = null;
            _clipOverride?.Dispose();
            _clipOverride = null;
            _opacityOverride?.Dispose();
            _opacityOverride = null;
        }

        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == MainWindowStylesAssist.MainWindowInEditModeProperty)
        {
            UpdateOpacityOverride();
        }
    }

    internal void ApplyWidth(double width)
    {
        if (_item is not { } item)
        {
            return;
        }

        // Apply the width outside the host's presenter so its margins are included in the allocation.
        if (_widthOverride is null || item.Width != width)
        {
            var previous = _widthOverride;
            _widthOverride = item.SetValue(WidthProperty, width, BindingPriority.Animation);
            previous?.Dispose();
        }

        // A theme's minimum content width can extend outside a zero-width item.
        if (width == 0)
        {
            _clipOverride ??= item.SetValue(ClipToBoundsProperty, true, BindingPriority.Animation);
        }
        else
        {
            _clipOverride?.Dispose();
            _clipOverride = null;
        }
    }

    private void SettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FlexSpaceComponentSettings.ForceTransparent))
        {
            UpdateOpacityOverride();
            return;
        }

        _row?.InvalidateLayout();
    }

    private void UpdateOpacityOverride()
    {
        if (_item is not { } item)
        {
            return;
        }

        if (Settings.ForceTransparent && !MainWindowStylesAssist.GetMainWindowInEditMode(this))
        {
            _opacityOverride ??= item.SetValue(OpacityProperty, 0, BindingPriority.Animation);
        }
        else
        {
            _opacityOverride?.Dispose();
            _opacityOverride = null;
        }
    }
}
