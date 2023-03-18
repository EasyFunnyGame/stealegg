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

    public bool founded = false;

    public string walkingLineType;
    public int up = 0;
    private void Update()
    {
        if(currentAction==null)
        {
            if (idleTime > 0)
            {
                idleTime -= Time.deltaTime;
                if (idleTime < 0)
                {
                    var player_idle_type = m_animator.GetFloat("idle_type");
                    m_animator.SetFloat("look_around", player_idle_type == 1 ? 0 : 1);
                    m_animator.SetTrigger("idle_too_long");
                    idleTime = Random.Range(3, 5);
                }
            }
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
        foreach (var kvp in boardManager.allItems)
        {
            var item = kvp.Value;
            if (item.itemType == ItemType.Growth && coord.name == item.coord.name)
            {
                hidding = !founded && true;
                break;
            }
        }
        //Debug.Log(string.Format("{0}到达{1}", gameObject.name, tile_s.gameObject.name));
    }

    override public void StartMove()
    {
        base.StartMove();
        m_animator.SetBool("moving", true);
        //Debug.Log(string.Format("{0}开始行走{1}", gameObject.name, tar_tile_s.name));
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
                    Game.Instance.gameCanvas.EnableWhistle();
                    stop = true;
                    break;
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
        for(int i = 0; i < boardManager.enemies.Count; i++)
        {
            var enemy = boardManager.enemies[i];
            if(enemy.currentAction!=null)
            {
                return;
            }
        }

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
        if(items.ContainsKey(coord.name))
        {
            var item = items[coord.name];
            if(item?.itemType == ItemType.LureBottle)
            {
                AudioPlay.Instance.PlayerPickBottle();
                item.gameObject?.SetActive(false);
                item.icon?.gameObject.SetActive(false);
                boardManager.allItems.Remove(coord.name);
                bottleCount++;
            }
            if(item?.itemType == ItemType.Graff)
            {
                item.picked = true;
                item.gameObject.SetActive(false);
                item.icon.gameObject.SetActive(false);
            }

        }
    }

}
