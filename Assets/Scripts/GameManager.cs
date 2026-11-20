using System.Collections;
using UnityEngine;

public enum GameState { Start, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager I;
    public static Camera Cam;
    public static int HighScore;

    public GameState State { get; private set; } = GameState.Start;
    public int Score { get; private set; }
    public int Level { get; private set; } = 1;
    public int Escaped { get; private set; }
    public int Resolved { get; private set; }
    public LevelRules Rules { get; private set; }
    public Duck CurrentHidingDuck { get; set; }

    public UIManager ui;
    public PlayerShooter shooter;
    public DuckSpawner spawner;

    int levelVersion;

    void Awake() { I = this; }

    void Start()
    {
        Rules = new LevelRules(1);
        ui.ShowStart();
    }

    void Update()
    {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        bool click = mouse != null && mouse.leftButton.wasPressedThisFrame;
        bool restart = keyboard != null && keyboard.rKey.wasPressedThisFrame;

        if (State == GameState.Start && click) StartGame();
        else if (State == GameState.GameOver && (click || restart)) StartGame();
    }

    public void StartGame()
    {
        Score = 0;
        Level = 1;
        BeginLevel();
    }

    void BeginLevel()
    {
        levelVersion++;
        State = GameState.Playing;
        Escaped = 0;
        Resolved = 0;
        CurrentHidingDuck = null;
        Rules = new LevelRules(Level);

        shooter.ResetAmmo();
        ui.ShowHud();
        ui.SetHud(Score, Level, Escaped);

        if (Level == 2) ui.ShowNotification("Cảnh báo: vịt đã nhận ra bạn.", 3f);
        else if (Level == 3) ui.ShowNotification("Cảnh báo: vịt đã biết giả chết.", 3f);
        else if (Level == 4) ui.ShowNotification("Cảnh báo: vịt đang học cách núp.", 3f);
        else if (Level >= 5) ui.ShowNotification("Vịt đã tỉnh ngộ hoàn toàn.", 3f);

        spawner.BeginLevel(Rules);
    }

    public void AddScore(int delta)
    {
        Score = Mathf.Max(0, Score + delta);
        ui.SetHud(Score, Level, Escaped);
    }

    public void OnDuckResolved(bool escaped)
    {
        if (State != GameState.Playing) return;
        Resolved++;
        if (escaped)
        {
            Escaped++;
            ui.SetHud(Score, Level, Escaped);
            if (Escaped >= GameConfig.EscapesToLose)
            {
                GameOver();
                return;
            }
        }
        if (Resolved >= GameConfig.DucksPerLevel) StartCoroutine(NextLevel());
    }

    IEnumerator NextLevel()
    {
        int version = levelVersion;
        yield return new WaitForSeconds(1.2f);
        if (version != levelVersion || State != GameState.Playing) yield break;
        Level++;
        BeginLevel();
    }

    void GameOver()
    {
        State = GameState.GameOver;
        levelVersion++;
        HighScore = Mathf.Max(HighScore, Score);
        spawner.ClearAll();
        CurrentHidingDuck = null;
        ui.ShowGameOver(Score, HighScore);
    }
}
