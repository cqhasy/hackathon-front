using Godot;
using System;

public partial class EnemyAI : CharacterBody2D
{
    private readonly float BulletSpeed = 900f;
    public int Id { get; set; }
    public Vector2 Direction { get; set; }
    public Action<int> OnDestroyedByBullet { get; set; }

    public void DestroySelf()
    {
        OnDestroyedByBullet?.Invoke(Id);
        QueueFree();
    }
    public void FireAt(Vector2 target)
    {
        var bulletScene = GD.Load<PackedScene>("res://prefbs/bullet.tscn");
        var bullet = bulletScene.Instantiate<BulletControl>();
        bullet.GlobalPosition = GlobalPosition;
        var dir = (target - GlobalPosition).Normalized();
        bullet.Velocity = dir * BulletSpeed;
        GetTree().CurrentScene.AddChild(bullet);
    }

}
