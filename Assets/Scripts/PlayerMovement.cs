using UnityEngine;

// This class handles the basic movement for a 2D platformer character
public class PlayerMovement : MonoBehaviour
{
    // SerializeField makes private variables visible in the Unity Inspector
    // These variables control how the player moves and jumps
    [SerializeField] public float moveSpeed; // How fast the player moves horizontally
    [SerializeField] public float jumpForce; // How high the player jumps
    [SerializeField] public float wallJumpForce = 10f; // Force applied when wall jumping
    [SerializeField] public float directedWallJumpForce = 12f; // Force when jumping opposite direction
    [SerializeField] public float wallSlideSpeed = 2f; // How fast player slides down walls
    [SerializeField] public LayerMask wallLayer; // Layer to detect walls
    [SerializeField] public float wallCheckDistance = 0.5f; // Distance to check for walls
    [SerializeField] public float wallJumpTime = 0.2f; // Time to control the wall jump trajectory
    
    // Private variables used within this class only
    private Rigidbody2D rb; // Reference to the physics component on this GameObject
    private float moveInput; // Stores the horizontal input value (-1 to 1)
    private bool facingRight = true; // Keeps track of which direction the player is facing
    private Animator animator; // Reference to the Animator component
    private bool isRunning = false; // Tracks if player is currently running
    private bool isFlipping = false; // Flag to track if we're currently flipping
    private bool isJumping = false; // Tracks if player is jumping
    private bool grounded = false; // Tracks if player is on the ground
    private bool isTouchingWall = false; // Tracks if player is touching a wall
    private bool isWallSliding = false; // Tracks if player is sliding down a wall
    private bool isWallJumping = false; // Track if currently in wall jump
    private float wallJumpCounter = 0f; // Counter for wall jump control period
    private int wallDirX = 0; // Direction of the wall (-1: left, 1: right)
    
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
        // Handle wall jump counter
        if (isWallJumping)
        {
            wallJumpCounter -= Time.deltaTime;
            if (wallJumpCounter <= 0)
            {
                isWallJumping = false;
            }
        }
        
        // GetAxisRaw returns -1 (left), 0 (no input), or 1 (right)
        // Input axes are defined in Unity's Input Manager
        moveInput = Input.GetAxisRaw("Horizontal");
        
        // Check if player is running (only if not wall jumping)
        isRunning = Mathf.Abs(moveInput) > 0.1f && !isWallJumping;
        
        // Check for walls
        CheckForWalls();
        
        // Handle wall sliding
        HandleWallSliding();
        
        // Check for jump or wall jump
        if (Input.GetButtonDown("Jump"))
        {
            if (grounded)
            {
                Jump();
            }
            else if (isWallSliding)
            {
                // Determine if this is a directed wall jump
                bool isDirectedJump = (wallDirX > 0 && moveInput < 0) || (wallDirX < 0 && moveInput > 0);
                
                if (isDirectedJump)
                {
                    // Player is pressing in the opposite direction of the wall
                    DirectedWallJump();
                }
                else
                {
                    // Regular wall jump
                    WallJump();
                }
            }
        }
        
        // Only flip based on input if not wall jumping
        if (!isWallJumping)
        {
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
        }
        
        // Set animator parameters - do this AFTER flipping logic
        if (animator != null)
        {
            // If we're running, make sure we're using the run animation
            // This ensures we don't switch animations during a flip
            animator.SetBool("Run", isRunning);
            animator.SetBool("grounded", grounded);
            animator.SetBool("WallSlide", isWallSliding);
        }
    }
    
    // FixedUpdate is called at a fixed interval (not every frame)
    // Used for physics calculations to ensure consistent behavior
    void FixedUpdate()
    {
        // If wall jumping, let the physics handle the movement
        if (isWallJumping)
        {
            return;
        }
        
        // Move the player horizontally at a constant speed
        // We only modify the X velocity, preserving the Y velocity (for gravity/jumping)
        // Don't apply horizontal movement during wall slide if pushing into wall
        if (!isWallSliding || (moveInput * (facingRight ? 1 : -1) < 0))
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
    }
    
    // Check if player is touching a wall
    private void CheckForWalls()
    {
        // Cast rays in both left and right directions
        RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
        RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
        
        // Determine wall direction
        if (hitRight.collider != null)
        {
            isTouchingWall = true;
            wallDirX = 1;
        }
        else if (hitLeft.collider != null)
        {
            isTouchingWall = true;
            wallDirX = -1;
        }
        else
        {
            isTouchingWall = false;
            wallDirX = 0;
        }
    }
    
    // Handle wall sliding
    private void HandleWallSliding()
    {
        // Don't wall slide if wall jumping
        if (isWallJumping)
        {
            isWallSliding = false;
            return;
        }
        
        // Wall slide if touching wall, not grounded, and moving into the wall or not moving
        isWallSliding = isTouchingWall && !grounded && 
                        ((wallDirX > 0 && moveInput > 0) || (wallDirX < 0 && moveInput < 0) || moveInput == 0);
        
        // Apply wall slide speed
        if (isWallSliding)
        {
            // Limit falling speed
            float yVelocity = Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, yVelocity);
        }
    }
    
    // Directed wall jump (jump + opposite direction)
    private void DirectedWallJump()
    {
        // Stop wall sliding
        isWallSliding = false;
        
        // Set wall jumping state
        isWallJumping = true;
        wallJumpCounter = wallJumpTime;
        
        // Apply a stronger impulse away from the wall (in the direction of input)
        Vector2 jumpForceVector = new Vector2(moveInput * directedWallJumpForce, jumpForce);
        
        // Zero out current velocity to ensure consistent jump behavior
        rb.linearVelocity = Vector2.zero;
        
        // Apply impulse force
        rb.AddForce(jumpForceVector, ForceMode2D.Impulse);
        
        // Flip to face away from wall if not already facing that way
        if ((moveInput > 0 && !facingRight) || (moveInput < 0 && facingRight))
        {
            Flip();
        }
        
        // Set jumping state and animation
        isJumping = true;
        
        if (animator != null)
        {
            // Trigger the jump animation
            animator.SetTrigger("jump");
        }
    }
    
    // Wall jump
    private void WallJump()
    {
        // Stop wall sliding
        isWallSliding = false;
        
        // Set wall jumping state
        isWallJumping = true;
        wallJumpCounter = wallJumpTime;
        
        // Apply a more forceful impulse away from the wall
        Vector2 jumpForceVector = new Vector2(-wallDirX * wallJumpForce, jumpForce);
        
        // Zero out current velocity to ensure consistent jump behavior
        rb.linearVelocity = Vector2.zero;
        
        // Apply impulse force
        rb.AddForce(jumpForceVector, ForceMode2D.Impulse);
        
        // Flip to face away from wall
        if ((wallDirX > 0 && facingRight) || (wallDirX < 0 && !facingRight))
        {
            Flip();
        }
        
        // Set jumping state and animation
        isJumping = true;
        
        if (animator != null)
        {
            // Trigger the jump animation
            animator.SetTrigger("jump");
        }
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
                
                // Also end wall jumping state
                isWallJumping = false;
                
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
