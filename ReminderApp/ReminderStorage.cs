using System.Text.Json;

namespace ReminderApp;

public static class ReminderStorage
{
    private static readonly string AppFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ReminderApp");

    private static readonly string FilePath = Path.Combine(AppFolder, "reminders.json");

    public static List<Reminder> Load()
    {
        try
        {
            if (!Directory.Exists(AppFolder))
                Directory.CreateDirectory(AppFolder);

            if (!File.Exists(FilePath))
                return new List<Reminder>();

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<Reminder>>(json) ?? new List<Reminder>();
        }
        catch
        {
            return new List<Reminder>();
        }
    }

    public static void Save(List<Reminder> reminders)
    {
        if (!Directory.Exists(AppFolder))
            Directory.CreateDirectory(AppFolder);

        var json = JsonSerializer.Serialize(reminders, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(FilePath, json);
    }
}
