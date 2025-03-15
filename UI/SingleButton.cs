using UnityEngine;
using UnityEngine.EventSystems;


public class SingleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
     public bool isPress;

     public void OnPointerDown(PointerEventData eventData)
     {
          isPress = true;
     }

     public void OnPointerUp(PointerEventData eventData)
     {
          isPress = false;
     }
}
