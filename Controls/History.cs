using Habits.Data;
using Habits.Models;

namespace Habits.Controls;

public partial class History : ContentView
{
	private VerticalStackLayout StackLayout { get; }

	public History()
	{
		StackLayout = [];

		StackLayout.Children.Add(new Label
		{
			Text = "History",
			FontSize = 24,
            HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		});

		Content = StackLayout;
	}

	public required DataAccess DataAccess { get; set; } = new();

	protected override void OnParentSet()
	{
		base.OnParentSet();
		_ = LoadAndRenderHistoricalDaysAsync();
	}

	private async Task LoadAndRenderHistoricalDaysAsync()
	{
		try
		{
			var views = await LoadHistoricalDays();

			// Update UI on the main thread
			Dispatcher.Dispatch(() =>
			{
				while (StackLayout.Children.Count > 1)
				{
					StackLayout.Children.RemoveAt(1);
				}

				foreach (var view in views)
				{
					StackLayout.Children.Add(view);
				}
			});
		}
		catch (Exception)
		{
			throw new InvalidOperationException("Failed to load historical days");
        }
	}

	private async Task<List<IView>> LoadHistoricalDays()
	{
		var entriesPerDate = await DataAccess.GetEntriesPerDate();
        return [.. entriesPerDate.Keys.OrderByDescending(date => date).Select(key => CreateView(entriesPerDate[key]))];
	}

    private static VerticalStackLayout CreateView(List<HabitEntry> entries) => new()
    {
        Padding = new Thickness(0, 10),
        Children =
			{
				CreateDateLabel(entries),
				CreateHabitsLayout(entries)
			}
    };

    private static Label CreateDateLabel(List<HabitEntry> entries) => new()
	{
		Text = entries.First().Date.ToShortDateString(),
        HorizontalOptions = LayoutOptions.Center,
        VerticalOptions = LayoutOptions.Center
    };

	private static VerticalStackLayout CreateHabitsLayout(List<HabitEntry> entries)
	{
		var habitsLayout = new VerticalStackLayout();
		foreach (var entry in entries)
		{
			var habitLabel = new Label
			{
				Text = $"{entry.Habit}",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			habitsLayout.Children.Add(habitLabel);
		}
		return habitsLayout;
    }
}