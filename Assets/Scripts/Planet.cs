using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    //м.б. не пригодится
    private int _id;
    public int Id { get => _id;}    

    private int _health;
    public int Health { get => _health; }

    [SerializeField]
    private int _maxHealth = 4;

    [SerializeField]
    private Transform _pinPoint;

    private Rigidbody2D _rb;

    [SerializeField]
    private SpriteRenderer _healthAura;
    private Color _healthAuraTargetColor;

    [SerializeField]
    private float _healthAuraAnimationDelay, _healthAuraColorChangeRate;
    private WaitForSeconds _healthAuraAnimationWait;
    private Coroutine _healthAuraAninmationCoroutine;
    
    void Start()
    {
        _health = 2;
        _rb = GetComponent<Rigidbody2D>();

        _healthAuraAnimationWait = new WaitForSeconds(_healthAuraAnimationDelay);

        _healthAura.color = new Color(_healthAura.color.r, _healthAura.color.g, _healthAura.color.b, 0);
    }
    
    void FixedUpdate()
    {
        _rb.MovePosition(_pinPoint.position);
    }

    public void GainHealth()
    {
        if (_health < _maxHealth)
        {
            _health++;
            //анимация

            Debug.Log("Теперь у " + transform.name + " здоровья " + _health);

            CheckSwitchHealthState();
        }
    }

    public void LoseHealth()
    {
        _health--;
        //анимация

        Debug.Log("Теперь у " + transform.name + " здоровья " + _health);

        CheckSwitchHealthState();


    }

    private void CheckSwitchHealthState()
    {
        if(_health > 2)
        {
            if (_healthAuraAninmationCoroutine != null)
                StopCoroutine(_healthAuraAninmationCoroutine);
            
            //м.б. 0.75 и 1 - не очень заметно
            _healthAuraAninmationCoroutine = StartCoroutine(HealthAuraAnimationCoroutine((float)_health/_maxHealth, true));
        }
        else if (_health == 2)
        {
            if (_healthAuraAninmationCoroutine != null)
            {
                StopCoroutine(_healthAuraAninmationCoroutine);
                _healthAuraAninmationCoroutine = StartCoroutine(HealthAuraAnimationCoroutine(0f, false));
            }

            //анимация здоровый
        }
        else if (_health == 1)
        {
            //анимация раненый
        }
        else if(_health <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Планета сдохла");
        //анимация
        gameObject.SetActive(false);
    }

    private IEnumerator HealthAuraAnimationCoroutine(float targetAlpha, bool loop)
    {

        Debug.Log(targetAlpha);

        float counter = 0;

        while (counter <= 1)
        {
            counter += _healthAuraColorChangeRate;

            _healthAuraTargetColor = _healthAura.color;
            _healthAuraTargetColor.a = targetAlpha;
            _healthAura.color = Color.Lerp(_healthAura.color, _healthAuraTargetColor, counter);

            yield return _healthAuraAnimationWait;
        }

        if (loop)
        {

            //почему-то не работает. Mathf.Abs(Mathf.Sin(counter)) всё считает верно,
            //но альфа просто уезжает один раз в предельное значение и больше не двигается.

            _healthAuraTargetColor.a = targetAlpha - 0.45f;
            Color startingColor = _healthAura.color;

            counter = 0;

            while (true)
            {
                _healthAura.color = Color.Lerp(startingColor, _healthAuraTargetColor, Mathf.Abs(Mathf.Sin(counter)));
                counter += _healthAuraColorChangeRate;
                yield return _healthAuraAnimationWait;
            }
        }
    }
    
}
