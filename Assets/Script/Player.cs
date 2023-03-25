using UnityEngine;
public class Player : Character
{
    public int bottleCount;

    public Animator playerMovePlay;

    public Animator whitslePlay;

    public Animator bottlePlay;

    public string startTileName;

    public GameObject bottle;

    public GameObject princers;

    public GameObject whitsle;

    public float idleTime = 5;

    public bool hidding = false;

    public string walkingLineType;

    public int up = 0;

    public Camera failCamera;

    private void Awake()
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

            if( enemy.hearSoundTile!=null || enemy.foundPlayerTile!=null || enemy.growthTile!=null)
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
        ShowReached();
        idleTime = Random.Range(3,5);

        hidding = false;
        var anyEnemyLookingAt = false;
        foreach (var kvp in boardManager.allItems)
        {
            var item = kvp.Value;
            if (item.itemType == ItemType.Growth && coord.name == item.coord.name)
            {
                for (var index = 0; index < boardManager.enemies.Count;  index++)
                {
                    var enemy = boardManager.enemies[index];
                    if(CanReachInSteps(enemy.currentTile.name, 1))
                    {
                        var direction = Utils.DirectionTo(enemy.currentTile.name, kvp.Key, enemy.direction);
                        if (direction == enemy.direction)
                        {
                            anyEnemyLookingAt = true;
                            break;
                        }
                    }
                }
                if (!anyEnemyLookingAt)
                {
                    hidding = true;
                }
                break;
            }
        }
        //Debug.Log(string.Format("{0}到达{1}", gameObject.name, tile_s.gameObject.name));
    }

    override public void StartMove()
    {
        base.StartMove();
        m_animator.SetBool("moving", true);
        var items = boardManager.allItems;
        if (items.ContainsKey(currentTile.name))
        {
            var item = items[currentTile.name];
            if (item.itemType == ItemType.Growth)
            {
                for( var i = 0; i < boardManager.enemies.Count; i++ )
                {
                    var enemy = boardManager.enemies[i];
                    var targetForward = transform.position - enemy.transform.position;
                    var enemyForward = enemy.transform.forward;
                    if (Mathf.Abs(enemyForward.x- targetForward.x)<0.01f &&
                        Mathf.Abs(enemyForward.y - targetForward.y) < 0.01f&&
                        Mathf.Abs(enemyForward.z - targetForward.z) < 0.01f)
                    {
                        enemy.LureGrowth(nextTile.name);
                    }
                }
            }
        }
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
        Game.Instance.gameCanvas.DisableWhistle();
        if (Game.Instance.playingLevel == 1)
        {
            Game.Instance.gameCanvas.DisableWhistle();
            return;
        }

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
        foreach (var kvp in nodes)
        {
            for (var index = 0; index < boardManager.enemies.Count; index++)
            {
                var enemy = boardManager.enemies[index];
                if (enemy.coord.Equals(kvp.Value.coord))
                {
                    var dis = Vector3.Distance(transform.position, enemy.transform.position);
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
        Game.Instance.gameCanvas.DisableBottle();
        //for(int i = 0; i < boardManager.enemies.Count; i++)
        //{
        //    var enemy = boardManager.enemies[i];
        //    if(enemy.currentAction!=null)
        //    {
        //        return;
        //    }
        //}

        if (bottleCount <= 0)
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
        playerMovePlay.transform.position = transform.position;
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
    }

    public void PlayBottleEffect(Vector3 position)
    {
        bottlePlay.gameObject.SetActive(false);
        bottlePlay.gameObject.SetActive(true);
        bottlePlay.transform.position = position;
        bottlePlay.transform.parent = null;
        bottlePlay.transform.rotation = Quaternion.identity;
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
                AudioPlay.Instance.PlayerPickBottle();
                item.gameObject?.SetActive(false);
                item.icon?.gameObject.SetActive(false);
                boardManager.allItems.Remove(pos_name);
                bottleCount++;
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
    public bool justSteal = false;
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

    //void LureBottle()
    //{

    //}

    void LureSteal()
    {
        var playerTileName = Game.Instance.player.currentTile.name;
        var item = Game.Instance.boardManager.allItems[playerTileName];
        if (item.itemType == ItemType.Graff)
        {
            var boardManager = Game.Instance.boardManager;

            var targetArray = playerTileName.Split('_');
            var x = int.Parse(targetArray[0]);
            var z = int.Parse(targetArray[1]);

            foreach (var enemy in boardManager.enemies)
            {
                var coord = enemy.coord;
                var distanceFromX = Mathf.Abs(x - coord.x);
                var distanceFromZ = Mathf.Abs(z - coord.z);
                if (distanceFromX <= 2 && distanceFromZ <= 2)
                {
                    enemy.LureSteal(playerTileName);
                }
            }
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
