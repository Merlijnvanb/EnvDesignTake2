using UnityEngine;

public class EndLightsTrigger : MonoBehaviour
{
    public GameObject Text;
    public Light[] SpotLights;
    public DoorShaderManager[] DoorShaders;
    public Light[] DoorLights;
    public Color NewTint;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        Text.SetActive(true);

        foreach (var light in SpotLights)
        {
            light.enabled = true;
        }

        foreach (var shader in DoorShaders)
        {
            var mat = shader.MeshRenderer.sharedMaterial;
            mat.SetColor("_Tint", NewTint);
        }

        foreach (var light in DoorLights)
        {
            light.color = NewTint;
        }
    }
}
