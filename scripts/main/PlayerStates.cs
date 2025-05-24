using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public partial class PlayerStates : Node
{
    public string UserName { get; set; } = "";
    public float MaxHealth { get; set; } = 100;
    public float Health { get; set; } = 100;
    public float MaxSpeed { get; set; } = 800f;
    public float ShieldCostTime { get; set; } = 7f;
    public float SlowDownCostTime { get; set; } = 7f;
    public float HPRegenerationRate { get; set; } = 10f;
    public int Score { get; set; } = 0;
    public int CurrentMoney { get; set; } = 100;
    public int DestroyedEnemies { get; set; } = 0;
    public bool UseActiveTrace { get; set; } = true;
    public static PlayerStates Instance { get; private set; }

    public override void _Ready()
    {
        Instance = this;
    }

}
