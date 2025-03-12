using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    private const float GRID_SIZE = 3.1f;

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += GameManager_OnClickedOnGridPosition;
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
