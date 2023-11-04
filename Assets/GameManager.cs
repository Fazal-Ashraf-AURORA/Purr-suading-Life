using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Canvas pauseMenuCanvas;
    private bool IsPaused = false;

    private void Start() {
        IsPaused = false;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {

            if (IsPaused) {
                Debug.Log("Resume Game");
                ResumeGame();
            } else {
                Debug.Log("Pause Game");
                PauseGame();
            }


            void PauseGame() {
                IsPaused = true;
                Time.timeScale = 0; // Pause the game
                pauseMenuCanvas.enabled = true; // Show the pause menu UI
            }

            void ResumeGame() {
                IsPaused = false;
                Time.timeScale = 1; // Resume the game
                pauseMenuCanvas.enabled = false; // Hide the pause menu UI
            }
        }
    }
}
