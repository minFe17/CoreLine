using System.Collections.Generic;

public class BombManager
{
    // ╫л╠шео
    List<ChefBomb> _bombList = new List<ChefBomb>();

    public void AddBomb(ChefBomb bomb)
    {
        _bombList.Add(bomb);
    }

    public void RemoveBomb()
    {
        for (int i = 0; i < _bombList.Count; i++)
            _bombList[i].Remove();
        _bombList.Clear();
    }
}