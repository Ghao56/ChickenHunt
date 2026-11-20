using UnityEngine;

public enum DuckState { Flying, Dodging, FakeDead, Hiding, Leaving, Falling, Escaped, Dead }

public class Duck : MonoBehaviour
{
    public DuckState State { get; private set; } = DuckState.Flying;
    public bool CanDodge, CanFakeDeath, CanHide;
    public bool IsResolved => State == DuckState.Dead || State == DuckState.Escaped;

    LevelRules rules;
    Vector2 velocity;
    float speed, age, stateTimer, attention, dodgeCooldown, turnTimer;
    float hideTimer, peekTimer, peekLeft;
    bool peeking;

    SpriteRenderer body, wing, beak;
    Transform eyeL, eyeR, pupilL, pupilR;
    GameObject xEyes, mark;
    Collider2D col;

    static readonly string[] SeeLines = { "Tôi thấy bạn rồi!", "Tâm ngắm kìa!", "Đừng có lia nhanh thế!" };
    static readonly string[] FakeLines = { "Tôi chết rồi. Thật đó.", "Đừng nhìn tôi.", "Chết hẳn rồi nhé." };
    static readonly string[] HideLines = { "Điểm số che tôi được!", "Bạn không bắn được UI đâu.", "Nấp ở đây là an toàn." };
    static readonly string[] HitLines = { "Ái... nhưng game này đâu có hồi sinh?" };

    public static Duck Create(LevelRules rules, Vector2 pos, bool dodge, bool fake, bool hide)
    {
        var go = new GameObject("Duck");
        go.transform.position = pos;
        var d = go.AddComponent<Duck>();
        d.rules = rules;
        d.CanDodge = dodge;
        d.CanFakeDeath = fake;
        d.CanHide = hide;
        d.Build();
        return d;
    }

    void Build()
    {
        var palette = new[]
        {
            new Color(0.55f, 0.36f, 0.22f), new Color(0.36f, 0.48f, 0.30f),
            new Color(0.70f, 0.55f, 0.28f), new Color(0.45f, 0.40f, 0.55f)
        };
        Color c = palette[Random.Range(0, palette.Length)];
        Color dark = c * 0.75f;

        body = SpriteFactory.AddSprite(transform, SpriteFactory.Circle, c, 2);
        body.transform.localScale = new Vector3(0.95f, 0.65f, 1f);

        wing = SpriteFactory.AddSprite(transform, SpriteFactory.Circle, dark, 3);
        wing.transform.localScale = new Vector3(0.45f, 0.30f, 1f);
        wing.transform.localPosition = new Vector3(-0.12f, 0f, 0f);

        beak = SpriteFactory.AddSprite(transform, SpriteFactory.Circle, new Color(0.95f, 0.62f, 0.15f), 4);
        beak.transform.localScale = new Vector3(0.24f, 0.15f, 1f);
        beak.transform.localPosition = new Vector3(0.46f, 0.02f, 0f);

        eyeL = Eye(new Vector3(0.17f, 0.15f, 0f), out pupilL);
        eyeR = Eye(new Vector3(0.36f, 0.15f, 0f), out pupilR);

        xEyes = new GameObject("x-eyes");
        xEyes.transform.SetParent(transform, false);
        MakeX(xEyes.transform, new Vector3(0.17f, 0.15f, 0f));
        MakeX(xEyes.transform, new Vector3(0.36f, 0.15f, 0f));
        xEyes.SetActive(false);

        mark = new GameObject("mark");
        mark.transform.SetParent(transform, false);
        mark.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        var bar = SpriteFactory.AddSprite(mark.transform, SpriteFactory.Square, Color.yellow, 6);
        bar.transform.localScale = new Vector3(0.07f, 0.22f, 1f);
        bar.transform.localPosition = new Vector3(0f, 0.1f, 0f);
        var dot = SpriteFactory.AddSprite(mark.transform, SpriteFactory.Square, Color.yellow, 6);
        dot.transform.localScale = new Vector3(0.07f, 0.07f, 1f);
        mark.SetActive(false);

        col = gameObject.AddComponent<CircleCollider2D>();
        ((CircleCollider2D)col).radius = 0.42f;

        speed = GameConfig.BaseDuckSpeed * rules.SpeedMultiplier * Random.Range(0.9f, 1.15f);
        velocity = new Vector2(Random.Range(-1f, 1f), Random.Range(0.15f, 1f)).normalized * speed;
        turnTimer = Random.Range(1.2f, 2.4f);
    }

    Transform Eye(Vector3 pos, out Transform pupilTransform)
    {
        var white = SpriteFactory.AddSprite(transform, SpriteFactory.Circle, Color.white, 4);
        white.transform.localScale = new Vector3(0.18f, 0.18f, 1f);
        white.transform.localPosition = pos;
        var pupil = SpriteFactory.AddSprite(white.transform, SpriteFactory.Circle, Color.black, 5);
        pupil.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        pupil.transform.localPosition = Vector3.zero;
        pupilTransform = pupil.transform;
        return white.transform;
    }

