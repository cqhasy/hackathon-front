using Godot;
using System;

public partial class KeyboardListener : Node
{
    [Signal]
    public delegate void CtrlPressedEventHandler();
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
    }

}
