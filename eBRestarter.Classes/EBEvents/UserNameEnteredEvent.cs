namespace eBRestarter.Classes.EBEvents
{
    public class UserNameEnteredEvent
    {
        public event EventHandler<NameEnteredEventArgs>? UserNameEntered;

        protected virtual void OnNameEntered(NameEnteredEventArgs e)
        {
            UserNameEntered?.Invoke(this, e);
        }

        public void RaiseNameEnteredEvent(string username)
        {
            OnNameEntered(new NameEnteredEventArgs(username));
        }
    }

    public sealed class NameEnteredEventArgs(string username) : EventArgs
    {
        public string UserName { get; } = username;
    }
}
