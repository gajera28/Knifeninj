using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelHUD : MonoBehaviour
{
    public Text titleText;
    public Text statusText;
    public Text objectiveText;
    public Text messageText;
    public GameObject winPanel;
    public Text winTitleText;
    public Text winDetailText;
    public Text creditsText;

    private PlayerThrow thrower;
    private PlayerHealth health;
    private Coroutine messageRoutine;

    void Start()
    {
        PlayerMovement player = FindFirstObjectByType<PlayerMovement>();
        if (player != null)
        {
            thrower = player.GetComponent<PlayerThrow>();
            health = player.GetComponent<PlayerHealth>();
        }

        if (titleText != null)
            titleText.text = "LEVEL 1  |  MOONLIT PASS";

        // Shrink the status line to fit on one row now that it also shows the score.
        if (statusText != null)
        {
            statusText.verticalOverflow = VerticalWrapMode.Truncate;
            statusText.resizeTextForBestFit = true;
            statusText.resizeTextMinSize = 18;
            statusText.resizeTextMaxSize = statusText.fontSize;
        }

        if (objectiveText != null)
        {
            objectiveText.text = "Destroy every training target, then reach the shrine gate.";

            // Make the objective stand out against the moon behind the panel.
            objectiveText.fontStyle = FontStyle.Bold;
            objectiveText.color = new Color(1f, 0.84f, 0.45f);
            if (objectiveText.GetComponent<Shadow>() == null)
            {
                Shadow shadow = objectiveText.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
                shadow.effectDistance = new Vector2(2f, -2f);
            }
        }

        if (creditsText != null)
            creditsText.text = "Developed by Ishan, Ayush & Meheraj";

        if (winPanel != null)
            winPanel.SetActive(false);
    }

    void Update()
    {
        Level1GameManager gm = Level1GameManager.Instance;
        if (gm == null || statusText == null)
            return;

        if (titleText != null)
            titleText.text =
                "LEVEL 1  |  MOONLIT PASS     TIME " + FormatTime(gm.ElapsedTime);

        int hp = health != null ? health.CurrentHealth : 0;
        int knives = thrower != null ? thrower.knivesRemaining : 0;

        statusText.text =
            "SCORE " + gm.Score +
            "   HP " + hp +
            "   KNIVES " + knives +
            "   COINS " + gm.Coins +
            "   TARGETS " + gm.TargetsDestroyed + "/" + gm.TargetsTotal;
    }

    public void ShowMessage(string text, float duration = 1.5f)
    {
        if (messageText == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(
            MessageRoutine(text, duration)
        );
    }

    IEnumerator MessageRoutine(string text, float duration)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        messageText.gameObject.SetActive(false);
        messageRoutine = null;
    }

    public void ShowWin(int coins, int score, float time, int timeBonus)
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (winTitleText != null)
            winTitleText.text = "LEVEL 1 COMPLETE";

        if (winDetailText != null)
            winDetailText.text =
                "Moonlit Pass cleared in " + FormatTime(time) +
                "\nTime bonus: +" + timeBonus +
                "\nFinal score: " + score +
                "\nCoins collected: " + coins +
                "\nDeveloped by Ishan, Ayush & Meheraj" +
                "\nPress R to replay";

        Time.timeScale = 0f;
    }

    public static string FormatTime(float seconds)
    {
        int total = Mathf.FloorToInt(seconds);
        return (total / 60) + ":" + (total % 60).ToString("00");
    }
}
