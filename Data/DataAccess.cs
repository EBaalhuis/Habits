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
            .Where(rec => rec.Habit == habit)
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
}
