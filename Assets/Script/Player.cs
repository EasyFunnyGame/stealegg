using UnityEngine;
public class Player : Character
{
    public Animator playerMovePlay;

    public Animator whitslePlay;

    public Animator bottlePlay;

    public string startTileName;

    public GameObject bottle;

    public GameObject princers;

    public GameObject whitsle;

    public float idleTime = 5;

    public bool isHidding = false;

    public string walkingLineType;

    public int up = 0;

    public Camera failCamera;

    public bool justSkipTurn;

    // 刚刚穿过网
    public bool justThroughNet = false;

    // 刚刚跳过井
    public bool justJump = false;

    // 刚刚吹过口哨
    public bool justWhistle = false;

    public string jumstJumpTileName = "";

    // 刚刚偷过菜
    public bool justSteal = false;

    public float stealStopTime = 2.0f;

    public void Awake()
    {
        failCamera.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(currentAction==null)
        {
            if (idleTime > 0)
            {
                idleTime -= Time.deltaTime;
                if (idleTime < 0)
                {
                    var idle_type = m_animator.GetFloat("idle_type");
                    m_animator.SetFloat("look_around", idle_type == 1 ? 0 : 1);
                    m_animator.SetTrigger("idle_too_long");
                    idleTime = Random.Range(3, 5);
                }
            }
        }

        var founded = false;
        for(var index = 0; index < boardManager.enemies.Count;index++)
        {
            var enemy = boardManager.enemies[index];
            if(enemy.coordTracing.isLegal)
            {
                founded = true;
            }
            if(founded)
            {
                break;
            }
        }

        // 更新主角站立狀態
        var player_idle_type = m_animator.GetFloat("idle_type");
        if (founded)
        {
            player_idle_type += .1f;
            if (player_idle_type >= 1)
            {
                player_idle_type = 1;
            }
            m_animator.SetFloat("idle_type", player_idle_type);
        }
        else
        {
            player_idle_type -= .1f;
            if (player_idle_type <= 0)
            {
                player_idle_type = 0;
            }
            m_animator.SetFloat("idle_type", player_idle_type);
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Reached();
        whitslePlay.gameObject.SetActive(false);
        playerMovePlay.gameObject.SetActive(false);
        bottlePlay.gameObject.SetActive(false);
        bottle.gameObject.SetActive(false);
        startTileName = coord.name;
    }
    
    override public void Reached()
    {
        base.Reached();
        boardManager.PickItem(currentTile.name,this);
        if(Game.Instance.result == GameResult.NONE)
        {
            m_animator.SetBool("moving", false);
        }
        m_animator.SetInteger("bottle",-1);
       
        idleTime = Random.Range(3,5);

        isHidding = false;
        var anyEnemyLookingAt = false;
        foreach (var kvp in boardManager.allItems)
        {
            var item = kvp.Value;
            if (item.itemType == ItemType.Growth && coord.name == item.coord.name)
            {
                for (var index = 0; index < boardManager.enemies.Count;  index++)
                {
                    var enemy = boardManager.enemies[index];
                    var body = enemy.transform.GetChild(0);
                    if (body == null) continue;
                    var eulerY = body.rotation.eulerAngles.y;
                    while (eulerY < 0)
                    {
                        eulerY += 360;
                    }
                    while (eulerY >= 360)
                    {
                        eulerY -= 360;
                    }

                    if (CanReachInSteps(enemy.currentTile.name, enemy.checkRange))
                    {
                        var targetName = "";
                        var zOffset = 0;
                        var xOffset = 0;

                        if (Mathf.Abs(eulerY - 0) < 10)// up
                        {
                            zOffset = 1;
                        }
                        else if (Mathf.Abs(eulerY - 180) < 10)// down
                        {
                            zOffset = -1;
                        }
                        else if (Mathf.Abs(eulerY - 270) < 10)// left 270
                        {
                            xOffset = -1;
                        }
                        else if (Mathf.Abs(eulerY - 90) < 10)// right
                        {
                            xOffset = 1;
                        }
                        var checkCoord = enemy.coord.Clone();
                        for (var idx = 0; idx < enemy.checkRange - 1; idx++)
                        {
                            checkCoord.x += xOffset;
                            checkCoord.z += zOffset;

                            var isObstructByOther = false;
                            for (var jndex = 0; jndex < Game.Instance.boardManager.enemies.Count; jndex++)
                            {
                                var otherEnemy = Game.Instance.boardManager.enemies[jndex];
                                if (otherEnemy == this) continue;
                                if (otherEnemy.coord.x == checkCoord.x && otherEnemy.coord.z == checkCoord.z)
                                {
                                    isObstructByOther = true;
                                }
                            }

                            if (isObstructByOther)
                            {
                                break;
                            }

                            targetName = new Coord(checkCoord.x, checkCoord.z, transform.position.y).name;
                            var lookingAtGrowthTile = boardManager.allItems.ContainsKey(targetName) && boardManager.allItems[targetName]?.itemType == ItemType.Growth && (targetName == lastTileName || targetName == coord.name);

                            if (lookingAtGrowthTile)
                            {
                                anyEnemyLookingAt = true;
                                break;
                            }
                        }
                    }
                }
                if (!anyEnemyLookingAt)
                {
                    isHidding = true;
                }
                break;
            }
        }
        if (!isHidding)
        {
            ShowReached();
        }
        //Debug.Log(string.Format("{0}到达{1}", gameObject.name, tile_s.gameObject.name));
    }

    override public void StartMove()
    {
        base.StartMove();
        m_animator.SetBool("moving", true);
    }

    public override void FootL()
    {
        base.FootL();
    }

    public override void FootR()
    {
        base.FootR();
    }

    public void CheckWhitsle()
    {
        if (!Game.Instance) return;
        Game.Instance.gameCanvas.DisableWhistle();
        if (Game.Instance.playingLevel == 1)
        {
            Game.Instance.gameCanvas.whitsleGroup.alpha = 0;
            return;
        }
        Game.Instance.gameCanvas.whitsleGroup.alpha = 1;
        var items = Game.Instance.boardManager.allItems;
        if(items.ContainsKey(coord.name))
        {
            var item = items[coord.name];
            if(item.itemType == ItemType.Growth)
            {
                return;
            }
        }

        var nodes = boardManager.FindNodesAround(currentTile.name, 2);
        var stop = false;
        var positionIgnoreY = new Vector3(transform.position.x, 0, transform.position.z);
        foreach (var kvp in nodes)
        {
            for (var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.EqualsIgnoreY(kvp.Value.coord))
                {
                    var enemyPositionIgnoreY = new Vector3(enemy.transform.position.x,0,enemy.transform.position.z);
                    var dis = Vector3.Distance(positionIgnoreY, enemyPositionIgnoreY);
                    if(dis * dis <=  2)
                    {
                        Game.Instance.gameCanvas.EnableWhistle();
                        stop = true;
                        break;
                    }
                    
                }
            }
            if (stop)
            {
                break;
            }
        }
    }

