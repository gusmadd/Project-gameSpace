using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Area")]
    public Vector2 areaSize = new Vector2(5, 5);  // ukuran area spawn (lebar x tinggi)
    public int enemyCount = 5;                    // berapa musuh yang mau muncul
    public float spawnDelay = 0.2f;               // jeda antar spawn

    [Header("Prefabs & References")]
    public List<GameObject> enemyPrefabs;         // daftar prefab musuh
    public Transform player;                      // referensi player

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // ambil prefab acak dari list
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];

            // tentukan posisi acak di dalam area
            Vector3 randomPos = transform.position + new Vector3(
                Random.Range(-areaSize.x / 2, areaSize.x / 2),
                Random.Range(-areaSize.y / 2, areaSize.y / 2),
                0f
            );

            // buat enemy
            GameObject enemy = Instantiate(prefab, randomPos, Quaternion.identity);

            // kasih tahu siapa player-nya
            EnemyAI ai = enemy.GetComponent<EnemyAI>();
            if (ai != null)
            {
                ai.SetPlayer(player);
            }

            yield return new WaitForSeconds(spawnDelay);
        }
    }

    // biar area kelihatan di editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(areaSize.x, areaSize.y, 0f));
    }
}
