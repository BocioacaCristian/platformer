using UnityEngine;

// This class handles the basic movement for a 2D platformer character
public class PlayerMovement : MonoBehaviour
{
    // SerializeField makes private variables visible in the Unity Inspector
    // These variables control how the player moves and jumps
    [SerializeField] public float moveSpeed; // How fast the player moves horizontally
    [SerializeField] public float jumpForce; // How high the player jumps
    
    // Private variables used within this class only
    private Rigidbody2D rb; // Reference to the physics component on this GameObject
    private float moveInput; // Stores the horizontal input value (-1 to 1)
    private bool facingRight = true; // Keeps track of which direction the player is facing
    private Animator animator; // Reference to the Animator component
    private bool isRunning = false; // Tracks if player is currently running
    private bool isFlipping = false; // Flag to track if we're currently flipping
    private bool isJumping = false; // Tracks if player is jumping
    private bool grounded = false; // Tracks if player is on the ground
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // This is where we initialize our components and variables
    void Start()
    {
        // GetComponent finds a component attached to the same GameObject
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    // Used for input detection and non-physics logic
    void Update()
    {
        // GetAxisRaw returns -1 (left), 0 (no input), or 1 (right)
        // Input axes are defined in Unity's Input Manager
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // Check if player is running
        isRunning = Mathf.Abs(moveInput) > 0.1f;
        
        // Check for jump button press (usually Space)
        // GetButtonDown triggers once when the button is first pressed
        if (Input.GetButtonDown("Jump") && grounded)
        {
            Jump();
        }
        
        // Flip the player sprite based on movement direction
        // Only flip if we're actually moving in that direction
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
        
        // Set animator parameters - do this AFTER flipping logic
        if (animator != null)
        {
            // If we're running, make sure we're using the run animation
            // This ensures we don't switch animations during a flip
            animator.SetBool("Run", isRunning);
            animator.SetBool("grounded", grounded);
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        
        // Set jumping state and animation
        isJumping = true;
        grounded = false;
        
        if (animator != null)
        {
            // Trigger the jump animation
            animator.SetTrigger("jump");
        }
    }
    
    // Flip the player sprite horizontally
    private void Flip()
    {
        // If we're already in a run animation, save that state so we don't flicker
        bool wasRunning = isRunning;
        
        // Toggle the facing direction flag
        facingRight = !facingRight;
        
        // Get the current scale
        Vector3 scale = transform.localScale;
        
        // Flip the X scale to mirror the sprite
        scale.x *= -1;
        
        // Apply the new scale
        transform.localScale = scale;
        
        // Force the run animation to stay on during flip if we were running
        if (wasRunning && animator != null)
        {
            animator.SetBool("Run", true);
        }
    }
    
    // Called when this collider/rigidbody has begun touching another rigidbody/collider
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the collision is happening below the player (ground)
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            // Contact normal pointing roughly upward indicates ground
            if (contact.normal.y >= 0.5f)
            {
                grounded = true;
                
                // If we were jumping, we're not anymore
                if (isJumping)
                {
                    isJumping = false;
                }
                
                break;
            }
        }
    }
    
    // Called when this collider/rigidbody has stopped touching another rigidbody/collider
    void OnCollisionExit2D(Collision2D collision)
    {
        // We need to check if we've completely left the ground
        // A simple approach is to reset grounded and rely on OnCollisionEnter2D
        // to detect ground again if we're still on it
        grounded = false;
    }
}
