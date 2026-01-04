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

    public static readonly BindableProperty DateProperty = BindableProperty.Create(
        nameof(Date),
        typeof(DateTime),
        typeof(HabitButton),
        default(DateTime),
        propertyChanged: OnDateChanged);

    public DateTime Date
    {
        get => ((DateTime)GetValue(DateProperty)).Date;
        set => SetValue(DateProperty, value);
    }

    public required DataAccess DataAccess { get; set; } = new();

    static Color ToggledOnColor => Colors.Orange;
    static Color ToggledOffColor => Colors.Black;

    private HabitEntry Entry { get; set; } = new();

    protected override void OnParentSet()
    {
        base.OnParentSet();
        _ = LoadEntry();
    }

    private static void OnDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HabitButton button)
        {
            _ = button.LoadEntry();
        }
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        _ = ToggleAndSaveAsync();
    }

    private async Task LoadEntry()
    {
        try
        {
            if (string.IsNullOrEmpty(Habit)) return;
            Entry = await GetEntryFromDatabase();
            UpdateButton();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Load entry error: {ex.Message}");
        }
    }

    private async Task ToggleAndSaveAsync()
    {
        Entry.Enabled = !Entry.Enabled;

        try
        {
            await DataAccess.SaveHabitEntry(Entry);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving habit entry: {ex.Message}");
        }

        UpdateButton();
    }

    private async Task<HabitEntry> GetEntryFromDatabase()
    {
        try
        {
            var entry = await DataAccess.GetHabitEntry(Habit, Date);
            return entry ?? new HabitEntry { Habit = Habit, Date = Date };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error retrieving habit entry: {ex.Message}");
            return new HabitEntry { Habit = Habit, Date = Date };
        }
    }

    private void UpdateButton()
    {
        InnerButton.BorderColor = Entry.Enabled ? ToggledOnColor : ToggledOffColor;
    }
}
