using UnityEngine;

public class GhostFrightened : GhostBehavior
{
    public SpriteRenderer bodyRenderer;  // assign di prefab Ghost_Base
    private Color originalColor;
    private bool isEaten = false;

    private void Awake()
    {
        if (bodyRenderer != null)
            originalColor = bodyRenderer.color;
    }

    public override void Enable(float duration)
    {
        base.Enable(duration);
        isEaten = false;

        if (bodyRenderer != null)
            bodyRenderer.color = Color.blue; // biru frightened
    }

    public override void Disable()
    {
        base.Disable();

        if (bodyRenderer != null)
            bodyRenderer.color = originalColor; // kembali normal
    }

    public void OnEaten()
    {
        if (isEaten) return;

        isEaten = true;

        GameManager.Instance.GhostEaten(ghost);

        ghost.ResetState();  // balik ke rumah
        Disable();
    }
}
