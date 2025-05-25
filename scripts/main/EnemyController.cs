using fiycraft.scripts.main;
using Godot;
using System.Collections.Generic;

public partial class EnemyController : Node2D
{
    private IEnemyData _enemyData = new LocalEnemyData();
    private Dictionary<int, EnemyAI> _enemyInstances = new();

    public override void _Process(double delta)
    {
        var player = GetParent().GetNode<Player>("Player");
        if (player == null) return;

        _enemyData.Update((float)delta, player.GlobalPosition);

        // 同步数据到Enemy实例
        foreach (var kv in _enemyData.Enemies)
        {
            int id = kv.Key;
            var info = kv.Value;

            if (info.ToDestroy)
            {
                if (_enemyInstances.ContainsKey(id))
                {
                    _enemyInstances[id].QueueFree();
                    _enemyInstances.Remove(id);
                }
                continue;
            }

            if (!_enemyInstances.ContainsKey(id))
            {
                var enemyScene = GD.Load<PackedScene>("res://prefbs/enemy.tscn");
                var enemy = enemyScene.Instantiate<EnemyAI>();
                AddChild(enemy);
                _enemyInstances[id] = enemy;
                enemy.Id = id;
                enemy.OnDestroyedByBullet = OnEnemyDestroyedByBullet; // 传递回调
            }

            var inst = _enemyInstances[id];
            inst.GlobalPosition = info.Position;
            inst.Direction = info.Direction;

            if (info.ShouldFire)
                inst.FireAt(player.GlobalPosition);
        }

        // 清理多余实例
        var idsToRemove = new List<int>();
        foreach (var id in _enemyInstances.Keys)
        {
            if (!(_enemyData as LocalEnemyData).Enemies.ContainsKey(id))
            {
                _enemyInstances[id].QueueFree();
                idsToRemove.Add(id);
            }
        }
        foreach (var id in idsToRemove)
            _enemyInstances.Remove(id);
    }

    public void OnEnemyDestroyedByBullet(int Id)
    {
        _enemyInstances.Remove(Id);
        _enemyData.DestroyEnemy(Id);
    }

    public void PlayerDestroyEnemy(int Id)
    {
        _enemyInstances.Remove(Id);
        _enemyData.DestroyEnemy(Id);
    }
    public EnemyAI FindNearestEnemy(Vector2 position, float maxRange)
    {
        EnemyAI nearest = null;
        float minDist = maxRange;
        foreach (var enemy in _enemyInstances.Values)
        {
            if (enemy == null || !enemy.IsInsideTree()) continue;
            float dist = enemy.GlobalPosition.DistanceTo(position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        return nearest;
    }

    public void SetEnemyCount(int count)
    {
        _enemyData.SetEnemyCount(count);
    }
}
