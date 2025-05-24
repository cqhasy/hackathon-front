using System;
using Godot;

public partial class KeyboardListener : Node
{
    [Signal]
    public delegate void CtrlPressedEventHandler();

    [Signal]
    public delegate void SpacePressedEventHandler();

    public override void _Ready()
    {
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        // 检测Ctrl按下  
        if (Input.IsKeyPressed(Key.Ctrl))
        {
            EmitSignal(SignalName.CtrlPressed);
        }
        if (Input.IsKeyPressed(Key.Space))
        {
            EmitSignal(SignalName.SpacePressed);
        }
        if(Input.IsKeyPressed(Key.Escape))
        {
            // 暂停当前场景
            GetTree().Paused = true;
            // 加载并切换到暂停场景（以弹窗方式，不影响主场景）
            var pauseScene = GD.Load<PackedScene>("res://scenes/pause.tscn");
            var pauseInstance = pauseScene.Instantiate();
            GetTree().CurrentScene.AddChild(pauseInstance);
        }
    }
}
