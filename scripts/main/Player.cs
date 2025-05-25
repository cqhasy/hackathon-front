using System;
using System.Collections.Generic;
using System.Linq;
using fiycraft.scripts.main;
using Godot;

public partial class Player : CharacterBody2D
{
    private const float MaxAcceleration = 500f;
    private const float MinAcceleration = 30f;
    private const float BasicMaxSpeed = 800f;
    private const float DashVelocity = 1600f;
    private const int MapWidth = 3840;
    private const int MapHeight = 2160;
    private const float DashTime = 0.4f;
    private const float HurtTime = 0.4f;
    private const float DeathTime = 1f;
    private const float TotalTime = 20;

    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _acceleration = Vector2.Zero;
    private Vector2 _lastUsableMouseMovement = Vector2.Zero;
    private bool _inDashMode = false;
    private bool _inHurtMode = false;
    private bool _inDeathMode = false;
    private double _dashingTime = 0;
    private double _recoverTimer = 0;
    private double _hurtTimer = 0;
    private double _deathTimer = 0;
    private double _passedTime = 0;

    //private bool dashingWaitingForSettingDirection = false;

    private MouseMovement _mouseMovement;
    private TimeController _timeController;
    private PlayerItems _playerItems;
    private PlayerAnimation _playerAnimation;
    private Node2D _borderArea;
    private KeyboardListener _keyboardListener;
    private Node2D _shadowContainer;
    private AnimatedSprite2D _healthBar;
    private EnemyController _enemyController;
    private Sprite2D _arrow;
    private AnimatedSprite2D _target;
    private Label[] _labels;
    public float Sensitivity { get; set; } = 1;

    public override void _Ready()
    {
        _mouseMovement = GetParent().GetNode<MouseMovement>("MouseMovement");
        _timeController = GetParent().GetNode<TimeController>("TimeController");
        _playerItems = new PlayerItems();
        _playerAnimation = GetNode<PlayerAnimation>("PlayerAnimation");
        _playerAnimation.PlayShield();
        _playerAnimation.Rotation = Mathf.Pi / 2;
        _borderArea = GetParent().GetNode<Node2D>("MapBorder");
        foreach (var x in new List<String>(["Top", "Bottom", "Left", "Right"]))
        {
            var border = _borderArea.GetNode<Area2D>(x);
            border.BodyEntered += (Node2D body) => OnMapBorderAreaEntered(body, border.Name);
        }
        _keyboardListener = GetParent().GetNode<KeyboardListener>("KeyboardListener");
        _keyboardListener.CtrlPressed += OnCtrlPressed;
        _keyboardListener.SpacePressed += OnSpacePressed;
        _shadowContainer = GetNode<Node2D>("ShadowContainer");
        _healthBar = GetParent()
            .GetNode<Camera2D>("Camera2D")
            .GetNode<AnimatedSprite2D>("HealthBar");
        UpdateHealthBar();
        _enemyController = GetParent().GetNode<EnemyController>("EnemyGroup");
        _arrow = GetNode<Sprite2D>("Arrow");
        _target = GetParent().GetNode<AnimatedSprite2D>("Target");
        _target.Visible = PlayerStates.Instance.UseActiveTrace;
        var camera = GetParent().GetNode<Camera2D>("Camera2D");
        _labels =
        [
            .. new List<String>(["Dash", "Shield", "SlowTime", "LifeRecover", "Time", "Level"]).Select(x =>
                camera.GetNode<Label>(x)
            ),
        ];
        InitPlayerStates();
    }
    private void InitPlayerStates()
    {
        PlayerStates.Instance.Health = 100;
        _playerItems.ItemsCD["SlowMode"] = PlayerStates.Instance.SlowDownCostTime;
        _playerItems.ItemsCD["Shield"] = PlayerStates.Instance.ShieldCostTime;
        _target.Visible = PlayerStates.Instance.UseActiveTrace;
        _enemyController.SetEnemyCount(PlayerStates.Instance.level++ * 3 + 10);
    }
    public override void _Process(double delta)
    {
        if (_inDeathMode)
        {
            _mouseMovement.ToNormalMode();
            UpdateDeathState(delta);
            return;
        }
        VelocityCalculate(delta);
        UpdateItemCD(delta);
        if (_inDashMode)
            UpdateDashState(delta);
        if (_inDashMode)
        {
            CreateShadow();
        }
        if (_inHurtMode)
        {
            UpdateHurtState(delta);
        }
        DrawArrow();
        DoHealthRecover(delta);
        if (PlayerStates.Instance.UseActiveTrace)
            DrawTarget();
        UpdateItemInfo(delta);
    }

