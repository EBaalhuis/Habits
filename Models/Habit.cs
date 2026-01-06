using SQLite;

namespace Habits.Models;

public class Habit : IComparable<Habit>
{
    [PrimaryKey, AutoIncrement]
    public int ID { get; set; }
    public string? Name { get; set; }

    public int CompareTo(Habit? other)
    {
        if (other is null) return 1;
        return ID.CompareTo(other.ID);
    }
}
