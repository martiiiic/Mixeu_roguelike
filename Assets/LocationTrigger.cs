using UnityEngine;

public class LocationTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("The location state to set when player enters this trigger")]
    [SerializeField] private CurrentPlayerLocation.PlayerLocationState targetState;

    [Tooltip("Optional tag to filter what can trigger this location change (leave empty for any)")]
    [SerializeField] private string triggerTag = "Player";

    [Header("Debug")]
    [Tooltip("Enable to show debug messages in the console")]
    [SerializeField] private bool showDebug = false;

    [Header("Exit Behavior")]
    [Tooltip("Should this trigger reset to a different state when exiting?")]
    [SerializeField] private bool resetOnExit = false;

    [Tooltip("Which state to transition to when exiting (if resetOnExit is true)")]
    [SerializeField] private CurrentPlayerLocation.PlayerLocationState exitState = CurrentPlayerLocation.PlayerLocationState.Exploring;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (string.IsNullOrEmpty(triggerTag) || collision.CompareTag(triggerTag))
        {
            if (CurrentPlayerLocation.Instance != null)
            {
                // Apply the location state change
                CurrentPlayerLocation.Instance.SetState(targetState);

                if (showDebug)
                {
                    Debug.Log($"LocationTrigger: Changed player state to {targetState} on {gameObject.name}");
                }
            }
            else if (showDebug)
            {
                Debug.LogWarning("LocationTrigger: CurrentPlayerLocation instance not found!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (resetOnExit && (string.IsNullOrEmpty(triggerTag) || collision.CompareTag(triggerTag)))
        {
            if (CurrentPlayerLocation.Instance != null)
            {
                // Reset to exit state
                CurrentPlayerLocation.Instance.SetState(exitState);

                if (showDebug)
                {
                    Debug.Log($"LocationTrigger: Reset player state to {exitState} on exit from {gameObject.name}");
                }
            }
        }
    }
}