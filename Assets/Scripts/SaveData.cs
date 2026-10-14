using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;


[System.Serializable]
public struct SaveData //ne treba da e pristuna vo scenata za da bide povikana koga e struct
{
    //sekoj pat koga igrachot kje vleze vo scena kje se zachuvuva
    public static SaveData Instance;

    //koristeme hashset za da nemame duplikati/sekogash kje ni se zachuvuvaat razlicni sceni
    public HashSet<string> sceneNames;

    //bench
    public string benchSceneName;
    public Vector2 benchPosition;

    //player
    public int playerHealth;
    public float playerMana;
    public bool playerHalfMana;
    public Vector2 playerPosition;
    public string lastScene;

    //shade
    public Vector2 shadePosition;
    public string sceneWithShade;
    public Quaternion shadeRotation;


    public void Initialize()
    {
        if (!File.Exists(Application.persistentDataPath + "/save.bench.data"))
        {
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.bench.data"));
        }
        if (!File.Exists(Application.persistentDataPath + "/save.player.data"))
        {
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.player.data"));
        }
        if (!File.Exists(Application.persistentDataPath + "/save.shade.data"))
        {
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.shade.data"));
        }
        if (sceneNames == null) {  
            sceneNames = new HashSet<string>(); 
        }
    }

    public void SaveBench()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.bench.data")))
        {
            writer.Write(benchSceneName);
            writer.Write(benchPosition.x);
            writer.Write(benchPosition.y);
        }
    }

    public void LoadBench()
    {
        string path = Application.persistentDataPath + "/save.bench.data";

        if (File.Exists(path))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                benchSceneName = reader.ReadString();
                benchPosition.x = reader.ReadSingle();
                benchPosition.y = reader.ReadSingle();
                
            }
        }
    }

    public void SavePlayerData()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.player.data")))
        {
            playerHealth = PlayerController.Instance.Health;
            writer.Write(playerHealth);
            playerMana = PlayerController.Instance.Mana;
            writer.Write(playerMana);
            playerHalfMana = PlayerController.Instance.halfmana;
            writer.Write(playerHalfMana);

            playerPosition = PlayerController.Instance.transform.position;
            writer.Write(playerPosition.x);
            writer.Write(playerPosition.y);

            lastScene = SceneManager.GetActiveScene().name;
            writer.Write(lastScene);
        }
    }
    public void LoadPlayerData()
    {
        if (File.Exists(Application.persistentDataPath + "/save.player.data"))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(Application.persistentDataPath + "/save.player.data")))
            {
                playerHealth = reader.ReadInt32();
                playerMana = reader.ReadSingle();
                playerHalfMana = reader.ReadBoolean();
                playerPosition.x = reader.ReadSingle();
                playerPosition.y = reader.ReadSingle();
                lastScene = reader.ReadString();

                SceneManager.LoadScene(lastScene);
                PlayerController.Instance.transform.position = playerPosition;
                PlayerController.Instance.halfmana = playerHalfMana;
                PlayerController.Instance.Health = playerHealth;
                PlayerController.Instance.Mana = playerMana;
            }
        }
        else
        {
            Debug.Log("File doesnt exist");
            
            PlayerController.Instance.halfmana = false;
            PlayerController.Instance.Health = PlayerController.Instance.maxHealth;
            PlayerController.Instance.Mana = 0.5f;
        }
    }

    public void SaveShadeData()
    {
        using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.persistentDataPath + "/save.shade.data")))
        {

            sceneWithShade = SceneManager.GetActiveScene().name;
            writer.Write(sceneWithShade);
            
            shadePosition = Shade.Instance.transform.position;
            writer.Write(shadePosition.x);
            writer.Write(shadePosition.y);

            shadeRotation = Shade.Instance.transform.rotation;
            writer.Write(shadeRotation.x);
            writer.Write(shadeRotation.y);
            writer.Write(shadeRotation.z);
            writer.Write(shadeRotation.w);

            
        }

    }
    public void LoadShadeData() {
        string path = Application.persistentDataPath + "/save.shade.data";

        if (File.Exists(path))
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
            {
                sceneWithShade = reader.ReadString();
                shadePosition.x = reader.ReadSingle();
                shadePosition.y = reader.ReadSingle();
                float rotationX = reader.ReadSingle();
                float rotationY = reader.ReadSingle();
                float rotationZ = reader.ReadSingle();
                float rotationW = reader.ReadSingle();
                shadeRotation = new Quaternion(rotationX, rotationY, rotationZ, rotationW);

            }
        }
        else
        {
            Debug.Log("file doesnt exist");
        }

    }
}
