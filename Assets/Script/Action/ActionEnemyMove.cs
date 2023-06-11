using UnityEngine;
using System.Collections.Generic;

public class ActionEnemyMove : ActionBase
{
    Vector3 velocity = new Vector3();
    private Vector3 targetPosition;
    float height = 0f;

    private bool reached = false;

    private Direction patrolTurnDirection;

    private bool actionForceBreak = false;

    private GridTile targetTile;

    // 转向等待时间
    private float turnEndWait = 0.25f;

    // 到达转向时间
    private float reachEndWait = 0.25f;

    // 等待时间
    private static float WAIT_TIME = 0.25f;

    public ActionEnemyMove(Enemy enemy, GridTile tile) : base(enemy, ActionType.EnemyMove)
    {
        targetTile = tile;

        actionForceBreak = false;

        if (enemy.coordPlayer.isLegal)
        {
            enemy.stepsAfterFoundPlayer++;
        }

        reached = false;

        patrolTurnDirection = enemy._direction;

        velocity = new Vector3();
        enemy.FindPathRealTime(tile,null, enemy.coordTracing.isLegal);

        // Debug.Log("位置偏移量:" + enemy.bodyPositionOffset);

        targetPosition = enemy.db_moves[0].position;
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
        enemy.m_animator.SetBool("moving", true);
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
        if (actionForceBreak) return true;
        var myPosition = character.transform.position;
        var tdist = Vector3.Distance(new Vector3(myPosition.x, 0, myPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        if (tdist >= 0.001f)
        {
            reachEndWait = WAIT_TIME;
            return false;
        }
        if(reachEndWait == WAIT_TIME)
        {
            enemy.Reached();
        }
        if (reachEndWait > 0.0f)
        {
            reachEndWait -= Time.deltaTime;
        }
        if (reachEndWait > 0) return false;

        if (!reached)
        {
            CheckNextStep();
            reached = true;
            enemy.Reached();
            var result = enemy.CheckPlayer();
            if(result != CheckPlayerResult.None)
            {
                return true;
            }
            // 巡逻敌人到达点之后检查是否需要转向
            var enemyPatrol = enemy as EnemyPatrol;
            if (enemyPatrol)
            {
                patrolTurnDirection = enemyPatrol.TurnDirection();
            }
        }

        if(enemy.coordTracing.isLegal)
        {
            return onReachedWhileTracing();
        }
        else if(enemy.originalTile != null)
        {
            return onReachedWhileReturning();
        }
        else if(enemy.patroling)
        {
            return onReachWhilePatrolling();
        }
        return true;
    }

    bool onReachWhilePatrolling()
    {
        var patrolEnemy = enemy as EnemyPatrol;
        if (patrolEnemy != null)
        {
            if (patrolTurnDirection != patrolEnemy._direction)
            {
                Utils.SetDirection(enemy, patrolTurnDirection);
                enemy.targetDirection = patrolTurnDirection;
                // enemy.LookAt(patrolEnemy.lastCoord.name);
                return false;
            }
            else
            {
                patrolEnemy.UpdateRouteMark();
                var result = patrolEnemy.CheckPlayer();

                // 巡逻敌人回头也需要检查敌人是否在警觉范围内
                if (result != CheckPlayerResult.None)
                {
                    return true;
                }
                patrolEnemy.ResetOriginal();
                //Debug.Log("巡逻敌人到达新的巡逻点");
                return true;
            }
        }
        return true;
    }

    bool onReachedWhileReturning()
    {
        if (enemy.coord.name == enemy.originalCoord.name)
        {
            // 回到原点要转向
            if (enemy._direction != enemy.originalDirection)
            {
                Utils.SetDirection(character, enemy.originalDirection);
                enemy.targetDirection = enemy.originalDirection;
                return false;
            }
            else
            {
                enemy.CheckPlayer();
                enemy.originalTile = null;
                return true;
            }
        }
        else
        {
            if (targetTile && targetTile.name != enemy.coord.name && enemy.nextTile == null)
            {
                enemy.FindPathRealTime(targetTile, null, true) ;
            }
            if(enemy.assignTile)
            {
                //Debug.Log("临时的点");
                enemy.FindPathRealTime(enemy.originalTile, null, true);
                enemy.assignTile = null;
            }
            if (enemy.nextTile)
            {
                enemy.LookAt(enemy.nextTile.name);
                var sameDirection = enemy._direction == enemy.targetDirection;
                if (sameDirection)
                {
                    enemy.CheckPlayer();
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (enemy.originalTile)
                {
                    var path = enemy.GetPathFromTo(enemy.originalTile, enemy.currentTile);
                    if (path.Count > 0)
                    {
                        enemy.LookAt(path[0]);
                    }
                    else
                    {
                        enemy.LookAt(enemy.originalTile.name);
                    }
                    var sameDirection = enemy._direction == enemy.targetDirection;
                    if (sameDirection)
                    {
                        enemy.CheckPlayer();
                    }
                    return sameDirection;
                }
                else
                {
                    return true;
                }
            }
        }
    }

    bool onReachedWhileTracing()
    {
        if (enemy.coord.Equals(enemy.coordTracing))
        {
            // 到达追踪点
            // 如果 coordPlayer 不合法，转向上一个路径点 如果合法，转向主角逃跑寻路点下一个路径点，这里要从主角开始寻路到敌人
            if (enemy.coordPlayer.isMin)
            {
                // 不转向
                //Debug.Log("到达追踪点不转向");
                var checkResult = enemy.CheckPlayer();
                if (checkResult == CheckPlayerResult.None)
                {
                    enemy.LostTarget();
                }
                return true;
            }
            else if (enemy.coordPlayer.isMax)
            {
                // 转向上一个路径点
                // Debug.Log("到达追踪点转向上一个路径点");
                enemy.LookAt(enemy.lastCoord.name);
                var sameDirection = enemy._direction == enemy.targetDirection;
                if (sameDirection)
                {
                    var checkResult = enemy.CheckPlayer();
                    if (checkResult == CheckPlayerResult.None)
                    {
                        enemy.LostTarget();
                    }
                }
                return sameDirection;
            }
            else
            {
                // 转向从主角寻路到敌人本身到倒数第二个点
                //var player = Game.Instance.player;
                //var lastTile = player.gridManager.GetTileByName(player.lastCoord.name);
                //var dstTile = player.gridManager.GetTileByName(enemy.currentTile.name);
                //var path = new List<string>();
                //var lookAtTileName = "";
                //if (player.lastCoord.name.Equals(enemy.currentTile.name))
                //{
                //    path = player.GetPathFromTo(dstTile, player.currentTile);
                //    if (path.Count >= 2)
                //    {
                //        lookAtTileName = path[path.Count - 2];
                //    }
                //    else
                //    {
                //        lookAtTileName = player.coord.name;
                //    }
                //}
                //else
                //{
                //    path = player.GetPathFromTo(dstTile, lastTile);
                //    if (path.Count >= 2)
                //    {
                //        lookAtTileName = path[path.Count - 2];
                //    }
                //    else
                //    {
                //        lookAtTileName = player.lastCoord.name;
                //    }
                //}
                var lookAtTileName = enemy.coordPlayer.name;


                if (!string.IsNullOrEmpty(lookAtTileName))
                {
                    enemy.LookAt(lookAtTileName);
                    var sameDirection = enemy._direction == enemy.targetDirection;
                    if (sameDirection)
                    {
                        var checkResult = enemy.CheckPlayer();
                        if (checkResult == CheckPlayerResult.None)
                        {
                            enemy.LostTarget();
                        }
                    }
                    return sameDirection;
                }
                else
                {
                    Debug.LogWarning("敌人看到主角后，到达追踪点转向异常");
                    return true;
                }
            }
        }
        else
        {
            if (targetTile && targetTile.name != enemy.coord.name && enemy.nextTile == null)
            {
                enemy.FindPathRealTime(targetTile, null,true);
            }
            // 继续转向下一个路径点
            if (enemy.nextTile)
            {
                enemy.LookAt(enemy.nextTile.name);
                var sameDirection = enemy._direction == enemy.targetDirection;
                if (sameDirection)
                {
                    enemy.CheckPlayer();
                }
                return sameDirection;
            }
            else
            {
                if(enemy.coordTracing.isLegal)
                {
                    var tileTracing = enemy.gridManager.GetTileByName(enemy.coordTracing.name);
                    var path = enemy.GetPathFromTo(tileTracing, enemy.currentTile);
                    if(path.Count>0)
                    {
                        enemy.LookAt(path[0]);
                    }
                    else
                    {
                        enemy.LookAt(enemy.coordTracing.name);
                    }
                    var sameDirection = enemy._direction == enemy.targetDirection;
                    if (sameDirection)
                    {
                        enemy.CheckPlayer();
                    }
                    return sameDirection;
                }
                else
                {
                    return true;
                }
            }
        }
    }

    public override void Run()
    {
        if (character.selected_tile_s != null && !character.moving && character.currentTile != character.selected_tile_s && character.selected_tile_s != null)
        {
            if (character.selected_tile_s.db_path_lowest.Count > 0)
                character.move_tile(character.selected_tile_s);
            //else
            //    Debug.Log("no valid tile selected");
        }

        // 先转向 再位移
        if (character.body_looking)
        {
            var dirPos = character.db_moves[1].position;
            Vector3 tar_dir = new Vector3(dirPos.x, character.tr_body.GetChild(0).position.y, dirPos.z)  - character.tr_body.position;

            Vector3 new_dir = Vector3.RotateTowards(character.tr_body.GetChild(0).forward, tar_dir, character.rotate_speed * Time.deltaTime / 2, 0f);

            new_dir.y = 0;
            character.tr_body.GetChild(0).transform.rotation = Quaternion.LookRotation(new_dir);

            var angle = Vector3.Angle(tar_dir, character.tr_body.GetChild(0).forward);

            
            if (angle <= 1 )
            {
                enemy.tr_body.GetChild(0).forward = tar_dir;
                if (turnEndWait == WAIT_TIME)
                {
                    enemy.Turned();
                }
                turnEndWait -= Time.deltaTime;
                if (turnEndWait <= 0)
                {
                    var result = enemy.CheckPlayer();
                    if (result != CheckPlayerResult.None)
                    {
                        actionForceBreak = true;
                        enemy.currentAction = null;
                    }
                }
            }
            else
            {
                turnEndWait = WAIT_TIME;
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
            
            var myPosition = character.transform.position;
            //var targetPosition = character.db_moves[0].position;
            var tdist = Vector3.Distance(new Vector3(myPosition.x, 0, myPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
            if (tdist < 0.001f)
            {
                //this.CheckNextStep();
            }
        }
    }

    private void CheckNextStep()
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
