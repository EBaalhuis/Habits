using SQLite;

namespace Habits.Models;

public class HabitEntry
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string HabitName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? WeightInKg { get; set; }
    public bool Enabled { get; set; }
}
