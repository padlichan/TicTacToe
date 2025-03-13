using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject crossYouTextGameObject;
    [SerializeField] private GameObject circleYouTextGameObject;

    private void Awake()
    {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);
    }
    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnNewTurn += GameManager_OnNewTurn;

    }

    private void GameManager_OnNewTurn(object sender, System.EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void GameManager_OnGameStarted(object sender, System.EventArgs e)
    {
        crossYouTextGameObject.SetActive(GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Crosses);
        circleYouTextGameObject.SetActive(GameManager.Instance.GetLocalPlayerType() == GameManager.PlayerType.Circles);
        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayerType() == GameManager.PlayerType.Crosses)
        {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        }
        else
        {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(true);
        }
        //crossArrowGameObject.SetActive(GameManager.Instance.GetCurrentPlayerType() == GameManager.PlayerType.Crosses);
        //circleArrowGameObject.SetActive(GameManager.Instance.GetCurrentPlayerType() == GameManager.PlayerType.Circles);
    }
}
