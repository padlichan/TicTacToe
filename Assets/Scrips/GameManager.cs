using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{

    public static GameManager Instance { get; private set; }
    public event EventHandler<OnClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class OnClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }

    public enum PlayerType
    {
        None,
        Circles,
        Crosses
    }

    private PlayerType localPlayerType;
    private PlayerType currentPlayerType;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogError("Multiple instances of GameManager in scene");
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0) localPlayerType = PlayerType.Crosses;
        else localPlayerType = PlayerType.Circles;
        if (IsServer) currentPlayerType = PlayerType.Crosses;
        Debug.Log(localPlayerType);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType localPlayerType)
    {
        if (localPlayerType != currentPlayerType) return;
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs { x = x, y = y, playerType = localPlayerType });
        switch (localPlayerType)
        {
            case PlayerType.Crosses:
            currentPlayerType = PlayerType.Circles;
            break;
            case PlayerType.Circles:
            currentPlayerType = PlayerType.Crosses;
            break;
        }
    }

    public PlayerType GetLocalPlayerType() => localPlayerType;
}
