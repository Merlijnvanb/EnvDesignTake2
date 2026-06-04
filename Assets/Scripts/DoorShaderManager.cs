using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class DoorShaderManager : MonoBehaviour
{
    public Shader DoorShader;
    
    public Color Tint = Color.white;
    public float Intensity = 2.5f;

    public bool StartActive = false;

    [HideInInspector]public MeshRenderer MeshRenderer;
    
    void Start()
    {
        MeshRenderer = GetComponent<MeshRenderer>();

        var mat = new Material(DoorShader);
        
        mat.SetColor("_Tint", Tint);
        mat.SetFloat("_Intensity", Intensity);
        
        MeshRenderer.sharedMaterial = mat;

        MeshRenderer.enabled = StartActive;
    }

    public void SetActive(bool active)
    {
        MeshRenderer.enabled = active;
    }
}
