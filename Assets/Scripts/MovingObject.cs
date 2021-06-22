using UnityEngine;

public abstract class MovingObject : MonoBehaviour
{ 
    public LayerMask blockingLayer;  

    public BoxCollider2D boxCollider;
    public Rigidbody2D rb2D;
    public SpriteRenderer spriteRenderer;
    public static float moveSpeed = 5f;

    protected virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        boxCollider.enabled = false;
        spriteRenderer.enabled = false;
    }

}
