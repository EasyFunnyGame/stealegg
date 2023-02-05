﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionEnemyMove : ActionBase
{
    private Vector3 nextStepTilePosition;

    public ActionEnemyMove(Enemy character, GridTile tile) : base(character, ActionType.EnemyMove)
    {
        character.FindPathRealTime(tile);
        nextStepTilePosition = character.db_moves[0].position;
        character.StartMove();
    }

    public Enemy enemy
    {
        get
        {
            return character as Enemy;
        }
    }

    public override bool CheckComplete()
    {
        var tdist = Vector3.Distance(character.tr_body.position, nextStepTilePosition);
        if (tdist < 0.001f)
        {
            if(enemy.foundPlayerTile)
            {
                if(character.tile_s.name == enemy.foundPlayerTile.name)
                {
                    // 到达地点后更新玩家的追踪位置
                    var canSeePlayer = Player.Instance.CanBeSee(character.tile_s.name);
                    if(canSeePlayer)
                    {
                        Debug.LogWarning("todo 能够看见主角,直接抓捕");
                        var targetDirection = Utils.DirectionTo(character.tile_s, Player.Instance.tile_s, character.direction);
                        if (character.direction == targetDirection)
                        {
                            Game.Instance.FailGame();
                            character.Reached();
                            return true;
                        }
                        else
                        {
                            Utils.SetDirection(character, targetDirection);
                            return false;
                        }
                    }

                    var playerTile = character.gridManager.GetTileByName(Player.Instance.tile_s.name);
                    if(playerTile != null)
                    {
                        enemy.foundPlayerTile = playerTile;
                        Debug.Log("更新追踪:"+playerTile.name);
                        character.FindPathRealTime(playerTile);
                        character.UpdateTargetDirection(character.nextTile);
                        if (character.direction == character.targetDirection)
                        {
                            character.Reached();
                            return true;
                        }
                    }
                }

                if(character.direction == character.targetDirection)
                {
                    character.Reached();
                    return true;
                }
            }
            else if(enemy.hearSoundTile)
            {
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == character.targetDirection)
                {
                    if (character.tile_s.name == enemy.hearSoundTile.name)
                    {
                        enemy.ShowQuestion();
                        enemy.hearSoundTile = null;
                        character.Reached();
                        return true;
                    }
                    else
                    {
                        character.Reached();
                        return true;
                    }
                }
            }
            else if(character.tile_s.name == character.originalCoord.name)
            {
                // 回到原点要转向
                if (character.direction != character.originalDirection)
                {
                    // character.originalDirection = Direction.Up;// for test 
                    Utils.SetDirection(character, character.originalDirection);
                    return false;
                }
                else
                {
                    enemy.originalTile = null;
                    enemy.OnReachedOriginal();
                    character.Reached();
                    return true;
                }
            }
            else if(enemy.originalTile!=null)
            {
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == character.targetDirection)
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
                        enemy.ShowQuestion();
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
