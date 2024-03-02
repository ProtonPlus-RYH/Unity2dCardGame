using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public TMP_Dropdown deckSelectDropDown_self;
    public TMP_Dropdown deckSelectDropDown_opponent;

    public List<string> deckNameList;

    void Start()
    {
        getDeckFromFolder();
        int selfDeck = 0;
        int opponentDeck = 0;
        if (PlayerPrefs.HasKey("LastSelfDeck"))
        {
            selfDeck = PlayerPrefs.GetInt("LastSelfDeck");
        }
        if (PlayerPrefs.HasKey("LastOpponentDeck"))
        {
            opponentDeck = PlayerPrefs.GetInt("LastOpponentDeck");
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
        SceneManager.LoadScene("MainGame");
    }

    public void getDeckFromFolder()
    {
        string[] deckFiles = Directory.GetFiles("Assets\\ResourceFiles\\Decks", "*.csv");
        deckNameList = new List<string>(deckFiles);
        for (int i = 0; i < deckNameList.Count; i++)
        {
            string[] pathFolders = deckNameList[i].Split("\\");
            string[] fileName = pathFolders[pathFolders.Length - 1].Split(".");
            deckNameList[i] = fileName[0];
        }
        deckSelectDropDown_self.ClearOptions();
        deckSelectDropDown_self.AddOptions(deckNameList);
        deckSelectDropDown_opponent.ClearOptions();
        deckSelectDropDown_opponent.AddOptions(deckNameList);
    }

    public void SelfDeckSelect(int deckOrderNum)
    {
        PlayerPrefs.SetString("SelfDeck", deckNameList[deckOrderNum]);
        PlayerPrefs.SetInt("LastSelfDeck", deckOrderNum);
        deckSelectDropDown_self.SetValueWithoutNotify(deckOrderNum);
    }

    public void OpponentDeckSelect(int deckOrderNum)
    {
        PlayerPrefs.SetString("OpponentDeck", deckNameList[deckOrderNum]);
        PlayerPrefs.SetInt("LastOpponentDeck", deckOrderNum);
        deckSelectDropDown_opponent.SetValueWithoutNotify(deckOrderNum);
    }
}
