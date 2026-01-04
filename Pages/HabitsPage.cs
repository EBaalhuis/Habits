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
            var views = await LoadHabits();

            // Update UI on the main thread
            Dispatcher.Dispatch(() =>
            {
                StackLayout.Children.Clear();
                foreach (var view in views)
                {
                    StackLayout.Children.Add(view);
                }
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
}