using fiycraft.scripts.main;
using Godot;
using System;
using System.Xml.Serialization;

public partial class Player : Node2D
{
    private const float MaxAcceleration = 500f;
    private const float MinAcceleration = 30f;
    private const float BasicMaxSpeed = 800f;

    private Vector2 _velocity = Vector2.Zero;
    private Vector2 _acceleration = Vector2.Zero;
    private Vector2 _mouseMovementVector = Vector2.Zero;

    private MouseMovement _mouseMovement;
    private TimeController _timeController;
    private PlayerStates _playerStates;
    private PlayerItems _playerItems;
    private PlayerAnimation _playerAnimation;

    public override void _Ready()
    {
        _mouseMovement = GetParent().GetNode<MouseMovement>("MouseMovement");
        _timeController = GetParent().GetNode<TimeController>("TimeController");
        _playerStates = new PlayerStates();
        _playerItems = new PlayerItems();
        _playerAnimation = GetNode<PlayerAnimation>("PlayerAnimation");
        _playerAnimation.PlayShield();
        _playerAnimation.Rotation = Mathf.Pi / 2;
    }

    public override void _Process(double delta)
    {
        VelocityCalculate(delta);
        UpdateItemCD(delta);
    }

    public void OnCtrlPressed()
    {

        GD.Print("Ctrl 被按下，Player 收到信号！");
        GoIntoSlowMode();
    }

    public void OnSpacePressed()
    {
        GoIntoDashMode();
    } 

    private void GoIntoSlowMode()
    {
        if (_playerItems.UseItemIfItUsable("SlowMode"))
            _timeController.GoIntoSlowMode();
    }

    private void GoIntoDashMode()
    {
        if(_playerItems.UseItemIfItUsable("Dash"))
        {
            
        }    
    }

    private void UpdateItemCD(double delta)
    {
        foreach(var (x,_) in _playerItems.CurrentItemsCD)
        {
            _playerItems.CurrentItemsCD[x] = Math.Max(_playerItems.CurrentItemsCD[x] - delta, 0);
        }
    }

    private void VelocityCalculate(double delta)
    {

        if (_mouseMovement.Direction == Vector2.Zero)
            GD.Print("Vector is Zero");
        else
            _acceleration = _mouseMovement.Direction;
        float accLen = _acceleration.Length();
        if (accLen < MinAcceleration / 3f)
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
        _velocity += _acceleration * (float)delta * 10f;

        if (_velocity.Length() > _playerStates.MaxSpeed)
            _velocity = _velocity.Normalized() * _playerStates.MaxSpeed;

        Position += _velocity * (float)delta;

        _acceleration = Vector2.Zero;

        if (_velocity.LengthSquared() > 1e-4f)
        {
            Rotation = Mathf.Atan2(_velocity.Y, _velocity.X);
        }
    }
}
