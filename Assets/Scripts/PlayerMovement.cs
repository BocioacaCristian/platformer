using UnityEngine;

// This class handles the basic movement for a 2D platformer character
public class PlayerMovement : MonoBehaviour
{
    // SerializeField makes private variables visible in the Unity Inspector
    // These variables control how the player moves and jumps
    [SerializeField] public float moveSpeed; // How fast the player moves horizontally
    [SerializeField] public float jumpForce = 10f; // How high the player jumps
    
    // Private variables used within this class only
    private Rigidbody2D rb; // Reference to the physics component on this GameObject
    private float moveInput; // Stores the horizontal input value (-1 to 1)
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // This is where we initialize our components and variables
    void Start()
    {
        // GetComponent finds a component attached to the same GameObject
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    // Used for input detection and non-physics logic
    void Update()
    {
        // GetAxisRaw returns -1 (left), 0 (no input), or 1 (right)
        // Input axes are defined in Unity's Input Manager
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // Check for jump button press (usually Space)
        // GetButtonDown triggers once when the button is first pressed
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }
    }
    
    // FixedUpdate is called at a fixed interval (not every frame)
    // Used for physics calculations to ensure consistent behavior
    void FixedUpdate()
    {
        // Move the player horizontally at a constant speed
        // We only modify the X velocity, preserving the Y velocity (for gravity/jumping)
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }
    
    // Custom method for handling the jump action
    private void Jump()
    {
        // Set the vertical velocity directly to create an instant jump effect
        // We keep the current horizontal velocity and only change the vertical component
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }
}
