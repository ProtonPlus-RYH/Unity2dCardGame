using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;
using System.Text;

public class DeckManager : MonoBehaviour
{
    public Transform deckPanel;
    public Transform libraryPanel1;
    public Transform libraryPanel2;

    public TMP_Dropdown deckSelectDropDown;
    public TMP_Dropdown weapon1SelectDropDown;
    public TMP_Dropdown weapon2SelectDropDown;
    public TMP_InputField InputTMP;

    public GameObject DeckCreateDialog;
    public GameObject LibraryCardPrefab;
    public GameObject DeckCardPrefab;

    public TextMeshProUGUI deckIllegalHint;
    public TextMeshProUGUI hintTMP;

    public CardPool library;

    public string APPpath;
    public string DeckPath;
    private int weapon1BelongID;
    private int weapon2BelongID;
    private int weapon1WeaponID;
    private int weapon2WeaponID;
    private List<string> deckNameList;
    private string editingDeckName;
    private List<int> editingDeckList;


    void Start()
    {
        string[] path = Application.dataPath.Split("/");
        StringBuilder pathSB = new StringBuilder();
        for (int i = 0; i < path.Length - 1; i++)
        {
            pathSB.Append(path[i]);
            pathSB.Append("/");
        }
        APPpath = pathSB.ToString();
        pathSB.Append("Decks");
        DeckPath = pathSB.ToString();

        library.getAllCards();
        getDeckFromFolder();

        weapon1BelongID = 1;//目前只有一种系列想到别的再搞，下同
        weapon2BelongID = 1;
        weapon1SelectDropDown.ClearOptions();
        weapon2SelectDropDown.ClearOptions();
        weapon1SelectDropDown.AddOptions(library.weaponList);
        weapon2SelectDropDown.AddOptions(library.weaponList);

        weapon1Select(0);
        weapon2Select(0);
        editingDeckName = deckNameList[0];
        if (PlayerPrefs.HasKey("editingDeck"))
        {
            if (deckNameList.Contains(PlayerPrefs.GetString("editingDeck")))
            {
                editingDeckName = PlayerPrefs.GetString("editingDeck");
            }
        }
        
        deckSelect(editingDeckName);
    }

    
    void Update()
    {
        
    }
    
    #region functions about weapon

    public void loadLibrary(Transform whichLibrary, int belongID, int weaponID)
    {
        for (int i = 0; i < library.cardPool.Count; i++)
        {
            if (library.cardPool[i].BelongID == belongID && library.cardPool[i].WeaponID == weaponID)
            {
                GameObject newCard = Instantiate(LibraryCardPrefab, whichLibrary);
                newCard.GetComponent<CardDisplay>().card = library.cardPool[i];
            }
        }
    }
    
    public void weapon1Select(int weaponID)
    {
        for(int i=0; i<libraryPanel1.childCount; i++)
        {
            Destroy(libraryPanel1.GetChild(i).gameObject);
        }
        weapon1WeaponID = weaponID;
        loadLibrary(libraryPanel1, weapon1BelongID, weapon1WeaponID);
    }

    public void weapon2Select(int weaponID)
    {
        for (int i = 0; i < libraryPanel2.childCount; i++)
        {
            Destroy(libraryPanel2.GetChild(i).gameObject);
        }
        weapon2WeaponID = weaponID;
        loadLibrary(libraryPanel2, weapon2BelongID, weapon2WeaponID);
    }

    #endregion

    #region functions about UIs

    public void OnButtonClickGotoMenu()
    {
        SceneManager.LoadScene("MenuPage");
    }
    
    public void OnButtonClickDeckDelete()
    {
        StringBuilder pathSB = new StringBuilder(DeckPath);
        pathSB.Append("/");
        pathSB.Append(editingDeckName);
        pathSB.Append(".csv");;
        if (File.Exists(pathSB.ToString()))
        {
            File.Delete(pathSB.ToString());
            pathSB.Append(".meta");
            File.Delete(pathSB.ToString());
        }
        else
        {
            //Debug.Log("未找到" + editingDeckName + ".csv");
        }
        getDeckFromFolder();
        deckSelect(deckNameList[0]);
    }
    
    public void OnButtonClickDeckCreate()
    {
        InputTMP.text = "";
        DeckCreateDialog.gameObject.SetActive(true);
    }

    public void OnButtonClickConfirmCreate()
    {
        string deckName = InputTMP.text;
        if(deckName == "")
        {
            deckName = "new deck";
        }
        DeckCreateDialog.gameObject.SetActive(false);
        createDeck(deckName, 0);
    }

