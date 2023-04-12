using UnityEngine;

public class ActionPlayerMove : ActionBase
{
    Vector3 velocity = new Vector3();

    float height = 0f;

    bool crounching=false;

    bool walkingExit = false;


    Vector3 startPosition;

    Vector3 endPosition;

    public ActionPlayerMove(Player player, GridTile tile) : base(player, ActionType.PlayerMove)
    {
        player.justThroughNet = false;

        endPosition = tile.transform.position;
        startPosition = player.transform.position;

        walkingExit = false;
        velocity = new Vector3();
        player.FindPathRealTime(tile);
        
        var currentNodeName = player.currentTile.name;
        var targetNodeName = tile.name;
        var linkLine = player.boardManager.FindLine(currentNodeName, targetNodeName);
        if(linkLine == null)
        {
            player.Clear();
            return;
        }
        crounching = false;
        player.m_animator.SetFloat("move_type", 0);
        
        Transform lineType = null;
        for(var index = 0; index < linkLine.transform.childCount; index++)
        {
            if(linkLine.transform.GetChild(index).gameObject.activeSelf)
            {
                lineType = linkLine.transform.GetChild(index);
                break;
            }
        }

        var targetNode = player.boardManager.FindNode(tile.name);
        height = targetNode.transform.position.y - player.transform.position.y;
        
        if (lineType != null)
        {
            player.walkingLineType = lineType.name;
            player.up = height > 0 ?  1 :  height < 0 ? -1 : 0;
            // Debug.Log("连线类型：" + lineType.name);

            if(linkLine.playerMoveType == 0.5f)
            {
                player.m_animator.SetFloat("move_type", 0.5f);
                player.justThroughNet = true;
                crounching = true;
            }
            else
            {
                player.m_animator.SetFloat("move_type", 0);
            }
        }

        if (Game.Instance.stealed)
        {
            if(player.boardManager.allItems.ContainsKey(tile.name))
            {
                var endItem = player.boardManager.allItems[tile.name];
                if (endItem != null && endItem.itemType == ItemType.End)
                {
                    player.m_animator.SetFloat("move_type", 1);
                    walkingExit = true;
                }
            }
        }
        else
        {

        }

        player.body_looking = true;
    }

    public Player player
    {
        get
        {
            return character as Player;
        }
    }

    public override bool CheckComplete()
    {
        //Debug.DrawLine(startPosition, endPosition, Color.red);
        if (character.selected_tile_s != null && character.selected_tile_s.db_path_lowest.Count==1)
        {
            var myPosition = character.tr_body.position;
            var targetPosition = character.db_moves[0].position;
            var tdist = Vector3.Distance(new Vector3(myPosition.x, 0, myPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
            if (tdist < 0.001f)
            {
                character.body_looking = false;
                character.Reached();
                if(walkingExit)
                {
                    AudioPlay.Instance.PlayReachExit();
                }
                return true;
            }
            return false;
            
        }
        character.body_looking = false;
        character.Reached();
        return true;
    }

    public override void Run()
    {
        if (character.selected_tile_s != null && !character.moving && character.currentTile != character.selected_tile_s && character.selected_tile_s != null)
        {
            if (character.selected_tile_s.db_path_lowest.Count > 0)
                character.move_tile(character.selected_tile_s);
            else
                Debug.Log("no valid tile selected");
        }

        // 先转向 再位移
        if (character.body_looking)
        {
            Vector3 tar_dir = character.db_moves[1].position - character.tr_body.position;
            Vector3 new_dir = Vector3.RotateTowards(character.tr_body.forward, tar_dir, character.rotate_speed * Time.deltaTime / 2, 0f);
            new_dir.y = 0;
            character.tr_body.transform.rotation = Quaternion.LookRotation(new_dir);

            var angle = Vector3.Angle(tar_dir, character.tr_body.forward);
            if (angle <= 1)
            {
                character.Turned();
                character.StartMove();
            }
            return;
        }

        if (character.moving)
        {
           
            if(crounching || walkingExit)
            {
                float step = character.move_speed * Time.deltaTime;
                character.transform.position = Vector3.MoveTowards(character.transform.position, character.db_moves[0].position + new Vector3(0, height, 0), step);
            }
            else
            {
                character.transform.position = Vector3.SmoothDamp(character.transform.position, character.db_moves[0].position + new Vector3(0, height, 0), ref velocity, 0.12f);
            }
            var myPosition = character.tr_body.position;
            var targetPosition = character.db_moves[0].position;
            var tdist = Vector3.Distance(new Vector3(myPosition.x,0, myPosition.z), new Vector3(targetPosition.x,0, targetPosition.z));
            if (tdist < 0.001f)
            {
                character.currentTile = character.tar_tile_s.db_path_lowest[character.num_tile];
                if (character.moving_tiles && character.num_tile < character.tar_tile_s.db_path_lowest.Count - 1)
                {
                    character.num_tile++;
                    var tpos = character.tar_tile_s.db_path_lowest[character.num_tile].transform.position;
                    tpos.y = character.transform.position.y;
                    character.db_moves[0].position = tpos;

                    character.nextTile = character.tar_tile_s.db_path_lowest[character.num_tile];

                    character.db_moves[1].position = tpos;
                }
                else
                {
                    character.db_moves[4].gameObject.SetActive(false);
                    character.moving = false;
                    character.moving_tiles = false;
                }
            }
        }
    }
}
