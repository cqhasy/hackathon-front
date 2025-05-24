using System;
using Godot;

public partial class TimeController : Node
{
    public bool IsBulletTime
    {
        get => isBulletTime;
    }
    private float targetTimeScale = 1.0f;
    private float timeScaleLerpSpeed = 2.0f; // 控制恢复速度
    private bool isBulletTime = false;
    private float bulletTimeDuration = 0.1f; // 进入子弹时间的持续时间
    private float bulletTimeTimer = 0f;

    public override void _Ready()
    {
        SetProcess(true);
    }

    public override void _Process(double delta)
    {
        if (isBulletTime)
        {
            bulletTimeTimer += (float)delta;
            if (bulletTimeTimer < bulletTimeDuration)
            {
                // 保持慢速
                Engine.TimeScale = 0.1f;
            }
            else
            {
                // 平滑恢复
                Engine.TimeScale = Mathf.Lerp(
                    Engine.TimeScale,
                    1.0f,
                    (float)delta * timeScaleLerpSpeed
                );
                if (Mathf.Abs(Engine.TimeScale - 1.0f) < 0.01f)
                {
                    Engine.TimeScale = 1.0f;
                    isBulletTime = false;
                }
            }
        }
    }

    public void GoIntoSlowMode()
    {
        isBulletTime = true;
        bulletTimeTimer = 0f;
        Engine.TimeScale = 0.1f; // 立即减慢
    }
}
