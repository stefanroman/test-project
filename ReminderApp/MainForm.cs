using System.Media;

namespace ReminderApp;

public class MainForm : Form
{
    private readonly List<Reminder> _reminders;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly NotifyIcon _notifyIcon;

    private ListBox _listBox = null!;
    private TextBox _messageTextBox = null!;
    private NumericUpDown _hourPicker = null!;
    private NumericUpDown _minutePicker = null!;
    private CheckBox _monday = null!;
    private CheckBox _tuesday = null!;
    private CheckBox _wednesday = null!;
    private CheckBox _thursday = null!;
    private CheckBox _friday = null!;
    private CheckBox _saturday = null!;
    private CheckBox _sunday = null!;
    private CheckBox _startupCheckBox = null!;

    public MainForm()
    {
        Text = "Reminder App";
        Width = 720;
        Height = 520;
        MinimumSize = new Size(600, 480);
        StartPosition = FormStartPosition.CenterScreen;

        _reminders = ReminderStorage.Load();

        BuildUi();

        _notifyIcon = new NotifyIcon
        {
            Text = "Reminder App",
            Icon = SystemIcons.Information,
            Visible = true,
            ContextMenuStrip = BuildTrayMenu()
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();

        _timer = new System.Windows.Forms.Timer { Interval = 1000 };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        Resize += MainForm_Resize;
        FormClosing += MainForm_FormClosing;

        RefreshReminderList();
    }

    private void BuildUi()
    {
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            Padding = new Padding(8)
        };
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
        Controls.Add(mainPanel);

        // Left panel: reminder list + action buttons
        var leftPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainPanel.Controls.Add(leftPanel, 0, 0);

        _listBox = new ListBox
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 9f)
        };
        leftPanel.Controls.Add(_listBox, 0, 0);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            Padding = new Padding(0, 4, 0, 0)
        };

        var toggleButton = new Button { Text = "Enable / Disable", AutoSize = true };
        toggleButton.Click += ToggleButton_Click;

        var deleteButton = new Button { Text = "Delete", AutoSize = true };
        deleteButton.Click += DeleteButton_Click;

        buttonPanel.Controls.Add(toggleButton);
        buttonPanel.Controls.Add(deleteButton);
        leftPanel.Controls.Add(buttonPanel, 0, 1);

        // Right panel: add reminder form
        var rightPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(8, 0, 0, 0)
        };
        mainPanel.Controls.Add(rightPanel, 1, 0);

        rightPanel.Controls.Add(MakeLabel("Message:"));
        _messageTextBox = new TextBox { Width = 240 };
        rightPanel.Controls.Add(_messageTextBox);

        rightPanel.Controls.Add(MakeLabel("Hour (0–23):"));
        _hourPicker = new NumericUpDown { Minimum = 0, Maximum = 23, Width = 70 };
        rightPanel.Controls.Add(_hourPicker);

        rightPanel.Controls.Add(MakeLabel("Minute (0–59):"));
        _minutePicker = new NumericUpDown { Minimum = 0, Maximum = 59, Width = 70 };
        rightPanel.Controls.Add(_minutePicker);

        rightPanel.Controls.Add(MakeLabel("Repeat on:"));

        _monday    = new CheckBox { Text = "Monday",    AutoSize = true };
        _tuesday   = new CheckBox { Text = "Tuesday",   AutoSize = true };
        _wednesday = new CheckBox { Text = "Wednesday", AutoSize = true };
        _thursday  = new CheckBox { Text = "Thursday",  AutoSize = true };
        _friday    = new CheckBox { Text = "Friday",    AutoSize = true };
        _saturday  = new CheckBox { Text = "Saturday",  AutoSize = true };
        _sunday    = new CheckBox { Text = "Sunday",    AutoSize = true };

        foreach (var cb in new[] { _monday, _tuesday, _wednesday, _thursday, _friday, _saturday, _sunday })
            rightPanel.Controls.Add(cb);

        rightPanel.Controls.Add(new Label { Height = 4, AutoSize = false });

        _startupCheckBox = new CheckBox
        {
            Text = "Start with Windows",
            AutoSize = true,
            Checked = StartupManager.IsEnabled()
        };
        _startupCheckBox.CheckedChanged += (_, _) => StartupManager.SetEnabled(_startupCheckBox.Checked);
        rightPanel.Controls.Add(_startupCheckBox);

        rightPanel.Controls.Add(new Label { Height = 4, AutoSize = false });

        var addButton = new Button { Text = "Add Reminder", Width = 150 };
        addButton.Click += AddButton_Click;
        rightPanel.Controls.Add(addButton);
    }

    private static Label MakeLabel(string text) =>
        new Label { Text = text, AutoSize = true, Padding = new Padding(0, 6, 0, 0) };

    private ContextMenuStrip BuildTrayMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Open", null, (_, _) => ShowMainWindow());
        menu.Items.Add("Exit",  null, (_, _) =>
        {
            _notifyIcon.Visible = false;
            Application.Exit();
        });
        return menu;
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        ShowInTaskbar = true;
        Activate();
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        var message = _messageTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(message))
        {
            MessageBox.Show("Please enter a reminder message.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var selectedDays = GetSelectedDays();
        if (selectedDays.Count == 0)
        {
            MessageBox.Show("Please select at least one day of the week.", "Validation",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var reminder = new Reminder
        {
            Message    = message,
            Hour       = (int)_hourPicker.Value,
            Minute     = (int)_minutePicker.Value,
            RepeatDays = selectedDays,
            Enabled    = true
        };

        _reminders.Add(reminder);
        ReminderStorage.Save(_reminders);
        RefreshReminderList();
        ClearInputs();
    }

    private void ToggleButton_Click(object? sender, EventArgs e)
    {
        if (_listBox.SelectedItem is not ReminderDisplayWrapper wrapper) return;

        wrapper.Reminder.Enabled = !wrapper.Reminder.Enabled;
        ReminderStorage.Save(_reminders);
        RefreshReminderList();
    }

    private void DeleteButton_Click(object? sender, EventArgs e)
    {
        if (_listBox.SelectedItem is not ReminderDisplayWrapper wrapper) return;

        var result = MessageBox.Show(
            $"Delete reminder \"{wrapper.Reminder.Message}\"?",
            "Confirm Delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _reminders.Remove(wrapper.Reminder);
            ReminderStorage.Save(_reminders);
            RefreshReminderList();
        }
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;

        foreach (var reminder in _reminders.Where(r => r.Enabled))
        {
            if (!reminder.RepeatDays.Contains(now.DayOfWeek))
                continue;

            if (reminder.Hour != now.Hour || reminder.Minute != now.Minute)
                continue;

            // Fire at most once per minute
            if (reminder.LastTriggered.HasValue
                && reminder.LastTriggered.Value.Date   == now.Date
                && reminder.LastTriggered.Value.Hour   == now.Hour
                && reminder.LastTriggered.Value.Minute == now.Minute)
            {
                continue;
            }

            reminder.LastTriggered = now;
            ReminderStorage.Save(_reminders);
            TriggerReminder(reminder);
        }
    }

    private void TriggerReminder(Reminder reminder)
    {
        SystemSounds.Asterisk.Play();

        _notifyIcon.BalloonTipTitle = "Reminder";
        _notifyIcon.BalloonTipText  = reminder.Message;
        _notifyIcon.BalloonTipIcon  = ToolTipIcon.Info;
        _notifyIcon.ShowBalloonTip(8000);

        MessageBox.Show(
            reminder.Message,
            "Reminder",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private List<DayOfWeek> GetSelectedDays()
    {
        var days = new List<DayOfWeek>();
        if (_monday.Checked)    days.Add(DayOfWeek.Monday);
        if (_tuesday.Checked)   days.Add(DayOfWeek.Tuesday);
        if (_wednesday.Checked) days.Add(DayOfWeek.Wednesday);
        if (_thursday.Checked)  days.Add(DayOfWeek.Thursday);
        if (_friday.Checked)    days.Add(DayOfWeek.Friday);
        if (_saturday.Checked)  days.Add(DayOfWeek.Saturday);
        if (_sunday.Checked)    days.Add(DayOfWeek.Sunday);
        return days;
    }

    private void ClearInputs()
    {
        _messageTextBox.Clear();
        _hourPicker.Value   = 0;
        _minutePicker.Value = 0;
        _monday.Checked    = false;
        _tuesday.Checked   = false;
        _wednesday.Checked = false;
        _thursday.Checked  = false;
        _friday.Checked    = false;
        _saturday.Checked  = false;
        _sunday.Checked    = false;
    }

    private void RefreshReminderList()
    {
        var selectedIndex = _listBox.SelectedIndex;

        _listBox.BeginUpdate();
        _listBox.DataSource    = null;
        _listBox.DisplayMember = nameof(ReminderDisplayWrapper.DisplayText);

        var wrapped = _reminders
            .Select(r => new ReminderDisplayWrapper(r))
            .ToList();

        _listBox.DataSource = wrapped;

        if (selectedIndex >= 0 && selectedIndex < _listBox.Items.Count)
            _listBox.SelectedIndex = selectedIndex;

        _listBox.EndUpdate();
    }

    private void MainForm_Resize(object? sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            Hide();
            ShowInTaskbar = false;
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
            ShowInTaskbar = false;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
        base.Dispose(disposing);
    }

    private sealed class ReminderDisplayWrapper
    {
        public Reminder Reminder { get; }

        public string DisplayText =>
            $"{(Reminder.Enabled ? "[ON] " : "[OFF]")} {Reminder.Hour:D2}:{Reminder.Minute:D2}  {Reminder.Message}" +
            $"  ({string.Join(", ", Reminder.RepeatDays.Select(d => d.ToString()[..3]))})";

        public ReminderDisplayWrapper(Reminder reminder) => Reminder = reminder;

        public override string ToString() => DisplayText;
    }
}
