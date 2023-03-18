using UnityEngine;

public class ActionEnemyMove : ActionBase
{
    Vector3 velocity = new Vector3();
    private Vector3 nextStepTilePosition;
    float height = 0f;

    bool tracingTarget = false;

    public ActionEnemyMove(Enemy enemy, GridTile tile) : base(enemy, ActionType.EnemyMove)
    {
        velocity = new Vector3();
        enemy.FindPathRealTime(tile);
        nextStepTilePosition = enemy.db_moves[0].position;
        //enemy.StartMove();

        var targetNode = enemy.boardManager.FindNode(enemy.nextTile.name);
        height = targetNode.transform.position.y - enemy.transform.position.y;

        var currentNodeName = enemy.currentTile.name;
        var targetNodeName = tile.name;
        var linkLine = enemy.boardManager.FindLine(currentNodeName, targetNodeName);
        if (linkLine != null)
        {
            Transform lineType = null;
            for (var index = 0; index < linkLine.transform.childCount; index++)
            {
                if (linkLine.transform.GetChild(index).gameObject.activeSelf)
                {
                    lineType = linkLine.transform.GetChild(index);
                    break;
                }
            }
            if (lineType != null)
            {
                enemy.walkingLineType = lineType.name;
                enemy.up = height > 0 ? 1 : height < 0 ? -1 : 0;
            }
        }

        if(enemy.hearSoundTile != null || enemy.foundPlayerTile !=null)
        {
            enemy.m_animator.SetFloat("move_type", 1);
            tracingTarget = true;
        }
        else
        {
            enemy.m_animator.SetFloat("move_type", 0);
            tracingTarget = false;
        }
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
        var myPosition = character.tr_body.position;
        var targetPosition = nextStepTilePosition;
        var tdist = Vector3.Distance(new Vector3(myPosition.x, 0, myPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        if (tdist < 0.001f)
        {
            if(enemy.foundPlayerTile)
            {
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == character.targetDirection)
                {
                    if (character.currentTile.name == enemy.foundPlayerTile.name)
                    {
                        if ( enemy.turnOnReached && enemy.hearSoundTile == null)
                        {
                            // Debug.LogWarning("todo 能够看见主角,直接抓捕");
                            
                            var player = Game.Instance.player;
                            player.FindPathRealTime(player.gridManager.GetTileByName(character.currentTile.name));
                            var path = player.path;
                            var nextTileName = "";
                            if (path.Count >= 2)
                            {
                                nextTileName = path[path.Count - 2].name;
                            }
                            else
                            {
                                nextTileName = player.currentTile.name;
                            }

                            var lookToTile = character.gridManager.GetTileByName(nextTileName);
                            if (lookToTile)
                            {
                                var targetDirection = Utils.DirectionTo(character.currentTile, lookToTile, character.direction);
                                if (character.direction == targetDirection)
                                {
                                    character.Reached();
                                    if (!enemy.CatchPlayer() && !enemy.TryFoundPlayer())
                                    {
                                        enemy.LostTarget();
                                    }
                                    return true;
                                }
                                else
                                {
                                    Utils.SetDirection(character, targetDirection);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            var playerTile = character.gridManager.GetTileByName(Game.Instance.player.currentTile.name);
                            if (playerTile != null)
                            {
                                character.FindPathRealTime(playerTile);
                                character.UpdateTargetDirection(character.nextTile);
                                //character.ResetMoves();
                                if (character.direction == character.targetDirection)
                                {
                                    character.Reached();
                                    enemy.LostTarget();
                                    return true;
                                }
                                else
                                {
                                    Utils.SetDirection(character, character.targetDirection);
                                    return false;
                                }
                            }
                        }
                    }
                    character.Reached();
                    if (enemy.TryFoundPlayer())
                    {
                        enemy.currentAction = new ActionFoundPlayer(enemy);
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
                        if (enemy.turnOnReached)// 主角吹口哨的时候能看见主角，到达响声点之后要转向   todo 这个转向 要从玩家寻路到敌人的方向相反
                        {
                            var playerTile = character.gridManager.GetTileByName(Game.Instance.player.currentTile.name);
                            if (playerTile != null)
                            {
                                character.UpdateTargetDirection(playerTile);
                            }
                            if(character.direction == character.targetDirection)
                            {
                                character.Reached();
                                enemy.LostTarget();
                                return true;
                            }
                        }
                        else
                        {
                            enemy.Reached();
                            if(!enemy.TryFoundPlayer())
                            {
                                enemy.LostTarget();
                            }
                            return true;
                        }
                    }
                    else
                    {
                        character.Reached();
                        return true;
                    }
                }
            }
            else if(enemy.originalTile!=null)
            {
                if (character.currentTile.name == character.originalCoord.name)
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
                character.UpdateTargetDirection(character.nextTile);
                if (character.direction == character.targetDirection)
                {
                    character.Reached();
                    return true;
                }
            }
            else if(enemy.patroling)
            {
                var patrolEnemy = enemy as EnemyPatrol;
                if (patrolEnemy.needTurn())
                {
                    Utils.SetDirection(patrolEnemy, patrolEnemy.targetDirection);
                }
                else
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
            var dirPos = character.db_moves[1].position;
            Vector3 tar_dir = new Vector3(dirPos.x, character.tr_body.position.y, dirPos.z)  - character.tr_body.position;

            Vector3 new_dir = Vector3.RotateTowards(character.tr_body.forward, tar_dir, character.rotate_speed * Time.deltaTime / 2, 0f);

            new_dir.y = 0;
            character.tr_body.transform.rotation = Quaternion.LookRotation(new_dir);

            var angle = Vector3.Angle(tar_dir, character.tr_body.forward);
            
            if (angle <= 1 )
            {
                character.transform.forward = tar_dir;
                character.ResetDirection();
                character.body_looking = false;

                //var tdist = Vector3.Distance(character.tr_body.position, character.db_moves[0].position);
                //if (tdist < 0.01f)
                //{
                enemy.Reached();
                //enemy.OnTurnEnd();
                //}
            }
        }
        else if (character.moving)
        {
            float step = character.move_speed * Time.deltaTime;

            //if(tracingTarget)
            //{
                character.transform.position = Vector3.SmoothDamp(character.transform.position, character.db_moves[0].position + new Vector3(0, height, 0), ref velocity, 0.12f);
            //}
            //else
            //{
            //    character.transform.position = Vector3.MoveTowards(character.transform.position, character.db_moves[0].position + new Vector3(0, height, 0), step);
            //}
            
            var myPosition = character.tr_body.position;
            var targetPosition = character.db_moves[0].position;
            var tdist = Vector3.Distance(new Vector3(myPosition.x, 0, myPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
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
