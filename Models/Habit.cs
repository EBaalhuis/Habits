using SQLite;

namespace Habits.Models;

public class Habit
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string? Name { get; set; }
}
