using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class GameCamera : MonoBehaviour
{
    public static GameCamera Instance;

    public CameraMultiTarget multiTarget;

    public Camera cam;

    public float distanceUpAndDown = 0;

    public float distanceLeftAndRight = 0;

    public GameObject multiNodeParent;

    public Vector3 followingPosition;

    public void OnSceneLoaded()
    {
        if (Game.Instance == null) return;

        if (multiNodeParent != null)
        {
            Destroy(multiNodeParent);
        }

        multiNodeParent = new GameObject("Multy Target Camera Nodes");
        multiNodeParent.transform.position = Vector3.zero;

        List<GameObject> nodeGameObjects = new List<GameObject>();
        var playerPosition = Game.Instance.player.transform.position;

        foreach (var kvp in Game.Instance.boardManager.nodes)
        {
            var node = kvp.Value;
            var nodePosition = node.transform.position;
            var distance = Vector3.Distance(nodePosition, playerPosition);
            if (distance <= 4)
            {
                // 用这个不准，可能是与MultiTargetCamera的Update有关
                //var copyNode = new GameObject(kvp.Value.gameObject.name);
                //copyNode.transform.parent = multiNodeParent.transform;
                //copyNode.transform.position = kvp.Value.transform.position;
                nodeGameObjects.Add(kvp.Value.gameObject);
            }
        }
        multiTarget.SetTargets(nodeGameObjects.ToArray());
        multiTarget.enabled = true;

        //multiTarget.InitTargets();
    }


    public void SeeAllNodes()
    {
        List<GameObject> nodeGameObjects = new List<GameObject>();
        foreach (var kvp in Game.Instance.boardManager.nodes)
        {
            var node = kvp.Value;
            nodeGameObjects.Add(kvp.Value.gameObject);
        }
        multiTarget.SetTargets(nodeGameObjects.ToArray());
    }

    public void RecoverCamera()
    {
        multiTarget.RecoverCamera();
    }

    Ray ray;

    public void MultiTargetCameraUpdateTargets()
    {
        #region no use
        //if (Game.Instance == null) return;
        //var player = Game.Instance.player;
        //var playerPosition = player.transform.position;

        //Vector3 screenPoint = new Vector3();
        //UiUtils.WorldToScreenPoint(cam, Game.Instance.gameCanvas, playerPosition, out screenPoint);


        //var limitScreenPoint = new Vector2(screenPoint.x, screenPoint.y);

        //var moveDirection = 0;
        //if (screenPoint.y > 0)
        //{
        //    var distanceFromTop = Screen.height / 2 - screenPoint.y;
        //    if (distanceFromTop < 350)
        //    {
        //        moveDirection = 1;
        //        limitScreenPoint.y = 350;
        //        Debug.Log("镜头向上移动");
        //        Debug.Log("镜头玩家位置 [ x: " + screenPoint.x + ", y:" + screenPoint.y + " ]" + distanceFromTop);
        //    }
        //}
        //else if (screenPoint.y < 0)
        //{
        //    var distanceFromBottom = Screen.height / 2 + screenPoint.y;
        //    if (distanceFromBottom < 200)
        //    {
        //        moveDirection = 2;
        //        limitScreenPoint.y = -350;
        //        Debug.Log("镜头向下移动");
        //        Debug.Log("镜头玩家位置 [ x: " + screenPoint.x + ", y:" + screenPoint.y + " ]" + distanceFromBottom);
        //    }
        //}
        //if (screenPoint.x > 0)
        //{
        //    var distanceFromRight = Screen.width / 2 - screenPoint.x;
        //    if (distanceFromRight < 350 )
        //    {
        //        moveDirection = 3;
        //        limitScreenPoint.x = 350;

        //        Debug.Log("镜头右向移动");
        //        Debug.Log("镜头玩家位置 [ x: " + screenPoint.x + ", y:" + screenPoint.y + " ]" + distanceFromRight);
        //    }
        //}
        //else if (screenPoint.x < 0)
        //{
        //    var distanceFromLeft = Screen.width / 2 + screenPoint.x;
        //    if (distanceFromLeft < 350 )
        //    {
        //        moveDirection = 4;
        //        limitScreenPoint.x = -350;

        //        Debug.Log("镜头向左移动");
        //        Debug.Log("镜头玩家位置 [ x: " + screenPoint.x + ", y:" + screenPoint.y + " ]" + distanceFromLeft);
        //    }
        //}

        //var inputPos = new Vector3(Screen.width / 2 + limitScreenPoint.x, Screen.height / 2 + limitScreenPoint.y);
        //ray = RectTransformUtility.ScreenPointToRay(cam, inputPos);


        ////if(Input.GetMouseButtonDown(0))
        ////{
        ////    var inputPosition = Input.mousePosition;
        ////}

        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit,100, LayerMask.GetMask("Ground")))
        //{
        //    Debug.Log("wolrdPoint" + hit.transform.position);
        //    var moveVec3 = Vector3.zero;
        //    if(moveDirection != 0)
        //    {
        //        if(moveDirection == 1)// 上
        //        {
        //            moveVec3 = hit.point - player.transform.position;
        //        }
        //        else if (moveDirection == 2)// 下
        //        {
        //            moveVec3 = player.transform.position - hit.point;
        //        }
        //        else if (moveDirection == 3)// 右
        //        {
        //            moveVec3 = hit.point - player.transform.position;
        //        }
        //        else if (moveDirection == 4)// 左
        //        {
        //            moveVec3 = hit.point - player.transform.position;
        //        }
        //        moveVec3.y = 0;
        //        followingPosition = transform.position + moveVec3;
        //        //multiNodeParent.transform.Translate(direction);
        //        Game.Instance.gameCanvas.playerPos.rectTransform.anchoredPosition = new Vector3(inputPos.x, inputPos.y, 0);

        //        Debug.Log("镜头移动" + moveDirection + ":" +
        //            " followingPosition.x:" + followingPosition.x +
        //            " followingPosition.y:" + followingPosition.y +
        //            " followingPosition.z:" + followingPosition.z);

        //        Debug.Log("镜头偏移" + moveDirection + ":" +
        //            " moveVec3.x:" + moveVec3.x +
        //            " moveVec3.y:" + moveVec3.y +
        //            " moveVec3.z:" + moveVec3.z);
        //        //Debug.Break();
        //    }
        //}

        #endregion

        if (Game.Instance == null) return;
        var player = Game.Instance.player;
        if (player == null) return;
        var node = player.boardManager.FindNode(player.coord.name);
        if (node == null) return;
        var targetPositionIndex = node.targetPositionIndex;
        if (targetPositionIndex == -1) return;
        multiTarget.ChangeCameraTargetPosition(targetPositionIndex);
    }

    private void Update()
    {
        #region no use
        //MultiTargetCameraUpdateTargets();
        //    Debug.DrawLine(ray.origin, ray.origin + ray.direction * 20, Color.red);

        //    if(multiTarget.stopContainTargets)
        //    {
        //        Vector3 velocity = Vector3.zero;
        //        transform.position = Vector3.SmoothDamp(transform.position, followingPosition, ref velocity, multiTarget.moveSmoothTime);
        //    }
        //    else
        //    {
        //        followingPosition = transform.position;
        //    }

        //    if (Game.Instance== null) return;
        //    var keepMoving = false;
        //    var player = Game.Instance.player;
        //    var playerPosition = player.transform.position;
        //    Vector3 screenPoint = new Vector3();
        //    UiUtils.WorldToScreenPoint(cam, Game.Instance.gameCanvas, playerPosition, out screenPoint);
        //    if (screenPoint.y > 0)
        //    {
        //        var distanceFromTop = Screen.height / 2 - screenPoint.y;
        //        if (distanceFromTop < 250)
        //        {
        //            keepMoving = true;
        //        }
        //    }
        //    else if (screenPoint.y < 0)
        //    {
        //        var distanceFromBottom = Screen.height / 2 + screenPoint.y;
        //        if (distanceFromBottom < 200)
        //        {
        //            keepMoving = true;
        //        }
        //    }
        //    if (screenPoint.x > 0)
        //    {
        //        var distanceFromRight = Screen.width / 2 - screenPoint.x;
        //        if (distanceFromRight < 350)
        //        {
        //            keepMoving = true;
        //        }
        //    }
        //    else if (screenPoint.x < 0)
        //    {
        //        var distanceFromLeft = Screen.width / 2 + screenPoint.x;
        //        if (distanceFromLeft < 350)
        //        {
        //            keepMoving = true;
        //        }
        //    }

        //    if(!keepMoving)
        //    {
        //        followingPosition = transform.position;
        //    }
        #endregion
    }
}
