using SQLite;

namespace Habits.Models;

public class HabitEntry
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string? Habit { get; set; }
    public DateTime Date { get; set; }
    public bool Enabled { get; set; }
}
