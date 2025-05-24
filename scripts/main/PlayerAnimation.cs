using Godot;
using System;

public partial class PlayerAnimation : AnimatedSprite2D
{
    public override void _Ready()
    {
        PlayIdle();
    }

    public void PlayIdle()
    {
        Play("idle");
    }

    public void PlayHurt()
    {
        Play("hurt");
    }

    public void PlayShield()
    {
        Play("shield");
    }
}
