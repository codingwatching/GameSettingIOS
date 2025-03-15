using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class PressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static bool isPress;
    public static bool isDown;

    private void OnEnable()
    {
        isPress = false;
        isDown = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isPress)
        {
            isDown = true;
        }

        isPress = true;
        StartCoroutine(ResetDown());
    }

    IEnumerator ResetDown()
    {
        yield return new WaitForEndOfFrame();
        isDown = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPress = false;
        isDown = false;
    }
}