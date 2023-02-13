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
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == character.targetDirection)
                {
                    if (character.currentTile.name == enemy.foundPlayerTile.name)
                    {
                        // 到达地点后更新玩家的追踪位置  todo 这里差一个转向
                        var canSeePlayer = Game.Instance.player.CanReach(character.currentTile.name);
                        if (canSeePlayer)
                        {
                            Debug.LogWarning("todo 能够看见主角,直接抓捕");
                            var playerLastTile = Game.Instance.player.gridManager.GetTileByName(Game.Instance.player.currentTile.name);
                            if (playerLastTile)
                            {
                                var targetDirection = Utils.DirectionTo(character.currentTile, playerLastTile, character.direction);
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
                        }

                        var playerTile = character.gridManager.GetTileByName(Game.Instance.player.currentTile.name);
                        if (playerTile != null)
                        {
                            Debug.Log("更新追踪:" + playerTile.name);
                            //enemy.foundPlayerTile = playerTile;
                            character.FindPathRealTime(playerTile);
                            character.UpdateTargetDirection(character.nextTile);
                            //character.ResetMoves();
                            if (character.direction == character.targetDirection)
                            {
                                character.Reached();
                                enemy.foundPlayerTile = null;
                                enemy.ShowNotFound();
                                // todo 显示问号，取消敌人  准备返回  取消追踪目标显示
                                return true;
                            }
                        }
                    }
                }

                if(character.direction == character.targetDirection)
                {
                    character.Reached();
                    var foundPlayer = enemy.TryFoundPlayer();
                    if (foundPlayer)
                    {
                        enemy.currentAction = new ActionFoundPlayer(enemy, ActionType.FoundPlayer);
                    }
                    return true;
                }
            }
            else if(enemy.hearSoundTile)
            {
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == character.targetDirection)
                {
                    if (character.currentTile.name == enemy.hearSoundTile.name)
                    {
                        enemy.ShowNotFound();
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
            else if(character.currentTile.name == character.originalCoord.name)
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

            
            if (angle <= 1 )
            {
                character.transform.forward = tar_dir;
                character.ResetDirection();
                character.body_looking = false;

                var tdist = Vector3.Distance(character.tr_body.position, character.db_moves[0].position);
                if (tdist < 0.01)
                {
                    if (enemy.foundPlayerTile != null)
                    {
                        enemy.ShowTraceTarget(enemy.foundPlayerTile);
                        var catchPlayer = enemy.TryCatchPlayer();
                        if (!catchPlayer)
                        {
                            var foundPlayer = enemy.TryFoundPlayer();
                            if (!foundPlayer)
                            {
                                enemy.ShowNotFound();
                                enemy.foundPlayerTile = null;
                                enemy.m_animator.Play("Player_Idle");
                            }
                        }
                        Debug.Log("追踪转向完毕");
                    }
                    if (enemy.hearSoundTile != null)
                    {
                        Debug.Log("巡声转向完毕");
                    }
                }
                //else
                //{
                //    var foundPlayer = enemy.TryFoundPlayer();
                //    if(foundPlayer)
                //    {
                //        enemy.currentAction = new ActionFoundPlayer(enemy, ActionType.FoundPlayer);
                //    }
                //}
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
