using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public Vector3 mousePosition;
    public void OnBeginDrag(PointerEventData eventData)
    {
        mousePosition = new Vector3(transform.position.x-eventData.position.x, transform.position.y-eventData.position .y, transform.position.z);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        transform.position += mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
        transform.position += mousePosition;
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}
