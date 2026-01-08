using Habits.Controls;
using Habits.Data;

namespace Habits.Pages;

public partial class MainPage : ContentPage
{
    public DataAccess DataAccess { get; set; } = new();
    public DatePicker DatePicker { get; } = new();
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
            StackLayout.Children.Add(DatePicker);

            var habits = (await DataAccess.GetHabits()).OrderBy(h => h.Name);
            foreach (var habit in habits)
            {
                var button = new HabitButton() 
                { 
                    DataAccess = DataAccess, 
                    Date = DatePicker.Date ?? DateTime.Today,
                    HabitName = habit.Name ?? ""
                };
                DatePicker.DateSelected += async (s, e) => await button.OnDateChanged(e.NewDate);
                StackLayout.Children.Add(button);
            }

            StackLayout.Children.Add(new History() { DataAccess = DataAccess });

            StackLayout.Children.Add(new Label
            {
                Text = "v0.3",
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
