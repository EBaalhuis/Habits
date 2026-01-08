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

    public static event EventHandler? Toggled;

    public static readonly BindableProperty HabitNameProperty = BindableProperty.Create(
        nameof(HabitName),
        typeof(string),
        typeof(HabitButton),
        default(string));

    public string HabitName
    {
        get => (string)GetValue(HabitNameProperty);
        set => SetValue(HabitNameProperty, value);
    }

    public static readonly BindableProperty DateProperty = BindableProperty.Create(
        nameof(Date),
        typeof(DateTime),
        typeof(HabitButton),
        default(DateTime));

    public DateTime Date
    {
        get => ((DateTime)GetValue(DateProperty)).Date;
        set => SetValue(DateProperty, value);
    }

    private string GetButtonText() => $"{HabitName}{GetWeightDisplay()}";
    private string GetWeightDisplay() => Entry.WeightInKg is null ? string.Empty : $" ({Entry.WeightInKg}kg)";

    public required DataAccess DataAccess { get; set; } = new();

    static Color ToggledOnColor => Colors.Orange;
    static Color ToggledOffColor => Colors.Black;

    private HabitEntry Entry { get; set; } = new();

    protected override void OnParentSet()
    {
        base.OnParentSet();
        _ = LoadEntry();
        InnerButton.Text = GetButtonText();
    }

    public async Task OnDateChanged(DateTime? date)
    {
        if (date is null) return;
        Date = date.Value;
        await LoadEntry();
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        _ = ToggleAndSaveAsync();
    }

    private async Task LoadEntry()
    {
        try
        {
            if (string.IsNullOrEmpty(HabitName)) return;
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

        if (Entry.Enabled)
        {
            var page = Application.Current?.Windows[0].Page;
            if (page is not null)
            {
                var weightInput = await page.DisplayPromptAsync("Weight", "Weight in kgs:",
                    keyboard: Keyboard.Numeric);
                if (int.TryParse(weightInput, out var weight))
                {
                    Entry.WeightInKg = weight;
                }
            }
        }
        else
        {
            Entry.WeightInKg = null;
        }


        try
        {
            await DataAccess.SaveHabitEntry(Entry);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving habit entry: {ex.Message}");
        }

        UpdateButton();
        Toggled?.Invoke(this, EventArgs.Empty);
    }

    private async Task<HabitEntry> GetEntryFromDatabase()
    {
        try
        {
            var entry = await DataAccess.GetHabitEntry(HabitName, Date);
            return entry ?? new HabitEntry { HabitName = HabitName, Date = Date };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error retrieving habit entry: {ex.Message}");
            return new HabitEntry { HabitName = HabitName, Date = Date };
        }
    }

    private void UpdateButton()
    {
        InnerButton.BorderColor = Entry.Enabled ? ToggledOnColor : ToggledOffColor;
        InnerButton.Text = GetButtonText();
    }
}
