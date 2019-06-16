﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static MenuController Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);
    }

    public GameObject pauseMenu;

    public GameObject startMenu;
    public GameObject settingsMenu;
    public GameObject waitingForPlayersMenu;
    public GameObject inGameMenu;
    public GameObject gameOverMenu;

    public TextMeshProUGUI score;
    public TextMeshProUGUI selection;
    public TextMeshProUGUI gameOverText;

    private List<Alphabet> currentSelection = new List<Alphabet>();
    private IDisposable clickSub;

    private bool isDrag;

    // Update is called once per frame
    void Update()
    {
        GameController.GameState state = GameController.Instance.GetState();
        switch(state) {
            case GameController.GameState.STARTED:
                score.SetText("SCORE " + GameController.Instance.SCORE);
                break;
        }

        if(Input.touchCount > 0) {
            Debug.Log(Input.touchCount);
        }

        if(Input.touchCount == 2) {
            this.onSubmitClicked();
        }

        if(Input.GetKeyDown(KeyCode.Escape)) {
            if (state == GameController.GameState.PAUSED)
            {
                Resume();
            }
            else if(state == GameController.GameState.STARTED)
            {
                Pause();
            }
        }
    }

    private void Start()
    {
        clickSub = this.UpdateAsObservable()
            .Where(_ => Input.GetMouseButton(0))
            .Select(_ => {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                return (Physics.Raycast(ray, out hit)) ? hit.transform.gameObject : null;
            })
            .Where(gameObject => gameObject != null && gameObject.tag.Equals("Cube"))
            .DistinctUntilChanged(gameObject => gameObject.name)
            .Subscribe(item => {
                //Debug.Log("unirx : " + item.name);
                Alphabet alphabet = item.GetComponent<Alphabet>();
                int index = currentSelection.FindIndex(it => it.name.Equals(item.name));
                if (index != -1) {
                    //Debug.Log("item already present, removing all proceeding indexes");
                    for (int it = index + 1; it < currentSelection.Count; ++it) {
                        currentSelection[it].SetIsSelected(false);
                    }
                    currentSelection
                        .RemoveRange(index + 1, currentSelection.Count - index - 1);
                }
                else {
                    currentSelection.Add(alphabet);
                    alphabet.SetIsSelected(true);
                    this.isDrag = false;
                }
                string currentText = "";
                foreach (Alphabet alp in currentSelection)
                    currentText += alp.character;
                selection.text = currentText;
            });
    }

    public void Pause()
    {
        pauseMenu.SetActive(true);
        GameController.Instance.SetState(GameController.GameState.PAUSED);
        Time.timeScale = 0f;
    }

    public void setDrag(bool isDrag) {
        this.isDrag = isDrag;
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        GameController.Instance.SetState(GameController.GameState.STARTED);
    }

    public void EndGame(int score)
    {
        gameOverText
            .SetText("Score "+ score);
        gameOverMenu.SetActive(true);
        inGameMenu.SetActive(false);
    }

    public void DisableWaitingForPlayersMenu() {
        waitingForPlayersMenu.SetActive(false);
        inGameMenu.SetActive(true);
    }

    public void onSubmitClicked()
    {
        Debug.Log("in on submit clicked");
        List<int> idlist = new List<int>();
        currentSelection.ForEach(alphabet => idlist.Add(alphabet.id));
        GameController.Instance.UpdateScore(selection.text, idlist, isDrag);
    }

    internal void DestroySelection()
    {
        selection.text = "";
        for (int i = 0; i < currentSelection.Count; ++i)
            currentSelection[i].Explode(i * 0.05f);
        currentSelection.Clear();
    }

    public void UnSelectAll()
    {
        selection.text = "";
        for (int i = 0; i < currentSelection.Count; ++i)
            currentSelection[i].SetIsSelected(false);
        currentSelection.Clear();
    }

    private void OnDestroy()
    {
        clickSub.Dispose();
    }

    public void reset() {
        GameObject[] alphabets = GameObject.FindGameObjectsWithTag("Alphabet");
        Debug.Log(alphabets.Length);
        foreach(GameObject item in alphabets) {
            Destroy(item);
        }
        NetworkController.Instance.reset();
        gameOverMenu.SetActive(false);
        inGameMenu.SetActive(true);
        GameController.Instance.StartGame((int)GameController.Instance.currentGameMode);
    }

    public void onBackPressed() {
        Debug.Log("in back pressed");
        GameObject[] alphabets = GameObject.FindGameObjectsWithTag("Alphabet");
        Debug.Log(alphabets.Length);
        foreach(GameObject item in alphabets) {
            Destroy(item);
        }
        inGameMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        startMenu.SetActive(true);
    }
}
