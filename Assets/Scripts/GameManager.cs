using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnFail()
    {
        SceneManager.LoadScene("DeadMenu");
    }

    public void OnWin()
    {
        SceneManager.LoadScene("WinMenu");
    }
}
