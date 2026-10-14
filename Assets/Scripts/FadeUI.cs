using System.Collections;
using UnityEngine;

public class FadeUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        
    }
    
    public void FadeUIIn(float _seconds)
    {
        StartCoroutine(FadeIn(_seconds));

    }
    public void FadeUIOut(float _seconds) {
        StartCoroutine(FadeOut(_seconds));
    }
    IEnumerator FadeOut(float _seconds)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1;
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.unscaledDeltaTime / _seconds; //ne sakame da e afektirana od changes vo timescale (koga pravime pauza timescale=0)
            yield return null;
        }
        yield return null;
    }

    IEnumerator FadeIn(float _seconds)
    {
        
        canvasGroup.alpha = 0;
        while (canvasGroup.alpha <1 )
        {
            canvasGroup.alpha += Time.unscaledDeltaTime / _seconds; //ne sakame da e afektirana od changes vo timescale (koga pravime pauza timescale=0)
            yield return null;
        }
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
