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
    public float cooldownTime = 5f;  // default cooldown
    public bool isReady = true;

    private Coroutine cooldownRoutine;

    private void Start()
    {
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f; // siap pakai
    }

    // 🔹 Bisa dipanggil tanpa parameter → pakai cooldownTime default
    public void TriggerCooldown()
    {
        TriggerCooldown(cooldownTime);
    }

    // 🔹 Bisa dipanggil dengan parameter → pakai durasi custom
    public void TriggerCooldown(float duration)
    {
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(CooldownCoroutine(duration));
    }

    private IEnumerator CooldownCoroutine(float duration)
    {
        isReady = false;
        float timer = duration;

        cooldownOverlay.fillAmount = 1f;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            cooldownOverlay.fillAmount = timer / duration;
            yield return null;
        }

        cooldownOverlay.fillAmount = 0f;
        isReady = true;
    }
}
