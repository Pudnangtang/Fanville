using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int speed = 5;
    [SerializeField] private int runSpeed = 10;

    private PlayerControls playerControls;
    private Rigidbody rb;
    private Vector3 movement;
    private bool isRunning;

    private bool canMove = true;

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    private void Awake()
    {
        playerControls = new PlayerControls();

        // Setup run action
        playerControls.Player.Run.performed += _ => isRunning = true;
        playerControls.Player.Run.canceled += _ => isRunning = false;
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }

    private void OnDisable()
    {
        playerControls.Disable();
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (canMove) // Only allow movement if canMove is true
        {
            /*
             * // Get horizontal input from keyboard (A/D or Left/Right Arrow keys)
            float horizontalInput = Input.GetAxis("Horizontal");

            // Create a movement vector based on the horizontal input
            Vector3 moveVector = new Vector3(horizontalInput, 0, 0);

            // Move the player
            transform.Translate(moveVector * speed * Time.deltaTime);
            */
        }

        float x = playerControls.Player.Move.ReadValue<Vector2>().x;
        float z = playerControls.Player.Move.ReadValue<Vector2>().y;

        //Debug.Log(x + "," + z);

        movement = new Vector3(x, 0, z).normalized;
    }

    private void FixedUpdate()
    {
        if (isRunning)
        {
            rb.MovePosition(transform.position + movement * runSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
        }
    }
}
