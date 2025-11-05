using UnityEngine;

public class Telegraph : MonoBehaviour
{
    [SerializeField] Renderer rend; // Renderer used to display the warning
    [SerializeField] float fadeTime; // How long it takes to fade out
    [SerializeField] float startAlpha = 1f; // Starting visibility of the telegraph
    [SerializeField] float endAlpha = 0f; // Ending visibility before disappearing
    float timer; // Tracks how long the telegraph has been active

    Color startColor; // Stores the telegraph’s original color

    void Start()
    {
        if (rend == null)
            rend = GetComponent<Renderer>(); // Gets renderer if not assigned in inspector

        if (rend != null)
            startColor = rend.material.color; // Stores the initial color of the telegraph
    }

    void Update()
    {
        timer += Time.deltaTime; // Increments timer every frame

        if (rend != null)
        {
            float t = timer / fadeTime; // Calculates fade progress
            Color c = startColor;
            c.a = Mathf.Lerp(startAlpha, endAlpha, t); // Smoothly fades the alpha value
            rend.material.color = c; // Applies new color each frame
        }

        // Destroys the telegraph after it fully fades out
        if (timer >= fadeTime)
        {
            Destroy(gameObject); // Removes telegraph from the scene
        }
    }
}
