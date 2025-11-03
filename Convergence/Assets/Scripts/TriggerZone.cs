using UnityEngine;

// Trigger zone that detects player entry and optionally completes a mission objective.
// Disables itself after triggering to prevent multiple activations.
public class TriggerZone : MonoBehaviour
{
    // Whether this trigger completes an objective
    [SerializeField] private bool isObjectiveTrigger = true;

    // Identifier for debug/logging
    [SerializeField] private string triggerName = "ObjectiveTrigger";

    // Prevent repeat triggers
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return; // Prevent multiple triggers

        if (other.CompareTag("Player"))
        {
            triggered = true;

            if (isObjectiveTrigger)
            {
                Mission.instance.CompleteObjective();
            }
           
            // Optionally disable trigger after activation
            gameObject.SetActive(false);
        }
    }
}
