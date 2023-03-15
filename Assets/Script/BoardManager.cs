using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    public int width;

    [SerializeField]
    public int height;

    [SerializeField]
    public Transform squareRoot;

    //[SerializeField]
    //public Transform visualRoot;

    [SerializeField]
    public Transform enemyRoot;

    [SerializeField]
    public Transform linkRoot;

    [SerializeField]
    public Transform itemRoot;

    [SerializeField]
    public GridManager playerGridManager;

    [SerializeField]
    public GridManager enemyGridManager;

    [SerializeField][Tooltip("显示/隐藏白模节点")]
    public bool tirggerVisibleNode = true;

    public new string name;

    public Player player;

    public List<Enemy> enemies = new List<Enemy>();

    public List<string> dangerNodeNames = new List<string>();

    public List<WalkThroughStep> steps = new List<WalkThroughStep>();

    private void Awake()
    {
        playerGridManager.gameObject.SetActive(true);
        enemyGridManager.gameObject.SetActive(true);
        name = gameObject.name;
        gameObject.name = "BoardManager";
        ResetItems();
        ResetEnemies();
        ResetSquareNodes();
        Game.Instance.SceneLoaded(this, name);
    }


    // Update is called once per frame
    void Update() {
        
    }

    #region RunTime

    public Dictionary<string, Item> allItems = new Dictionary<string, Item>();

    public Dictionary<string, BoardNode> nodes = new Dictionary<string, BoardNode>();

    void ResetItems()
    {
        for (var index = 0; index < itemRoot.childCount; index++)
        {
            var itemTr = itemRoot.GetChild(index);
            var item = itemTr.GetComponent<Item>();
            if (item == null)
            {
                Debug.Log(string.Format("未挂载脚本Item{0}", itemTr.name));
                continue;
            }
            item.Init();
            allItems.Add(item.coord.name, item);
            switch (itemTr.name)
            {
                case ItemName.Item_Star:
                    //star = item;
                    break;
                case ItemName.Item_Pincers:
                    //pincerses.Add(itemTr.GetComponent<Item>());
                    break;
                case ItemName.Item_ManholeCover:
                    //manholeCovers.Add(item);
                    break;
                case ItemName.Item_LureBottle:
                    //bottles.Add(item);
                    break;
                case ItemName.item_Growth:
                    //grouthes.Add(item);
                    break;
                case ItemName.Item_Graff:
                    //graff = item;
                    break;
                case ItemName.Item_End:
                    //end = item;
                    break;
                default:
                    Debug.LogError(string.Format("未处理未定义Item{0}", itemTr.name));
                    break;
            }
        }
    }

    void ResetEnemies()
    {
        for (var index = 0; index < enemyRoot.childCount; index++)
        {
            var enemyTr = enemyRoot.GetChild(index);
            var enemy = enemyTr.GetComponent<Enemy>();
            if (enemy == null)
            {
                Debug.Log(string.Format("未挂载脚本Enemy{0}", enemyTr.name));
                continue;
            }
            
            enemies.Add(enemy);

            switch (enemyTr.name)
            {
                case EnemyName.Enemy_Static:
                case EnemyName.Enemy_Sentinel:
                case EnemyName.Enemy_Patrol:
                case EnemyName.Enemy_Distracted:
                    break;
                default:
                    Debug.LogError(string.Format("未定义敌人{0}", enemyTr.name));
                    break;
            }
        }
    }

    public void ResetSquareNodes()
    {
        for(var index = 0; index < squareRoot.childCount; index++)
        {
            var boardNodeGameObject = squareRoot.GetChild(index);
            var boardNode = boardNodeGameObject.GetComponent<BoardNode>();
            if(boardNode)
            {
                nodes.Add(boardNode.coord.name, boardNode);
                boardNode.contour.gameObject.SetActive(false);
            }
        }
    }

    public void HideAllSuqreContour()
    {
        for (var index = 0; index < squareRoot.childCount; index++)
        {
            var boardNodeGameObject = squareRoot.GetChild(index);
            var boardNode = boardNodeGameObject.GetComponent<BoardNode>();
            if (boardNode)
            {
                boardNode.contour.gameObject.SetActive(false);
            }
        }
    }

    public void PickItem(string name, Player player)
    {
        var item = allItems.ContainsKey(name) ? allItems[name] : null;
        if (item)
        {
            if (item.Picked(player))
            {
                allItems.Remove(name);
            }
        }
    }

    public LinkLine FindLine(string node1, string node2)
    {
        for(var index = 0; index < linkRoot.childCount; index++)
        {
            var linkLineGo = linkRoot.GetChild(index);
            var linkLine = linkLineGo.GetComponent<LinkLine>();
            var linkingNodes = linkLine.LinkingNode(node1, node2);
            if(linkingNodes)
            {
                return linkLine;
            }
        }
        return null;
    }

    public BoardNode FindNode(string name, bool inCludeDisable=false)
    {
        for (var index = 0; index < squareRoot.childCount; index++)
        {
            var child = squareRoot.GetChild(index);
            var boardNode = child.GetComponent<BoardNode>();
            if(child.gameObject.name == name )
            {
                if(inCludeDisable)
                {
                    return boardNode;
                }
                else
                {
                    if(child.gameObject.activeSelf)
                    {
                        return boardNode;
                    }
                }
            }
        }
        return null;
    }


    //public void ClearReds()
    //{
    //    redNodes.ForEach((BoardNode node) =>
    //    {
    //        node.targetIcon.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/UI_Light_Blue_Mat.mat");
    //    });

    //    redLines.ForEach((LinkLine line) =>
    //    {
    //        line.transform.GetChild(0).GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/UI_Light_Blue_Mat.mat");
    //    });
    //}

    //public void RedNode(BoardNode boardNode)
    //{
    //    if (boardNode == null) return;
    //    boardNode.targetIcon.GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/UI_Red_Mat.mat");
    //}

    //public void RedLine(LinkLine linkLine)
    //{
    //    if (linkLine == null) return;
    //    linkLine.transform.GetChild(0).GetComponent<MeshRenderer>().material = Resources.Load<Material>("Material/UI_Red_Mat.mat");
    //}

    public Dictionary<string,BoardNode> FindNodesAround(string curNodeName, int range, bool inCludeDisable = false)
    {
        var nodes = new Dictionary<string,BoardNode>();

        var coordArr = curNodeName.Split('_');
        var x = int.Parse(coordArr[0]);
        var z = int.Parse(coordArr[1]);

        for(var i = 0; i < range; i++)
        {
            for(var j = 0; j < range; j++)
            {
                var name = (x + i).ToString() + "_" + (z + j).ToString();
                var node = FindNode(name, inCludeDisable);
                if (node != null && !nodes.ContainsKey(name))
                {
                    nodes.Add(name,node);
                }
                name = (x + i).ToString() + "_" + (z - j).ToString();
                node = FindNode(name, inCludeDisable);
                if (node != null && !nodes.ContainsKey(name))
                {
                    nodes.Add(name,node);
                }
                name = (x - i).ToString() + "_" + (z + j).ToString();
                node = FindNode(name, inCludeDisable);
                if (node != null && !nodes.ContainsKey(name))
                {
                    nodes.Add(name,node);
                }
                name = (x - i).ToString() + "_" + (z - j).ToString();
                node = FindNode(name, inCludeDisable);
                if (node != null && !nodes.ContainsKey(name))
                {
                    nodes.Add(name,node);
                }
            }
        }
        //Debug.Log("范围节点名字");
        return nodes;
    }
    #endregion



    #region Editor Process 编辑器代码

