using System;
using System.Collections.Generic;

/// <summary>
/// Mediator 패턴을 기반으로 이벤트를 중개하는 클래스.
/// 다양한 객체 간의 느슨한 결합을 통해 이벤트 전달을 처리.
/// </summary>
namespace Utils
{
    public class Mediator<TEnum> : IMediator where TEnum : Enum
    {
        Dictionary<TEnum, List<IMediatorEvent>> _eventDict = new Dictionary<TEnum, List<IMediatorEvent>>();

        /// <summary>
        /// 특정 이벤트 타입에 이벤트 리스너 등록
        /// </summary>
        /// <param name="key">이벤트 타입</param>
        /// <param name="value">이벤트 대상 객체</param>
        public void Register(TEnum key, IMediatorEvent value)
        {
            List<IMediatorEvent> list;

            if (!_eventDict.TryGetValue(key, out list))
            {
                list = new List<IMediatorEvent> { value };
                _eventDict[key] = list;
            }
            else if (!list.Contains(value))
                list.Add(value);
        }

        /// <summary>
        /// 특정 이벤트 타입에서 이벤트 리스너를 제거.
        /// 리스너가 모두 제거되면 해당 키도 Dictionary에서 제거됨.
        /// </summary>
        /// <param name="key">이벤트 타입</param>
        /// <param name="value">제거할 이벤트 대상 객체</param>
        public void Unregister(TEnum key, IMediatorEvent value)
        {
            if (_eventDict.TryGetValue(key, out List<IMediatorEvent> list))
            {
                list.Remove(value);
                if (list.Count == 0)
                    _eventDict.Remove(key);
            }
        }

        /// <summary>
        /// 특정 이벤트 타입의 모든 리스너에 이벤트 발생 알림
        /// </summary>
        /// <param name="key">이벤트 타입</param>
        /// <param name="data">이벤트와 함께 전달할 데이터</param>
        public void Notify(TEnum key, object data = null)
        {
            if (_eventDict.TryGetValue(key, out List<IMediatorEvent> list))
            {
                for (int i = 0; i < list.Count; i++)
                    list[i].HandleEvent(data);
            }
        }

         /// <summary>
        /// 등록된 모든 이벤트 리스너 제거
        /// 씬 이동 시 사용
        /// </summary>
        public void Clear()
        {
            _eventDict.Clear();
        }
    }
}