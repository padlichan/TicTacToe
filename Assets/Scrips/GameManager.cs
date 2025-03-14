using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    //Events
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
    }

    public event EventHandler OnGameStarted;
    public event EventHandler OnNewTurn;

    //Player types
    public enum PlayerType
    {
        None,
        Circles,
        Crosses
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayerType = new();
    private PlayerType[,] playedPositionsArray;

    //Lines
    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }
    public struct Line
    {
        public List<Vector2Int> gridPositions;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }
    private List<Line> lineList;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogError("Multiple instances of GameManager in scene");
        playedPositionsArray = new PlayerType[3, 3];
        lineList = new List<Line>
        {
            //Horizontal lines
            new Line {gridPositions = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) },
                centerGridPosition =  new Vector2Int(1,0),
                orientation = Orientation.Horizontal},
            new Line {gridPositions = new List<Vector2Int>{new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) },
                centerGridPosition =  new Vector2Int(1,1),
                orientation = Orientation.Horizontal},
            new Line {gridPositions = new List <Vector2Int> { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2) },
                centerGridPosition =  new Vector2Int(1,2),
                orientation = Orientation.Horizontal},
            //Vertical lines
            new Line {gridPositions = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(0,2) },
                centerGridPosition =  new Vector2Int(0,1),
                orientation = Orientation.Vertical},
            new Line {gridPositions = new List<Vector2Int>{new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(1,2) },
                centerGridPosition =  new Vector2Int(1,1),
                orientation = Orientation.Vertical},
            new Line {gridPositions = new List<Vector2Int>{new Vector2Int(2,0), new Vector2Int(2, 1), new Vector2Int(2,2) },
                centerGridPosition =  new Vector2Int(2,1),
                orientation = Orientation.Vertical},
            //Diagonal lines
            new Line {gridPositions = new List<Vector2Int>{new Vector2Int(0,0), new Vector2Int(1,1), new Vector2Int(2,2) },
                centerGridPosition =  new Vector2Int(1,1),
                orientation = Orientation.DiagonalA},
            new Line {gridPositions = new List<Vector2Int>{new Vector2Int(0,2), new Vector2Int(1,1), new Vector2Int(2,0) },
                centerGridPosition =  new Vector2Int(1,1),
                orientation = Orientation.DiagonalB}
        };
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0) localPlayerType = PlayerType.Crosses;
        else localPlayerType = PlayerType.Circles;
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
        }
        currentPlayerType.OnValueChanged += (PlayerType oldPlayerType, PlayerType newPlayerType) =>
        {
            OnNewTurn?.Invoke(this, EventArgs.Empty);
        };
    }

    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayerType.Value = PlayerType.Crosses;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, new EventArgs());

    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayerType.Value) return;
        if (playedPositionsArray[x, y] != PlayerType.None) return;
        playedPositionsArray[x, y] = playerType;
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs { x = x, y = y, playerType = playerType });
        switch (playerType)
        {
            case PlayerType.Crosses:
            currentPlayerType.Value = PlayerType.Circles;
            break;
            case PlayerType.Circles:
            currentPlayerType.Value = PlayerType.Crosses;
            break;
        }
        CheckWinCondition();
    }


    private bool CheckWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None &&
               aPlayerType == bPlayerType &&
               bPlayerType == cPlayerType;
    }

    private bool CheckWinnerLine(Line line)
    {
        return CheckWinnerLine
            (
                playedPositionsArray[line.gridPositions[0].x, line.gridPositions[0].y],
                playedPositionsArray[line.gridPositions[1].x, line.gridPositions[1].y],
                playedPositionsArray[line.gridPositions[2].x, line.gridPositions[2].y]
            );
    }

    private void CheckWinCondition()
    {
        foreach (Line l in lineList)
        {

            if (CheckWinnerLine(l))
            {
                currentPlayerType.Value = PlayerType.None;
                OnGameWin?.Invoke(this, new OnGameWinEventArgs { line = l });
                break;
            }
        }
    }
    public PlayerType GetLocalPlayerType() => localPlayerType;
    public PlayerType GetCurrentPlayerType() => currentPlayerType.Value;
}
