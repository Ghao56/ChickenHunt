using System.Collections.Generic;
using UnityEngine;

public class DuckSpawner : MonoBehaviour
{
    public readonly List<Duck> Ducks = new List<Duck>();

    LevelRules rules;
    int spawned;
    float timer;

    public void BeginLevel(LevelRules levelRules)
    {
        ClearAll();
        rules = levelRules;
        spawned = 0;
        timer = 0.6f;
    }

    void Update()
    {
        if (GameManager.I.State != GameState.Playing) return;

        Ducks.RemoveAll(d => d == null);
        timer -= Time.deltaTime;

        int alive = 0;
        foreach (var d in Ducks)
            if (d != null && !d.IsResolved) alive++;

        if (spawned < GameConfig.DucksPerLevel && alive < GameConfig.MaxDucksAlive && timer <= 0f)
            Spawn();
    }

    void Spawn()
    {
        spawned++;
        timer = Random.Range(0.9f, 1.5f);

        float x = Random.Range(PlayArea.MinX + 0.5f, PlayArea.MaxX - 0.5f);
        var pos = new Vector2(x, GameConfig.GrassTop + 0.3f);

        bool all = rules.Level >= 5;
        bool dodge = rules.CanDodge && (all || Random.value < 0.6f);
        bool fake = rules.CanFakeDeath && (all || Random.value < 0.6f);
        bool hide = rules.CanHide && (all || Random.value < 0.5f);

        Ducks.Add(Duck.Create(rules, pos, dodge, fake, hide));
    }

    public void ClearAll()
    {
        foreach (var d in Ducks)
            if (d != null) Destroy(d.gameObject);
        Ducks.Clear();
    }
}