    public void UpdateItemInfo(double delta)
    {
        _labels[0].Text =
            _playerItems.CurrentItemsCD["Dash"] == 0
                ? "充能完成"
                : _playerItems.CurrentItemsCD["Dash"].ToString("F3");
        _labels[1].Text =
            _playerItems.CurrentItemsCD["Shield"] == 0
                ? "充能完成"
                : _playerItems.CurrentItemsCD["Shield"].ToString("F3");
        _labels[2].Text =
            _playerItems.CurrentItemsCD["SlowMode"] == 0
                ? "充能完成"
                : _playerItems.CurrentItemsCD["SlowMode"].ToString("F3");
        _labels[3].Text =
            PlayerStates.Instance.Health == PlayerStates.Instance.MaxHealth
                ? "无需恢复"
                : (1 - _recoverTimer).ToString("F3");
        _passedTime += delta;
        double leftTime = Math.Max(0, TotalTime - _passedTime);
        int minutes = (int)(leftTime / 60);
        int seconds = (int)(leftTime % 60);
        _labels[4].Text = $"{minutes:D2}:{seconds:D2}";
        _labels[5].Text = $"第{PlayerStates.Instance.level}关";
        if (leftTime == 0)
        {
            _mouseMovement.ToNormalMode();
            PlayerStates.Instance.CurrentMoney = (int)((float)PlayerStates.Instance.DestroyedEnemies * 17.3) + 50;
            GetTree().ChangeSceneToFile("res://scenes/store.tscn");
        }
    }

    private void DrawTarget()
    {
        float searchRange = 500f;
        EnemyAI nearestEnemy = _enemyController.FindNearestEnemy(GlobalPosition, searchRange);
        GD.Print($" enemy: {nearestEnemy is not null}");

        if (nearestEnemy is not null)
        {
            _target.GlobalPosition = nearestEnemy.Position;
        }
    }

    private void DrawArrow()
    {
        if (_lastUsableMouseMovement.LengthSquared() > 0.01f)
        {
            // 计算世界坐标下的目标点
            Vector2 world_point = GlobalPosition + _lastUsableMouseMovement.Normalized() * 50f;
            // 计算世界角度
            float angle = Mathf.Atan2(
                world_point.Y - GlobalPosition.Y,
                world_point.X - GlobalPosition.X
            );
            // 赋值到 _arrow
            _arrow.GlobalRotation = angle + Mathf.Pi / 2;
            _arrow.Visible = true;
        }
        else
        {
            _arrow.Visible = false;
        }
    }

    public void OnHitEnemy(int enemyId)
    {
        GD.Print("On hit enemy");
        GetNode("/root/GlobalAudio").Call("play_sfx", GD.Load<AudioStream>("res://assets/music/explode.ogg"));
        _enemyController.PlayerDestroyEnemy(enemyId);
        PlayerStates.Instance.DestroyedEnemies++;
        if (!_playerItems.ItemUsable("Dash"))
            _playerItems.CurrentItemsCD["Dash"] = 0;
        //GD.Print(PlayerStates.Instance.DestroyedEnemies);
    }

    public void OnHitByBullet()
    {
        //GD.Print("hit by bullet");
        if (_inDashMode)
            return;
        else if (_playerItems.UseItemIfItUsable("Shield"))
        {
            _playerAnimation.PlayIdle();
        }
        else
        {
            _playerAnimation.PlayHurt();
            DoHealthDecrease();
        }
    }

    public void OnCtrlPressed()
    {
        //GD.Print("Ctrl 被按下，Player 收到信号！");
        GoIntoSlowMode();
    }

