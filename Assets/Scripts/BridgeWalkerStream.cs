using System.Collections.Generic;
using UnityEngine;

public class BridgeWalkerStream : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public GameObject walkerPrefab;

    [Header("Stream")]
    public float walkSpeed = 1.5f;
    public float spawnIntervalMin = 1.5f;
    public float spawnIntervalMax = 4f;

    [Header("Fade")]
    public float fadeInDistance = 3f;
    public float fadeOutDistance = 3f;

    private struct Walker
    {
        public GameObject obj;
        public Renderer rend;
        public MaterialPropertyBlock mpb;
        public float t;
    }

    private List<Walker> _walkers = new();
    private float _spawnTimer;
    private float _pathLength;
    private Quaternion _walkRotation;

    // URP uses _BaseColor; set walker material to Alpha/Transparent surface type
    private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");

    void Start()
    {
        _pathLength = Vector3.Distance(startPoint.position, endPoint.position);
        _walkRotation = startPoint.rotation;

        _spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);

        // Pre-populate walkers spread along the full path so the stream looks full from the start
        float avgInterval = (spawnIntervalMin + spawnIntervalMax) * 0.5f;
        float tSpacing = walkSpeed * avgInterval / _pathLength;
        for (float t = 0f; t < 1f; t += tSpacing)
            SpawnWalker(t);
    }

    void Update()
    {
        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            _spawnTimer = Random.Range(spawnIntervalMin, spawnIntervalMax);
            SpawnWalker(0f);
        }

        float tStep = walkSpeed * Time.deltaTime / _pathLength;

        for (int i = _walkers.Count - 1; i >= 0; i--)
        {
            var w = _walkers[i];
            w.t += tStep;

            if (w.t >= 1f)
            {
                Destroy(w.obj);
                _walkers.RemoveAt(i);
                continue;
            }

            w.obj.transform.position = Vector3.Lerp(startPoint.position, endPoint.position, w.t);

            float distFromStart = w.t * _pathLength;
            float distFromEnd   = (1f - w.t) * _pathLength;
            float alpha = Mathf.Min(
                Mathf.InverseLerp(0f, fadeInDistance, distFromStart),
                Mathf.InverseLerp(0f, fadeOutDistance, distFromEnd)
            );

            w.mpb.SetColor(BaseColorProp, new Color(1f, 1f, 1f, alpha));
            w.rend.SetPropertyBlock(w.mpb);

            _walkers[i] = w;
        }
    }

    void SpawnWalker(float startT)
    {
        Vector3 pos = Vector3.Lerp(startPoint.position, endPoint.position, startT);
        GameObject obj = Instantiate(walkerPrefab, pos, _walkRotation);

        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Destroy(obj);
            return;
        }

        _walkers.Add(new Walker
        {
            obj  = obj,
            rend = rend,
            mpb  = new MaterialPropertyBlock(),
            t    = startT,
        });
    }
}
