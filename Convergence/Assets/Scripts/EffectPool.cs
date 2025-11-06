using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;

public class EffectPool : MonoBehaviour
{
    public static EffectPool Instance;
    public GameObject prefab;
       
    [SerializeField] int initialRiftJumpPoolSize = 6;
    [SerializeField] AudioClip teleportStartSound; //assign rift jump SFX here

    private Queue<GameObject> pool = new Queue<GameObject>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        for (int i = 0; i < initialRiftJumpPoolSize; i++)
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

        // play start sound once (if assigned)
        if (teleportStartSound != null)
            AudioSource.PlayClipAtPoint(teleportStartSound, pos, 1f);

        var ps = go.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

    // One-shot spawn for different VFX prefabs + separate SFX (no pooling)
    public void SpawnOneShot(GameObject prefabOneShot, Vector3 pos, Quaternion rot, AudioClip sfx = null, float lifeTime = 2f, float sfxVolume = 1f)
    {
        if (prefabOneShot == null)
        {
            Debug.LogWarning("EffectPool.SpawnOneShot called with null prefabOneShot");
            return;
        }

        GameObject go = Instantiate(prefabOneShot, pos, rot);

        // Start ParticleSystems cleanly
        var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);
            ps.Play(true);
        }

        // Start VisualEffect if present
        var vfxs = go.GetComponentsInChildren<VisualEffect>(true);
        foreach (var vfx in vfxs)
        {
            vfx.Stop();
            vfx.Play();
        }

        // Play sound if given (or fallback to pool pulse sound if assigned and sfx null)
        if (sfx != null)
            AudioSource.PlayClipAtPoint(sfx, pos, sfxVolume);

        Destroy(go, lifeTime);
    }

    private IEnumerator ReturnWhenFinished(GameObject go)
    {
        var systems = go.GetComponentsInChildren<ParticleSystem>(true);
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
        // Ensure no residual emission when pooled
        var particleSystems = go.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(true);
        }

        var vfxs = go.GetComponentsInChildren<VisualEffect>(true);
        foreach (var vfx in vfxs)
        {
            vfx.Stop();
            // vfx.Reinit(); // optional if you need a full reset next play
        }

        go.SetActive(false);
        go.transform.SetParent(transform);
        pool.Enqueue(go);
    }
}