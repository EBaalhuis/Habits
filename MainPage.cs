using Habits.Controls;
using Habits.Data;

namespace Habits;

public partial class MainPage : ContentPage
{
    public DataAccess DataAccess { get; set; } = new();
    private VerticalStackLayout StackLayout { get; }

    public MainPage()
    {
        StackLayout = new VerticalStackLayout
        {
            Padding = 30,
            Spacing = 25
        };

        Content = StackLayout;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = Render();
    }

    private async Task Render()
    {
        try
        {
            StackLayout.Children.Clear();

            var datePicker = new DatePicker();
            StackLayout.Children.Add(datePicker);

            var habits = (await DataAccess.GetHabits()).OrderBy(h => h.Name);
            foreach (var habit in habits)
            {
                var button = new HabitButton() { DataAccess = DataAccess, HabitName = habit.Name };
                StackLayout.Children.Add(button);
            }

            StackLayout.Children.Add(new History() { DataAccess = DataAccess });

            StackLayout.Children.Add(new Label
            {
                Text = "v0.2",
                FontSize = 10,
                HorizontalOptions = LayoutOptions.Center
            });
        }
        catch
        {
            throw new InvalidOperationException("Failed to render main page");
        }
    }
}
