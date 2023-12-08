using UnityEngine;

public class Spikes : MonoBehaviour {
    [SerializeField] private BoxCollider2D spikesCollider;

    private void Start() {
        if (spikesCollider == null) {
            spikesCollider = GetComponent<BoxCollider2D>();

            if (spikesCollider == null) {
                Debug.LogError("BoxCollider2D not found on Spikes object.");
            }
        }
    }

    public void ActivateSpikesCollider() {
        if (spikesCollider != null) {
            //Debug.Log("Collider ON");
            spikesCollider.enabled = true;
        } else {
            Debug.LogError("BoxCollider2D is not assigned.");
        }
    }

    public void DeactivateSpikesCollider() {
        if (spikesCollider != null) {
            //Debug.Log("Collider OFF");
            spikesCollider.enabled = false;
        } else {
            Debug.LogError("BoxCollider2D is not assigned.");
        }
    }
}
