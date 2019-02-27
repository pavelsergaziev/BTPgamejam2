using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidsController : MonoBehaviour
{


    //TODO: Корутина для регулярного запуска астероидов через рандомный дилей.

    [SerializeField]
    private Vector3 _playerStarPos;//заменить на трансформ?

    [SerializeField]
    private float _spawnPointBorderLeft, _spawnPointBorderRight, _spawnPointBorderTop, _spawnPointBorderBottom;


    [SerializeField]
    private GameObject _asteroidPrefab;
    [SerializeField]
    private int _asteroidsPoolSize;

    private struct Asteroid
    {
        public GameObject GameObj;
        public Rigidbody2D Rb;

        public Asteroid(GameObject gameObject)
        {
            GameObj = gameObject;
            Rb = gameObject.GetComponent<Rigidbody2D>();
        }
    }

    private Queue<Asteroid> _asteroidsPool;
    private List<Asteroid> _activeAsteroids;    

    [SerializeField]
    private float _asteroidSlowdownRate, _asteroidSlowdownDelay;
    private bool _asteroidsDoSlowdown;

    [SerializeField]
    private int _healingAsteroidsCount, _damagingAsteroidsCount;

    [SerializeField]
    private float _asteroidsLaunchDelayMin, _asteroidsLaunchDelayMax, _asteroidSqrMaxSpeed, _asteroidInitialImpulse, _asteroidInitialAngleMax, _gravity;
    

    private bool _allAsteroidsBeenLaunched;
    

    private IAsteroidsMode _currentMode;
    private IAsteroidsMode _modeHeal, _modeDamage;

    private RaycastHit2D[] _hitArray;
    private ContactFilter2D _contactFilter;

    // Start is called before the first frame update
    void Start()
    {
        _asteroidsPool = new Queue<Asteroid>();
        for (int i = 0; i < _asteroidsPoolSize; i++)
        {
            GameObject asteroidObj = Instantiate(_asteroidPrefab, new Vector3(-10,-10,0), Quaternion.identity, transform);

            var emission = asteroidObj.GetComponentInChildren<ParticleSystem>().emission;
            emission.enabled = false;

            _asteroidsPool.Enqueue(new Asteroid(asteroidObj));
            asteroidObj.SetActive(false);
        }

        _activeAsteroids = new List<Asteroid>();

        _contactFilter.useTriggers = true;
        _contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(_asteroidPrefab.layer));
        _contactFilter.useLayerMask = true;

        _hitArray = new RaycastHit2D[1];
        

        _modeHeal = new AsteroidsMode_Heal() as IAsteroidsMode;
        _modeDamage = new AsteroidsMode_Damage() as IAsteroidsMode;

        _currentMode = _modeHeal;

        StartCoroutine("AsteroidsSlowdownCoroutine");
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < _activeAsteroids.Count; i++)
        {
            if (_activeAsteroids[i].Rb.Cast(Vector2.zero, _contactFilter, _hitArray, 0f) > 0)
            {
                _currentMode.HandleRaycastHit(_hitArray[0]);
                DestroyAsteroid(_activeAsteroids[i--]);
            }
        }
    }

    void FixedUpdate()
    {
        //перемещение астероидов
        foreach (var asteroid in _activeAsteroids)
        {
            if (_asteroidsDoSlowdown)
            {
                asteroid.Rb.velocity *= _asteroidSlowdownRate;
            }            

            asteroid.Rb.AddForce((_playerStarPos - asteroid.GameObj.transform.position).normalized * _gravity, ForceMode2D.Force);

            if (asteroid.Rb.velocity.sqrMagnitude > _asteroidSqrMaxSpeed)
            {
                asteroid.Rb.velocity = asteroid.Rb.velocity.normalized * Mathf.Sqrt(_asteroidSqrMaxSpeed);
            }           

        }

        if (_asteroidsDoSlowdown)
            _asteroidsDoSlowdown = false;
    }

    public void SwitchAsteroidsToDamagePlanets()
    {
        _currentMode = _modeDamage;
    }

    private bool TryLaunchAsteroid()
    {
        if (_asteroidsPool.Count > 0)
        {
            Asteroid asteroid = _asteroidsPool.Dequeue();
            _activeAsteroids.Add(asteroid);
            asteroid.GameObj.SetActive(true);

            asteroid.GameObj.GetComponentInChildren<ParticleSystem>().Play();
            var emission = asteroid.GameObj.GetComponentInChildren<ParticleSystem>().emission;
            emission.enabled = true;

            //определяем положение и направление

            //0 - из-за левой границы, 1 - из-за верхней границы, 2 - из-за правой границы
            switch (Random.Range(0, 3))
            {
                case 0:
                    asteroid.GameObj.transform.position = new Vector3(_spawnPointBorderLeft, Random.Range(_spawnPointBorderBottom, _spawnPointBorderTop), 0);
                    break;
                case 1:
                    asteroid.GameObj.transform.position = new Vector3(Random.Range(_spawnPointBorderLeft, _spawnPointBorderRight), _spawnPointBorderTop, 0);
                    break;
                case 2:
                    asteroid.GameObj.transform.position = new Vector3(_spawnPointBorderRight, Random.Range(_spawnPointBorderBottom, _spawnPointBorderTop), 0);
                    break;

                default:
                    break;
            }

            //ищем вектор-направление к звезде
            Vector2 startingDirection = (_playerStarPos - asteroid.GameObj.transform.position).normalized;

            //рандомно поворачиваем
            float randomAngle = Mathf.Deg2Rad * Random.Range(-_asteroidInitialAngleMax, _asteroidInitialAngleMax);
            startingDirection = new Vector2(startingDirection.x * Mathf.Cos(randomAngle) - startingDirection.y * Mathf.Sin(randomAngle), startingDirection.x * Mathf.Sin(randomAngle) + startingDirection.y * Mathf.Cos(randomAngle));

            //запускаем астероид в заданном направлении
            asteroid.Rb.AddForce(startingDirection * _asteroidInitialImpulse, ForceMode2D.Impulse);

            return true;
        }

        return false;
    }

    private void DestroyAsteroid(Asteroid asteroid)
    {
        _activeAsteroids.Remove(asteroid);
        _asteroidsPool.Enqueue(asteroid);

        asteroid.GameObj.GetComponentInChildren<ParticleSystem>().Pause();
        var emission = asteroid.GameObj.GetComponentInChildren<ParticleSystem>().emission;
        emission.enabled = false;

        asteroid.GameObj.SetActive(false);
    }
    
    public void StartLaunchingAsteroids()
    {
        _allAsteroidsBeenLaunched = false;
        StartCoroutine("AsteroidsLaunchCoroutine");
    }

    public bool CheckIfAllAsteroidsAreLaunchedAndDestroyed()
    {
        return _allAsteroidsBeenLaunched && _activeAsteroids.Count == 0;
    }

    private IEnumerator AsteroidsLaunchCoroutine()
    {
        WaitForSeconds coroutineDelay = new WaitForSeconds(_asteroidsLaunchDelayMin);

        int asteroidsMax = _currentMode == _modeHeal ? _healingAsteroidsCount : _damagingAsteroidsCount;
        int asteroidsLaunched = 0;
        
        while(asteroidsLaunched < asteroidsMax)
        {
            if (TryLaunchAsteroid())
                asteroidsLaunched++;

            yield return coroutineDelay;
            coroutineDelay = new WaitForSeconds(Random.Range(_asteroidsLaunchDelayMin, _asteroidsLaunchDelayMax));
        }

        _allAsteroidsBeenLaunched = true;
    }

    private IEnumerator AsteroidsSlowdownCoroutine()
    {
        WaitForSeconds coroutineDelay = new WaitForSeconds(_asteroidSlowdownDelay);

        while (true)
        {
            _asteroidsDoSlowdown = _activeAsteroids.Count > 0 ? true : false;
            yield return coroutineDelay;
        }
    }

}
