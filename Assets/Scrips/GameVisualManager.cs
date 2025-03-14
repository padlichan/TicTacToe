using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform winLinePrefab;

    private const float GRID_SIZE = 3.1f;

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += GameManager_OnGameWin;
    }

    private void GameManager_OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        //Spawn win line
        Transform winLineTransform;
        float eulerZ = 0f;
        switch (e.line.orientation)
        {
            default:
            case GameManager.Orientation.Horizontal:
            eulerZ = 0f;
            break;
            case GameManager.Orientation.Vertical:
            eulerZ = 90f;
            break;
            case GameManager.Orientation.DiagonalA:
            eulerZ = 45f;
            break;
            case GameManager.Orientation.DiagonalB:
            eulerZ = -45f;
            break;
        }
        winLineTransform = Instantiate(winLinePrefab, GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y), Quaternion.Euler(0, 0, eulerZ));
        winLineTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private void GameManager_OnClickedOnGridPosition(object sender, GameManager.OnClickedOnGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Transform spawnedTransform;
        switch (playerType)
        {
            default:
            case GameManager.PlayerType.Crosses:
            spawnedTransform = Instantiate(crossPrefab, GetGridWorldPosition(x, y), Quaternion.identity);
            break;
            case GameManager.PlayerType.Circles:
            spawnedTransform = Instantiate(circlePrefab, GetGridWorldPosition(x, y), Quaternion.identity);
            break;
        }
        spawnedTransform.GetComponent<NetworkObject>().Spawn(true);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}
