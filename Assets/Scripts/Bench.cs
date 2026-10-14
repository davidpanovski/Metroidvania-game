using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Bench : MonoBehaviour
{
    public bool interacted;
    bool inRange = false;


    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.vKey.wasPressedThisFrame && inRange)
        {
            interacted = true;
            //health and mana replenishes when resting ona bench
            //mozhe da se iskomentira zs ne e najidealno implemenitrano
            PlayerController player = PlayerController.Instance;
            player.Health = player.maxHealth;
            player.Mana = 1f;

            SaveData.Instance.benchSceneName = SceneManager.GetActiveScene().name;
            SaveData.Instance.benchPosition = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
            SaveData.Instance.SaveBench();
            SaveData.Instance.SavePlayerData();

            Debug.Log("benched");
        }
    }

    void OnTriggerEnter2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player")) inRange = true;
    }

    private void OnTriggerExit2D(Collider2D _collision)
    {
        if (_collision.CompareTag("Player"))
        {
            inRange = false;
        }
    }
}