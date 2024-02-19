using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardClick_Editor_AddToDeck : MonoBehaviour, IPointerDownHandler
{

    private DeckManager deckManager;

    void Start()
    {
        deckManager = GameObject.Find("DeckManager").GetComponent<DeckManager>();
    }
    
    void Update()
    {
        
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        deckManager.addCardToDeck(transform.GetComponent<CardDisplay>().card.CardID);
    }
}
