using UnityEngine;

// 1. Inherit from MonoBehaviour, NOT Workstation
public class CustomerTable : MonoBehaviour
{
    // 2. Use the exact same function name and parameter as PanStation
    public void Interact(PlayerCombat player)
    {
        // =========================
        // PLAYER HOLDING ITEM → TRY TO SERVE
        // =========================
        if (player.heldItem != null)
        {
            GameObject item = player.heldItem;
            Plate plate = item.GetComponent<Plate>();

            // Check if it is a valid plate with food
            if (plate != null && plate.currentDishValue > 0)
            {
                int points = plate.currentDishValue;

                // --- ADD SCORE ---
                PlayerScore pScore = player.GetComponent<PlayerScore>();
                if (pScore != null)
                {
                    pScore.AddScore(points);
                }
                else
                {
                    ScoreManager sm = FindObjectOfType<ScoreManager>();
                    if (sm != null) sm.AddScorePlayer1(points);
                }

                Debug.Log($"Served! Earned {points} points.");

                // --- REMOVE ITEM ---
                Destroy(item);

                // --- CLEAR PLAYER HAND ---
                // PanStation does: player.heldItem = null;
                // We do the same here.
                player.heldItem = null;
            }
            else
            {
                Debug.Log("Cannot serve this (Not a plate or empty).");
            }
        }
        
        // We don't need an "Empty Hand" section because you can't pick things up from a customer table.
    }
}