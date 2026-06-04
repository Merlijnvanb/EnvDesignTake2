using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class CollisionSceneLoader : MonoBehaviour
{
    public bool Win = false;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Win)
            GameManager.Instance.OnWin();
        else
            GameManager.Instance.OnFail();
    }
}
