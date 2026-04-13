using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D coll) 
    {
        if (coll.gameObject.CompareTag("Player"))
        {
            int sceneNumber = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneNumber);
        }
    }
}
