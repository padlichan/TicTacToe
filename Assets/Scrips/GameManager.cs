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

    public event EventHandler OnGameStarted;
    public event EventHandler OnNewTurn;
    public enum PlayerType
    {
        None,
        Circles,
        Crosses
    }

    private PlayerType localPlayerType;
    private NetworkVariable<PlayerType> currentPlayerType = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogError("Multiple instances of GameManager in scene");
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
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType localPlayerType)
    {
        if (localPlayerType != currentPlayerType.Value) return;
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs { x = x, y = y, playerType = localPlayerType });
        switch (localPlayerType)
        {
            case PlayerType.Crosses:
            currentPlayerType.Value = PlayerType.Circles;
            break;
            case PlayerType.Circles:
            currentPlayerType.Value = PlayerType.Crosses;
            break;
        }
    }

    public PlayerType GetLocalPlayerType() => localPlayerType;
    public PlayerType GetCurrentPlayerType() => currentPlayerType.Value;
}
