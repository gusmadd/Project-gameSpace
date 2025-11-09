using System.Collections;
using UnityEngine;

public class IceDamage : MonoBehaviour
{
    [Header("Settings")]
    public float slowFactor = 0.3f;       // 30% dari kecepatan asli (berarti -70%)
    public float slowDuration = 2f;       // efek lambat 2 detik
    public float respawnDelay = 3f;       // waktu respawn setelah mati
    public GameObject freezeEffect;       // prefab efek es (muncul saat kena)

    private void OnTriggerEnter2D(Collider2D other)
    {
        Ghost ghost = other.GetComponent<Ghost>();
        EnemyAI ai = other.GetComponent<EnemyAI>();

        if (ghost != null && ai != null)
        {
            Debug.Log($"[❄️ IceDamage] {ghost.name} kena efek es!");
            StartCoroutine(HandleFreezeAndRespawn(ghost, ai));
        }
    }

    private IEnumerator HandleFreezeAndRespawn(Ghost ghost, EnemyAI ai)
    {
        float originalSpeed = ai.moveSpeed;

        // 🔹 1. Kurangi kecepatan 70%
        ai.moveSpeed = originalSpeed * slowFactor;
        Debug.Log($"[❄️ IceDamage] {ghost.name} melambat jadi {ai.moveSpeed}");

        // 🔹 2. Efek visual es
        if (freezeEffect != null)
        {
            GameObject fx = Instantiate(freezeEffect, ghost.transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }

        // 🔹 3. Tunggu selama efek lambat
        yield return new WaitForSeconds(slowDuration);

        // 🔹 4. Bekukan (matikan sementara)
        ghost.gameObject.SetActive(false);
        Debug.Log($"[❄️ IceDamage] {ghost.name} membeku total dan nonaktif.");

        // 🔹 5. Tunggu sebelum respawn
        yield return new WaitForSeconds(respawnDelay);

        // 🔹 6. Aktifkan lagi dan reset posisi
        ghost.gameObject.SetActive(true);
        ghost.ResetState();

        // 🔹 7. Pastikan komponen EnemyAI aktif
        if (ai != null)
        {
            ai.moveSpeed = originalSpeed;      // ✅ Kembalikan speed normal
            ai.RestartMovement();              // Pastikan AI bisa jalan lagi
        }

        Debug.Log($"[❄️ IceDamage] {ghost.name} respawn dan kecepatan dikembalikan ({ai.moveSpeed}).");
    }
}