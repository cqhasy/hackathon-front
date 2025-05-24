using Godot;
using System;

public partial class GlobalConfig : Node2D
{
    public override void _Ready()
    {
        var keyboardListener = GetNode<KeyboardListener>("KeyboardListener");
        var player = GetNode<Player>("Player");
        keyboardListener.CtrlPressed += player.OnCtrlPressed;
    }
}
