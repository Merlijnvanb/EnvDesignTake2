using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public bool isCorrect;
    public Transform destination;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DoorTrigger] Trigger entered by: {other.name} (tag: {other.tag})");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[DoorTrigger] Ignored — not tagged Player");
            return;
        }

        Debug.Log($"[DoorTrigger] Player entered. isCorrect={isCorrect}, destination={(destination != null ? destination.name : "NULL")}");

        if (isCorrect)
        {
            if (destination == null)
            {
                Debug.LogWarning("[DoorTrigger] Correct door has no destination assigned!");
                return;
            }
            Debug.Log($"[DoorTrigger] Teleporting player to {destination.position}");
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.Teleport(destination.position, destination.rotation);
            else
                Debug.LogWarning("[DoorTrigger] PlayerController not found on player!");
        }
        else
        {
            Debug.Log("[DoorTrigger] Wrong door — restarting");
            GameManager.Instance.RestartGame();
        }
    }
}
