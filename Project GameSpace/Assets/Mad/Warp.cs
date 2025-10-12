using System.Collections;
using UnityEngine;

public class TeleportGate : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform connection;         // tujuan teleport
    public float cooldown = 0.5f;        // waktu jeda agar tidak bolak balik terus
    private static bool canTeleport = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTeleport || connection == null) return;

        // Hanya teleport Player dan Enemy
        if (!other.CompareTag("Player") && !other.CompareTag("Enemy")) return;

        StartCoroutine(TeleportObject(other.transform));
    }

    private IEnumerator TeleportObject(Transform obj)
    {
        canTeleport = false;

        // pindahkan ke portal tujuan
        Vector3 newPos = connection.position;
        newPos.z = obj.position.z;
        obj.position = newPos;

        // Kalau Player -> reset posisi biar tetap bisa kontrol
        PlayerController pc = obj.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.ForceRecenter();
        }

        // Kalau Enemy -> reset agar tetap align ke grid
        EnemyAI ai = obj.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.ForceRecenterAfterTeleport();
        }

        yield return new WaitForSeconds(cooldown);
        canTeleport = true;
    }
}
