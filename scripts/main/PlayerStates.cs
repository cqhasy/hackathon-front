using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PlayerStates
{
	public string UserName { get; set; } = "";
	public float Health { get; set; } = 100;
	public float MaxSpeed { get; set; } = 800f;
	public float HPRegenerationRate { get; set; } = 10f;
	public int Score { get; set; } = 0;
	public int CurrentMoney { get; set; } = 0;
}
