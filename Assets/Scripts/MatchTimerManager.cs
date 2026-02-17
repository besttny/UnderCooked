using System.Linq;
using TMPro;
using UnityEngine;

public class MatchTimerManager : MonoBehaviour
{
    [Header("Timer")]
    public float matchDurationSeconds = 6 * 60f; // 6 นาที
    public bool autoStart = true;

    [Header("UI (optional)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI resultText; // แสดงผู้ชนะ
    public GameObject resultPanel;     // ถ้ามี panel ให้โชว์ตอนจบ (optional)

    private float timeLeft;
    private bool running;

    void Start()
    {
        timeLeft = matchDurationSeconds;
        UpdateTimerUI();

        if (resultPanel != null) resultPanel.SetActive(false);
        if (resultText != null) resultText.text = "";

        if (autoStart) running = true;
    }

    void Update()
    {
        if (!running) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
            UpdateTimerUI();
            EndMatch();
            return;
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        if (timerText == null) return;

        int t = Mathf.CeilToInt(timeLeft);
        int min = t / 60;
        int sec = t % 60;
        timerText.text = $"{min:00}:{sec:00}";
    }

    void EndMatch()
    {
        // หา PlayerScore ทั้งหมดในฉาก
        var players = FindObjectsOfType<PlayerScore>();
        if (players == null || players.Length == 0)
        {
            ShowResult("No players found");
            return;
        }

        // หาคะแนนสูงสุด
        int bestScore = players.Max(p => p.score);

        // คนที่ได้คะแนนสูงสุด (กันกรณีเสมอ)
        var winners = players.Where(p => p.score == bestScore).ToArray();

        if (winners.Length == 1)
        {
            string winnerName = GetPlayerDisplayName(winners[0]);
            ShowResult($"{winnerName} WIN!");
        }
        else
        {
            // เสมอ
            string names = string.Join(", ", winners.Select(GetPlayerDisplayName));
            ShowResult($"DRAW! ({names})");
        }

        FreezeAllPlayers();
    }

    string GetPlayerDisplayName(PlayerScore ps)
    {
        var bridge = ps.GetComponent<PlayerInputBridge>();
        if (bridge != null) return $"Player {bridge.playerIndex + 1}";
        return ps.gameObject.name;
    }

    void ShowResult(string msg)
    {
        Debug.Log("[Match] " + msg);

        if (resultPanel != null) resultPanel.SetActive(true);
        if (resultText != null) resultText.text = msg;
    }

    void FreezeAllPlayers()
    {
        // ปิด input + ปิดการเคลื่อนที่/ตี/หยิบ เพื่อไม่ให้เล่นต่อหลังหมดเวลา
        foreach (var b in FindObjectsOfType<PlayerInputBridge>()) b.enabled = false;
        foreach (var c in FindObjectsOfType<PlayerController>()) c.enabled = false;
        foreach (var cb in FindObjectsOfType<PlayerCombat>()) cb.enabled = false;
        foreach (var it in FindObjectsOfType<PlayerInteract>()) it.enabled = false;

        // หยุด Rigidbody ที่ยังไหลอยู่
        foreach (var rb in FindObjectsOfType<Rigidbody>())
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
