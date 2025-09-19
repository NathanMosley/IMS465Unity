using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 _Input;
    private Vector2 _direction;
    private CharacterController _characterController;

    [SerializeField] private float speed; 

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        _characterController.Move(_direction * speed * Time.deltaTime);
    }


    public void move(InputAction.CallbackContext context)
    {
        _Input = context.ReadValue<Vector2>();
        _direction = new Vector2(_Input.x, _Input.y);
    }
}
