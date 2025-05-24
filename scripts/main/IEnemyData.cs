using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

public interface IEnemyData
{
    public void Update(float delta, Vector2 playerPos);
    public void DestroyEnemy(int Id);
    public Dictionary<int, EnemyInfo> Enemies { get; }
}