    public void OnSpacePressed()
    {
        //GD.Print("Space 被按下，Player 收到信号！");
        GoIntoDashMode();
    }

    public void OnMapBorderAreaEntered(Node body, string borderName)
    {
        //GD.Print("Touch the border");
        //GD.Print(borderName);
        if (body != this)
            return;

        switch (borderName)
        {
            case "Left":
                Position = new Vector2(MapWidth, Position.Y);
                break;
            case "Right":
                Position = new Vector2(0, Position.Y);
                break;
            case "Top":
                Position = new Vector2(Position.X, MapHeight);
                break;
            case "Bottom":
                Position = new Vector2(Position.X, 0);
                break;
        }
    }

    private void UpdateHealthBar()
    {
        _healthBar.Frame = (int)(PlayerStates.Instance.Health / PlayerStates.Instance.MaxHealth * 10);
    }

    private void DoHealthRecover(double delta)
    {
        _recoverTimer += (float)delta;
        if (_recoverTimer >= 1f)
        {
            if (PlayerStates.Instance == null)
                return;
            if (PlayerStates.Instance.Health < PlayerStates.Instance.MaxHealth)
            {
                PlayerStates.Instance.Health = Math.Min(
                    PlayerStates.Instance.Health + PlayerStates.Instance.HPRegenerationRate,
                    PlayerStates.Instance.MaxHealth
                );
                //GD.Print($"HP recovered: {PlayerStates.Instance.Health}");
                _recoverTimer = 0f;
                UpdateHealthBar();
            }
        }
    }

    private void DoHealthDecrease()
    {
        PlayerStates.Instance.Health = Math.Max(0, PlayerStates.Instance.Health - 30);
        UpdateHealthBar();
        if (PlayerStates.Instance.Health <= 0f)
        {
            DoDeath();
        }
    }

    private void DoDeath()
    {
        _inDeathMode = true;
        _playerAnimation.PlayExplode();
    }

    private void CreateShadow()
    {
        var shadow = new Sprite2D();
        // 获取当前动画帧的贴图
        string anim = _playerAnimation.Animation;
        int frame = _playerAnimation.Frame;
        var frames = _playerAnimation.SpriteFrames;
        shadow.Texture = frames.GetFrameTexture(anim, frame);

        // 用 PlayerAnimation 的全局变换
        shadow.GlobalPosition = _playerAnimation.GlobalPosition;
        shadow.GlobalRotation = _playerAnimation.GlobalRotation;
        shadow.GlobalScale = _playerAnimation.GlobalScale;
        shadow.FlipH = _playerAnimation.FlipH;
        shadow.FlipV = _playerAnimation.FlipV;
        shadow.Modulate = new Color(1, 1, 1, 0.5f); // 半透明

        // 残影建议加到场景根节点或与 PlayerAnimation 同级节点
        GetTree().CurrentScene.AddChild(shadow);

        // 使用 Tween 淡出并销毁
        var tween = CreateTween();
        tween.TweenProperty(shadow, "modulate:a", 0, 0.3f);
        tween.TweenCallback(Callable.From(() => shadow.QueueFree()));
    }

    private void GoIntoSlowMode()
    {
        if (_timeController.IsBulletTime)
            return;
        if (_playerItems.UseItemIfItUsable("SlowMode"))
        {
            GetNode("/root/GlobalAudio").Call("play_sfx", GD.Load<AudioStream>("res://assets/music/slowdownmixed.ogg"));
            _timeController.GoIntoSlowMode();
        }
    }

