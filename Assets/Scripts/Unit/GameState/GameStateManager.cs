using System.Collections.Generic;
using UnityEngine;
using Utils;

public class GameStateManager : MonoBehaviour
{
    // ╫л╠шео
    Dictionary<EGameStateType, IState> _gameStateDict;

    IState _currentState;

    void Update()
    {
        Loop();
    }

    public void SetState()
    {
        _gameStateDict = new Dictionary<EGameStateType, IState>
        {
            { EGameStateType.SelectKingTile, new SelectKingTileState() },
            {EGameStateType.Game, new GameState() }
        };

        if(_gameStateDict[EGameStateType.Game] is GameState gameState)
            gameState.Init();

        ChangeState(EGameStateType.SelectKingTile);
    }

    public void ChangeState(EGameStateType key)
    {
        if (_currentState == _gameStateDict[key])
            return;

        if (_currentState != null)
            _currentState.Exit();
        _currentState = _gameStateDict[key];
        _currentState.Enter();
    }

    public void Loop()
    {
        if (_currentState == null)
            return;
        _currentState.Loop();
    }

    public void CreateUnit(EUnitType unitType)
    {
        if (_currentState is GameState gameState)
            gameState.CreateUnit(unitType);
    }
}