#if UNITY_EDITOR
    private void OnValidate()
    {
        //visualRoot?.gameObject.SetActive(tirggerVisibleNode);
    }



    [ContextMenu("处理关卡(不要点)")]
    void Process()
    {
        FindNodes();
        ProcessSquareNodes();
        ProcessVisialNodes();
        ProcessLinkedNodes();
        ProcessItem();
        ProcessEnemy();
        ProcessPlayer();

        var SceneNodeGameObject = GameObject.Find("Scene");
        SceneNodeGameObject.transform.localPosition = Vector3.zero;
        // SaveAsPrefab(Selection.activeGameObject);
    }


    void FindNodes()
    {
        var isAnyPrefabInstanceRoot = PrefabUtility.IsAnyPrefabInstanceRoot(gameObject);
        if (isAnyPrefabInstanceRoot)
        {
            PrefabUtility.UnpackPrefabInstance(gameObject, PrefabUnpackMode.Completely, InteractionMode.UserAction);
        }
        
        var boardNode = transform.Find("Board_1");
        if(boardNode != null)
        {
            squareRoot = boardNode.Find("BoardSquares_Root");
            //visualRoot = boardNode.GetChild(1);
            enemyRoot = boardNode.Find("Enemies_Root");
            linkRoot = boardNode.Find("Links_Root");
            linkRoot.gameObject.SetActive(true);
            itemRoot = boardNode.Find("ItemRoot");
            if(itemRoot==null)
            {
                var itemRootGo = new GameObject("ItemRoot");
                itemRoot = itemRootGo.transform;
                itemRoot.parent = boardNode;
            }
        }
    }

    void ProcessSquareNodes()
    {
        var minX = int.MaxValue;
        var minZ = int.MaxValue;

        for(var index = 0; index < squareRoot.childCount; index++)
        {
            var nodeTransform = squareRoot.GetChild(index);
            nodeTransform.name = nodeTransform.name.Replace("Square_", "");
            var childCount = nodeTransform.childCount;
            while(childCount>0)
            {
                var childGameObject = nodeTransform.GetChild(0).gameObject;
                DestroyImmediate(childGameObject);
                childCount = nodeTransform.childCount;
            }

            var base_Square = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/__Resources/Prefab/Base_Square.prefab");
            var base_SquareInstance = Instantiate(base_Square);
            base_SquareInstance.transform.parent = nodeTransform;
            base_SquareInstance.name = "Base_Square";
            base_SquareInstance.transform.localPosition = new Vector3(0, 0, 0);

            var coord = nodeTransform.name.Split('_');

            var coordX = int.Parse(coord[0]);

            var coordZ = int.Parse(coord[1]);


            if(coordX < minX)
            {
                minX = coordX;
            }

            if(coordZ < minZ)
            {
                minZ = coordZ;
            }

            //nodeTransform.transform.localPosition = new Vector3(coordX, 0, coordZ);

            var script = nodeTransform.GetComponent<BoardNode>();

            if(script == null)
            {
                script = nodeTransform.gameObject.AddComponent<BoardNode>();
            }

            script.initWithTransform(nodeTransform);
        }

        Debug.Log(string.Format("X坐标最小值{0},Z坐标最小值{1}",minX,minZ));
        for (var index = 0; index < squareRoot.childCount; index++)
        {
            var nodeTransform = squareRoot.GetChild(index);
            var nodeCoordArr = nodeTransform.name.Split('_');
            var coordX = int.Parse(nodeCoordArr[0]);
            var coordZ = int.Parse(nodeCoordArr[1]);
            coordX -= minX;
            coordZ -= minZ;
            nodeTransform.name = string.Format("{0}_{1}", coordX, coordZ);
        }

        var lastSquare = squareRoot.GetChild(squareRoot.childCount - 1);
        var widthAndHeight = lastSquare.name.Split('_');
        width = int.Parse(widthAndHeight[0]);
        height = int.Parse(widthAndHeight[1]);
    }

    void ProcessVisialNodes()
    {
        //if (visualRoot == null) return;
        //for (var index = 0; index < visualRoot.childCount; index++)
        //{
        //    var nodeGameObject = visualRoot.GetChild(index);
        //    var nameArr = nodeGameObject.name.Split(' ');
        //    nodeGameObject.name = nameArr[0];
        //}
    }

    void ProcessLinkedNodes()
    {
        for(var index = 0; index < linkRoot.childCount; index++)
        {
            var linkNodeGameObject = linkRoot.GetChild(index);
            var gameObjectName = linkNodeGameObject.name.Replace("Link_Square_", "").Replace(" - Square_", "=");
            linkNodeGameObject.name = gameObjectName;
            for(var childIndex = 0; childIndex < linkNodeGameObject.childCount; childIndex++)
            {
                var childGameObject =  linkNodeGameObject.GetChild(childIndex);
                var childNameArr = childGameObject.name.Split(' ');
                childGameObject.name = childNameArr[0];
            }

            var linkLine = linkNodeGameObject.GetComponent<LinkLine>() ?? linkNodeGameObject.gameObject.AddComponent<LinkLine>();
            var names = linkNodeGameObject.gameObject.name.Split('=');
            linkLine.node1 = names[0];
            linkLine.node2 = names[1];

            var lineGameObject = linkNodeGameObject.GetChild(0);
            var lineName = lineGameObject.name;
            switch(lineName)
            {
                case "Hor_Doted_Visual":
                case "Hor_Fenced_Visual":
                case "Hor_Normal_Visual":
                    break;
            }
            var prefab = Resources.Load("Prefab/" + lineName);
            if(prefab!=null)
            {
                DestroyImmediate(lineGameObject.gameObject);
                var linInstance = Instantiate(prefab, linkNodeGameObject.transform) as GameObject;
                linInstance.transform.localPosition = Vector3.zero;
                linInstance.name = lineName;
            }
            else
            {
                Debug.LogError("No Line Preafab Named:" + lineName);
            }
        }
    }

    //[ContextMenu("替换物品预设")]
    void ProcessItem()
    {
        var prefabRoot = "Assets/__Resources/Prefab/{0}.prefab";
        var childCount = itemRoot.childCount;
        for ( var index = 0; index < childCount; index++ )
        {
            var itemTransform = itemRoot.GetChild(index);
            var prefapUrl = string.Format(prefabRoot, itemTransform.name);
            //Debug.Log(prefapUrl);
            var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefapUrl);
            var itemInstance = Instantiate(itemPrefab);
            itemInstance.transform.parent = itemRoot;
            itemInstance.gameObject.name = itemTransform.name;
            itemInstance.transform.localPosition = new Vector3(itemTransform.localPosition.x, itemTransform.localPosition.y, itemTransform.localPosition.z);
            itemInstance.transform.localRotation = itemTransform.localRotation;
            itemInstance.transform.SetSiblingIndex(itemTransform.GetSiblingIndex());
            var itemScript = itemInstance.GetComponent<Item>();
            if(itemScript == null)
            {
                itemScript = itemInstance.AddComponent<Item>();
            }
            itemScript.coord = new Coord(itemTransform.position);
            switch (itemTransform.name)
            {
                case ItemName.Item_Star:
                    itemScript.itemType = ItemType.Star;
                    break;
                case ItemName.Item_Pincers:
                    itemScript.itemType = ItemType.Pincers;
                    break;
                case ItemName.Item_ManholeCover:
                    itemScript.itemType = ItemType.ManHoleCover;
                    break;
                case ItemName.Item_LureBottle:
                    itemScript.itemType = ItemType.LureBottle;
                    break;
                case ItemName.item_Growth:
                    itemScript.itemType = ItemType.Growth;
                    break;
                case ItemName.Item_Graff:
                    itemScript.itemType = ItemType.Graff;
                    break;
                case ItemName.Item_End:
                    itemScript.itemType = ItemType.End;
                    break;
                default:
                    Debug.LogError(string.Format("没有定义Item{0}", itemTransform.name));
                    break;
            }
            DestroyImmediate(itemTransform.gameObject);
        }
    }

    //[ContextMenu("替换敌人预设")]
    void ProcessEnemy()
    {
        var prefabRoot = "Assets/__Resources/Prefab/{0}.prefab";
        var childCount = enemyRoot.childCount;
        for (var index = 0; index < childCount; index++)
        {
            var enemyTransform = enemyRoot.GetChild(index);
            var prefapUrl = string.Format(prefabRoot, enemyTransform.name);
            //Debug.Log(prefapUrl);
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefapUrl);
            var enemyInstance = Instantiate(enemyPrefab);
            //PrefabUtility.ApplyPrefabInstance(enemyInstance,InteractionMode.UserAction);
            var eulerY = enemyTransform.transform.eulerAngles.y;
            while(eulerY < 0)
            {
                eulerY += 360;
            }
            while (eulerY > 360)
            {
                eulerY -= 360;
            }
            var isDown = eulerY == 180;
            var isUp = eulerY ==360 || eulerY == 0;
            var isLeft = eulerY == 270;
            var isRight = eulerY == 90;
            var direction = Direction.Up;
            if (isDown)
            {
                direction = Direction.Down;
            }
            else if (isUp)
            {
                direction = Direction.Up;
            }
            else if (isLeft)
            {
                direction = Direction.Left;
            }
            else if (isRight)
            {
                direction = Direction.Right;
            }

            enemyInstance.GetComponent<Enemy>().direction = direction;
            enemyInstance.transform.parent = enemyRoot;
            enemyInstance.gameObject.name = enemyTransform.name;
            enemyInstance.transform.localPosition = new Vector3(Mathf.RoundToInt(enemyTransform.localPosition.x), enemyTransform.localPosition.y, Mathf.RoundToInt(enemyTransform.localPosition.z));
            enemyInstance.transform.localRotation = enemyTransform.localRotation;
            enemyInstance.transform.SetSiblingIndex(enemyTransform.GetSiblingIndex());
            Enemy enemyScript = enemyInstance.GetComponent<Enemy>();
            Debug.Log("敌人:" + enemyInstance.name + " 方向:" + direction);
            switch (enemyTransform.name)
            {
                case EnemyName.Enemy_Static:
                    // enemyScript = enemyInstance.AddComponent<EnemyStatic>();
                    enemyScript.enemyType = EnemyType.Static;
                    break;
                case EnemyName.Enemy_Distracted:
                    // enemyScript = enemyInstance.AddComponent<EnemyDistracted>();
                    enemyScript.enemyType = EnemyType.Distracted;
                    break;
                case EnemyName.Enemy_Sentinel:
                    // enemyScript = enemyInstance.AddComponent<EnemySentinel>();
                    enemyScript.enemyType = EnemyType.Sentinel;
                    break;
                case EnemyName.Enemy_Patrol:
                    // enemyScript = enemyInstance.AddComponent<EnemyPatrol>();
                    enemyScript.enemyType = EnemyType.Patrol;
                    break;
                default:
                    Debug.LogError(string.Format("没有定义敌人{0}", enemyInstance.name));
                    break;
            }

            if(enemyScript!=null)
            {
                enemyScript.coord = new Coord(enemyTransform.position);
            }
            else
            {
                Debug.LogError(string.Format("没有绑定敌人脚本{0}", enemyInstance.name));
            }
            DestroyImmediate(enemyTransform.gameObject);
        }
    }

    void ProcessPlayer()
    {
        var go = GameObject.Find("Player");
        var player = go.GetComponent<Player>();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/__Resources/Prefab/Player.prefab");
        var playerInstance = Instantiate(prefab);
        playerInstance.name = player.name;
        playerInstance.transform.position = go.transform.position;
        playerInstance.transform.rotation = go.transform.rotation;
        
        var eulerY = playerInstance.transform.eulerAngles.y;
        while (eulerY < 0)
        {
            eulerY += 360;
        }
        while (eulerY > 360)
        {
            eulerY -= 360;
        }
        var isDown = eulerY == 180;
        var isUp = eulerY == 360 || eulerY == 0;
        var isLeft = eulerY == 270;
        var isRight = eulerY == 90;
        var direction = Direction.Up;
        if (isDown)
        {
            direction = Direction.Down;
        }
        else if (isUp)
        {
            direction = Direction.Up;
        }
        else if (isLeft)
        {
            direction = Direction.Left;
        }
        else if (isRight)
        {
            direction = Direction.Right;
        }
        Debug.Log("主角:" + playerInstance.name + " 方向:" + direction);
        playerInstance.GetComponent<Player>().direction = direction;
        DestroyImmediate(go);
    }

    [ContextMenu("保存预设")]
    void SaveAsPrefab()
    {
        var chapterAndLevel = gameObject.name.Split('-');
        var chapter = int.Parse(chapterAndLevel[0]);
        var level = int.Parse(chapterAndLevel[1]);
        var url = string.Format("Assets/__Resources/Prefab/Level/{0}/{1}.prefab", chapter, gameObject.name);
        PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, url,InteractionMode.UserAction);
        Debug.Log(string.Format("保存关卡预设{0}", url));
    }
#endif

    #endregion
}
