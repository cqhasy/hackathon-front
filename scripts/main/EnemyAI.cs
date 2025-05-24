using Godot;
using System;
using static Godot.TextServer;

public partial class EnemyAI : Area2D
{
    private readonly float BulletSpeed = 900f;
    private readonly float DestroyAnimation = 0.5f;
    private bool _inDestroyAnimation = false;
    private double _destroyTimer = 0;
    public int Id { get; set; }
    private Vector2 _direction = Vector2.Right;
    public Vector2 Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            // 更新朝向  
            if (_direction.LengthSquared() > 0.01f)
                Rotation = Mathf.Atan2(_direction.Y, _direction.X) - Mathf.Pi / 2;
        }
    }

    public Action<int> OnDestroyedByBullet { get; set; }

    public override void _Ready()
    {
        BodyEntered += OnEnemyHited;
        GD.Print("EventSetted");
    }

    public override void _Process(double delta)
    {
        if (_inDestroyAnimation)
            _destroyTimer += delta;
        if (_destroyTimer > DestroyAnimation)
        {
            QueueFree();
        }
    }

    public void OnEnemyHited(Node body)
    {
        GD.Print($"hited {body is Player}");
        if (body is Player player)
        {
            player.OnHitEnemy(Id);
            _inDestroyAnimation = true;
            AnimatedSprite2D animate = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
            animate.Play("Destroy");
            animate.Scale = new Vector2(1, 1);
        }
    }

    public void DestroySelfBecauseBullet()
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
