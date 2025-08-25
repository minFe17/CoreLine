using UnityEngine;
using Utils;
using System.Collections.Generic;
using System;

/*
 !�Ű����� �ִ°Ŷ� ���°Ŷ�, Ȥ�� 1���ΰŶ� 2���ΰŶ� �����ø� �ȵ˴ϴ�.
=================================================================================
��� �޴���
1. �̺�Ʈ ���
ex) Subscribe("Key", Func); => �Ű����� 1�� Ű��, 2�� �Լ����� �˴ϴ�.
*�Ű����� �ִ� �̺�Ʈ
ex) Subscribe<Type>("Key",Func); => Type�� �Ű����� Ÿ�� �����ֽð� �Ȱ��� ������ֽø� �˴ϴ�.
<Type1, Type2> �� ����

2. �̺�Ʈ ȣ��
ex) Invoke("Key") => Ű�����θ� ȣ�� ���� (�Ű�����x)
*�Ű����� �ִ� �̺�Ʈ
ex) Invoke<Type>("Key", �Ű�������) => Type�� Ÿ�� ����, �� �ѱ�� �˴ϴ�. 
ex) Invoke<Type1, Type2>("Key", �Ű�����1, �Ű�����2) => �ΰ��ϰ�� �̷��� �Ѱ��ֽø� �˴ϴ�.

3. �̺�Ʈ ��� ����
ex) UnSubscribe("Key", Func) => �Ű����� ���� �Լ��� �׳� �̷��� ���� �����մϴ�.
ex) UnSubscribe("Key", (Action<Type>)Func) => �Ű����� �ִ� �Լ��� �̷������� ����ȯ�ؼ� �Ѱ���ߵ˴ϴ�.

=================================================================================
 */
public class EventManager : SimpleSingleton<EventManager>
{
    private Dictionary<string, Delegate> _eventBus = new Dictionary<string, Delegate>();

    public void Subscribe(string key, Action callBack)
    {
        AddDelegate(key, callBack);
    }
    public void Subscribe<T>(string key, Action<T> callBack)
    {
        AddDelegate(key, callBack);
    }
    public void Subscribe<T1, T2>(string key, Action<T1, T2> callBack)
    {
        AddDelegate(key, callBack);
    }

    public void UnSubscribe(string key, Delegate callBack)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            Delegate tempDelegate = Delegate.Remove(existing, callBack);
            if (tempDelegate == null)
            {
                _eventBus.Remove(key);
            }
            else
            {
                _eventBus[key] = tempDelegate;
            }
        }
    }

    public void Invoke(string key)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if(existing is Action callBack)
            {
                callBack.Invoke();
            }
        }
        else
        {
            Debug.Log(key + "�����߻�");
        }
    }
    public void Invoke<T>(string key, T param)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if (existing is Action<T> callBack)
            {
                callBack.Invoke(param);
            }
        }
        else
        {
            Debug.Log(key + "�����߻�");
        }
    }
    public void Invoke<T1,T2>(string key, T1 param1, T2 param2)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if (existing is Action<T1,T2> callBack)
            {
                callBack.Invoke(param1, param2);
            }
        }
        else
        {
            Debug.Log(key + "�����߻�");
        }
    }

    private void AddDelegate(string key, Delegate callBack)
    {
        if (_eventBus.TryGetValue(key, out Delegate existing))
        {
            if (existing.GetType() == callBack.GetType())
            {
                _eventBus[key] = Delegate.Combine(existing, callBack);
                return;
            }
        }
        else
        {
            _eventBus[key] = callBack;
        }
    }
}
