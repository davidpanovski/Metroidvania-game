using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private string transitionTo;
    [SerializeField] private float exitTime;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Vector2 exitDirection;

    private void Start()
    {
        if (transitionTo==GameManager.Instance.transitionedFromScene)
        {
            PlayerController.Instance.transform.position = startPoint.position;

            StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDirection, exitTime));
        }
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerController player = PlayerController.Instance;

        if (player == null || player.pState == null) return;

        // Ignore doorways while walking into the new scene.
        if (player.pState.cutscene) return;
        CheckShadeData();
        GameManager.Instance.transitionedFromScene =
            SceneManager.GetActiveScene().name;

        player.pState.cutscene = true;
        StartCoroutine(UIManager.Instance.sceneFader.SadeAndLoadScene(SceneFader.FadeDirection.In,transitionTo));
    }
    void CheckShadeData()
    {
        GameObject[] enemyObjects = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemyObjects.Length; i++)
        {
            if (enemyObjects[i].GetComponent<Shade>()!=null)
            {
                SaveData.Instance.SaveShadeData();
            }
        }
    }
}
