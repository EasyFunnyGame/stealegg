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

    public Animator animator;

    public static int count;

    public bool tracingPlayer = false;

    public GridTile foundPlayerTile = null;

    public GridTile hearSoundTile = null;

    public GridTile originalTile = null;

    public GameObject question;
    public GameObject back;
    public GameObject exclamation;
    public GameObject sleep;

    public override void ResetDirection()
    {
        base.ResetDirection();
        UpdateRouteMark();
    }


    public void Alert(string tileName)
    {
        //Debug.Log("警觉" + tileName);
        var targetTile = gridManager.GetTileByName(tileName); 
        if(targetTile!=null)
        {
            animator.Play("Enemy_Alert");
            ShowAlert();
            if (hearSoundTile && hearSoundTile.name == targetTile.name)
            {
                currentAction = new ActionEnemyMove(this, hearSoundTile);
                return;
            }
            hearSoundTile = targetTile;
            var canSeePlayer = Player.Instance.CanReach(tile_s.name);
            if (canSeePlayer)
            {
                currentAction = new ActionTurnDirection(this, Utils.DirectionTo(tile_s, Player.Instance.tile_s, direction));
                return;
            }
            // 如果追踪方向和当前方向相同  直接行进
            if(foundPlayerTile)
            {
                currentAction = new ActionEnemyMove(this, foundPlayerTile);
                return;
            }
            // 判断寻路的方向  
            FindPathRealTime(targetTile);
            UpdateTargetDirection(nextTile);
            if (targetDirection != direction)
            {
                currentAction = new ActionTurnDirection(this, targetDirection);
            }
        }
    }


    public virtual void CheckAction()
    {
        // Debug.Log("检查行为");
        var caught = TryCatchPlayer();
        if (caught)
        {
            return;
        }

        if (foundPlayerTile != null)
        {
            originalTile = null;
            currentAction = new ActionEnemyMove(this,  foundPlayerTile);
            return;
        }

        if (hearSoundTile != null)
        {
            originalTile = null;
            currentAction = new ActionEnemyMove(this, hearSoundTile);
            return;
        } 

        if (originalTile != null)
        {
            currentAction = new ActionEnemyMove(this, originalTile);
            return;
        }

        if (ReturnOriginal(true))
        {
            ShowReturnOriginal();
            return;
        }

        var trace = TryFoundPlayer();
        if (trace)
        {
            currentAction = new ActionFoundPlayer(this, ActionType.FoundPlayer);
            return;
        }
    }

    public List<Transform> redLines = new List<Transform>();

    public List<MeshRenderer> redNodes = new List<MeshRenderer>();

    public bool ReturnOriginal(bool needAction)
    {
        if (tile_s.name != originalCoord.name)
        {
            originalTile = gridManager.GetTileByName(originalCoord.name);
            if (originalTile != null)
            {
                FindPathRealTime(originalTile);
                if (needAction)
                {
                    if (direction == targetDirection)
                    {
                        currentAction = new ActionEnemyMove(this, originalTile);
                    }
                    else
                    {
                        currentAction = new ActionTurnDirection(this, targetDirection);
                    }
                }
            }
            
            return true;
        }
        return false;
    }

    public void RedLineByName(string nodeName1, string nodeName2)
    {
        var line = boardManager.FindLine(nodeName1, nodeName2);

        if (line != null)
        {
            var lineTr = line.transform;
            var copyLine = Instantiate(lineTr);
            copyLine.transform.position = lineTr.position;
            copyLine.transform.Translate(new Vector3(0, 0.001f, 0));
            copyLine.transform.rotation = lineTr.rotation;
            redLines.Add(copyLine);
            copyLine.GetChild(0).GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/RouteRed");
        }
    }

    public void RedNodeByName(string nodeName)
    {
        var node = boardManager.FindNode(nodeName);

        if (node != null)
        {
            var nodeTr = node.targetIcon;
            var copyNode = Instantiate(nodeTr.GetComponent<MeshRenderer>());
            redNodes.Add(copyNode);
            copyNode.transform.position = nodeTr.transform.position;
            copyNode.transform.Translate(new Vector3(0, 0.001f, 0));
            copyNode.transform.rotation = nodeTr.transform.rotation;
            copyNode.material = Resources.Load<Material>("Material/RouteRed");
        }
    }

    public override void Reached()
    {
        base.Reached();

        UpdateRouteMark();

        // 
        var catchPlayer = TryCatchPlayer();
        if (catchPlayer)
        {
            return;
        }
        var foundPlayer = TryFoundPlayer();
        if (foundPlayer)
        {
            currentAction = new ActionFoundPlayer(this, ActionType.FoundPlayer);
            return;
        }

        if (Game.Instance.status == GameStatus.FAIL)
        {
            animator.CrossFade("Enemy_Caught", 0.1f);
        }
        else if (foundPlayerTile != null || hearSoundTile != null)
        {
            animator.CrossFade("Enemy_Alert", 0.1f);
        }
        else
        {
            animator.CrossFade("Player_Idle", 0.1f);
        }

        if (Player.Instance.tile_s)
        {
            Player.Instance.CheckBottle();
            Player.Instance.CheckWhistle();
        }
        Debug.Log("敌人到达路径点:"+tile_s.name);
    }

    public virtual void UpdateRouteMark()
    {

    }

    public virtual bool TryCatchPlayer()
    {
        if (Player.Instance == null || Player.Instance.tile_s == null) return false;
        var canSeePlayer = Player.Instance.CanReach(tile_s.name);
        var targetDirection = Utils.DirectionTo(tile_s, Player.Instance.tile_s, direction);
        if(targetDirection == direction)
        {
            if (foundPlayerTile != null && canSeePlayer)
            {
                Game.Instance.FailGame();
                animator.CrossFade("Enemy_Caught", 0.1f);
                return true;
            }
            var canReach = CanReach(Player.Instance.tile_s.name);
            if (canReach)
            {
                Game.Instance.FailGame();
                animator.CrossFade("Enemy_Caught", 0.1f);
                return true;
            }
        }
        return false;;
    }

    public virtual bool TryFoundPlayer()
    {
        var xOffset = 0;
        var zOffset = 0;
        if (direction == Direction.Up)
        {
            zOffset = 1;
        }
        else if (direction == Direction.Down)
        {
            zOffset = -1;
        }
        else if (direction == Direction.Right)
        {
            xOffset = 1;
        }
        else if (direction == Direction.Left)
        {
            xOffset = -1;
        }
        var catchNodeX = coord.x + xOffset * 2;
        var catchNodeZ = coord.z + zOffset * 2;
        var next1NodeName = string.Format("{0}_{1}", catchNodeX, catchNodeZ);
        if (Player.Instance.tile_s && Player.Instance.tile_s.name == next1NodeName)
        {
            var targetTile = gridManager.GetTileByName(next1NodeName);
            if (targetTile != null)
            {
                foundPlayerTile = targetTile;
                hearSoundTile = null;
                originalTile = null;
                Debug.Log("开始追踪:" + targetTile.name);
                return true;
            }
        }
        return false;
    }

    public override void StartMove()
    {
        animator.CrossFade("Player_Sprint", 0.1f);
    }

    public void ShowQuestion()
    {
        question.gameObject.SetActive(true);
        exclamation.gameObject.SetActive(false);
        sleep.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
        questionShowTimer = 2;
    }

    public void ShowAlert()
    {
        question.gameObject.SetActive(false);
        exclamation.gameObject.SetActive(true);
        sleep.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
    }

    public void ShowReturnOriginal()
    {
        question.gameObject.SetActive(false);
        exclamation.gameObject.SetActive(false);
        sleep.gameObject.SetActive(false);
        back.gameObject.SetActive(true);
    }

    public void ShowSleep()
    {
        question.gameObject.SetActive(false);
        exclamation.gameObject.SetActive(false);
        sleep.gameObject.SetActive(true);
        back.gameObject.SetActive(false);
    }

    private float questionShowTimer = 0;

    private void Update()
    {
        if(questionShowTimer>0)
        {
            questionShowTimer -= Time.deltaTime;
            if(questionShowTimer<=0)
            {
                question.gameObject.SetActive(false);
            }
        }
    }

    public virtual void OnReachedOriginal()
    {

    }

}
