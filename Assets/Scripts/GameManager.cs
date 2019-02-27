using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    
    private AsteroidsController _asteroids;

    private DialogueOutputter _dialogueOutputter;

    [SerializeField]
    private GameObject[] _starAndPlanets;

    [SerializeField]
    private float _gameStagesSwitcherDelay = 1f;

    [SerializeField]
    private DialogueElementSO _startingDialogue, _middleDialogue, _finalDialogueGood, _finalDialogueBad;

    [SerializeField]
    private DialogueElementSO[] _idleDialoguesGood, _healDialogues, _damageDialogues, _deathDialogues;

    [SerializeField]
    private float _dialogueLineShowTime;
    private float _dialogueLineShowCounter;

    [SerializeField]
    private Text _characterTextUI, _lineTextUI;

    private bool _dialogueIsRunning;

    private enum GameStages//или сделать FSM
    {
        //добавить стартовое меню сюда же наверно
        LaunchingMainGame,
        StartingDialogues,
        HealingAsteroids,
        MiddleDialogues,
        DamagingAsteroids,
        FinalPart
    }

    private GameStages _currentGameStage;

    // Start is called before the first frame update
    void Start()
    {
        _dialogueOutputter = new DialogueOutputter(_characterTextUI, _lineTextUI);
        _dialogueOutputter.LoadCharacters(_starAndPlanets);

        _asteroids = FindObjectOfType<AsteroidsController>();

        _currentGameStage = GameStages.LaunchingMainGame;

        StartCoroutine("GameStagesSwitcherCoroutine");

        //пытаемся убрать тормоза при вкл UI:
        _characterTextUI.transform.parent.parent.gameObject.SetActive(true);
        _characterTextUI.transform.parent.parent.gameObject.SetActive(false);

        Debug.Log(_startingDialogue);
    }

    // Update is called once per frame
    void Update()
    {

        //input типа паузы м.б.
        //м.б. стартскрин сюда же засунуть
        

        //прокрутка диалогов
        //добавить условие, что _currentGameStage != стартовое меню, пауза и всякое такое где диалоги не работают
        if (_dialogueIsRunning && 
            (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Return)
            || _dialogueLineShowCounter > _dialogueLineShowTime)
           )
        {

            _dialogueLineShowCounter = 0;

            if (!_dialogueOutputter.TryShowNextDialoguePiece())
                _dialogueIsRunning = false;
        }

        _dialogueLineShowCounter += Time.deltaTime;

    }

    private void StartDialogue()
    {
        _dialogueIsRunning = true;
        _dialogueLineShowCounter = 0;
        _dialogueOutputter.TryShowNextDialoguePiece();
    }

    private IEnumerator GameStagesSwitcherCoroutine()
    {
        WaitForSeconds coroutineDelay = new WaitForSeconds(_gameStagesSwitcherDelay);

        while(_currentGameStage != GameStages.FinalPart)
        {
            Debug.Log(_currentGameStage);

            yield return coroutineDelay;

            switch (_currentGameStage)
            {
                case GameStages.LaunchingMainGame:
                    if (_dialogueLineShowCounter > _dialogueLineShowTime)//ВРЕМЕННО, ТЕСТ, УБРАТЬ УСЛОВИЕ!
                    {
                        _dialogueOutputter.LoadDialogue(_startingDialogue);
                        StartDialogue();
                        _currentGameStage = GameStages.StartingDialogues;
                    }
                    
                    break;

                case GameStages.StartingDialogues:

                    if (!_dialogueIsRunning)
                    {
                        _currentGameStage = GameStages.HealingAsteroids;
                        _asteroids.StartLaunchingAsteroids();
                    }

                    break;
                case GameStages.HealingAsteroids:
                    if (_asteroids.CheckIfAllAsteroidsAreLaunchedAndDestroyed())
                    {
                        _dialogueOutputter.LoadDialogue(_middleDialogue);
                        StartDialogue();
                        _currentGameStage = GameStages.MiddleDialogues;
                    }
                    break;
                case GameStages.MiddleDialogues:
                    if (!_dialogueIsRunning)
                    {
                        _currentGameStage = GameStages.DamagingAsteroids;
                        _asteroids.SwitchAsteroidsToDamagePlanets();
                        _asteroids.StartLaunchingAsteroids();
                    }
                    break;
                case GameStages.DamagingAsteroids:
                    if (_asteroids.CheckIfAllAsteroidsAreLaunchedAndDestroyed())
                        _currentGameStage = GameStages.FinalPart;
                    break;
                case GameStages.FinalPart:
                    break;
            }            
        }

    }
}
