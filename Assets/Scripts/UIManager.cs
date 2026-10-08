using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public static UIManager Instance;
    public void Awake()
    {
        if (Instance != null && this != Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public SceneFader sceneFader;
    private void Start()
    {
        sceneFader=GetComponentInChildren<SceneFader>();
    }

}
