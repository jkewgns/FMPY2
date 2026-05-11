using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    // Triggered when another object with a Collider2D touches this object
    void OnCollisionEnter2D(Collision2D coll) 
    {
        // Check if the object that hit the spikes is tagged as "Player"
        if (coll.gameObject.CompareTag("Player"))
        {
            // Get the build index of the current level
            int sceneNumber = SceneManager.GetActiveScene().buildIndex;
            
            // Reload the scene to "kill" the player and restart the challenge
            SceneManager.LoadScene(sceneNumber);
        }
    }
}
