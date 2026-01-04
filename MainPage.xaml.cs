using Habits.Data;
using Habits.Models;

namespace Habits;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    public required DataAccess DataAccess { get; set; } = new();

    static string Gym => "Gym";
    static Color ToggledOnColor => Colors.Orange;
    static Color ToggledOffColor => Colors.Black;

    private HabitEntry GymEntry { get; set; } = new HabitEntry { Habit = Gym, Date = new DateTime(2026, 01, 04) };

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            var entry = await DataAccess.GetHabitEntryByHabit(Gym);
            GymEntry = entry ?? new HabitEntry { Habit = Gym, Date = new DateTime(2026, 01, 04) };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading habit entry: {ex.Message}");
        }

        UpdateButton();
    }

    private async void OnGymClicked(object? sender, EventArgs e)
    {
        GymEntry.Enabled = !GymEntry.Enabled;
        UpdateButton();
        await DataAccess.SaveHabitEntry(GymEntry);
    }

    private void UpdateButton()
    {
        GymBtn.BorderColor = GymEntry.Enabled ? ToggledOnColor : ToggledOffColor;
    }
}
