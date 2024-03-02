using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    static readonly KeyCode[] supportedKeys = new KeyCode[] {
        //just the alphabet for user input
        KeyCode.A, KeyCode.B, KeyCode.C, KeyCode.D, KeyCode.E, KeyCode.F, KeyCode.G, KeyCode.H, KeyCode.I, KeyCode.J, KeyCode.K, KeyCode.L, KeyCode.M, KeyCode.N, KeyCode.O, KeyCode.P,
        KeyCode.Q, KeyCode.R, KeyCode.S, KeyCode.T, KeyCode.U, KeyCode.V, KeyCode.W, KeyCode.X, KeyCode.Y, KeyCode.Z,
    };

    private Row[] rows;

    //words
    private string[] solutions;
    private string[] validWords;
    private string[] usedWords = { "x", "x", "x", "x", "x", "x" };
    private string word;

    int rowIndex;
    int columnIndex;

    //state color
    [Header("States")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State wrongState;

    //invalid guess
    [Header("UI")]
    public TextMeshProUGUI invalidWordText;
    public Button newWordButton;
    public Button quitButton;
    
    //functions
    void SubmitRow(Row row)//submit ans function
    {
        if (!IsValidWord(row.word))
        {
            invalidWordText.gameObject.SetActive(true);
            return;
        }
        usedWords[rowIndex] = row.word;
        string remaining = word;

        for (int i = 0; i < row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if (tile.letter == word[i])
            {
                //perfect match
                tile.SetState(correctState);
                remaining = remaining.Remove(i, 1);
                remaining = remaining.Insert(i, " ");

            }
            else if (!word.Contains(tile.letter))
            {
                //wrong
                tile.SetState(wrongState);
            }
        }
        for(int i = 0;i<row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];

            if(tile.state != correctState && tile.state != wrongState)
            {
                if (remaining.Contains(tile.letter))
                {
                    //wrong spot
                    tile.SetState(wrongSpotState);
                    
                    int index = remaining.IndexOf(tile.letter);
                    remaining = remaining.Remove(index, 1);
                    remaining = remaining.Insert(index, " ");
                }
                else
                {
                    //wrong
                    tile.SetState(wrongState);
                }
            }
        }

        if (HasWon(row))
        {
            enabled = false;
        }

        rowIndex++;
        columnIndex = 0;

        if(rowIndex >= rows.Length) //complete loss
        {
            enabled = false;
        }
    }

    void LoadData() //load the words
    {
        //valid words
        TextAsset textFile = Resources.Load("allowed words") as TextAsset;
        validWords = textFile.text.Split('\n');

        //answers
        textFile = Resources.Load("possible answers") as TextAsset;
        solutions = textFile.text.Split('\n');
    }

    private void SetRandomWord() //setting the answer
    {
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
    }

    private bool IsValidWord(string word) //checks if guess is in the allowed words list or in possible answers
    {
        for(int i = 0; i < usedWords.Length; i++)
        {
            if (usedWords[i].ToLower().Trim() == word)//used words
            {
                return false;
            }
        }
        for(int i = 0; i < validWords.Length; i++)
        {
            if (validWords[i].ToLower().Trim() == word)//allowed words
            {
                return true;
            }
        }
        for(int i = 0; i< solutions.Length; i++)
        {
            if (solutions[i].ToLower().Trim() == word)//possible answers
            {
                return true;
            }
        }
        return false;
    }

    private bool HasWon(Row row)
    {
        for(int i = 0; i<row.tiles.Length; i++)
        {
            if (row.tiles[i].state != correctState)
            {
                return false;
            }
        }
        return true;
    }

    public void NewGame()
    {
        ClearBoard();
        SetRandomWord();
        enabled = true;
    }

    public void quitGame()
    {
        Application.Quit();
    }

    private void ClearBoard()
    {
        for (int i = 0; i<rows.Length; i++)
        {
            for(int j = 0; j < rows[i].tiles.Length; j++) {
                rows[i].tiles[j].SetLetter('\0');
                rows[i].tiles[j].SetState(emptyState);
            }
        }
        rowIndex = 0;
        columnIndex = 0;
        for(int i = 0; i<usedWords.Length; i++)//resets used words
        {
            usedWords[i] = "x";
        }
    }

    private void OnDisable()
    {
        newWordButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        newWordButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }


    //start
    void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    void Start()
    {
        LoadData();
        SetRandomWord();
    }

    void Update()
    {
        Row currentRow = rows[rowIndex];
        
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            //backspace
            columnIndex = Mathf.Max(columnIndex - 1, 0);
            currentRow.tiles[columnIndex].SetLetter('\0');
            currentRow.tiles[columnIndex].SetState(emptyState);
            invalidWordText.gameObject.SetActive(false);
        }
        else if (columnIndex >= rows[rowIndex].tiles.Length)
        {
            //submit answer
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitRow(currentRow);
            }
        }
        else
        {
             //update tile
            for(int i = 0; i<supportedKeys.Length; i++)
            {
                if (Input.GetKeyDown(supportedKeys[i]))
                {
                    currentRow.tiles[columnIndex].SetLetter((char)supportedKeys[i]);
                    currentRow.tiles[columnIndex].SetState(occupiedState);
                    columnIndex++;
                    break;
                }
            }
        }
        
    }
}
