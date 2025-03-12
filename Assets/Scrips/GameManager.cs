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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogError("Multiple instances of GameManager in scene");
    }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.Singleton.LocalClientId == 0) localPlayerType = PlayerType.Crosses;
        else localPlayerType = PlayerType.Circles;
        Debug.Log(localPlayerType);
    }
    public void ClickedOnGridPosition(int x, int y)
    {
        OnClickedOnGridPosition?.Invoke(this, new OnClickedOnGridPositionEventArgs { x = x, y = y, playerType = localPlayerType });
    }
}
