using UnityEngine;

public class GhostHome : MonoBehaviour
{
    public Transform inside;  // posisi ghost di rumah
    public Transform outside; // posisi ghost keluar rumah

    // Pindah keluar rumah (saat spawn atau exit)
    public void ExitHome(Transform ghost)
    {
        if (outside != null)
            ghost.position = outside.position;
    }

    // Pindah masuk rumah (ketika ghost dimakan)
    public void EnterHome(Transform ghost)
    {
        if (inside != null)
            ghost.position = inside.position;
    }
}
