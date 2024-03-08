using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public TMP_Dropdown deckSelectDropDown_self;
    public TMP_Dropdown deckSelectDropDown_opponent;
    public TextMeshProUGUI deckIllegalHint_self;
    public TextMeshProUGUI deckIllegalHint_opponent;

    public List<string> deckNameList;
    public CardPool library;

    public string APPpath;
    public string DeckPath;

    void Start()
    {
        library.getAllCards();
        //文件路径初始化
        string[] path = Application.dataPath.Split("/");
        StringBuilder pathSB = new StringBuilder();
        for (int i=0; i<path.Length-1; i++)
        {
            pathSB.Append(path[i]);
            pathSB.Append("/");
        }
        APPpath = pathSB.ToString();
        pathSB.Append("Decks/");
        DeckPath = pathSB.ToString();
        if (!Directory.Exists(DeckPath))
        {
            Directory.CreateDirectory(DeckPath);
        }
        getDeckFromFolder();
        Invoke(nameof(DeckInitialize), 0.1f);
    }
    public void DeckInitialize()
    {
        int selfDeck = 0;
        int opponentDeck = 0;
        if (PlayerPrefs.HasKey("LastSelfDeck"))
        {
            if (deckNameList.Contains(PlayerPrefs.GetString("SelfDeck")))
            {
                selfDeck = PlayerPrefs.GetInt("LastSelfDeck");
            }
        }
        if (PlayerPrefs.HasKey("LastOpponentDeck"))
        {
            if (deckNameList.Contains(PlayerPrefs.GetString("OpponentDeck")))
            {
                opponentDeck = PlayerPrefs.GetInt("LastOpponentDeck");
            }
        }
        SelfDeckSelect(selfDeck);
        OpponentDeckSelect(opponentDeck);
    }

    void Update()
    {
        
    }
    
    public void OnButtonClickGoToDeckEdit()
    {
        SceneManager.LoadScene("DeckEdit");
    }

    public void OnButtonClickGoToSingleGame()
    {
        if (DeckCheck())
        {
            SceneManager.LoadScene("MainGame");
        }
    }

    public void getDeckFromFolder()
    {
        string[] deckFiles = Directory.GetFiles(DeckPath, "*.csv");
        deckNameList = new List<string>(deckFiles);
        for (int i = 0; i < deckNameList.Count; i++)
        {
            string[] pathFolders = deckNameList[i].Split("/");
            string[] fileName = pathFolders[pathFolders.Length - 1].Split(".");
            string[] fileNameDeletingReverseSlash = fileName[0].Split("\\");
            if (fileNameDeletingReverseSlash.Length == 2)
            {
                deckNameList[i] = fileNameDeletingReverseSlash[1];
            }
            else if (fileNameDeletingReverseSlash.Length == 1)
            {
                deckNameList[i] = fileNameDeletingReverseSlash[0];
            }
        }
        deckSelectDropDown_self.ClearOptions();
        deckSelectDropDown_self.AddOptions(deckNameList);
        deckSelectDropDown_opponent.ClearOptions();
        deckSelectDropDown_opponent.AddOptions(deckNameList);

        if (deckNameList.Count == 0)
        {
            createDeck("new deck", 0);
        }
    }

    private int repeatDeckNameCount = 0;
    public void createDeck(string deckName, int repeatCount)
    {
        StringBuilder deckNameSB = new StringBuilder(deckName);
        if (repeatCount != 0)
        {
            deckNameSB.Append(repeatCount.ToString());
        }
        if (deckNameList.Contains(deckNameSB.ToString()))
        {
            repeatDeckNameCount++;
            repeatCount = repeatDeckNameCount;
            createDeck(deckName, repeatCount);
        }
        else
        {
            StringBuilder pathSB = new StringBuilder(DeckPath);
            pathSB.Append("/");
            pathSB.Append(deckNameSB.ToString());
            pathSB.Append(".csv");
            using (StreamWriter streamWriter = new StreamWriter(pathSB.ToString()))
            {
                streamWriter.WriteLine("Versus Card Game Simulation Made By ProtonPlus");
            }
            repeatDeckNameCount = 0;
            getDeckFromFolder();
        }
    }

    public void SelfDeckSelect(int deckOrderNum)
    {
        PlayerPrefs.SetString("SelfDeck", deckNameList[deckOrderNum]);
        PlayerPrefs.SetInt("LastSelfDeck", deckOrderNum);
        deckSelectDropDown_self.SetValueWithoutNotify(deckOrderNum);
        DeckCheck();
    }

    public void OpponentDeckSelect(int deckOrderNum)
    {
        PlayerPrefs.SetString("OpponentDeck", deckNameList[deckOrderNum]);
        PlayerPrefs.SetInt("LastOpponentDeck", deckOrderNum);
        deckSelectDropDown_opponent.SetValueWithoutNotify(deckOrderNum);
        DeckCheck();
    }

    public bool DeckCheck()
    {
        List<int> weaponIDRecord_self = new List<int>();
        List<int> weaponIDRecord_opponent = new List<int>();
        List<int> selfDeck;
        List<int> opponentDeck;
        if (PlayerPrefs.HasKey("SelfDeck") && deckNameList.Contains(PlayerPrefs.GetString("SelfDeck")))
        {
            selfDeck = library.ReadDeck(PlayerPrefs.GetString("SelfDeck"));
        }
        else
        {
            selfDeck = library.ReadDeck(deckNameList[0]);
        }
        if (PlayerPrefs.HasKey("OpponentDeck") && deckNameList.Contains(PlayerPrefs.GetString("OpponentDeck")))
        {
            opponentDeck = library.ReadDeck(PlayerPrefs.GetString("OpponentDeck"));
        }
        else
        {
            opponentDeck = library.ReadDeck(deckNameList[0]);
        }

        bool result_self = true;
        foreach (int cardID in selfDeck)
        {
            if (!weaponIDRecord_self.Contains(library.cardPool[cardID].WeaponID) && library.cardPool[cardID].WeaponID != 0)
            {
                weaponIDRecord_self.Add(library.cardPool[cardID].WeaponID);
            }
        }
        if (weaponIDRecord_self.Count > 2 || selfDeck.Count == 0)
        {
            result_self = false;
            deckIllegalHint_self.gameObject.SetActive(true);
        }
        else
        {
            deckIllegalHint_self.gameObject.SetActive(false);
        }

        bool result_opponent = true;
        foreach (int cardID in opponentDeck)
        {
            if (!weaponIDRecord_opponent.Contains(library.cardPool[cardID].WeaponID) && library.cardPool[cardID].WeaponID != 0)
            {
                weaponIDRecord_opponent.Add(library.cardPool[cardID].WeaponID);
            }
        }
        if (weaponIDRecord_opponent.Count > 2 || opponentDeck.Count == 0)
        {
            result_opponent = false;
            deckIllegalHint_opponent.gameObject.SetActive(true);
        }
        else
        {
            deckIllegalHint_opponent.gameObject.SetActive(false);
        }

        deckIllegalHint_opponent.gameObject.SetActive(!result_opponent);
        return result_self && result_opponent;
    }

}
