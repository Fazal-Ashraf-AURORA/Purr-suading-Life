using System.Collections;
using UnityEngine;

public class ArrowTrap : MonoBehaviour {
    public Transform player;
    public Transform arrowSpawnPoint;
    public GameObject arrowPrefab;
    public float detectionRange = 5f;
    public float shootInterval = 2f;

    private bool playerInRange = false;

    void Start() {
        StartCoroutine(ShootArrows());
    }

    void Update() {
        CheckPlayerInRange();
    }

    void CheckPlayerInRange() {
        if (Vector2.Distance(transform.position, player.position) < detectionRange) {
            playerInRange = true;
        } else {
            playerInRange = false;
        }
    }

    IEnumerator ShootArrows() {
        while (true) {
            yield return new WaitForSeconds(shootInterval);

            if (playerInRange) {
                ShootArrow();
            }
        }
    }
    void ShootArrow() {
        // Calculate the direction to the player
        Vector2 directionToPlayer = (player.position - arrowSpawnPoint.position).normalized;

        // Calculate the angle between the trap's forward direction and the direction to the player
        float angle = Vector2.Angle(transform.right, directionToPlayer);

        // Adjust the shooting angle threshold based on your requirements
        float shootingAngleThreshold = 45f;

        // Check if the player is within the shooting angle threshold
        if (angle < shootingAngleThreshold) {
            // Instantiate arrow prefab at arrowSpawnPoint position and rotation
            GameObject arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.identity);

            // Set the rotation of the arrow to match the trap's forward direction
            arrow.transform.rotation = Quaternion.LookRotation(Vector3.forward, transform.right);

            // Add force to the arrow in the direction of the trap's forward direction
            arrow.GetComponent<Rigidbody2D>().AddForce(transform.right * 10f, ForceMode2D.Impulse);
        }
    }


}
