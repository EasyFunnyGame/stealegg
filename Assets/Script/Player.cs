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

    private void Update()
    {
        if(currentAction==null)
        {
            if (idleTime > 0)
            {
                idleTime -= Time.deltaTime;
                if (idleTime < 0)
                {
                    var randomIdleMotion = Random.Range(0, 2);
                    m_animator.SetFloat("look_around", randomIdleMotion);
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
        m_animator.SetBool("moving", false);
        m_animator.SetInteger("bottle",-1);
        m_animator.SetInteger("jump", -1);
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
        if( moving || body_looking )
        {
            Game.Instance.gameCanvas.DisableWhistle();
            return;
        }
        if( Game.Instance.playingLevel == 1 )
        {
            Game.Instance.gameCanvas.DisableWhistle();
            return;
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
        if (bottleCount > 0)
        {

            Game.Instance.gameCanvas.EnableBottle();
        }
        else
        {
            Game.Instance.gameCanvas.DisableBottle();
        }
    }

    public void ShowReached()
    {
        playerMovePlay.gameObject.SetActive(true);
        playerMovePlay.Play("Movement_Animation_01");
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

}
