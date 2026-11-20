using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager I;
    public RectTransform ScoreBar { get; private set; }

    Canvas canvas;
    RectTransform root;
    Font font;

    Text scoreText, levelText, notifyText;
    readonly Image[] ammo = new Image[GameConfig.AmmoPerRound];
    Image reloadBg, reloadFill;

    RectTransform bubbleRect;
    Text bubbleText;
    Transform bubbleTarget;
    float bubbleLeft;

    GameObject startPanel, overPanel;
    Text overText;

    Vector2 barHome;
    float shakeLeft, notifyLeft;

    void Awake()
    {
        I = this;
        Build();
    }

    void Build()
    {
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        var canvasGo = new GameObject("Canvas", typeof(RectTransform));
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        root = canvasGo.GetComponent<RectTransform>();

        // Thanh điểm số (vật cản che vịt).
        var bar = NewImage(root, new Color(0.05f, 0.09f, 0.16f, 0.96f));
        ScoreBar = bar.rectTransform;
        ScoreBar.anchorMin = ScoreBar.anchorMax = new Vector2(0f, 1f);
        ScoreBar.pivot = new Vector2(0f, 1f);
        ScoreBar.anchoredPosition = new Vector2(16f, -16f);
        ScoreBar.sizeDelta = new Vector2(700f, 88f);
        barHome = ScoreBar.anchoredPosition;

        scoreText = NewText(ScoreBar, "SCORE 0", 34, TextAnchor.MiddleLeft, Color.white);
        Stretch(scoreText.rectTransform, 28f, 0f, 0.55f, 1f);

        levelText = NewText(ScoreBar, "MÀN 1   THOÁT 0/3", 30, TextAnchor.MiddleRight, new Color(1f, 0.85f, 0.35f));
        Stretch(levelText.rectTransform, 0f, 24f, 1f, 1f);

        // Đạn + thanh nạp.
        for (int i = 0; i < ammo.Length; i++)
        {
            var icon = NewImage(root, new Color(1f, 0.82f, 0.2f));
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0f, 0f);
            icon.rectTransform.pivot = new Vector2(0f, 0f);
            icon.rectTransform.anchoredPosition = new Vector2(28f + i * 40f, 26f);
            icon.rectTransform.sizeDelta = new Vector2(26f, 46f);
            ammo[i] = icon;
        }

        reloadBg = NewImage(root, new Color(0f, 0f, 0f, 0.55f));
        reloadBg.rectTransform.anchorMin = reloadBg.rectTransform.anchorMax = new Vector2(0f, 0f);
        reloadBg.rectTransform.pivot = new Vector2(0f, 0f);
        reloadBg.rectTransform.anchoredPosition = new Vector2(28f, 84f);
        reloadBg.rectTransform.sizeDelta = new Vector2(150f, 12f);

        reloadFill = NewImage(reloadBg.rectTransform, new Color(1f, 0.85f, 0.3f));
        reloadFill.rectTransform.anchorMin = Vector2.zero;
        reloadFill.rectTransform.anchorMax = Vector2.one;
        reloadFill.rectTransform.offsetMin = reloadFill.rectTransform.offsetMax = Vector2.zero;
        reloadFill.type = Image.Type.Filled;
        reloadFill.fillMethod = Image.FillMethod.Horizontal;
        reloadFill.fillAmount = 0f;
        reloadBg.gameObject.SetActive(false);

        // Thông báo giữa màn hình.
        notifyText = NewText(root, "", 40, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.35f));
        var nr = notifyText.rectTransform;
        nr.anchorMin = nr.anchorMax = new Vector2(0.5f, 1f);
        nr.pivot = new Vector2(0.5f, 1f);
        nr.anchoredPosition = new Vector2(0f, -150f);
        nr.sizeDelta = new Vector2(1200f, 70f);
        var outline = notifyText.gameObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);
        notifyText.gameObject.SetActive(false);

        // Bong bóng thoại.
        var bubbleBg = NewImage(root, new Color(1f, 1f, 1f, 0.92f));
        bubbleRect = bubbleBg.rectTransform;
        bubbleRect.anchorMin = bubbleRect.anchorMax = new Vector2(0.5f, 0.5f);
        bubbleRect.pivot = new Vector2(0.5f, 0f);
        bubbleRect.sizeDelta = new Vector2(380f, 64f);
        bubbleText = NewText(bubbleRect, "", 24, TextAnchor.MiddleCenter, new Color(0.12f, 0.1f, 0.08f));
        bubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        Stretch(bubbleText.rectTransform, 10f, 6f, 1f, 1f);
        bubbleBg.gameObject.SetActive(false);

        BuildStartPanel();
        BuildOverPanel();
    }

    void BuildStartPanel()
    {
        startPanel = NewImage(root, new Color(0f, 0f, 0f, 0.72f)).gameObject;
        Stretch(startPanel.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);

        var title = NewText(startPanel.transform, "DUCK HUNT\n— THE FOURTH WALL —", 72, TextAnchor.MiddleCenter, Color.white);
        var tr = title.rectTransform;
        tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
        tr.sizeDelta = new Vector2(1400f, 260f);
        tr.anchoredPosition = new Vector2(0f, 120f);

        var hint = NewText(startPanel.transform, "Click để chơi\n2 viên đạn · hết đạn tự nạp", 32, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
        var hr = hint.rectTransform;
        hr.anchorMin = hr.anchorMax = new Vector2(0.5f, 0.5f);
        hr.sizeDelta = new Vector2(1000f, 120f);
        hr.anchoredPosition = new Vector2(0f, -80f);
    }

    void BuildOverPanel()
    {
        overPanel = NewImage(root, new Color(0f, 0f, 0f, 0.8f)).gameObject;
        Stretch(overPanel.GetComponent<RectTransform>(), 0f, 0f, 1f, 1f);

        overText = NewText(overPanel.transform, "", 54, TextAnchor.MiddleCenter, Color.white);
        var r = overText.rectTransform;
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(1200f, 400f);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (shakeLeft > 0f)
        {
            shakeLeft -= dt;
            ScoreBar.anchoredPosition = barHome + Random.insideUnitCircle * 10f;
        }
        else ScoreBar.anchoredPosition = barHome;

        if (notifyLeft > 0f)
        {
            notifyLeft -= dt;
            if (notifyLeft <= 0f) notifyText.gameObject.SetActive(false);
        }

        if (bubbleLeft > 0f)
        {
            bubbleLeft -= dt;
            if (bubbleLeft <= 0f || bubbleTarget == null)
                bubbleRect.gameObject.SetActive(false);
            else
                PlaceAtWorld(bubbleRect, bubbleTarget.position + Vector3.up * 1.0f);
        }
    }

    public void ShowStart()
    {
        startPanel.SetActive(true);
        overPanel.SetActive(false);
    }

    public void ShowHud()
    {
        startPanel.SetActive(false);
        overPanel.SetActive(false);
    }

    public void ShowGameOver(int score, int best)
    {
        overPanel.SetActive(true);
        overText.text = "GAME OVER\n\nĐiểm: " + score + "    Cao nhất: " + best + "\n\nClick hoặc R để chơi lại";
    }

    public void SetHud(int score, int level, int escaped)
    {
        scoreText.text = "SCORE " + score;
        levelText.text = "MÀN " + level + "   THOÁT " + escaped + "/" + GameConfig.EscapesToLose;
    }

    public void SetAmmo(int count)
    {
        for (int i = 0; i < ammo.Length; i++)
            ammo[i].gameObject.SetActive(i < count);
    }

    public void SetReload(float t)
    {
        bool on = t >= 0f;
        reloadBg.gameObject.SetActive(on);
        if (on) reloadFill.fillAmount = t;
    }

    public void ShowNotification(string text, float duration)
    {
        notifyText.text = text;
        notifyText.gameObject.SetActive(true);
        notifyLeft = duration;
    }

    public void ShowBubble(string text, Transform target, float duration)
    {
        bubbleText.text = text;
        bubbleTarget = target;
        bubbleLeft = duration;
        bubbleRect.gameObject.SetActive(true);
        PlaceAtWorld(bubbleRect, target.position + Vector3.up * 1.0f);
    }

    public void ShowPopup(string text, Vector3 worldPos, Color color)
    {
        var t = NewText(root, text, 34, TextAnchor.MiddleCenter, color);
        t.rectTransform.sizeDelta = new Vector2(600f, 60f);
        var shadow = t.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shadow.effectDistance = new Vector2(2f, -2f);
        StartCoroutine(PopupRoutine(t, worldPos, color));
    }

    IEnumerator PopupRoutine(Text t, Vector3 worldPos, Color color)
    {
        const float dur = 0.9f;
        float e = 0f;
        while (e < dur)
        {
            e += Time.deltaTime;
            PlaceAtWorld(t.rectTransform, worldPos + Vector3.up * (0.4f + e * 0.9f));
            Color c = color;
            c.a = 1f - e / dur;
            t.color = c;
            yield return null;
        }
        Destroy(t.gameObject);
    }

    public void ShakeScoreBar() { shakeLeft = 0.4f; }

    public bool ScoreBarContains(Vector2 screenPos)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(ScoreBar, screenPos, null);
    }

    public Rect ScoreBarWorldRect
    {
        get
        {
            var corners = new Vector3[4];
            ScoreBar.GetWorldCorners(corners);
            Vector2 a = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
            Vector2 b = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
            Vector3 wa = GameManager.Cam.ScreenToWorldPoint(new Vector3(a.x, a.y, 0f));
            Vector3 wb = GameManager.Cam.ScreenToWorldPoint(new Vector3(b.x, b.y, 0f));
            return Rect.MinMaxRect(wa.x, wa.y, wb.x, wb.y);
        }
    }

    void PlaceAtWorld(RectTransform rt, Vector3 world)
    {
        Vector2 screen = GameManager.Cam.WorldToScreenPoint(world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screen, null, out Vector2 local);
        rt.anchoredPosition = local;
    }

    Image NewImage(Transform parent, Color color)
    {
        var go = new GameObject("img", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    Text NewText(Transform parent, string text, int size, TextAnchor anchor, Color color)
    {
        var go = new GameObject("text", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = font;
        t.text = text;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = color;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    static void Stretch(RectTransform rt, float left, float bottom, float right, float top)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }
}
