using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public class CollisionSceneLoader : MonoBehaviour
{
    public string Name = "DeadMenu";

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        SceneManager.LoadScene(Name);
    }
}
