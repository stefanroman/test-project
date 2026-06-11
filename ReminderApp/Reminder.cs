namespace ReminderApp;

public class Reminder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Message { get; set; } = "";
    public int Hour { get; set; }
    public int Minute { get; set; }
    public List<DayOfWeek> RepeatDays { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public DateTime? LastTriggered { get; set; }
}
