using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


public class ZoomUI_HandCard : MonoBehaviour, IPointerEnterHandler
{

    private DeckManager deckManager;
    public float zoomSize;

    void Start()
    {
        
    }
    
    void Update()
    {
        
    }
    

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        transform.localScale = new Vector3(zoomSize, zoomSize, 1.0f);

        for (int i=0; i < transform.parent.childCount; i++)
        {
            Transform searchedCard = transform.parent.GetChild(i);
            if (searchedCard.gameObject.GetComponent<Canvas>() != null)
            {
                Destroy(searchedCard.gameObject.GetComponent<Canvas>());
                searchedCard.localScale = Vector3.one;
            }
        }

        Canvas frontCanvas = transform.gameObject.AddComponent<Canvas>();
        frontCanvas.overrideSorting = true;
        frontCanvas.sortingOrder++;
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        //transform.localScale = Vector3.one;
    }

}
