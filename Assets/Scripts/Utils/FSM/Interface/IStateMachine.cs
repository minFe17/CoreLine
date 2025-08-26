using System;

namespace Utils
{
    public interface IStateMachine
    {
        void ChangeState<TEnum>(TEnum key);
        void Loop();
    }
}