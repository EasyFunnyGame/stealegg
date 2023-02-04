using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Static,
    Distracted,
    Patrol,
    Sentinel
}

public static class EnemyName
{
    public const string Enemy_Distracted = "Enemy_Distracted";
    public const string Enemy_Patrol = "Enemy_Patrol";
    public const string Enemy_Sentinel = "Enemy_Sentinel";
    public const string Enemy_Static = "Enemy_Static"; 
}

public class Enemy : Character
{
    [SerializeField]
    public EnemyType enemyType;

    

    public Tile tracingTile = null;

    

    // Start is called before the first frame update

    void Awake()
    {
        base.Awake ();
    }

    void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    public override void Update()
    {
        if (Game.Instance.turn != Turn.ENEMY) return;
        base.Update();
    }

    public override void OnDirectionRested()
    {
        base.OnDirectionRested();
        Debug.Log("检查敌人");
    }

    public void Alert(string tileName)
    {
        Debug.Log("警觉" + tileName);
        var targetTile = gridManager.GetTileByName(tileName);
        if(targetTile!=null)
        {
            //ClearPath();
            tracingTile = targetTile;
            selected_tile_s = tracingTile;
            gridManager.find_paths_realtime(this, tracingTile);
            hasAction = true;
        }
    }

    public virtual void CheckAction()
    {
        //Debug.Log("检查行为");
        if(tracingTile!=null )
        {
            if(tile_s != tracingTile)
            {
                hasAction = true;
            }
            else
            {
                if(tracingTile.name != originalCoord.name)
                {
                    //ClearPath();
                    tracingTile = gridManager.GetTileByName(originalCoord.name);
                    selected_tile_s = tracingTile;
                    gridManager.find_paths_realtime(this, tracingTile);
                    hasAction = true;
                }
                else
                {
                    tracingTile = null;
                    //ClearPath();
                }
            }
            
        }
        
    }

    protected override void OnReached()
    {
        base.OnReached();
    }

    public virtual void UpdateRouteMark()
    {

    }

    public virtual void TryCatch()
    {

    }

    public virtual void TryTrace()
    {

    }

}
