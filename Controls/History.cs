using Habits.Data;
using Habits.Models;
using Habits.Pages;

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

		if (Parent is not null)
		{
			HabitButton.Toggled += OnHabitButtonToggled;
			_ = LoadAndRenderHistoricalDaysAsync();
		}
		else
		{
			HabitButton.Toggled -= OnHabitButtonToggled;
		}
	}

	private void OnHabitButtonToggled(object? sender, EventArgs e)
	{
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

    private static Label CreateDateLabel(List<HabitEntry> entries)
	{
		var date = entries.First().Date;
        var label = new Label
		{
			Text = date.ToShortDateString(),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		return WithGestureRecognizer(label, date);
    }

	private static VerticalStackLayout CreateHabitsLayout(List<HabitEntry> entries)
	{
		var habitsLayout = new VerticalStackLayout();

		foreach (var entry in entries)
		{
			var habitLabel = new Label
			{
				Text = $"{entry.HabitName} - {entry.WeightInKg}kg",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
			habitLabel = WithGestureRecognizer(habitLabel, entry.Date);
            habitsLayout.Children.Add(habitLabel);
		}

		return habitsLayout;
    }

	private static Label WithGestureRecognizer(Label label, DateTime date)
	{
		label.GestureRecognizers.Add(new TapGestureRecognizer
		{
			Command = new Command(() =>
			{
				// climb up the visual tree to find the containing page
				var parent = label.Parent;
				while (parent != null && parent is not ContentPage)
				{
					parent = parent.Parent;
				}
				if (parent is MainPage page)
				{
					page.DatePicker.Date = date;
				}
			})
		});

		return label;
    }
}