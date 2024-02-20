using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZoomUI_WithInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public float zoomSize;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        transform.localScale = new Vector3(zoomSize, zoomSize, 1.0f);
        BattleManager_Single.Instance.infoDisplay(gameObject.GetComponent<CardDisplay>().card);
    }

    public void OnPointerExit(PointerEventData pointerEventData)
    {
        transform.localScale = Vector3.one;
    }
}