    void MakeX(Transform parent, Vector3 pos)
    {
        var root = new GameObject("x");
        root.transform.SetParent(parent, false);
        root.transform.localPosition = pos;
        for (int i = 0; i < 2; i++)
        {
            var bar = SpriteFactory.AddSprite(root.transform, SpriteFactory.Square, Color.black, 6);
            bar.transform.localScale = new Vector3(0.05f, 0.20f, 1f);
            bar.transform.localRotation = Quaternion.Euler(0, 0, i == 0 ? 45f : -45f);
        }
    }

    void Update()
    {
        if (State == DuckState.Dead || State == DuckState.Escaped) return;
        float dt = Time.deltaTime;
        age += dt;
        dodgeCooldown -= dt;

        switch (State)
        {
            case DuckState.Flying: UpdateFlying(dt); break;
            case DuckState.Dodging: UpdateDodging(dt); break;
            case DuckState.FakeDead: UpdateFakeDead(dt); break;
            case DuckState.Hiding: UpdateHiding(dt); break;
            case DuckState.Leaving: UpdateLeaving(dt); break;
            case DuckState.Falling: UpdateFalling(dt); break;
        }

        if (State != DuckState.Falling && State != DuckState.Leaving && age >= rules.DuckLifetime) BeginEscape();

        // Không nảy ở mép trên: bay ra khỏi màn hình là thoát.
        if (State != DuckState.Falling && transform.position.y > GameManager.Cam.ViewportToWorldPoint(new Vector3(0f, 1f, 0f)).y + 1.2f)
            FinishEscape();
    }

    void UpdateFlying(float dt)
    {
        Move(dt);

        turnTimer -= dt;
        if (turnTimer <= 0f)
        {
            turnTimer = Random.Range(1.2f, 2.4f);
            velocity = Rotate(velocity, Random.Range(-40f, 40f));
        }

        if (CanDodge && dodgeCooldown <= 0f)
        {
            Vector2 toCross = PlayerShooter.CrosshairWorldPos - (Vector2)transform.position;
            float dist = toCross.magnitude;
            bool watching = dist < rules.SeeRadius && PlayerShooter.CrosshairSpeed > 0.5f;
            if (watching)
            {
                attention += dt;
                LookAt(toCross);
                mark.SetActive(true);
                if (attention >= rules.ReactionTime) Dodge(toCross);
            }
            else
            {
                attention = 0f;
                mark.SetActive(false);
                LookAt(velocity);
            }
        }

        if (CanHide && State == DuckState.Flying && age > 1.5f &&
            age < rules.DuckLifetime - 2.5f && GameManager.I.CurrentHidingDuck == null &&
            Random.value < dt * 0.45f)
            EnterHiding();
    }

    void UpdateDodging(float dt)
    {
        Move(dt);
        stateTimer -= dt;
        if (stateTimer <= 0f)
        {
            State = DuckState.Flying;
            velocity = velocity.normalized * speed;
            dodgeCooldown = GameConfig.DodgeCooldown;
        }
    }

    void Dodge(Vector2 toCross)
    {
        Vector2 side = Random.value < 0.5f
            ? new Vector2(-toCross.y, toCross.x)
            : new Vector2(toCross.y, -toCross.x);
        velocity = side.normalized * speed * GameConfig.DodgeSpeedMultiplier;
        State = DuckState.Dodging;
        stateTimer = GameConfig.DodgeDuration;
        attention = 0f;
        mark.SetActive(false);
        Say(SeeLines);
    }

    void UpdateFakeDead(float dt)
    {
        float landY = GameConfig.GrassTop + PlayArea.Height * 0.2f;
        velocity.y -= 12f * dt;
        transform.position += (Vector3)(velocity * dt);
        if (transform.position.y <= landY)
        {
            transform.position = new Vector3(transform.position.x, landY, 0f);
            velocity = Vector2.zero;
            stateTimer += dt;
            if (stateTimer >= GameConfig.FakeDeadDuration) WakeUp();
        }
    }

    void WakeUp()
    {
        State = DuckState.Flying;
        xEyes.SetActive(false);
        SetEyesVisible(true);
        UIManager.I.ShowBubble("...hehe", transform, 1.2f);
        velocity = new Vector2(Random.Range(-0.6f, 0.6f), 1f).normalized * speed;
        turnTimer = 1.5f;
    }

    void UpdateHiding(float dt)
    {
        Rect bar = UIManager.I.ScoreBarWorldRect;
        Vector3 park = new Vector3(bar.center.x, bar.center.y - 0.1f, 0f);
        Vector3 peek = new Vector3(bar.center.x, bar.yMin - 0.55f, 0f);

        hideTimer += dt;
        peekTimer += dt;
        if (!peeking && peekTimer >= GameConfig.PeekInterval)
        {
            peeking = true;
            peekTimer = 0f;
            peekLeft = GameConfig.PeekDuration;
        }
        if (peeking)
        {
            peekLeft -= dt;
            if (peekLeft <= 0f) peeking = false;
        }

        Vector3 target = peeking ? peek : park;
        transform.position = Vector3.MoveTowards(transform.position, target, speed * 1.4f * dt);

        if (hideTimer >= GameConfig.HideDuration) ExitHiding();
    }

