using System;
using System.Collections.Generic;
using System.Xml.Serialization;
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

    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _acceleration = Vector2.Zero;
    private Vector2 _lastUsableMouseMovement = Vector2.Zero;
    private bool _inDashMode = false;
    private double _dashingTime = 0;
    private double _recoverTimer = 0;

    //private bool dashingWaitingForSettingDirection = false;

    private MouseMovement _mouseMovement;
    private TimeController _timeController;
    private PlayerStates _playerStates;
    private PlayerItems _playerItems;
    private PlayerAnimation _playerAnimation;
    private Node2D _borderArea;
    private KeyboardListener _keyboardListener;
    private Node2D _shadowContainer;
    private AnimatedSprite2D _healthBar;

    public float Sensitivity { get; set; } = 1;

    public override void _Ready()
    {
        _mouseMovement = GetParent().GetNode<MouseMovement>("MouseMovement");
        _timeController = GetParent().GetNode<TimeController>("TimeController");
        _playerStates = new PlayerStates();
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
    }

    public override void _Process(double delta)
    {
        VelocityCalculate(delta);
        UpdateItemCD(delta);
        if (_inDashMode)
            UpdateDashState(delta);
        if (_inDashMode)
        {
            CreateShadow();
        }
        QueueRedraw();
        DoHealthRecover(delta);
    }

    public override void _Draw()
    {
        if (_lastUsableMouseMovement.LengthSquared() > 0.01f)
        {
            // 计算世界坐标下的目标点
            Vector2 world_point = GlobalPosition + _lastUsableMouseMovement.Normalized() * 50f;
            // 转换为本地坐标
            Vector2 local_point = ToLocal(world_point);
            DrawCircle(local_point, 10f, Colors.Red);
        }
    }

    public void OnHitByBullet()
    {
        GD.Print("hit by bullet");
        if (_inDashMode)
            return;
        else
            DoHealthDecrease();
    }

    public void OnCtrlPressed()
    {
        GD.Print("Ctrl 被按下，Player 收到信号！");
        GoIntoSlowMode();
    }

	public void OnSpacePressed()
	{
		GD.Print("Space 被按下，Player 收到信号！");
		GoIntoDashMode();
	}

	public void OnMapBorderAreaEntered(Node body, string borderName)
	{
		GD.Print("Touch the border");
		GD.Print(borderName);
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
        // 假设最大血量为100，血条10帧
        //int maxFrame = 9;
        //float percent = _playerStates.Health / 100f;
        //int frame = Mathf.Clamp(Mathf.RoundToInt(percent * maxFrame), 0, maxFrame);
        //GD.Print(frame);
        
        _healthBar.Frame = (int)_playerStates.Health / 10;
    }

    private void DoHealthRecover(double delta)
    {
        _recoverTimer += (float)delta;
        if (_recoverTimer >= 1f)
        {
            if (_playerStates == null)
                return;
            // 假设有最大生命值 MaxHealth
            float maxHealth = 100f;
            if (_playerStates.Health < maxHealth)
            {
                _playerStates.Health = Math.Min(
                    _playerStates.Health + _playerStates.HPRegenerationRate,
                    maxHealth
                );
                GD.Print($"HP recovered: {_playerStates.Health}");
                _recoverTimer = 0f;
                UpdateHealthBar();
            }
        }
    }

    private void DoHealthDecrease()
    {
        _playerStates.Health = Math.Max(0, _playerStates.Health - 30);
        UpdateHealthBar();
        if (_playerStates.Health <= 0f)
        {
            DoDeath();
        }
    }

    private void DoDeath() { }

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
			_timeController.GoIntoSlowMode();
	}

    private void GoIntoDashMode()
    {
        if (_inDashMode)
            return;
        _dashingTime = 0;
        if (_playerItems.UseItemIfItUsable("Dash"))
        {
            _playerAnimation.PlayDash();
            _inDashMode = true;
            if (_lastUsableMouseMovement != Vector2.Zero)
                _velocity = _lastUsableMouseMovement.Normalized() * DashVelocity;
            else
                _velocity = Vector2.Right * DashVelocity; // 默认向右
        }
    }

    private void UpdateDashState(double delta)
    {
        _dashingTime += delta;
        GD.Print($"Dashing Time {_dashingTime}");
        if (_dashingTime > DashTime)
        {
            _inDashMode = false;
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
        if (!_inDashMode && _playerItems.ItemUsable("Shield"))
            _playerAnimation.PlayShield();
    }

	private void VelocityCalculate(double delta)
	{
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
        if (accLen < MinAcceleration)
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

		if (_velocity.Length() > _playerStates.MaxSpeed)
			_velocity = _velocity.Normalized() * _playerStates.MaxSpeed;

		Position += _velocity * (float)delta;

        //_acceleration = Vector2.Zero;

		if (_velocity.LengthSquared() > 1e-4f)
		{
			Rotation = Mathf.Atan2(_velocity.Y, _velocity.X);
		}
	}
}
