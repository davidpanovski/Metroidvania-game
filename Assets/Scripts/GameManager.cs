using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager Instance { get; private set; }
    public string transitionedFromScene;

    public Vector2 respawn_point;//za obichen platforming
    public Vector2 benchRespawnPoint;//za respawn point
    [SerializeField] Bench bench;
    public GameObject shade;
    [SerializeField] private FadeUI pauseMenu;
    [SerializeField] private float fadeTIme;
    public bool GameIsPaused;


    private void Awake()
    {
        SaveData.Instance.Initialize();
        if(Instance!=null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        if (PlayerController.Instance != null)
        {
            if (PlayerController.Instance.halfmana)
            {
                SaveData.Instance.LoadShadeData();
                if (SaveData.Instance.sceneWithShade==SceneManager.GetActiveScene().name || SaveData.Instance.sceneWithShade=="")
                {
                    Instantiate(shade, SaveData.Instance.shadePosition, SaveData.Instance.shadeRotation);
                }
            }
        }
        
        SaveScene();
        DontDestroyOnLoad(gameObject);
        bench = FindAnyObjectByType<Bench>();
    }
    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame && !GameIsPaused)
        {
            pauseMenu.FadeUIIn(fadeTIme);
            Time.timeScale = 0;
            GameIsPaused = true;
        }
    }

    public void UnpauseGame()
    {
        
        Time.timeScale = 1;
        GameIsPaused = false;
        pauseMenu.FadeUIOut(fadeTIme);


    }
    public void RespawnPlayerBench()
    {
        
        SaveData.Instance.LoadBench();
        if (SaveData.Instance.benchSceneName != null) { //loadni scenata kaj shto se naogja posledniot bench
            SceneManager.LoadScene(SaveData.Instance.benchSceneName);
        
        }

        if (SaveData.Instance.benchPosition != null) { //respawn point na posledniot bench
            benchRespawnPoint = SaveData.Instance.benchPosition;

        }
        else
        {
            benchRespawnPoint = respawn_point;
        }
        
        PlayerController.Instance.transform.position = benchRespawnPoint;
        
        StartCoroutine(UIManager.Instance.DeactivateDeathScreen());
        PlayerController.Instance.Respawn();
    }
    public void SaveScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SaveData.Instance.sceneNames.Add(currentSceneName);
    }
}