    private void GoIntoDashMode()
    {
        float searchRange = 500f;
        EnemyAI nearestEnemy = _enemyController.FindNearestEnemy(GlobalPosition, searchRange);
        //if (nearestEnemy != null)
        //{
        //    GD.Print($"最近的敌人: {nearestEnemy.Id}, 距离: {nearestEnemy.GlobalPosition.DistanceTo(GlobalPosition)}");
        //    // 你可以在这里做指示、锁定等操作
        //}
        if (_inDashMode)
            return;
        _dashingTime = 0;
        if (_playerItems.UseItemIfItUsable("Dash"))
        {
            GetNode("/root/GlobalAudio").Call("play_sfx", GD.Load<AudioStream>("res://assets/music/zoomin.ogg"));
            _playerAnimation.PlayDash();
            _inDashMode = true;
            if (_lastUsableMouseMovement != Vector2.Zero)
            {
                if (PlayerStates.Instance.UseActiveTrace && nearestEnemy != null)
                    _velocity =
                        (nearestEnemy.GlobalPosition - GlobalPosition).Normalized() * DashVelocity;
                else
                    _velocity = _lastUsableMouseMovement.Normalized() * DashVelocity;
            }
            else
                _velocity = Vector2.Right * DashVelocity; // 默认向右
        }
    }

    private void UpdateDashState(double delta)
    {
        _dashingTime += delta;
        //GD.Print($"Dashing Time {_dashingTime}");
        if (_dashingTime > DashTime)
        {
            _inDashMode = false;
            if (_playerItems.ItemUsable("Shield"))
                _playerAnimation.PlayShield();
            else
                _playerAnimation.PlayIdle();
        }
    }

    private void UpdateDeathState(double delta)
    {
        _velocity = Vector2.Zero;
        _acceleration = Vector2.Zero;
        // 角度固定为当前角度（可根据需要设为特定值）
        Rotation = -Mathf.Pi / 2; // 或者保留当前 Rotation
        _velocity = Vector2.Zero;
        _acceleration = Vector2.Zero;
        _arrow.Visible = false;
        _deathTimer += delta;
        if (_deathTimer > DeathTime)
        {
            PlayerStates.Instance.Score = PlayerStates.Instance.DestroyedEnemies * 10;
            GetTree().ChangeSceneToFile("res://scenes/final.tscn");
        }
    }

    private void UpdateHurtState(double delta)
    {
        _hurtTimer += delta;
        if (_hurtTimer > HurtTime)
        {
            _inHurtMode = false;
            if (_playerItems.ItemUsable("Shield"))
                _playerAnimation.PlayShield();
            else
                _playerAnimation.PlayIdle();
        }
    }

    private void UpdateItemCD(double delta)
    {
        foreach (var (x, _) in _playerItems.CurrentItemsCD)
        {
            _playerItems.CurrentItemsCD[x] = Math.Max(_playerItems.CurrentItemsCD[x] - delta, 0);
        }
        if (!_inDashMode && !_inHurtMode && !_inDeathMode && _playerItems.ItemUsable("Shield"))
            _playerAnimation.PlayShield();
    }

    private void VelocityCalculate(double delta)
    {
        if (_inDeathMode)
            return;
        if (_inDashMode)
        {
            Position += _velocity * (float)delta;
            // 保持朝向
            if (_velocity.LengthSquared() > 1e-4f)
                Rotation = Mathf.Atan2(_velocity.Y, _velocity.X);
            return;
        }

        if (_mouseMovement.Direction != Vector2.Zero)
            _acceleration = _mouseMovement.Direction * Sensitivity;
        float accLen = _acceleration.Length();
        if (accLen < MinAcceleration / 2)
        {
            _acceleration = Vector2.Zero;
        }
        else if (accLen < MinAcceleration)
        {
            _acceleration = _acceleration.Normalized() * MinAcceleration;
        }
        else if (accLen > MaxAcceleration)
        {
            _acceleration = _acceleration.Normalized() * MaxAcceleration;
        }
        _lastUsableMouseMovement =
            _acceleration == Vector2.Zero ? _lastUsableMouseMovement : _acceleration;
        _velocity += _acceleration * (float)delta * 10f;

        if (_velocity.Length() > PlayerStates.Instance.MaxSpeed)
            _velocity = _velocity.Normalized() * PlayerStates.Instance.MaxSpeed;

        Position += _velocity * (float)delta;

        //_acceleration = Vector2.Zero;

        if (_velocity.LengthSquared() > 1e-4f)
        {
            Rotation = Mathf.Atan2(_velocity.Y, _velocity.X);
        }
    }
}
