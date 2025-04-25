using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class PressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool IsPressed;
    public bool isDown;

    private void OnEnable()
    {
        IsPressed = false;
        isDown = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsPressed)
        {
            isDown = true;
        }

        IsPressed = true;
        StartCoroutine(ResetDown());
    }

    IEnumerator ResetDown()
    {
        yield return new WaitForEndOfFrame();
        isDown = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsPressed = false;
        isDown = false;
    }
}