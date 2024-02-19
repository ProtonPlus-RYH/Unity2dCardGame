using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    
    
    void Start()
    {
        
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
}
