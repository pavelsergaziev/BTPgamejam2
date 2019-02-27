using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetsController : MonoBehaviour
{

    //переименовать в "орбиты". PlanetObj, хелс и айди не нужны
    private struct Planet
    {
        public int Id;
        public GameObject PlanetObj;
        public int Health;
        public SpriteRenderer OrbitSpriteRenderer;
        public float MaxSpinSpeed;
        public float MinSpinSpeed;
        public float CurrentSpinSpeed;
    }

    private Planet[] _planets;    

    [SerializeField]
    private GameObject[] _orbitObjs;

    //положим спрайты для повреждённых планет, если будет надо
    //[SerializeField]
    //private Sprite[] _planetSprites;

    [SerializeField]
    private int _planetsStartingHealth;

    private int _currentChosenPlanetId;

    [SerializeField]
    private float _planetsRotationSpeedAvrg, _planetsRotationSpeedRandomClamps,
        _planetsRotationSpeedIncrement, _planetsRotationSpeedDecrement,
        _orbitOpacityChangeSpeed;
    private float _horizontal, _vertical;
    private bool _verticalAxisWasZero;

    private Color _tempColor;

    // Start is called before the first frame update
    void Start()
    {
        _planets = new Planet[_orbitObjs.Length];

        for (int i = 0; i < _planets.Length; i++)
        {
            _planets[i].Id = i;
            _planets[i].Health = _planetsStartingHealth;
            _planets[i].PlanetObj = _orbitObjs[i].transform.GetChild(0).gameObject;
            _planets[i].OrbitSpriteRenderer = _orbitObjs[i].GetComponent<SpriteRenderer>();
            
            _planets[i].MaxSpinSpeed = _planetsRotationSpeedAvrg * 1.5f + Random.Range(0, _planetsRotationSpeedRandomClamps);
            _planets[i].MinSpinSpeed = _planetsRotationSpeedAvrg / 2 + Random.Range(0, _planetsRotationSpeedRandomClamps);
            _planets[i].CurrentSpinSpeed = _planets[i].MinSpinSpeed;

            _tempColor = _planets[i].OrbitSpriteRenderer.color;
            _tempColor.a = 0;
            _planets[i].OrbitSpriteRenderer.color = _tempColor;
        }

        _verticalAxisWasZero = true;
    }

    // Update is called once per frame
    void Update()
    {

        #region ПЕРЕКЛЮЧЕНИЕ ПЛАНЕТ
        //если планета сдохла, мы всё равно можем выбрать её орбиту (тут можно диалог запустить)

        _vertical = Input.GetAxisRaw("Vertical");
        if (_vertical == 0)
        {
            _verticalAxisWasZero = true;
        }
        else if (_verticalAxisWasZero)
        {
            _verticalAxisWasZero = false;

            if (_vertical > 0)
            {
                if (++_currentChosenPlanetId > _orbitObjs.Length - 1)
                    _currentChosenPlanetId = 0;
            }
            else
            {
                if (--_currentChosenPlanetId < 0)
                    _currentChosenPlanetId = _orbitObjs.Length - 1;
            }
        }


        #endregion

        #region ПЕРЕМЕЩЕНИЕ ПЛАНЕТ И ОКРАШИВАНИЕ ОРБИТЫ
        _horizontal = Input.GetAxisRaw("Horizontal");
        
        if (_horizontal != 0)
        {
            _planets[_currentChosenPlanetId].CurrentSpinSpeed = Mathf.Clamp(_planets[_currentChosenPlanetId].CurrentSpinSpeed - (int)_horizontal * _planetsRotationSpeedIncrement * Time.deltaTime, -_planets[_currentChosenPlanetId].MaxSpinSpeed, _planets[_currentChosenPlanetId].MaxSpinSpeed);
        }

        for (int i = 0; i < _orbitObjs.Length; i++)
        {

            _orbitObjs[i].transform.rotation *= Quaternion.AngleAxis(_planets[i].CurrentSpinSpeed * Time.deltaTime, Vector3.forward);

            if (_horizontal == 0 && Mathf.Abs(_planets[i].CurrentSpinSpeed) > _planets[i].MinSpinSpeed)
            {
                if (_planets[i].CurrentSpinSpeed > 0)
                {
                    _planets[i].CurrentSpinSpeed -= _planetsRotationSpeedDecrement * Time.deltaTime;
                    if (_planets[i].CurrentSpinSpeed < _planets[i].MinSpinSpeed)
                        _planets[i].CurrentSpinSpeed = _planets[i].MinSpinSpeed;
                }
                else if (_planets[i].CurrentSpinSpeed < 0)
                {
                    _planets[i].CurrentSpinSpeed += _planetsRotationSpeedDecrement * Time.deltaTime;
                    if (_planets[i].CurrentSpinSpeed > -_planets[i].MinSpinSpeed)
                        _planets[i].CurrentSpinSpeed = -_planets[i].MinSpinSpeed;
                }
            }

            #region ОКРАШИВАНИЕ ОРБИТЫ

            Color _tempColor = _planets[i].OrbitSpriteRenderer.color;
            if (i == _currentChosenPlanetId && _tempColor.a < 1)
            {
                _tempColor.a += _orbitOpacityChangeSpeed * Time.deltaTime;
                _planets[i].OrbitSpriteRenderer.color = _tempColor;
            }
            else if (i != _currentChosenPlanetId && _tempColor.a > 0)
            {
                _tempColor.a -= _orbitOpacityChangeSpeed * Time.deltaTime;
                _planets[i].OrbitSpriteRenderer.color = _tempColor;
            }

            #endregion

        }

        #endregion

    }


}
