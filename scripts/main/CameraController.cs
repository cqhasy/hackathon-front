using Godot;
using System;
using System.Collections.Generic;

public partial class CameraController : Camera2D
{
    [Export]
    public NodePath PlayerPath { get; set; }

    [Export]
    public Vector2 CameraSize { get; set; } = new Vector2(1280, 720);

    [Export]
    public Vector2 CanvasSize { get; set; } = new Vector2(3840, 2160);

    [Export]
    public Vector2 DeadZoneSize { get; set; } = new Vector2(200, 120);

    private Node2D _player;

    public override void _Ready()
    {
        _player = GetNode<Node2D>(PlayerPath);
        MakeCurrent();
    }

    public override void _Process(double delta)
    {

        if (_player == null) return;

        Vector2 cameraCenter = GlobalPosition;
        Vector2 playerPos = _player.GlobalPosition;

        Rect2 deadZone = new(
            cameraCenter - DeadZoneSize / 2,
            DeadZoneSize
        );

        if (!deadZone.HasPoint(playerPos))
        {
            Vector2 offset = playerPos - cameraCenter;
            Vector2 move = Vector2.Zero;

            if (offset.X > DeadZoneSize.X / 2)
                move.X = offset.X - DeadZoneSize.X / 2;
            else if (offset.X < -DeadZoneSize.X / 2)
                move.X = offset.X + DeadZoneSize.X / 2;

            if (offset.Y > DeadZoneSize.Y / 2)
                move.Y = offset.Y - DeadZoneSize.Y / 2;
            else if (offset.Y < -DeadZoneSize.Y / 2)
                move.Y = offset.Y + DeadZoneSize.Y / 2;

            cameraCenter += move;
        }

        cameraCenter.X = Mathf.Clamp(cameraCenter.X, CameraSize.X / 2, CanvasSize.X - CameraSize.X / 2);
        cameraCenter.Y = Mathf.Clamp(cameraCenter.Y, CameraSize.Y / 2, CanvasSize.Y - CameraSize.Y / 2);

        GlobalPosition = cameraCenter;
    }
}
