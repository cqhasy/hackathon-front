using System;
using Godot;

public partial class MouseMovement : Node
{
	public Vector2 Direction { get; private set; } = Vector2.Zero;
	public bool Enable { get; set; } = true;

	public override void _Ready()
	{
		SetProcessInput(true);
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (!Enable)
			return;
		if (@event is InputEventMouseMotion mouseMotion)
		{
			Direction = mouseMotion.Relative;
			// 你可以选择归一化方向向量，或保留原始速度
			// Direction = mouseMotion.Relative.Normalized();

			//GD.Print($"方向: {Direction}, 长度: {Direction.Length()}");
		}
	}

	public void ToNormalMode()
	{
		Input.MouseMode = Input.MouseModeEnum.Visible;
		Enable = false;
	}
}
