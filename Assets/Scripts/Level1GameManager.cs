using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Level1GameManager : MonoBehaviour
{
    public static Level1GameManager Instance { get; private set; }

    public int TargetsTotal { get; private set; }
    public int TargetsDestroyed { get; private set; }
    public int Coins { get; private set; }
    public int Score { get; private set; }
    public float ElapsedTime { get; private set; }
    public int TimeBonus { get; private set; }
    public bool LevelComplete { get; private set; }

    public const int TargetPoints = 100;
    public const int EnemyPoints = 50;
    public const int CoinPoints = 10;

    // Finishing under ParTime earns BonusPerSecond for every second saved.
    public const float ParTime = 120f;
    public const int BonusPerSecond = 5;

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
        CheckpointFlag.CreateLevelFlags(transform);
        hud?.ShowMessage(
            "A/D move  |  SPACE jump  |  F throw",
            3.2f
        );
    }

    void Update()
    {
        if (!LevelComplete)
            ElapsedTime += Time.deltaTime;

        if (!LevelComplete && Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            PlayerThrow player = FindAnyObjectByType<PlayerThrow>();
            if (player != null && player.knivesRemaining <= 0)
                player.GetComponent<PlayerHealth>()?.Kill();
        }

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
        AddScore(TargetPoints);

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
        AddScore(CoinPoints);
    }

    public void EnemyDefeated()
    {
        AddScore(EnemyPoints);
    }

    public void AddScore(int points)
    {
        if (LevelComplete)
            return;

        Score += points;
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

        TimeBonus = Mathf.Max(
            0,
            Mathf.RoundToInt((ParTime - ElapsedTime) * BonusPerSecond)
        );
        Score += TimeBonus;

        LevelComplete = true;
        KnifeNinjaAudio.Instance?.PlayWin();
        hud?.ShowWin(Coins, Score, ElapsedTime, TimeBonus);
    }
}
