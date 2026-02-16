using TMPro;
using UnityEngine;

public class PlayerScore : MonoBehaviour
{
    public int score = 0;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        // get player index
        PlayerInputBridge input = GetComponent<PlayerInputBridge>();

        if (input.playerIndex == 0)
            scoreText = GameObject.Find("Player1ScoreText").GetComponent<TextMeshProUGUI>();

        else if (input.playerIndex == 1)
            scoreText = GameObject.Find("Player2ScoreText").GetComponent<TextMeshProUGUI>();

        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }
}
