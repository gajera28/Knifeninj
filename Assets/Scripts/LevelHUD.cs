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

        if (objectiveText != null)
            objectiveText.text = "Destroy every training target, then reach the shrine gate.";

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

        int hp = health != null ? health.CurrentHealth : 0;
        int knives = thrower != null ? thrower.knivesRemaining : 0;

        statusText.text =
            "HP " + hp +
            "    KNIVES " + knives +
            "    COINS " + gm.Coins +
            "    TARGETS " + gm.TargetsDestroyed + "/" + gm.TargetsTotal;
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

    public void ShowWin(int coins)
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        if (winTitleText != null)
            winTitleText.text = "LEVEL 1 COMPLETE";

        if (winDetailText != null)
            winDetailText.text =
                "Moonlit Pass cleared\nCoins collected: " + coins +
                "\n\nDeveloped by Ishan, Ayush & Meheraj" +
                "\nPress R to replay";

        Time.timeScale = 0f;
    }
}
