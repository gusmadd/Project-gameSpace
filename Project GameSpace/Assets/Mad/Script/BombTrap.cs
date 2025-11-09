using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombTrap : MonoBehaviour
{
    [Header("Settings")]
    public float lifeTime = 5f;        // waktu aktif bom
    public float respawnDelay = 2f;    // waktu delay sebelum ghost respawn
    public GameObject boomEffect;      // prefab efek ledakan
    private bool exploded = false;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded) return;

        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost != null)
        {
            StartCoroutine(Explode(ghost));
        }
    }

    private IEnumerator Explode(Ghost ghost)
    {
        exploded = true;

        // Tampilkan efek ledakan
        if (boomEffect != null)
        {
            GameObject effect = Instantiate(boomEffect, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // Matikan bom (supaya gak keliatan tapi masih bisa jalan coroutinenya)
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Matikan ghost
        ghost.StopAllCoroutines();
        ghost.gameObject.SetActive(false);

        // Tunggu sebelum respawn
        yield return new WaitForSeconds(respawnDelay);

        // Respawn ghost
        ghost.gameObject.SetActive(true);
        ghost.ResetState();

        EnemyAI ai = ghost.GetComponent<EnemyAI>();
        if (ai != null)
            ai.RestartMovement();

        // Hancurkan bom sepenuhnya
        Destroy(gameObject);
    }
}
