using System;

namespace Utils
{
    public interface IStateMachine<TEnum> where TEnum : Enum
    {
        void ChangeState(TEnum key);
        void Loop();
    }
}