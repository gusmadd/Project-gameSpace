using System.Collections;
using UnityEngine;

public class FireDamage : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 1f;       // durasi api
    public float respawnDelay = 3f;   // delay sebelum ghost respawn
    public GameObject burnEffect;     // efek terbakar

    private bool burned = false;

    private void Start()
    {
        StartCoroutine(SelfDestruct());
        Debug.Log("[🔥 FireDamage] Fire aktif");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (burned) return;

        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost != null)
        {
            burned = true;
            Debug.Log("[🔥 FireDamage] Ghost kena api: " + ghost.name);

            // efek visual
            if (burnEffect != null)
            {
                GameObject fx = Instantiate(burnEffect, ghost.transform.position, Quaternion.identity);
                Destroy(fx, 1f);
            }

            // 🚀 Jalankan coroutine di FireDamage, bukan di Ghost
            GameManager.Instance.RespawnGhost(ghost, respawnDelay);
        }
        else
        {
            Debug.Log("[🔥 FireDamage] Kena sesuatu tapi bukan ghost: " + other.name);
        }
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(lifeTime);

        if (TryGetComponent<SpriteRenderer>(out var sr))
            sr.enabled = false;

        if (TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}