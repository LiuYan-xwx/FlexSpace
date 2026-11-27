using Avalonia;
using ClassIsland.Core.Abstractions.Controls;

namespace FlexSpace;

public sealed partial class FlexSpaceSettingsControl : ComponentBase<FlexSpaceComponentSettings>
{
    public FlexSpaceSettingsControl()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DataContext = Settings;
    }
}
