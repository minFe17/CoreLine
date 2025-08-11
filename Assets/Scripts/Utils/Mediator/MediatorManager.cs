using System;
using System.Collections.Generic;

namespace Utils
{
    public class MediatorManager
    {
        // 싱글턴
        Dictionary<Type, IMediator> _mediators = new Dictionary<Type, IMediator>();

        Mediator<TEnum> GetOrCreateMediator<TEnum>() where TEnum : Enum
        {
            Type key = typeof(TEnum);

            IMediator mediator;
            if (_mediators.TryGetValue(key, out mediator))
                return mediator as Mediator<TEnum>;

            Mediator<TEnum> newMediator = new Mediator<TEnum>();
            _mediators[key] = newMediator;
            return newMediator;
        }

        public void Register<TEnum>(TEnum eventKey, IMediatorEvent listener) where TEnum : Enum
        {
            Mediator<TEnum> mediator = GetOrCreateMediator<TEnum>();
            mediator.Register(eventKey, listener);
        }

        public void Unregister<TEnum>(TEnum eventKey, IMediatorEvent listener) where TEnum : Enum
        {
            Mediator<TEnum> mediator = GetOrCreateMediator<TEnum>();
            mediator.Unregister(eventKey, listener);
        }

        public void Notify<TEnum>(TEnum eventKey, object data = null) where TEnum : Enum
        {
            Mediator<TEnum> mediator = GetOrCreateMediator<TEnum>();
            mediator.Notify(eventKey, data);
        }

        public void ClearAll()
        {
            foreach (IMediator mediator in _mediators.Values)
                mediator.Clear();

            _mediators.Clear();
        }
    }
}