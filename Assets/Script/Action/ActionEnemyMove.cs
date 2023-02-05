using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionEnemyMove : ActionBase
{
    private Vector3 nextStepTilePosition;

    public ActionEnemyMove(Enemy character, ActionType actionType, GridTile tile) : base(character, actionType)
    {
        character.FindPathRealTime(tile);
        nextStepTilePosition = character.db_moves[0].position;
        character.StartMove();
    }

    public Enemy enemy
    {
        get{
            return character as Enemy;
        }
    }

    public override bool CheckComplete()
    {
        var tdist = Vector3.Distance(enemy.tr_body.position, nextStepTilePosition);
        if (tdist < 0.001f)
        {
            if(enemy.foundPlayerTile)
            {
                if(enemy.tile_s.name == enemy.foundPlayerTile.name)
                {
                    // 到达地点后更新玩家的追踪位置
                    var playerTile = enemy.gridManager.GetTileByName(Player.Instance.tile_s.name);
                    if(playerTile != null)
                    {
                        enemy.foundPlayerTile = playerTile;
                        character.FindPathRealTime(playerTile);
                        enemy.UpdateTargetDirection(enemy.nextTile);
                        if (enemy.direction == enemy.targetDirection)
                        {
                            character.Reached();
                            return true;
                        }
                        for (int x = 0; x < enemy.gridManager.db_tiles.Count; x++)
                            enemy.gridManager.db_tiles[x].db_path_lowest.Clear(); //Clear all previous lowest paths for this char//

                    }
                }

                if(enemy.direction == enemy.targetDirection)
                {
                    character.Reached();
                    return true;
                }
            }
            else if(enemy.hearSoundTile)
            {
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == enemy.targetDirection)
                {
                    if (enemy.tile_s.name == enemy.hearSoundTile.name)
                    {
                        Debug.Log("到达声音地点,Todo 敌人头顶问号");
                        enemy.hearSoundTile = null;
                    }
                    character.Reached();
                    return true;
                }
            }
            else if(enemy.tile_s.name == enemy.originalCoord.name)
            {
                // 回到原点要转向
                if (enemy.direction != enemy.originalDirection)
                {
                    enemy.body_looking = true;

                     // enemy.originalDirection = Direction.Up;// for test 

                    if (enemy.originalDirection == Direction.Up)
                    {
                        character.db_moves[1].position = character.db_moves[0].position + new Vector3(0,0,1);
                    }
                    else if(enemy.originalDirection == Direction.Left)
                    {
                        character.db_moves[1].position = character.db_moves[0].position + new Vector3(-1,0,0);
                    }
                    else if (enemy.originalDirection == Direction.Right)
                    {
                        character.db_moves[1].position = character.db_moves[0].position + new Vector3(1,0,0);
                    }
                    else if (enemy.originalDirection == Direction.Down)
                    {
                        character.db_moves[1].position = character.db_moves[0].position + new Vector3(0,0,-1);
                    }
                    return false;
                }
                else
                {
                    enemy.originalTile = null;
                    character.Reached();
                    return true;
                }
            }
            else if(enemy.originalTile!=null)
            {
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == enemy.targetDirection)
                {
                    character.Reached();
                    return true;
                }
            }
            else 
            {
                character.Reached();
                return true;
            }
        }
        return false;
    }

    public override void Run()
    {
        if (character.selected_tile_s != null && !character.moving && character.tile_s != character.selected_tile_s && character.selected_tile_s != null)
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

                if(enemy.foundPlayerTile != null)
                {
                    var foundPlayer = enemy.TryFoundPlayer();
                    if (!foundPlayer)
                    {
                        enemy.foundPlayerTile = null;
                        enemy.animator.Play("Player_Idle");
                        Debug.Log("Todo 敌人头顶问号");
                    }
                }

                if (enemy.hearSoundTile != null)
                {
                    Debug.Log("巡声转向完毕");
                }
            }
            return;
        }

        if (character.moving)
        {
            float step = character.move_speed * Time.deltaTime;
            character.transform.position = Vector3.MoveTowards(character.transform.position, character.db_moves[0].position, step);
            var tdist = Vector3.Distance(character.tr_body.position, character.db_moves[0].position);
            if (tdist < 0.001f)
            {
                character.tile_s = character.tar_tile_s.db_path_lowest[character.num_tile];
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
