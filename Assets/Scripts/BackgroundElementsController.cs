using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundElementsController : MonoBehaviour
{

    [SerializeField]
    private GameObject _farObjectsPrefab, _nearObjectsPrefab;
    [SerializeField]
    private Sprite[] _nearObjectsSprites, _farObjectsSprites;

    [SerializeField]
    private float _borderX, _borderY;
    private float _nearObjectsYExtents, _farObjectsYExtents;

    [SerializeField]
    private int _farObjectsPoolsSize, _nearObjectsPoolsSize,
        _farObjectsMinSpawnDelay, _farObjectsMaxSpawnDelay, _nearObjectsSpawnProbability;
    [SerializeField]
    private float _nearObjectsSpawnDelay;


    private Queue<BackgroundObject> _poolOfFarObjects, _poolOfNearObjects;
    private List<BackgroundObject> _activeFarObjects, _activeNearObjects;

    [SerializeField]
    private float _farObjectsSpeed, _nearObjectsSpeed;

    private const float DestroyCoroutineDelay = .5f;

    private struct BackgroundObject
    {
        public GameObject GameObj;
        public SpriteRenderer Sr;

        public BackgroundObject(GameObject gameObject)
        {
            GameObj = gameObject;
            Sr = gameObject.GetComponent<SpriteRenderer>();
        }
    }

    // Start is called before the first frame update
    void Start()
    {

        _poolOfNearObjects = new Queue<BackgroundObject>();
        _poolOfFarObjects = new Queue<BackgroundObject>();

        _activeNearObjects = new List<BackgroundObject>();
        _activeFarObjects = new List<BackgroundObject>();

        PopulatePool(_poolOfNearObjects, _nearObjectsPoolsSize, _nearObjectsPrefab);
        PopulatePool(_poolOfFarObjects, _farObjectsPoolsSize, _farObjectsPrefab);

        _nearObjectsYExtents = GetLargestObjectYExtents(_nearObjectsSprites, _nearObjectsPrefab);
        _farObjectsYExtents = GetLargestObjectYExtents(_farObjectsSprites, _farObjectsPrefab);
        

        InitialSpawn();

        StartCoroutine("DestroyObjectsCoroutine");

        StartCoroutine("SpawnFarObjectsCoroutine");
        StartCoroutine("SpawnNearObjectsCoroutine");
    }

    // Update is called once per frame
    void Update()
    {
        //перемещение объектов
        MoveObjects(_activeNearObjects, _nearObjectsSpeed * Time.deltaTime);
        MoveObjects(_activeFarObjects, _farObjectsSpeed * Time.deltaTime);
    }

    private float GetLargestObjectYExtents(Sprite[] sprites, GameObject prefab)
    {
        float extentsY = 0;

        foreach (var sprite in sprites)
        {
            if (sprite.bounds.extents.y > extentsY)
                extentsY = sprite.bounds.extents.y;
        }
        
        return extentsY * prefab.transform.lossyScale.y;
    }

    private void PopulatePool(Queue<BackgroundObject> pool, int poolSize, GameObject prefab)
    {
        for (int i = 0; i < poolSize; i++)
        {
            BackgroundObject obj = new BackgroundObject(Instantiate(prefab, transform));
            pool.Enqueue(obj);
            obj.GameObj.SetActive(false);
        }
    }

    private void MoveObjects(List<BackgroundObject> list, float moveIncrement)
    {
        foreach (var obj in list)
            obj.GameObj.transform.Translate(0, -moveIncrement, 0, Space.World);
    }

    private void TrySpawnObjectFromPool(Queue<BackgroundObject> pool, List<BackgroundObject> activeObjList, Sprite[] possibleSprites, float spriteExtentsY)
    {
        if (pool.Count > 0)
        {
            BackgroundObject obj = pool.Dequeue();
            obj.GameObj.SetActive(true);
            
            obj.GameObj.transform.position = new Vector3(Random.Range(-_borderX, _borderX), _borderY + spriteExtentsY, 0);

            obj.Sr.sprite = possibleSprites[Random.Range(0, possibleSprites.Length)];
            activeObjList.Add(obj);
        }
    }

    private void ReturnObjectsToPool(Queue<BackgroundObject> pool, List<BackgroundObject> activeObjList, float spriteExtentsY)
    {
        for (int i = 0; i < activeObjList.Count; i++)
        {
            if (activeObjList[i].GameObj.transform.position.y < -_borderY - spriteExtentsY)
            {
                activeObjList[i].GameObj.SetActive(false);
                pool.Enqueue(activeObjList[i]);
                activeObjList.Remove(activeObjList[i--]);                
            }
        }
    }

    private void InitialSpawn()
    {
        //ВРЕМЕННО
        for (int i = 0; i < 5; i++)
        {
            TrySpawnObjectFromPool(_poolOfFarObjects, _activeFarObjects, _farObjectsSprites, _farObjectsYExtents);
            _activeFarObjects[_activeFarObjects.Count - 1].GameObj.transform.Translate(new Vector3(0, Random.Range(5,-5)));
        }

        for (int i = 0; i < 20; i++)
        {
            TrySpawnObjectFromPool(_poolOfNearObjects, _activeNearObjects, _nearObjectsSprites, _nearObjectsYExtents);
            _activeNearObjects[_activeNearObjects.Count - 1].GameObj.transform.Translate(new Vector3(0, Random.Range(6,-4)));
        }
    }

    private IEnumerator SpawnNearObjectsCoroutine()
    {
        WaitForSeconds coroutineDelay = new WaitForSeconds(_nearObjectsSpawnDelay);

        while (true)
        {
            if (Random.Range(0,100) < _nearObjectsSpawnProbability)
                TrySpawnObjectFromPool(_poolOfNearObjects, _activeNearObjects, _nearObjectsSprites, _nearObjectsYExtents);

            yield return coroutineDelay;
        }
    }

    private IEnumerator SpawnFarObjectsCoroutine()
    {
        WaitForSeconds coroutineDelay = new WaitForSeconds(_farObjectsMinSpawnDelay);

        while(true)
        {
            TrySpawnObjectFromPool(_poolOfFarObjects, _activeFarObjects, _farObjectsSprites, _farObjectsYExtents);
            yield return coroutineDelay;
            coroutineDelay = new WaitForSeconds(Random.Range(_farObjectsMinSpawnDelay, _farObjectsMaxSpawnDelay));
        }
    }

    private IEnumerator DestroyObjectsCoroutine()
    {

        while (true)
        {
            ReturnObjectsToPool(_poolOfNearObjects, _activeNearObjects, _nearObjectsYExtents);
            ReturnObjectsToPool(_poolOfFarObjects, _activeFarObjects, _farObjectsYExtents);

            yield return new WaitForSeconds(DestroyCoroutineDelay);
        }

    }
}
