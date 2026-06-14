namespace Questionable.External;

// B1: the NotificationMasterAPI NuGet (the IPC-client wrapper) is unavailable on the TC
// net9/API12 feed. Out-of-game tray/taskbar notifications are stubbed out — Enabled
// reports false (so the NotificationMaster UI controls stay disabled) and Notify is a
// no-op. The in-game chat notification path in SendNotification is unaffected.
internal sealed class NotificationMasterIpc
{
    public bool Enabled => false;

    public void Notify(string message)
    {
    }
}
