using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;

    private int player1Score = 0;
    private int player2Score = 0;

    public void AddScorePlayer1(int amount)
    {
        player1Score += amount;
        UpdateUI();
    }

    public void AddScorePlayer2(int amount)
    {
        player2Score += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        player1Text.text = "P1 Score: " + player1Score;
        player2Text.text = "P2 Score: " + player2Score;
    }
}
