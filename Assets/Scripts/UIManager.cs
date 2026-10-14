using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public static UIManager Instance;
    [SerializeField] GameObject deathScreen;
    [SerializeField] GameObject halfMana, fullMana;

    public enum ManaState
    {
        FullMana,
        HalfMana
    }
    public ManaState manaState;
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

    public void SwitchMana(ManaState _manaState)
    {
        switch (_manaState)
        {
            case ManaState.FullMana:
                halfMana.SetActive(false); 
                fullMana.SetActive(true);
                
                
                break;
            case ManaState.HalfMana:
                fullMana.SetActive(false);
                halfMana.SetActive(true);
                break;

        }
        manaState = _manaState;
    }
    public IEnumerator ActivateDeathScreen()
    {
        yield return new WaitForSeconds(0.8f);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.In));

        yield return new WaitForSeconds(0.8f);
        deathScreen.SetActive(true);
    }

    public IEnumerator DeactivateDeathScreen()
    {
        yield return new WaitForSeconds(0.5f);
        deathScreen.SetActive(false);
        StartCoroutine(sceneFader.Fade(SceneFader.FadeDirection.Out));

        
    }

}
