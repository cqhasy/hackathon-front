using Godot;
using System;

public partial class TimeController : Node
{
    private float targetTimeScale = 1.0f;
    private float timeScaleLerpSpeed = 2.0f; // 控制恢复速度
    private bool isBulletTime = false;
    private float bulletTimeDuration = 1.0f; // 进入子弹时间的持续时间
    private float bulletTimeTimer = 0f;

    public override void _Ready()
    {
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        // 检测Ctrl按下
        if (Input.IsKeyPressed(Key.Ctrl) && !isBulletTime)
        {
            isBulletTime = true;
            bulletTimeTimer = 0f;
            targetTimeScale = 0.1f;
        }

        if (isBulletTime)
        {
            bulletTimeTimer += (float)delta;
            // 1秒后开始恢复
            if (bulletTimeTimer > bulletTimeDuration)
            {
                targetTimeScale = 1.0f;
                // 平滑恢复
                Engine.TimeScale = Mathf.Lerp(Engine.TimeScale, targetTimeScale, (float)delta * timeScaleLerpSpeed);
                if (Mathf.Abs(Engine.TimeScale - 1.0f) < 0.01f)
                {
                    Engine.TimeScale = 1.0f;
                    isBulletTime = false;
                }
            }
            else
            {
                // 进入子弹时间，平滑减速
                Engine.TimeScale = Mathf.Lerp(Engine.TimeScale, targetTimeScale, (float)delta * timeScaleLerpSpeed);
            }
        }
    }
}
