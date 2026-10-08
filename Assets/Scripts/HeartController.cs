using UnityEngine;
using UnityEngine.UI;
public class HeartController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    PlayerController player;

    private GameObject[] heartContainers;
    private Image[] heartFills;
    public Transform heartsParent;
    public GameObject heartsContainerPrefab;
    [SerializeField] private Image manaFill;

    void Start()
    {
        player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogError("HealthDisplay could not find the player.", this);
            return;
        }
        player.BindManaUI(manaFill);



    heartContainers = new GameObject[player.maxHealth];
        heartFills = new Image[player.maxHealth];

        player.onHealthChangedCallback += UpdateHeartsHUD;
        InstantiateHeartConatainers();
        UpdateHeartsHUD();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < PlayerController.Instance.health)
            {
                heartContainers[i].SetActive(true);
            }
            else
            {
                heartContainers[i].SetActive(true);
            }
            
        }
    }

    void SetFilledContainers()
    {
        for (int i = 0; i < heartFills.Length; i++)
        {
            if (i < PlayerController.Instance.health)
            {
                heartFills[i].fillAmount=1;
            }
            else
            {
                heartFills[i].fillAmount = 0;
            }

        }
    }

    void InstantiateHeartConatainers()
    {
        for (int i = 0; i < PlayerController.Instance.maxHealth; i++)
        {
            GameObject temp = Instantiate(heartsContainerPrefab);
            temp.transform.SetParent(heartsParent, false);
            heartContainers[i] = temp;
            heartFills[i] =temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }

    void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledContainers();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.onHealthChangedCallback -= UpdateHeartsHUD;
        }
    }

}
