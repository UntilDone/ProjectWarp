using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{

    public InputAction inputAction;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
    }

}
