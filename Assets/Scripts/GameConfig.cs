using UnityEngine;

public static class GameConfig
{
    public const int AmmoPerRound = 2;
    public const float ReloadTime = 1.2f;

    public const float BaseSeeRadius = 2.0f;
    public const float BaseReactionTime = 0.35f;
    public const float DodgeCooldown = 1.5f;
    public const float DodgeDuration = 0.4f;
    public const float DodgeSpeedMultiplier = 2f;

    public const float FakeDeathChance = 0.6f;
    public const float NearMissRadius = 1.1f;
    public const float FakeDeadDuration = 1.5f;

    public const float PeekInterval = 2.5f;
    public const float PeekDuration = 0.8f;
    public const float HideDuration = 6f;

    public const float BaseDuckLifetime = 8f;
    public const float BaseDuckSpeed = 2.1f;

    public const int DucksPerLevel = 5;
    public const int MaxDucksAlive = 2;
    public const int EscapesToLose = 3;

    public const int HitScore = 100;
    public const int CatchScore = 150;
    public const int UiPenalty = -50;

    public const float GrassTop = -3.7f;
    public const float DuckMargin = 0.7f;
}

public class LevelRules
{
    public readonly int Level;

    public LevelRules(int level) { Level = level; }

    public float SpeedMultiplier => Mathf.Pow(1.1f, Level - 1);
    public float SeeRadius => GameConfig.BaseSeeRadius * Mathf.Pow(1.1f, Level - 1);
    public float ReactionTime => Mathf.Max(0.2f, GameConfig.BaseReactionTime * Mathf.Pow(0.88f, Level - 1));
    public float DuckLifetime => Mathf.Max(4f, GameConfig.BaseDuckLifetime - 0.5f * (Level - 1));
    public float FakeDeathChance => Mathf.Min(0.9f, GameConfig.FakeDeathChance + 0.05f * Mathf.Max(0, Level - 3));

    public bool CanDodge => Level >= 2;
    public bool CanFakeDeath => Level >= 3;
    public bool CanHide => Level >= 4;
}

public static class PlayArea
{
    static Camera Cam => GameManager.Cam;

    public static float Height => Cam.orthographicSize * 2f;

    public static float MinX => Cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).x + GameConfig.DuckMargin;
    public static float MaxX => Cam.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - GameConfig.DuckMargin;
    public static float Top => Cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y - GameConfig.DuckMargin;
    public static float FlightFloor => GameConfig.GrassTop + 0.6f;
}
