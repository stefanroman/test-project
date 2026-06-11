# test-project

A test repo for multiple tests

---

# Reminder App

A Windows desktop reminder application built with C# and WinForms (.NET 8).

## Features

- Create reminders with a message, hour, minute, and repeat days of the week
- Plays the default Windows notification sound at the scheduled time
- Shows a Windows notification (balloon tip) and a popup dialog
- Runs in the system tray — minimizing or closing hides the window to the tray
- Option to start automatically with Windows (via the Windows registry)
- Reminders saved locally to `%AppData%\ReminderApp\reminders.json`
- Simple UI to add, enable/disable, and delete reminders

## Requirements

- Windows 10 or later
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (to build)  
  **or** .NET 8 Desktop Runtime (to run a published executable)

## Build and Run

### Option 1 — Run directly with the .NET SDK

```powershell
cd ReminderApp
dotnet run
```

### Option 2 — Build and launch the executable

```powershell
cd ReminderApp
dotnet build -c Release
.\bin\Release\net8.0-windows\ReminderApp.exe
```

### Option 3 — Publish a self-contained .exe (no .NET install needed on the target machine)

```powershell
cd ReminderApp
dotnet publish -c Release -r win-x64 --self-contained true -o publish
.\publish\ReminderApp.exe
```

## Usage

1. **Add a reminder** — fill in a message, pick a time (hour/minute), tick the days you want it to repeat, then click **Add Reminder**.
2. **Enable / Disable** — select a reminder from the list and click **Enable / Disable** to toggle it on or off.
3. **Delete** — select a reminder and click **Delete** to remove it.
4. **Tray** — closing or minimizing the window sends the app to the system tray. Double-click the tray icon or right-click → **Open** to bring it back.
5. **Start with Windows** — tick the **Start with Windows** checkbox to have the app launch automatically at login (writes a key to `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`).