    public void CheckBottle()
    {
        if (!Game.Instance) return;
        Game.Instance.gameCanvas.DisableBottle();
        if (Game.Instance.playingLevel == 0)
        {
            Game.Instance.gameCanvas.bottleGroup.alpha = 0;
            return;
        }
        Game.Instance.gameCanvas.bottleGroup.alpha = 1;
        if (Game.Instance.boardManager.bottleCount <= 0)
        {
            return;
        }
        Game.Instance.gameCanvas.EnableBottle();
    }

    public void ShowReached()
    {
        playerMovePlay.gameObject.SetActive(true);
        playerMovePlay.Play("Movement_Animation_01");
        playerMovePlay.transform.parent = null;
        playerMovePlay.transform.position = transform.position + new Vector3(0,0.05f,0);
    }

    public override void PlayerReached()
    {
        base.PlayerReached();
        playerMovePlay.gameObject.SetActive(false);
    }

    public void PlayStealEffect(Vector3 position)
    {
        bottlePlay.gameObject.SetActive(false);
        bottlePlay.gameObject.SetActive(true);
        bottlePlay.transform.position = position;
        bottlePlay.transform.parent = null;
        AudioPlay.Instance.PlayHeard();
    }

    public void PlayBottleEffect(Vector3 position)
    {
        bottlePlay.gameObject.SetActive(false);
        bottlePlay.gameObject.SetActive(true);
        bottlePlay.transform.position = position;
        bottlePlay.transform.parent = null;
        bottlePlay.transform.rotation = Quaternion.identity;
        AudioPlay.Instance.PlayHeard();
    }
    public void PlayeWhitsleEffect(Vector3 position)
    {
        whitslePlay.gameObject.SetActive(false);
        whitslePlay.gameObject.SetActive(true);
        whitslePlay.transform.position = position;
        whitslePlay.transform.parent = null;
        whitslePlay.transform.rotation = Quaternion.identity;
    }

    public void PlayWhitsle()
    {
        PlayeWhitsleEffect(transform.position);
        m_animator.SetTrigger("whistle");
    }

   
    public override void AnimationEnd(string clipName)
    {
        base.AnimationEnd(clipName);
    }

    public override void AfterStealVegetable()
    {
        base.AfterStealVegetable();
        Game.Instance.gameCanvas.Hide();
        Game.Instance.camera.gameObject.SetActive(false);
        Game.Instance.graffCanvas.Show();
    }

    public override void Pick()
    {
        base.Pick();
        var items = boardManager.allItems;

        var x = Mathf.RoundToInt(transform.position.x);
        var z = Mathf.RoundToInt(transform.position.z);
        var pos_name = string.Format("{0}_{1}", x, z);

        if(items.ContainsKey(pos_name))
        {
            var item = items[pos_name];
            if(item?.itemType == ItemType.LureBottle)
            {
                if(Game.Instance.boardManager.bottleCount==0)
                {
                    AudioPlay.Instance.PlayerPickBottle();
                    item.gameObject?.SetActive(false);
                    item.icon?.gameObject.SetActive(false);
                    boardManager.allItems.Remove(pos_name);
                    Game.Instance.boardManager.bottleCount = 1;
                }
            }
            if(item?.itemType == ItemType.Graff)
            {
                item.picked = true;
                item.gameObject.SetActive(false);
                item.icon.gameObject.SetActive(false);
                (item as GraffItem)?.sceneGameObject?.SetActive(false);
            }
        }
    }

    private bool triggerSteal = false;
    public override void Lure()
    {
        if (justSteal && !triggerSteal)
        {
            LureSteal();
            triggerSteal = true;
            justSteal = false;
        }
    }

    void LureSteal()
    {
        var playerTileName = Game.Instance.player.currentTile.name;
        var item = Game.Instance.boardManager.allItems[playerTileName];
        if (item.itemType == ItemType.Graff)
        {
            var boardManager = Game.Instance.boardManager;
            Game.Instance.boardManager.coordLure = coord.Clone();
            Game.Instance.boardManager.rangeLure = 2;
        }

        var items = this.boardManager.allItems;
        foreach (var kvp in items)
        {
            var endItem = kvp.Value;
            if (endItem.itemType == ItemType.End)
            {
                var notActive = endItem.transform.Find("Exit_not_active");
                var active = endItem.transform.Find("Exit_active");
                notActive.gameObject.SetActive(false);
                active.gameObject.SetActive(true);
                break;
            }
        }
    }
}