    public void OnButtonClickCancelCreate()
    {
        DeckCreateDialog.gameObject.SetActive(false);
    }

    public void OnButtonClickSaveDeck()
    {
        if (WeaponTypeCheck())
        {
            List<string> savedDeck = new List<string> { "Versus Card Game Simulation Made By ProtonPlus" };
            foreach (var line in editingDeckList)
            {
                savedDeck.Add(line.ToString());
            }
            StringBuilder pathSB = new StringBuilder(DeckPath);
            pathSB.Append("/");
            pathSB.Append(editingDeckName);
            pathSB.Append(".csv"); ;
            File.WriteAllLines(pathSB.ToString(), savedDeck);
        }
        else
        {
            hint(LanguageManager.Instance.GetLocalizedString("LocalizationText_WeaponOver2"));
        }
    }

    public void DropDownDeckSelect(int deckOrderNum)
    {
        deckSelect(deckNameList[deckOrderNum]);
    }

    public void hint(string str)
    {
        hintTMP.text = str;
        hintTMP.gameObject.SetActive(true);
        Invoke(nameof(closeHint), 2.0f);
    }
    public void closeHint()
    {
        hintTMP.gameObject.SetActive(false);
    }

    #endregion

    public bool WeaponTypeCheck()
    {
        List<int> weaponIDRecord = new List<int>();
        bool result = true;
        foreach (int cardID in editingDeckList)
        {
            if (!weaponIDRecord.Contains(library.cardPool[cardID].WeaponID) && library.cardPool[cardID].WeaponID != 0)
            {
                weaponIDRecord.Add(library.cardPool[cardID].WeaponID);
            }
        }
        if (weaponIDRecord.Count > 2)
        {
            result = false;
        }
        deckIllegalHint.gameObject.SetActive(!result);
        return result;
    }

    public void addCardToDeck(int cardIDNum)
    {
        if (editingDeckList.Count < 10)
        {
            GameObject newCard = Instantiate(DeckCardPrefab, deckPanel);
            newCard.GetComponent<CardDisplay>().card = library.cardPool[cardIDNum];
            editingDeckList.Add(cardIDNum);
            WeaponTypeCheck();
        }
    }

    public void deleteCardFromDeck(int cardOrderNum)
    {
        if(cardOrderNum < editingDeckList.Count)
        {
            Destroy(deckPanel.GetChild(cardOrderNum).gameObject);
            editingDeckList.RemoveAt(cardOrderNum);
            WeaponTypeCheck();
        }
    }
    
    public void deckSelect(string deckName)
    {
        for (int i = 0; i < deckPanel.childCount; i++)
        {
            Destroy(deckPanel.GetChild(i).gameObject);
        }
        List<int> cardsInDeck = library.ReadDeck(deckName);
        for (int i = 0; i < cardsInDeck.Count; i++)
        {
            GameObject newCard = Instantiate(DeckCardPrefab, deckPanel);
            newCard.GetComponent<CardDisplay>().card = library.cardPool[cardsInDeck[i]];
        }
        PlayerPrefs.SetString("editingDeck", deckName);
        editingDeckName = deckName;
        editingDeckList = library.ReadDeck(deckName);
        deckSelectDropDown.SetValueWithoutNotify(getOrderFromDeckName(deckName));
        WeaponTypeCheck();
    }

    private int repeatDeckNameCount = 0;
    public void createDeck(string deckName, int repeatCount)
    {
        StringBuilder deckNameSB = new StringBuilder(deckName);
        if(repeatCount != 0)
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
            deckSelect(deckNameSB.ToString());
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
            }else if (fileNameDeletingReverseSlash.Length == 1)
            {
                deckNameList[i] = fileNameDeletingReverseSlash[0];
            }
        }
        deckSelectDropDown.ClearOptions();
        deckSelectDropDown.AddOptions(deckNameList);

        if (deckNameList.Count == 0)
        {
            createDeck("new deck", 0);
        }
    }

    public int getOrderFromDeckName(string deckName)
    {
        int result = -1;
        bool ifSelected = false;
        while (ifSelected == false && result < deckNameList.Count)
        {
            result++;
            if (deckNameList[result] == deckName)
            {
                ifSelected = true;
            }
        }
        if (ifSelected == false)
        {
            //Debug.Log("没找到");
            result = 0;
        }
        return result;
    }

}