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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("处理关卡")]
    void Process()
    {
        this.FindNodes();
        ProcessSquareNodes();
        ProcessVisialNodes();
        ProcessLinkedNodes();
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
        for(var index = 0; index < squareRoot.childCount; index++)
        {
            var nodeTransform = squareRoot.GetChild(index);
            nodeTransform.name = nodeTransform.name.Replace("Square_", "");
            var childCount = nodeTransform.childCount;
            while(childCount>0)
            {
                var childGameObject = nodeTransform.GetChild(0).gameObject;
                GameObject.DestroyImmediate(childGameObject);
                childCount = nodeTransform.childCount;
            }

            var base_Square = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefab/Items/Base_Square.prefab");
            var base_SquareInstance = Instantiate(base_Square);
            base_SquareInstance.transform.parent = nodeTransform;
            base_SquareInstance.name = "Base_Square";
            base_SquareInstance.transform.localPosition = new Vector3(0, 0, 0);

            var coord = nodeTransform.name.Split('_');
            nodeTransform.transform.localPosition = new Vector3(int.Parse(coord[0]),0, int.Parse(coord[1]));

            var script = nodeTransform.GetComponent<BoardNode>();
            if(script==null)
            {
                script = nodeTransform.gameObject.AddComponent<BoardNode>();
            }
            script.initWithTransform(nodeTransform);
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
}
