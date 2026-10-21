using System.Collections;
using UnityEngine;

public class Spike : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player")){
            StartCoroutine(RespawnPoint());
        }
    }

    IEnumerator RespawnPoint()
    {
        /*D*//*ebug.Log($"Player: {PlayerController.Instance != null}, " +
          $"pState: {PlayerController.Instance?.pState != null}, " +
          $"rb: {PlayerController.Instance?.rb != null}, " +
          $"UIManager: {UIManager.Instance != null}, " +
          $"sceneFader: {UIManager.Instance?.sceneFader != null}, " +
          $"GameManager: {GameManager.Instance != null}");*/
        PlayerController.Instance.pState.cutscene = true;
        PlayerController.Instance.pState.invincible = true;
        PlayerController.Instance.rb.linearVelocity = Vector2.zero;
        Time.timeScale = 0;
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.In));
        PlayerController.Instance.TakeDamage(1);
        yield return new WaitForSecondsRealtime(1);
        PlayerController.Instance.transform.position = GameManager.Instance.respawn_point;
        StartCoroutine(UIManager.Instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
        yield return new WaitForSecondsRealtime(UIManager.Instance.sceneFader.fadeTime);
        PlayerController.Instance.pState.cutscene = false;
        PlayerController.Instance.pState.invincible = false;
        Time.timeScale = 1;
    }
}
