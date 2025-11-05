using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance;
    public GameObject prefab;
    public int initialSize = 6;

    [SerializeField] AudioClip teleportStartSound;
    [SerializeField] AudioClip teleportEndSound;


    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        for (int i = 0; i < initialSize; i++)
        {
            var go = Instantiate(prefab, transform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    public GameObject Spawn(Vector3 pos, Quaternion rot, Transform parent = null)
    {
        GameObject go = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
        go.transform.SetParent(parent != null ? parent : null);
        go.transform.SetPositionAndRotation(pos, rot);
        go.SetActive(true);

        var ps = go.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Clear(true);
            ps.Play(true);
            StartCoroutine(ReturnWhenFinished(go));
            return go;
        }

        var vfx = go.GetComponentInChildren<VisualEffect>();
        if (vfx != null)
        {
            vfx.Stop();
            vfx.Play();
            StartCoroutine(ReturnAfterDefault(go, 3f));
            return go;
        }

        StartCoroutine(ReturnAfterDefault(go, 2f));
        return go;
    }

    private IEnumerator ReturnWhenFinished(GameObject go)
    {
        var systems = go.GetComponentsInChildren<ParticleSystem>();
        while (true)
        {
            bool anyAlive = false;
            foreach (var s in systems) if (s.IsAlive(true)) { anyAlive = true; break; }
            if (!anyAlive) break;
            yield return null;
        }
        Return(go);
    }

    private IEnumerator ReturnAfterDefault(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        Return(go);
    }

    private void Return(GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
        pool.Enqueue(go);
    }
}