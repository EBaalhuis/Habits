namespace Habits;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    static Color ToggledOnColor => Colors.Orange;
    static Color ToggledOffColor => Colors.Black;


    bool GymToggled = false;

    private void OnGymClicked(object? sender, EventArgs e)
    {
        GymToggled = !GymToggled;
        GymBtn.BorderColor = GymToggled ? ToggledOnColor : ToggledOffColor;
    }
}
