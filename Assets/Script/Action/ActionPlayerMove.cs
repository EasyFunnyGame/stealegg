using UnityEngine;

public class ActionPlayerMove : ActionBase
{
    Vector3 velocity = new Vector3();

    bool crounching=false;
    public ActionPlayerMove(Player player, GridTile tile) : base(player, ActionType.PlayerMove)
    {
        velocity = new Vector3();
        character.FindPathRealTime(tile);
        player.m_animator.SetFloat("crouch", 0);
        var currentNodeName = player.currentTile.name;
        var targetNodeName = tile.name;
        var linkLine = player.boardManager.FindLine(currentNodeName, targetNodeName);
        if(linkLine!=null)
        {
            Transform lineType = null;
            for(var index = 0; index < linkLine.transform.childCount; index++)
            {
                if(linkLine.transform.GetChild(index).gameObject.activeSelf)
                {
                    lineType = linkLine.transform.GetChild(index);
                    break;
                }
            }
            if(lineType != null)
            {
                // Debug.Log("连线类型：" + lineType.name);
                switch(lineType.name)
                {
                    case "Hor_Normal_Visual":
                        player.m_animator.SetFloat("crouch",0);
                        break;

                    case "Hor_Doted_Visual":
                        player.m_animator.SetFloat("crouch", 1);
                        crounching = true;
                        break;
                }
            }
        }
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
        if(character.selected_tile_s != null && character.selected_tile_s.db_path_lowest.Count==1)
        {
            var tdist = Vector3.Distance(character.tr_body.position, character.db_moves[0].position);
            if (tdist < 0.001f)
            {
                character.Reached();
                return true;
            }
            return false;
            
        }
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
                character.ResetDirection();
                character.body_looking = false;
                character.StartMove();
            }

            return;
        }

        if (character.moving)
        {
           

            if(crounching)
            {
                float step = character.move_speed * Time.deltaTime;
                character.transform.position = Vector3.MoveTowards(character.transform.position, character.db_moves[0].position, step);
            }
            else
            {
                character.transform.position = Vector3.SmoothDamp(character.transform.position, character.db_moves[0].position, ref velocity, 0.12f);
            }
            
            var tdist = Vector3.Distance(character.tr_body.position, character.db_moves[0].position);
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
