using CommunityToolkit.Mvvm.ComponentModel;

namespace FlexSpace;

public sealed partial class FlexSpaceComponentSettings : ObservableObject
{
    private decimal _weight = 1;
    private int _targetWidth;

    public decimal Weight
    {
        get => _weight;
        set => SetProperty(ref _weight, value > 0 ? value : 1);
    }

    public int TargetWidth
    {
        get => _targetWidth;
        set => SetProperty(ref _targetWidth, Math.Max(0, value));
    }

    [ObservableProperty]
    private bool _forceTransparent = true;


}
