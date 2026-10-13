using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneFader : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] public float fadeTime;

    private Image fadeOutUIimage;
    public enum FadeDirection
    {
        In,
        Out
    }

    
    void Awake()
    {
       fadeOutUIimage = GetComponent<Image>();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public IEnumerator Fade(FadeDirection _fadeDirection)
    {
        float _alpha = _fadeDirection == FadeDirection.Out ? 1 : 0;
        float _fadeEndValue = _fadeDirection == FadeDirection.Out ? 0 : 1;
        if (_fadeDirection == FadeDirection.Out)
        {
            while (_alpha >= _fadeEndValue)
            {


                SetColorImage(ref _alpha, _fadeDirection);
                yield return null;
            }
            fadeOutUIimage.enabled = false;
        }
        else
        {
            fadeOutUIimage.enabled = true;
            while (_alpha <= _fadeEndValue)
            {
                SetColorImage(ref _alpha, _fadeDirection);
                yield return null;
            }
            
        }
    }

    public IEnumerator SadeAndLoadScene(FadeDirection _fadeDirection,string levelToLoad)
    {
        fadeOutUIimage.enabled = true;

        yield return Fade(_fadeDirection);

        SceneManager.LoadScene(levelToLoad);
    }

    void SetColorImage(ref float _alpha ,  FadeDirection _fadeDirection)
    {

        fadeOutUIimage.color = new Color(fadeOutUIimage.color.r, fadeOutUIimage.color.g,fadeOutUIimage.color.b,_alpha);
        _alpha += Time.deltaTime * (1 / fadeTime) * (_fadeDirection == FadeDirection.Out ? -1 : 1);
    }
}
