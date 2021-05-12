using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveAccel;
    public float maxSpeed;

    [Header("Jump")]
    public float jumpAccel;

    [Header("Ground Raycast")]
    public float groundRaycastDistance;
    public LayerMask groundLayerMask;

    [Header("Scoring")]
    public ScoreController score;
    public float scoringRatio;
    private float lastPositionX;

    [Header("GameOver")]
    public GameObject gameOverScreen;
    public float fallPositionY;

    [Header("Camera")]
    public CameraFollow gameCamera;

    bool isJumping;
    bool isOnGround;

    Rigidbody2D rigid;
    Animator anim;
    PlayerSoundController sound;

    // Start is called before the first frame update
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sound = GetComponent<PlayerSoundController>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isOnGround)
            {
                isJumping = true;
                sound.PlayJump();
            }
        }

        anim.SetBool("isOnGround" , isOnGround);

        int distancePassed = Mathf.FloorToInt(transform.position.x - lastPositionX);
        int scoreIncrement = Mathf.FloorToInt(distancePassed / scoringRatio);

        if (scoreIncrement > 0)
        {
            score.IncreaseCurrentScore(scoreIncrement);
            lastPositionX += distancePassed;
        }

        if (transform.position.y < fallPositionY)
        {
            GameOver();
        }
    }

    private void FixedUpdate()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundRaycastDistance, groundLayerMask);
        if (hit)
        {
            if(!isOnGround && rigid.velocity.y <= 0)
            {
                isOnGround = true;
            }   
        }
        else
        {
            isOnGround = false;
        }

        Vector2 vectorVelocity = rigid.velocity;

        if (isJumping)
        {
            vectorVelocity.y += jumpAccel;
            isJumping = false;
        }

        vectorVelocity.x = Mathf.Clamp(vectorVelocity.x + moveAccel * Time.deltaTime, 0f, maxSpeed);

        rigid.velocity = vectorVelocity;
    }

    private void OnDrawGizmos()
    {
        Debug.DrawLine(transform.position, transform.position + (Vector3.down * groundRaycastDistance), Color.white);
    }

    private void GameOver()
    {
        score.FinishScoring();

        gameCamera.enabled = false;

        gameOverScreen.SetActive(true);

        this.enabled = false;
    }
}
