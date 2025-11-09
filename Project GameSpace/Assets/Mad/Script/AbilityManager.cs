using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AbilityManager : MonoBehaviour
{
    [Header("References")]
    public Tilemap wallTilemap;
    public GameObject bombPrefab;

    [Header("UI")]
    public AbilityUI bombUI; // Drag dari Canvas

    [Header("Cooldown Settings")]
    public float bombCooldown = 7f;
    private bool canUseBomb = true;

    [Header("Options")]
    public bool startOnCooldown = true; // biar bisa diatur dari Inspector

    void Start()
    {
        if (startOnCooldown)
        {
            canUseBomb = false;
            StartCoroutine(InitialCooldown());
        }
    }

    IEnumerator InitialCooldown()
    {
        if (bombUI != null)
           bombUI.TriggerCooldown(); // UI juga langsung jalan

        yield return new WaitForSeconds(bombCooldown);
        canUseBomb = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            TryUseBomb();
        }
        // nanti T dan Y bisa ditambah kayak TryUseChili(), TryUseIce(), dst
    }

    void TryUseBomb()
    {
        if (!canUseBomb) return;

        PlaceBomb();
        StartCoroutine(CooldownRoutine());

        if (bombUI != null)
            bombUI.TriggerCooldown();
    }

    void PlaceBomb()
    {
        Vector3Int cell = wallTilemap.WorldToCell(transform.position);
        Vector3 spawnPos = wallTilemap.CellToWorld(cell) + (Vector3)wallTilemap.cellSize * 0.5f;

        Instantiate(bombPrefab, spawnPos, Quaternion.identity);
    }

    IEnumerator CooldownRoutine()
    {
        canUseBomb = false;
        yield return new WaitForSeconds(bombCooldown);
        canUseBomb = true;
    }
}