using Habits.Models;
using SQLite;

namespace Habits.Data;

public class DataAccess
{
    SQLiteAsyncConnection? Database;

    async Task Init()
    {
        if (Database is not null) return;

        Database = new SQLiteAsyncConnection(Constants.DatabasePath, Constants.Flags);
        await Database.CreateTableAsync<Habit>();
        await Database.CreateTableAsync<HabitEntry>();
    }

    public async Task<List<HabitEntry>> GetHabitEntries()
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");
        return await Database.Table<HabitEntry>().ToListAsync();
    }

    public async Task<HabitEntry> GetHabitEntry(int id)
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");
        return await Database.Table<HabitEntry>().Where(i => i.ID == id).FirstOrDefaultAsync();
    }

    public async Task<HabitEntry> GetHabitEntry(string habit, DateTime date)
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");

        return await Database.Table<HabitEntry>()
            .Where(rec => rec.HabitName == habit)
            .Where(rec => rec.Date == date)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveHabitEntry(HabitEntry entry)
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");

        if (entry.ID != 0)
        {
            return await Database.UpdateAsync(entry);
        }
        else
        {
            return await Database.InsertAsync(entry);
        }
    }

    public async Task<int> DeleteHabitEntry(HabitEntry entry)
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");
        return await Database.DeleteAsync(entry);
    }

    public async Task<Dictionary<DateTime, List<HabitEntry>>> GetEntriesPerDate()
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");

        var enabledEntries = await Database.Table<HabitEntry>()
            .Where(entry => entry.Enabled)
            .ToListAsync();

        return enabledEntries
            .GroupBy(entry => entry.Date)
            .ToDictionary(group => group.Key, group => group.ToList());
    }

    public async Task<List<Habit>> GetHabits()
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");
        return await Database.Table<Habit>().ToListAsync();
    }

    public async Task AddHabit(string name)
    {
        await Init();
        if (Database is null) throw new InvalidOperationException("Database init failed");
        await Database.InsertAsync(new Habit { Name = name });
    }
}
