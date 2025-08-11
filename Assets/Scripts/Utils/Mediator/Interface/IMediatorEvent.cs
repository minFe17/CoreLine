namespace Utils
{
    public interface IMediatorEvent
    {
        void HandleEvent(object data = null);
    }
}