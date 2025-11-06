using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
     private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player masuk portal! Next level!");

            if (GameManager.Instance.IsLastLevel())
                GameManager.Instance.WinGame();
            else
                GameManager.Instance.LevelCompleted();
        }
    }
}
