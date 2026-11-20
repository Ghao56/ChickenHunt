using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public static Vector2 CrosshairWorldPos;
    public static float CrosshairSpeed;

    public int Ammo { get; private set; } = GameConfig.AmmoPerRound;
    public bool Reloading { get; private set; }

    Transform crosshair;
    float reloadLeft;

    void Awake()
    {
        var go = new GameObject("Crosshair");
        crosshair = go.transform;
        var ring = SpriteFactory.AddSprite(crosshair, SpriteFactory.Ring, Color.white, 20);
        ring.transform.localScale = Vector3.one * 0.55f;
        var dot = SpriteFactory.AddSprite(crosshair, SpriteFactory.Circle, Color.red, 21);
        dot.transform.localScale = Vector3.one * 0.06f;
        Cursor.visible = false;
    }

    void OnDestroy() { Cursor.visible = true; }

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screen = mouse.position.ReadValue();
        Vector3 world = GameManager.Cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 0f));
        Vector2 pos = new Vector2(world.x, world.y);
        CrosshairSpeed = (pos - CrosshairWorldPos).magnitude / Mathf.Max(Time.deltaTime, 0.0001f);
        CrosshairWorldPos = pos;
        crosshair.position = new Vector3(pos.x, pos.y, 0f);
        crosshair.gameObject.SetActive(GameManager.I.State == GameState.Playing);

        if (Reloading)
        {
            reloadLeft -= Time.deltaTime;
            UIManager.I.SetReload(1f - Mathf.Clamp01(reloadLeft / GameConfig.ReloadTime));
            if (reloadLeft <= 0f)
            {
                Reloading = false;
                Ammo = GameConfig.AmmoPerRound;
                UIManager.I.SetAmmo(Ammo);
                UIManager.I.SetReload(-1f);
            }
        }

        if (GameManager.I.State == GameState.Playing && mouse.leftButton.wasPressedThisFrame)
            FireAt(screen);
    }

    public void ResetAmmo()
    {
        Ammo = GameConfig.AmmoPerRound;
        Reloading = false;
        reloadLeft = 0f;
        UIManager.I.SetAmmo(Ammo);
        UIManager.I.SetReload(-1f);
    }

    public void FireAt(Vector2 screenPos)
    {
        if (Reloading || Ammo <= 0) return;

        Ammo--;
        UIManager.I.SetAmmo(Ammo);
        if (Ammo <= 0)
        {
            Reloading = true;
            reloadLeft = GameConfig.ReloadTime;
            UIManager.I.SetReload(0f);
        }

        // Thanh điểm che vịt: bắn vào UI thì đạn bị chặn.
        if (UIManager.I.ScoreBarContains(screenPos))
        {
            if (GameManager.I.CurrentHidingDuck != null)
            {
                GameManager.I.AddScore(GameConfig.UiPenalty);
                UIManager.I.ShakeScoreBar();
                UIManager.I.ShowNotification("Đừng bắn UI! " + GameConfig.UiPenalty, 1.2f);
            }
            return;
        }

        var hits = Physics2D.OverlapPointAll(CrosshairWorldPos);
        foreach (var hit in hits)
        {
            var duck = hit.GetComponent<Duck>();
            if (duck != null)
            {
                duck.Hit();
                return;
            }
        }

        // Bắn hụt nhưng tâm sát vịt -> vịt giả chết.
        foreach (var duck in GameManager.I.spawner.Ducks)
        {
            if (duck == null || duck.IsResolved || !duck.CanFakeDeath) continue;
            if (duck.State != DuckState.Flying && duck.State != DuckState.Dodging) continue;
            if (Vector2.Distance(duck.transform.position, CrosshairWorldPos) > GameConfig.NearMissRadius) continue;
            if (Random.value <= GameManager.I.Rules.FakeDeathChance)
            {
                duck.FakeDeath();
                return;
            }
        }
    }
}
