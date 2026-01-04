using Habits.Data;
using Habits.Models;

namespace Habits.Controls;

public partial class HabitButton : ContentView
{
    public HabitButton()
    {
        InitializeComponent();
        InnerButton.BindingContext = this;
    }

    public static readonly BindableProperty HabitProperty = BindableProperty.Create(
        nameof(Habit),
        typeof(string),
        typeof(HabitButton),
        default(string));

    public string Habit
    {
        get => (string)GetValue(HabitProperty);
        set => SetValue(HabitProperty, value);
    }

    public required DataAccess DataAccess { get; set; } = new();

    static Color ToggledOnColor => Colors.Orange;
    static Color ToggledOffColor => Colors.Black;

    private HabitEntry Entry { get; set; } = new HabitEntry { Habit = string.Empty, Date = new DateTime(2026, 01, 04) };

    protected override async void OnParentSet()
    {
        base.OnParentSet();

        if (string.IsNullOrEmpty(Habit)) return;

        try
        {
            var entry = await DataAccess.GetHabitEntryByHabit(Habit);
            Entry = entry ?? new HabitEntry { Habit = Habit, Date = DateTime.Now };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading habit entry: {ex.Message}");
        }

        UpdateButton();
    }

    private async void OnButtonClicked(object? sender, EventArgs e)
    {
        Entry.Enabled = !Entry.Enabled;
        UpdateButton();
        await DataAccess.SaveHabitEntry(Entry);
    }

    private void UpdateButton()
    {
        InnerButton.BorderColor = Entry.Enabled ? ToggledOnColor : ToggledOffColor;
    }
}
