using UnityEngine;

public class Feather : MonoBehaviour
{
    public Vector2 velocity;
    public SpriteRenderer sr;
    float life = 0.9f;

    void Update()
    {
        float dt = Time.deltaTime;
        velocity.y -= 9f * dt;
        transform.position += (Vector3)(velocity * dt);
        transform.Rotate(0, 0, 200f * dt);
        life -= dt;
        Color c = sr.color;
        c.a = Mathf.Clamp01(life / 0.9f);
        sr.color = c;
        if (life <= 0f) Destroy(gameObject);
    }
}

public static class Fx
{
    public static void Feathers(Vector3 pos, Color color)
    {
        for (int i = 0; i < 8; i++)
        {
            var go = new GameObject("Feather");
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 0.12f;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Square;
            sr.color = color;
            sr.sortingOrder = 8;
            var f = go.AddComponent<Feather>();
            f.sr = sr;
            f.velocity = Random.insideUnitCircle.normalized * Random.Range(1.5f, 3.5f) + Vector2.up * 2f;
        }
    }
}