    void EnterHiding()
    {
        State = DuckState.Hiding;
        GameManager.I.CurrentHidingDuck = this;
        hideTimer = 0f;
        peekTimer = 0f;
        peeking = false;
        mark.SetActive(false);
        Say(HideLines);
    }

    void ExitHiding()
    {
        State = DuckState.Flying;
        if (GameManager.I.CurrentHidingDuck == this) GameManager.I.CurrentHidingDuck = null;
        velocity = new Vector2(Random.Range(-1f, 1f), -0.2f).normalized * speed;
        turnTimer = 1.5f;
    }

    void UpdateFalling(float dt)
    {
        velocity.y -= 14f * dt;
        transform.position += (Vector3)(velocity * dt);
        transform.Rotate(0, 0, 220f * dt);
        if (transform.position.y < GameConfig.GrassTop - 1f) Destroy(gameObject);
    }

    void Move(float dt)
    {
        transform.position += (Vector3)(velocity * dt);
        Vector3 p = transform.position;
        if (p.x < PlayArea.MinX) { p.x = PlayArea.MinX; velocity.x = Mathf.Abs(velocity.x); }
        if (p.x > PlayArea.MaxX) { p.x = PlayArea.MaxX; velocity.x = -Mathf.Abs(velocity.x); }
        if (p.y < PlayArea.FlightFloor) { p.y = PlayArea.FlightFloor; velocity.y = Mathf.Abs(velocity.y); }
        transform.position = p;
    }

    void LookAt(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f) return;
        dir.Normalize();
        Vector2 offset = dir * 0.035f;
        pupilL.localPosition = offset;
        pupilR.localPosition = offset;
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cs = Mathf.Cos(rad), sn = Mathf.Sin(rad);
        return new Vector2(v.x * cs - v.y * sn, v.x * sn + v.y * cs).normalized * v.magnitude;
    }

    void SetEyesVisible(bool on)
    {
        eyeL.gameObject.SetActive(on);
        eyeR.gameObject.SetActive(on);
    }

    public void Hit()
    {
        if (State == DuckState.Dead || State == DuckState.Escaped || State == DuckState.Falling || State == DuckState.Leaving) return;

        if (State == DuckState.FakeDead)
        {
            Catch();
            return;
        }

        if (State == DuckState.Hiding && GameManager.I.CurrentHidingDuck == this)
            GameManager.I.CurrentHidingDuck = null;

        State = DuckState.Falling;
        GameManager.I.AddScore(GameConfig.HitScore);
        UIManager.I.ShowPopup("+" + GameConfig.HitScore, transform.position, Color.white);
        Fx.Feathers(transform.position, body.color);
        Say(HitLines);
        velocity = new Vector2(Random.Range(-1.5f, 1.5f), 1.5f);
        GameManager.I.OnDuckResolved(false);
        Destroy(gameObject, 1.6f);
    }

    void Catch()
    {
        State = DuckState.Dead;
        GameManager.I.AddScore(GameConfig.CatchScore);
        UIManager.I.ShowPopup("Bắt quả tang! +" + GameConfig.CatchScore, transform.position, new Color(1f, 0.85f, 0.2f));
        Fx.Feathers(transform.position, body.color);
        GameManager.I.OnDuckResolved(false);
        Destroy(gameObject, 0.05f);
    }

    public void FakeDeath()
    {
        if (State != DuckState.Flying && State != DuckState.Dodging) return;
        State = DuckState.FakeDead;
        stateTimer = 0f;
        velocity = new Vector2(0f, -1.5f);
        xEyes.SetActive(true);
        SetEyesVisible(false);
        mark.SetActive(false);
        Say(FakeLines);
    }

    void BeginEscape()
    {
        if (IsResolved || State == DuckState.Falling) return;
        if (GameManager.I.CurrentHidingDuck == this) GameManager.I.CurrentHidingDuck = null;
        State = DuckState.Leaving;
        xEyes.SetActive(false);
        SetEyesVisible(true);
        mark.SetActive(false);
        // Bay thẳng ra khỏi màn hình rồi mới tính là thoát.
        velocity = new Vector2(Random.Range(-0.35f, 0.35f), 1f).normalized * speed * 1.6f;
    }

    void UpdateLeaving(float dt)
    {
        transform.position += (Vector3)(velocity * dt);
    }

    void FinishEscape()
    {
        State = DuckState.Escaped;
        GameManager.I.OnDuckResolved(true);
        Destroy(gameObject);
    }

    void Say(string[] lines)
    {
        UIManager.I.ShowBubble(lines[Random.Range(0, lines.Length)], transform, 1.8f);
    }
}
