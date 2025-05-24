using System.Collections.Generic;
using fiycraft.scripts.main;
using Godot;

public class LocalEnemyData : IEnemyData
{
    private readonly float _borderWidth = 3180;
    private readonly float _borderHeight = 2160;
    public int NextEnemyId => _enemyId++;
    private int _enemyId = 0;
    public Dictionary<int, EnemyInfo> Enemies { get; private set; } = [];

    public LocalEnemyData()
    {
        for (int i = 0; i < 20; i++)
        {
            Enemies[i] = new EnemyInfo
            {
                Id = i,
                Position = new Vector2(GD.RandRange(0, 3840), GD.RandRange(0, 2160)),
                Direction = Vector2.Right,
                State = EnemyState.Wander,
                FireCooldown = 0,
                ToDestroy = false,
            };
        }
    }

    private Vector2 GetRandomPositionWithDistance(Vector2 playerPos, float minDistance)
    {
        Vector2 pos;
        int tryCount = 0;
        do
        {
            pos = new Vector2(
                (float)GD.RandRange(0, _borderWidth),
                (float)GD.RandRange(0, _borderHeight)
            );
            tryCount++;
            // 防止极端情况下死循环
            if (tryCount > 100) break;
        } while (pos.DistanceTo(playerPos) < minDistance);
        return pos;
    }

    public void Update(float delta, Vector2 playerPos)
    {
        while (Enemies.Count < 10)
        {
            int id = NextEnemyId;
            Vector2 pos = GetRandomPositionWithDistance(playerPos, 400f); // 400为最小间隔
            Enemies[id] = new EnemyInfo
            {
                Id = id,
                Position = pos,
                Direction = Vector2.Right,
                State = EnemyState.Wander,
                FireCooldown = 0,
                ToDestroy = false,
            };
        }

        foreach (var enemy in Enemies.Values)
        {
            if (enemy.ToDestroy)
                continue;

            float dist = enemy.Position.DistanceTo(playerPos);
            if (dist < enemy.DetectRadius)
            {
                // 见到玩家，后退并准备发射
                enemy.Direction = (enemy.Position - playerPos).Normalized();
                enemy.State = EnemyState.Flee;
                enemy.FireCooldown -= delta;
                if (enemy.FireCooldown <= 0)
                {
                    enemy.ShouldFire = true;
                    enemy.FireCooldown = enemy.FireInterval;
                }
                else
                {
                    enemy.ShouldFire = false;
                }
            }
            else
            {
                // 游走
                enemy.State = EnemyState.Wander;
                enemy.WanderTimer -= delta;
                if (enemy.WanderTimer <= 0)
                {
                    enemy.Direction = Vector2.FromAngle(GD.Randf() * Mathf.Tau);
                    enemy.WanderTimer = (float)GD.RandRange(0.5f, 2f);
                }
                enemy.ShouldFire = false;
            }

            // 位置更新
            float speed =
                (enemy.State == EnemyState.Flee)
                    ? enemy.MaxAcceleration * 0.5f
                    : enemy.MaxAcceleration;
            enemy.Position += enemy.Direction * speed * delta;

            // 边界穿越
            if (enemy.Position.X < -50)
                enemy.Position = new Vector2(_borderWidth, enemy.Position.Y);
            if (enemy.Position.X > _borderWidth + 50)
                enemy.Position = new Vector2(0, enemy.Position.Y);
            if (enemy.Position.Y < -50)
                enemy.Position = new Vector2(enemy.Position.X, _borderHeight);
            if (enemy.Position.Y > _borderHeight + 50)
                enemy.Position = new Vector2(enemy.Position.X, 0);
        }
    }


    public void MarkToDestroy(int id)
    {
        if (Enemies.ContainsKey(id))
            Enemies[id].ToDestroy = true;
    }

    public void DestroyEnemy(int Id)
    {
        GD.Print($"Enemy {Id} destroyed by bullet");
        if(!Enemies.ContainsKey(Id))
            { return; }
        Enemies.Remove(Id);
    }
}

public class EnemyInfo
{
    public int Id;
    public Vector2 Position;
    public Vector2 Direction;
    public EnemyState State;
    public float MaxAcceleration = 200f;
    public float FireInterval = 0.8f;
    public float FireCooldown = 0f;
    public float DetectRadius = 400f;
    public float WanderTimer = 0f;
    public bool ShouldFire = false;
    public bool ToDestroy = false;
}

public enum EnemyState
{
    Wander,
    Flee,
}
