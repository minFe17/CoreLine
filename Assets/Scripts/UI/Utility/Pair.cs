using UnityEditor;
using UnityEngine;

public class Pair<T1, T2>
{
    private T1 _first;
    private T2 _second;

    public T1 First
    {
        get { return _first; }
        set { _first = value; }
    }
    public T2 Second
    {
        get { return _second; }
        set { _second = value; }
    }
    public Pair() { }

    public Pair(T1 first, T2 second)
    {
        _first = first;
        _second = second;
    }
}