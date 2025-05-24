using Godot;
using System;

public partial class BulletControl : Area2D
{
    [Export]
    public Vector2 Velocity { get; set; } = Vector2.Zero;

    private float _lifeTime = 0f;
    private const float MaxLifeTime = 10f;
    private const int MapWidth = 3840;
    private const int MapHeight = 2160;

    private CollisionObject2D collision;
    public override void _Ready()
    {
        // 设置朝向
        if (Velocity.LengthSquared() > 0.01f)
            Rotation = Mathf.Atan2(Velocity.Y, Velocity.X) + Mathf.Pi / 2;
        BodyEntered += OnBodyEntered;
    }

    private void OnBodyEntered(Node body)
    {
        if (body is Player player)
        {
            // 调用Player的方法（如受伤或销毁等）
            player.OnHitByBullet();

            // 调用Bullet自身的方法（如销毁）
            DestroySelf();
        }else if(body is EnemyAI enemy && _lifeTime > 1)
        {
            enemy.DestroySelfBecauseBullet();
            DestroySelf();
        }
    }

    public void DestroySelf()
    {
        // 这里可以做特效、音效等
        QueueFree();
    }

    public override void _Process(double delta)
    {
        // 移动
        Position += Velocity * (float)delta;

        // 边界穿越
        Vector2 pos = Position;
        if (pos.X < 0)
            pos.X = MapWidth;
        else if (pos.X > MapWidth)
            pos.X = 0;
        if (pos.Y < 0)
            pos.Y = MapHeight;
        else if (pos.Y > MapHeight)
            pos.Y = 0;
        Position = pos;

        // 计时销毁
        _lifeTime += (float)delta;
        if (_lifeTime > MaxLifeTime)
            QueueFree();
    }
}
