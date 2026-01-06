using Habits.Data;
using Habits.Models;

namespace Habits.Pages;

public partial class HabitsPage : ContentPage
{
    public DataAccess DataAccess { get; set; } = new();
    private VerticalStackLayout StackLayout { get; }

    public HabitsPage()
	{
        StackLayout = [];
        Content = StackLayout;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = LoadAndRenderHabits();
    }

    private async Task LoadAndRenderHabits()
    {
        try
        {
            var habitViews = await LoadHabits();

            // Update UI on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                StackLayout.Children.Clear();
                foreach (var view in habitViews)
                {
                    StackLayout.Children.Add(view);
                }
                StackLayout.Children.Add(GetAddHabitButton());
            });
        }
        catch (Exception)
        {
            throw new InvalidOperationException("Failed to load habits");
        }
    }

    private async Task<List<IView>> LoadHabits()
    {
        var habits = (await DataAccess.GetHabits()).OrderBy(h => h);
        return [.. habits.Select(CreateView)];
    }

    private static Label CreateView(Habit habit)
    {
        return new Label { Text = habit.Name ?? "Unnamed Habit", HorizontalOptions = LayoutOptions.Center };
    }

    private Button GetAddHabitButton()
    {
        var button = new Button
        {
            Text = "Add Habit",
            HorizontalOptions = LayoutOptions.Fill
        };
        button.Clicked += OnAddClicked;
        return button;
    }

    private async void OnAddClicked(object? sender, EventArgs e)
    {
        var result = await DisplayPromptAsync("New Habit", "Name:");

        if (!string.IsNullOrWhiteSpace(result))
        {
            var dataAccess = new DataAccess();
            await dataAccess.AddHabit(result);
            await LoadAndRenderHabits();
        }
    }
}