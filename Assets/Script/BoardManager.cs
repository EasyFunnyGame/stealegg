using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    public Transform squareRoot;

    [SerializeField]
    public Transform visualRoot;

    [SerializeField]
    public Transform enemyRoot;

    [SerializeField]
    public Transform linkRoot;

    [SerializeField]
    public Transform itemRoot;

    [SerializeField][Tooltip("显示/隐藏白模节点")]
    public bool tirggerVisibleNode = true;


    // Start is called before the first frame update
    void Start() { }

    // Update is called once per frame
    void Update() { }


    #region Editor Process

    private void OnValidate()
    {
        if(visualRoot==null)
        {
            return;
        }
        visualRoot.gameObject.SetActive(tirggerVisibleNode);
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
        SaveAsPrefab(Selection.activeGameObject);
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
            squareRoot = boardNode.GetChild(0);
            visualRoot = boardNode.GetChild(1);
            enemyRoot = boardNode.GetChild(2);
            linkRoot = boardNode.GetChild(3);
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

            var base_Square = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefab/Item/Base_Square.prefab");
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

            nodeTransform.transform.localPosition = new Vector3(coordX, 0, coordZ);

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
    }

    void ProcessVisialNodes()
    {
        for (var index = 0; index < visualRoot.childCount; index++)
        {
            var nodeGameObject = visualRoot.GetChild(index);
            var nameArr = nodeGameObject.name.Split(' ');
            nodeGameObject.name = nameArr[0];
        }
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
        }
    }

    [ContextMenu("替换物品预设")]
    void ProcessItem()
    {
        var prefabRoot = "Assets/Resources/Prefab/Item/{0}.prefab";
        var childCount = itemRoot.childCount;
        for ( var index = 0; index < childCount; index++ )
        {
            var itemTransform = itemRoot.GetChild(index);
            var prefapUrl = string.Format(prefabRoot, itemTransform.name);
            Debug.Log(prefapUrl);
            var itemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefapUrl);
            var itemInstance = Instantiate(itemPrefab);
            itemInstance.transform.parent = itemRoot;
            itemInstance.gameObject.name = itemTransform.name;
            itemInstance.transform.localPosition = new Vector3(itemTransform.localPosition.x, itemTransform.localPosition.y, itemTransform.localPosition.z);
            itemInstance.transform.localRotation = itemTransform.localRotation;
            itemInstance.transform.SetSiblingIndex(itemTransform.GetSiblingIndex());
            var itemScript = itemInstance.AddComponent<Item>();
            itemScript.coord = new Coord(itemTransform.position);
            switch (itemTransform.name)
            {
                case ItemName.Item_Start:
                    itemScript.itemType = ItemType.Start;
                    break;
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

    [ContextMenu("替换敌人预设")]
    void ProcessEnemy()
    {
        var prefabRoot = "Assets/Resources/Prefab/Character/{0}.prefab";
        var childCount = enemyRoot.childCount;
        for (var index = 0; index < childCount; index++)
        {
            var enemyTransform = enemyRoot.GetChild(index);
            var prefapUrl = string.Format(prefabRoot, enemyTransform.name);
            Debug.Log(prefapUrl);
            var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefapUrl);
            var enemyInstance = Instantiate(enemyPrefab);
            enemyInstance.transform.parent = enemyRoot;
            enemyInstance.gameObject.name = enemyTransform.name;
            enemyInstance.transform.localPosition = new Vector3(Mathf.RoundToInt(enemyTransform.localPosition.x), enemyTransform.localPosition.y, Mathf.RoundToInt(enemyTransform.localPosition.z));
            enemyInstance.transform.localRotation = enemyTransform.localRotation;
            enemyInstance.transform.SetSiblingIndex(enemyTransform.GetSiblingIndex());
            var enemyScript = enemyInstance.AddComponent<Enemy>();
            enemyScript.coord = new Coord(enemyTransform.position);
            switch (enemyTransform.name)
            {
                case EnemyName.Enemy_Static:
                    enemyScript.enemyType = EnemyType.Static;
                    break;
                case EnemyName.Enemy_Distracted:
                    enemyScript.enemyType = EnemyType.Distracted;
                    break;
                case EnemyName.Enemy_Sentinel:
                    enemyScript.enemyType = EnemyType.Sentinel;
                    break;
                case EnemyName.Enemy_Patrol:
                    enemyScript.enemyType = EnemyType.Patrol;
                    break;
                default:
                    Debug.LogError(string.Format("没有定义敌人{0}", enemyInstance.name));
                    break;
            }
            DestroyImmediate(enemyTransform.gameObject);
        }

    }

    [ContextMenu("保存预设")]
    void SaveAsPrefab(GameObject go)
    {
        var chapterAndLevel = go.name.Split('-');
        var chapter = int.Parse(chapterAndLevel[0]);
        var level = int.Parse(chapterAndLevel[1]);
        var url = string.Format("Assets/Resources/Prefab/Level/{0}/{1}.prefab", chapter, name);
        PrefabUtility.SaveAsPrefabAssetAndConnect(go, url,InteractionMode.UserAction);
        Debug.Log(string.Format("保存关卡预设{0}", url));
    }

    #endregion


    #region RunTime

    public Item start;

    public Item star;

    public Item graff;

    public Item end;

    public List<Item> lureBottles = new List<Item>();

    public List<Item> pincerses = new List<Item>();

    public List<Item> grouthes = new List<Item>();

    public List<Item> manholeCovers = new List<Item>();

    public List<Enemy> enemies = new List<Enemy>();

    public void init()
    {
        resetItems();
        resetEnemies();
        spawnPlayer();
    }

    void resetItems()
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
            switch (itemTr.name)
            {
                case ItemName.Item_Start:
                    start = item;
                    break;
                case ItemName.Item_Star:
                    star = item;
                    break;
                case ItemName.Item_Pincers:
                    pincerses.Add(itemTr.GetComponent<Item>());
                    break;
                case ItemName.Item_ManholeCover:
                    manholeCovers.Add(item);
                    break;
                case ItemName.Item_LureBottle:
                    lureBottles.Add(item);
                    break;
                case ItemName.item_Growth:
                    grouthes.Add(item);
                    break;
                case ItemName.Item_Graff:
                    graff = item;
                    break;
                case ItemName.Item_End:
                    end = item;
                    break;
                default:
                    Debug.LogError(string.Format("未处理未定义Item{0}", itemTr.name));
                    break;
            }
        }
    }

    void resetEnemies()
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
                    break;
                case EnemyName.Enemy_Sentinel:
                    break;
                case EnemyName.Enemy_Patrol:
                    break;
                case EnemyName.Enemy_Distracted:
                    break;
                default:
                    Debug.LogError(string.Format("未处理未定义Item{0}", enemyTr.name));
                    break;
            }
        }
    }

    void spawnPlayer()
    {
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefab/Character/Player.prefab");
        var playerInstance = Instantiate(playerPrefab,transform);
        playerInstance.transform.position = start.transform.position;
        playerInstance.transform.rotation = start.transform.rotation;
        start.gameObject.SetActive(false);
    }

    #endregion
}
