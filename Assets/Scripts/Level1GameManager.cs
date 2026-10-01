using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Level1GameManager : MonoBehaviour
{
    public static Level1GameManager Instance { get; private set; }

    public int TargetsTotal { get; private set; }
    public int TargetsDestroyed { get; private set; }
    public int Coins { get; private set; }
    public bool LevelComplete { get; private set; }

    [SerializeField] private LevelHUD hud;
    [SerializeField] private GameObject gateBarrier;

    public LevelHUD HUD => hud;

    void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        hud?.ShowMessage(
            "A/D move  |  SPACE jump  |  F throw",
            3.2f
        );
    }

    void Update()
    {
        if (LevelComplete &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void SetHUD(LevelHUD value)
    {
        hud = value;
    }

    public void SetGateBarrier(GameObject barrier)
    {
        gateBarrier = barrier;
    }

    public void RegisterTarget()
    {
        TargetsTotal++;
    }

    public void TargetDestroyed()
    {
        TargetsDestroyed++;

        if (TargetsTotal > 0 &&
            TargetsDestroyed >= TargetsTotal)
        {
            if (gateBarrier != null)
                gateBarrier.SetActive(false);

            KnifeNinjaAudio.Instance?.PlayGate();
            hud?.ShowMessage(
                "All targets destroyed - the shrine gate is open!",
                2.6f
            );
        }
        else
        {
            hud?.ShowMessage(
                "Target destroyed!",
                0.9f
            );
        }
    }

    public void AddCoin()
    {
        Coins++;
    }

    public void TryCompleteLevel()
    {
        if (TargetsTotal == 0 ||
            TargetsDestroyed < TargetsTotal)
        {
            int remaining = Mathf.Max(
                0,
                TargetsTotal - TargetsDestroyed
            );

            hud?.ShowMessage(
                "The gate is sealed - " + remaining +
                " target" + (remaining == 1 ? "" : "s") + " remain.",
                1.8f
            );
            return;
        }

        if (LevelComplete)
            return;

        LevelComplete = true;
        KnifeNinjaAudio.Instance?.PlayWin();
        hud?.ShowWin(Coins);
    }
}
