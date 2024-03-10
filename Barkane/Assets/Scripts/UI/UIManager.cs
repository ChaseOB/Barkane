using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class UIManager : Singleton<UIManager>
{
    public TMP_Text shardCountText;
    public TMP_Text foldCountText;
    public TMP_Text yourFoldCountText;
    public TMP_Text bestFoldCountText;
    public TMP_Text glowstickText;
    public Image glowstickSprite;
    public Sprite glowstickPrimed;
    public Sprite glowstickActive;
    public Sprite glowstickDead;


    public GameObject shardCountGroup;
    public GameObject glowstickGroup;
    public int menuIndex;
    public GameObject endLevelGroup;
    public GameObject inGameGroup;

    public GameObject cosmeticGroup;
    public Image cosmeticImage;
    public List<string> cosmeticStrings;
    public List<Sprite> cosmeticSprites;
    public Sprite cosmeticNormal;
    public Sprite cosmeticHighlight;
    public Image cosmeticBackground;
    public TMP_Text cosmeticText;
    public GameObject cosmeticButton;

    private bool showCosmetic = false;
   // private string cosmetic;
    private List<string> cosmeticUnlocks = new();
    private int glowstickHealth;
    private int numFolds;

    public StarsUI starsUI;
    public StarsUI bestStarsUI;

    private void Awake() {
        InitializeSingleton();
    }

    private void OnEnable() {
        GlowStickLogic.OnGlowstickChange += OnGlowstickChange;
    }

    private void OnDisable() {
        GlowStickLogic.OnGlowstickChange -= OnGlowstickChange;
    }

    private void Start() {
        Goal g = FindObjectOfType<Goal>();
        if(g != null)
            ResetCounts(g.numShards);
        else
            ResetCounts();
        
        GlowStickLogic glow = FindObjectOfType<GlowStickLogic>();
        if(glow != null) {
            glowstickText.text = glow.lifetime.ToString();
            glowstickHealth = glow.lifetime;
        }
        else 
        {
            glowstickGroup.SetActive(false);
        }

    }

    public void ToggleLevelGroup(bool val)
    {
        inGameGroup.SetActive(val);
    }

    public void ResetCounts(int numShards = 0)
    {
        UpdateFC(0);
        UpdateSC(0, numShards);
    }

    public static void UpdateShardCount(int currShards, int totalShards)
    {
        Instance?.UpdateSC(currShards, totalShards);
    }

    public void UpdateSC(int currShards, int totalShards)
    {
        if(totalShards == 0) {
            shardCountGroup.SetActive(false);
            return;
        }

        shardCountGroup.SetActive(true);
        shardCountText.text = $"{currShards}/{totalShards}";
    }


    public static void UpdateFoldCount(int numFolds)
    {
        Instance?.UpdateFC(numFolds);
    }

    public void UpdateFC(int numFolds)
    {
        this.numFolds = numFolds;
        string s = numFolds < 1000 ? numFolds.ToString() : "999+";
        foldCountText.text = s;
        yourFoldCountText.text = s;

    }

    private void OnGlowstickChange(object sender, GlowStickLogic.GlowStickArgs e) {
        UpdateGlow(e);
        print(e.lifetime);
    }

    private void UpdateGlow(GlowStickLogic.GlowStickArgs e)
    {
        int lifetime = e.lifetime;
        glowstickText.text = lifetime.ToString();
        if(e.state == GlowstickState.CRACKED)
            glowstickSprite.sprite = glowstickActive;
        if(e.state == GlowstickState.OFF)
            glowstickSprite.sprite = glowstickDead;
        if(e.state == GlowstickState.PRIMED)
            glowstickSprite.sprite = glowstickPrimed;
    }

    public void AddCosmetic(string cosmetic)
    {
        if(cosmetic == null 
            || !cosmeticStrings.Contains(cosmetic) 
            || cosmeticUnlocks.Contains(cosmetic)
            || SaveSystem.Current.GetCosmeticUnlock(cosmetic))
            return;
        showCosmetic = true;
        cosmeticUnlocks.Add(cosmetic);
        SaveSystem.Current.SetCosmeticUnlock(cosmetic, true);
    }

    public void EndLevel()
    {
        Level level = LevelManager.Instance.GetCurrentLevel();
        int bestFolds = SaveSystem.Current.GetFolds(level.levelName);
        if(bestFolds == -1 || numFolds < bestFolds) 
            bestFolds = numFolds;
        bestFoldCountText.text =  bestFolds < 1000 ? bestFolds.ToString() : "999+";
        Time.timeScale = 0;

        starsUI.DisplayStars(level, numFolds);
        bestStarsUI.DisplayStars(level, bestFolds);

        if(showCosmetic) {
            ShowNextCosmetic();
        }
        else
            endLevelGroup.SetActive(true);
    }

    public void ShowNextCosmetic()
    {
        string cosmetic = cosmeticUnlocks[0];
        cosmeticImage.sprite = cosmeticSprites[cosmeticStrings.IndexOf(cosmetic)];
        cosmeticBackground.sprite = cosmeticNormal;
        cosmeticText.text = "Click to Equip";
        cosmeticGroup.SetActive(true);
        cosmeticButton.SetActive(true);
    }


    public void EquiptCosmetic()
    {
        cosmeticButton.SetActive(false);
        SaveSystem.Current.SetCosmetic(cosmeticUnlocks[0]);
        cosmeticBackground.sprite = cosmeticHighlight;
        cosmeticText.text = "Equipped";
        SaveSystem.SaveGame();
    }

    public void TryShowNextCosmetic()
    {
        cosmeticUnlocks.RemoveAt(0);
        if(cosmeticUnlocks.Count == 0)
        {
            showCosmetic = false;
            cosmeticGroup.SetActive(false);
            endLevelGroup.SetActive(true);
        }
        else{
            ShowNextCosmetic();
        }
    }
    
    public void ToggleEndLevelGroup(bool val)
    {
        endLevelGroup.SetActive(val);
    }

    public void ResetLevel()
    {
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
        Time.timeScale = 1;
        LevelManager.Instance.ResetLevel();
        endLevelGroup.SetActive(false);
    }

    public void LoadNextLevel()
    {
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
        Time.timeScale = 1;
        Level level = LevelManager.Instance.GetCurrentLevel();
                endLevelGroup.SetActive(false);

        if(level.CutsceneSceneIndex != -1)
        {
            SceneManager.LoadScene(level.CutsceneSceneIndex);
        }
        else{
            LevelManager.Instance.LoadNextLevel();
        }
    }

    public void ReturnToMenu()
    {
        endLevelGroup.SetActive(false);
        LevelManager.Instance.UnloadLevel();
        Time.timeScale = 1;
    }
}
