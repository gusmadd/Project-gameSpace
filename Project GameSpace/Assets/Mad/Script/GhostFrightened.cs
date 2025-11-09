using UnityEngine;
using System.Collections;

public class GhostFrightened : GhostBehavior
{
    public SpriteRenderer bodyRenderer;
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
            bodyRenderer.color = Color.blue;
    }

    public override void Disable()
    {
        base.Disable();
        if (bodyRenderer != null)
            bodyRenderer.color = originalColor;
    }

    public void OnEaten()
    {
        if (isEaten) return;
        isEaten = true;

        GetComponent<EnemyAI>().RespawnToStart();
        Disable();
    }

    private IEnumerator RespawnDelay()
    {
        gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        GetComponent<EnemyAI>().RespawnToStart();
        gameObject.SetActive(true);
        Disable();
    }
}