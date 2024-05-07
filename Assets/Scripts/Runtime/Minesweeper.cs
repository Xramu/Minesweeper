
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Responsible for conencting the menu UI with the MinesweeperGame
/// </summary>
public class Minesweeper : MonoBehaviour
{
    #region Inspector Exposed

    [SerializeField]
    private GamePreset currentGamePreset;

    [SerializeField]
    private int mineAmount = 30;

    [Header("Game Presets")]

    [SerializeField]
    private GamePreset[] gamePresets;

    #endregion

    #region UI References

    private UIDocument baseUIDocument;

    private VisualElement uiRoot;

    private VisualElement menuContainer;
    private VisualElement gameContainer;

    private Button startGameButton;
    private Button endGameButton;

    private IntegerField gameSizeXField;
    private IntegerField gameSizeYField;
    private IntegerField minesAmountField;

    #endregion

    #region Private

    private MinesweeperGame minesweeperGame;

    #endregion

    private void Awake()
    {
        baseUIDocument = GetComponent<UIDocument>();
        uiRoot = baseUIDocument.rootVisualElement;

        minesweeperGame = uiRoot.Q<MinesweeperGame>();

        menuContainer = uiRoot.Q<VisualElement>("MenuContainer");
        gameContainer = uiRoot.Q<VisualElement>("GameContainer");

        startGameButton = uiRoot.Q<Button>("StartGameButton");
        startGameButton.RegisterCallback<ClickEvent>(OnClickStartGameButton);

        endGameButton = uiRoot.Q<Button>("EndGameButton");
        endGameButton.RegisterCallback<ClickEvent>(OnClickEndGameButton);

        gameSizeXField = uiRoot.Q<IntegerField>("IntFieldGameX");
        gameSizeXField.RegisterCallback<ChangeEvent<int>>(OnChangeGameSizeXField);

        gameSizeYField = uiRoot.Q<IntegerField>("IntFieldGameY");
        gameSizeYField.RegisterCallback<ChangeEvent<int>>(OnChangeGameSizeYField);

        minesAmountField = uiRoot.Q<IntegerField>("IntFieldMines");
        minesAmountField.RegisterCallback<ChangeEvent<int>>(OnChangeMinesAmountField);

        CreatePresetButtons();
    }

    private void CreatePresetButtons()
    {
        VisualElement presetsColumn = uiRoot.Q<VisualElement>("PresetsColumn");

        if (gamePresets.IsEmptyOrNull())
        {
            // No presets?
            return;
        }

        for (int i = 0; gamePresets.Length > i; i++)
        {
            // Create button and set it's label to preset name
            Button presetButton = new Button();
            presetButton.text = gamePresets[i].Name;

            // Register callback
            presetButton.RegisterCallback<ClickEvent, GamePreset>(OnPresetButtonClick, gamePresets[i]);

            // Add button to column
            presetsColumn.Add(presetButton);
        }
    }

    #region UI Events

    private void OnPresetButtonClick(ClickEvent clickEvent, GamePreset gamePreset)
    {
        currentGamePreset = gamePreset;

        // Set UI field values
        gameSizeXField.SetValueWithoutNotify(currentGamePreset.GameSize.x);
        gameSizeYField.SetValueWithoutNotify(currentGamePreset.GameSize.y);
        minesAmountField.SetValueWithoutNotify(currentGamePreset.MineAmount);
    }

    private void OnClickStartGameButton(ClickEvent clickEvent)
    {
        StartNewGame();
    }

    private void OnClickEndGameButton(ClickEvent clickEvent)
    {
        EndGameAndGoToMenu();
    }

    private void OnGameCustomValueSet()
    {
        // Set preset name to custom
        currentGamePreset.Name = "Custom";
    }

    private void OnChangeGameSizeXField(ChangeEvent<int> changeEvent)
    {
        // Set X, keep Y
        currentGamePreset.GameSize = new Vector2Int(changeEvent.newValue, currentGamePreset.GameSize.y);

        OnGameCustomValueSet();
    }

    private void OnChangeGameSizeYField(ChangeEvent<int> changeEvent)
    {
        // Set Y, keep X
        currentGamePreset.GameSize = new Vector2Int(currentGamePreset.GameSize.x, changeEvent.newValue);

        OnGameCustomValueSet();
    }

    private void OnChangeMinesAmountField(ChangeEvent<int> changeEvent)
    {
        currentGamePreset.MineAmount = changeEvent.newValue;

        OnGameCustomValueSet();
    }

    #endregion

    private void Update()
    {
        // Debug showing mines
        if (Input.GetKeyDown(KeyCode.M))
        {
            minesweeperGame.ToggleMineDebug();
        }
    }

    private void SetGameVisible()
    {
        gameContainer.style.display = DisplayStyle.Flex;
        menuContainer.style.display = DisplayStyle.None;
    }

    private void SetMenuVisible()
    {
        gameContainer.style.display = DisplayStyle.None;
        menuContainer.style.display = DisplayStyle.Flex;
    }

    private void StartNewGame()
    {
        SetGameVisible();
        minesweeperGame.StartNewGame(currentGamePreset);
    }

    private void EndGameAndGoToMenu()
    {
        SetMenuVisible();
        minesweeperGame.EndCurrentGame(true);
    }
}