using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
public class Ghost : MonoBehaviour
{
    public int points = 200;
    [HideInInspector] public EnemyAI movement;
    [HideInInspector] public GhostBehavior currentBehavior;

    public GhostHome home;
    public GhostFrightened frightened; // assign prefab Ghost_Base
    public SpriteRenderer bodyRenderer;

    private void Awake()
    {
        movement = GetComponent<EnemyAI>();
    }

    public void ResetState()
    {
        if (currentBehavior != null)
            currentBehavior.Disable();

        gameObject.SetActive(true);

        if (home != null && home.inside != null)
        {
            transform.position = home.inside.position;
            movement.startPos = home.inside.position; // update startPos supaya sinkron
        }

        if (movement != null)
        {
            movement.enabled = true;
            movement.RespawnToStart();
        }

        if (bodyRenderer != null)
            bodyRenderer.enabled = true;
    }

    public void SetBehavior(GhostBehavior behavior)
    {
        if (currentBehavior != null)
            currentBehavior.Disable();

        currentBehavior = behavior;

        if (currentBehavior != null)
            currentBehavior.Enable();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // cek apakah objek ini player

        Debug.Log("Ghost menabrak sesuatu: " + other.name);

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        if (currentBehavior is GhostFrightened)
        {
            ((GhostFrightened)currentBehavior).OnEaten();
            player.AddScore(points);
        }
        else
        {
            Debug.Log("Ghost menabrak Player — Kurangi nyawa!");
            GameManager.Instance.PacmanEaten();
        }
    }
}
