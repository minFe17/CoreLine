using System.Collections.Generic;
using UnityEngine;

public enum StoreType
{
    Money, Gem, InfinityKey
}
public struct StoreData
{
    public int Key;
    public StoreType StoreType;
    public int Price;
    public int Quantity;
    public string ImageName;
}