using UnityEngine;

public class ActionEnemyMove : ActionBase
{
    Vector3 velocity = new Vector3();
    private Vector3 targetPosition;
    float height = 0f;
    //bool findPathSuccess = true;

    private bool reached = false;

    public ActionEnemyMove(Enemy enemy, GridTile tile) : base(enemy, ActionType.EnemyMove)
    {
        if (enemy.coordPlayer.isLegal)
        {
            enemy.stepsAfterFoundPlayer++;
        }
        reached = false;
        velocity = new Vector3();
        //var player = Game.Instance.player;
        //findPathSuccess =
            enemy.FindPathRealTime(tile);
        //if (!findPathSuccess)
        //{
        //    enemy.ShowNotFound();
        //    enemy.ReturnOriginal(false);
        //}
        //else
        //{
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
        //}
        
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
        var myPosition = character.transform.position;
        var tdist = Vector3.Distance(new Vector3(myPosition.x, 0, myPosition.z), new Vector3(targetPosition.x, 0, targetPosition.z));
        if (tdist < 0.001f)
        {
            if(!reached)
            {
                enemy.Reached();
                enemy.CheckPlayer();
                reached = true;
            }

            if(enemy.coordTracing.isLegal)
            {
                return onReachedWhileTracing();
            }
            else if(enemy.originalTile != null)
            {
                return onReachedWhileReturning();
            }


            return OnReachPosition();
        }
        return false;
    }

    bool onReachedWhileReturning()
    {
        if (enemy.coord.name == enemy.originalCoord.name)
        {
            // 回到原点要转向
            if (enemy.direction != enemy.originalDirection)
            {
                Utils.SetDirection(character, enemy.originalDirection);
                var sameDirection = enemy._direction == enemy.originalDirection;
                if(sameDirection)
                {
                    enemy.CheckPlayer();
                    enemy.originalTile = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                enemy.originalTile = null;
                return true;
            }
        }
        else
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
                Debug.Log("到达追踪点不转向");
                var checkResult = enemy.CheckPlayer();
                if (!checkResult)
                {
                    enemy.LostTarget();
                }
                return true;
            }
            else if (enemy.coordPlayer.isMax)
            {
                // 转向上一个路径点
                Debug.Log("到达追踪点转向上一个路径点");
                enemy.LookAt(enemy.lastCoord.name);
                var sameDirection = enemy._direction == enemy.targetDirection;
                if (sameDirection)
                {
                    var checkResult = enemy.CheckPlayer();
                    if (!checkResult)
                    {
                        enemy.LostTarget();
                    }
                }
                return sameDirection;
            }
            else
            {
                // 转向从主角寻路到敌人本身到倒数第二个点
                var player = Game.Instance.player;
                player.FindPathRealTime(player.gridManager.GetTileByName(character.currentTile.name));
                var path = player.path;
                var nextTileName = "";
                if (path.Count >= 2)
                {
                    nextTileName = path[path.Count - 2];
                }
                else
                {
                    nextTileName = player.currentTile.name;
                }
                Debug.Log("到达追踪点转向转向从主角寻路到敌人本身到倒数第二个点");
                enemy.LookAt(nextTileName);
                var sameDirection = enemy._direction == enemy.targetDirection;
                if (sameDirection)
                {
                    var checkResult = enemy.CheckPlayer();
                    if (!checkResult)
                    {
                        enemy.LostTarget();
                    }
                }
                return sameDirection;
            }
        }
        else
        {
            // 继续转向下一个路径点
            enemy.LookAt(character.nextTile.name);
            var sameDirection = enemy._direction == enemy.targetDirection;
            if (sameDirection)
            {
                enemy.CheckPlayer();
            }
            return sameDirection;
        }
    }


    bool OnReachPosition()
    {
        
        //else if (enemy.originalTile != null)
        //{
        //    if (character.currentTile.name == character.originalCoord.name)
        //    {
        //        // 回到原点要转向
        //        if (character.direction != character.originalDirection)
        //        {
        //            // character.originalDirection = Direction.Up;// for test 
        //            Utils.SetDirection(character, character.originalDirection);
        //            return false;
        //        }
        //        else
        //        {
        //            enemy.originalTile = null;
        //            enemy.OnReachedOriginal();
        //            character.Reached();
        //            return true;
        //        }
        //    }
        //    character.UpdateTargetDirection(character.nextTile);
        //    if (character.direction == character.targetDirection)
        //    {
        //        character.Reached();
        //        return true;
        //    }
        //}
        //else if (enemy.patroling)
        //{
        //    var patrolEnemy = enemy as EnemyPatrol;
        //    if (patrolEnemy.needTurn())
        //    {
        //        Utils.SetDirection(patrolEnemy, patrolEnemy.targetDirection);
        //    }
        //    else
        //    {
        //        character.Reached();
        //        return true;
        //    }
        //}
        //else
        //{
        //    character.Reached();
        //    return true;
        //}

        return true;
    }


    void OnTurnToDirection()
    {

    }

    void OnActionEnd()
    {

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
            Vector3 tar_dir = new Vector3(dirPos.x, character.tr_body.GetChild(0).position.y, dirPos.z)  - character.tr_body.position;

            Vector3 new_dir = Vector3.RotateTowards(character.tr_body.GetChild(0).forward, tar_dir, character.rotate_speed * Time.deltaTime / 2, 0f);

            new_dir.y = 0;
            character.tr_body.GetChild(0).transform.rotation = Quaternion.LookRotation(new_dir);

            var angle = Vector3.Angle(tar_dir, character.tr_body.GetChild(0).forward);
            
            if (angle <= 1 )
            {
                enemy.tr_body.GetChild(0).forward = tar_dir;
                enemy.Turned();
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
