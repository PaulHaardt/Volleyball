using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public bool isOpen;
    public GameObject parent;

    // Start is called before the first frame update
    void Start()
    {
        isOpen = false;
        parent.SetActive(false);
    }

    public void Pause(InputAction.CallbackContext callbackContext)
    {
        StopAllCoroutines();
        Time.timeScale = 0;
        parent.SetActive(true);
    }

    public void Resume()
    {
        StopAllCoroutines();
        Time.timeScale = 1;
        parent.SetActive(false);
    }
}