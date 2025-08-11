namespace Utils
{
    public interface IState
    {
        void Enter();
        void Loop();
        void Exit();
    }
}