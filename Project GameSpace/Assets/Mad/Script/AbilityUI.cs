using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;          // ikon ability
    public Image cooldownOverlay;    // overlay abu-abu

    [Header("Ability Settings")]
    public float cooldownTime = 5f;  // durasi cooldown
    public bool isReady = true;

    private Coroutine cooldownRoutine;

    private void Start()
    {
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f; // 0 artinya tidak tertutup (siap pakai)
    }

    public void TriggerCooldown()
    {
        if (isReady && cooldownOverlay != null)
        {
            if (cooldownRoutine != null)
                StopCoroutine(cooldownRoutine);

            cooldownRoutine = StartCoroutine(CooldownCoroutine());
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isReady = false;
        float timer = cooldownTime;

        // overlay penuh abu-abu saat baru dipakai
        cooldownOverlay.fillAmount = 1f;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            cooldownOverlay.fillAmount = timer / cooldownTime;
            yield return null;
        }

        cooldownOverlay.fillAmount = 0f;
        isReady = true;
    }
}
