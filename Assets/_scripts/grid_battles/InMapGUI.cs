using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnergyBarToolkit;
public class InMapGUI : MonoBehaviour
{
    public bool isDisplayed;
    GameManager gameManager;
    TGSCellSelect gridSelector;

    bool inBattleMode = false;
    bool hpBarsDrawn = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = this.gameObject.GetComponent<GameManager>();
        gridSelector = this.gameObject.GetComponent<TGSCellSelect>();    
    }

    void OnGUI() {
        if (!hpBarsDrawn)
            DrawHealthBars();

        
        if (isDisplayed && !inBattleMode) {
            gridSelector.SetActionSelectMode();

            // Make a background box
            GUI.Box(new Rect(10,10,100,90), "Actions");

            PlayerEntity selectedPlayer = gridSelector.GetSelectedPlayer();
            if (gridSelector.CanAttack() && !selectedPlayer.hasAttacked) {                
                EnemyEntity selectedEnemy = gridSelector.GetSelectedEnemy();
                
                if(GUI.Button(new Rect(20,40,80,20), "Attack"))
                {
                    inBattleMode = true;           
                    gameManager.LoadBattleScene(selectedEnemy);
                }

                Dictionary<string, int> preview = selectedPlayer.PreviewAttack(selectedEnemy);

                GUI.Box(new Rect((Screen.width - (Screen.width - 500)), (Screen.height - (Screen.height - 10)), 200, 100), "Preview Attack");
                GUI.Label(new Rect((Screen.width - (Screen.width - 500)), (Screen.height - (Screen.height - 30)), 100, 20), "ATK DMG: " + preview["ATK_DMG"]);
                GUI.Label(new Rect((Screen.width - (Screen.width - 500)), (Screen.height - (Screen.height - 50)), 100, 20), "ACCURACY: " + preview["ACCURACY"]);
                GUI.Label(new Rect((Screen.width - (Screen.width - 500)), (Screen.height - (Screen.height - 70)), 100, 20), "CRIT RATE: " + preview["CRIT_RATE"]);
            }
        
            // Make the second button.
            if (!selectedPlayer.HasMoved) {
                if(GUI.Button(new Rect(20,70,80,20), "Wait")) 
                {   
                    isDisplayed = false;
                    
                    gameManager.NextPlayer();
                    
                    selectedPlayer.hasAttacked = true;
                }
            } else {
                isDisplayed = false;
            }
        }

        if (!inBattleMode) {
            // Make a background box
            GUI.Box(new Rect((Screen.width - (Screen.width - 300)), (Screen.height - (Screen.height - 10)), 200, 100), "Game Info");
            GUI.Label(new Rect((Screen.width - (Screen.width - 300)), (Screen.height - (Screen.height - 30)), 100, 20), "Turn #: " + gameManager.turn);
            GUI.Label(new Rect((Screen.width - (Screen.width - 300)), (Screen.height - (Screen.height - 50)), 100, 20), gameManager.currentPhase + " Phase ");
        }
    }

    public void ClearBattleMode() {
        inBattleMode = false;
        ShowHealthBars();
    }

    public void HideHealthBars() {
        foreach(Entity entity in gameManager.entities) {
            entity.DisableHealthBar();
        }
    }
    
    void ShowHealthBars() {
        foreach(Entity entity in gameManager.entities) {
            entity.EnableHealthBar();
        }
    }

    void DrawHealthBars() {
        foreach(Entity entity in gameManager.entities) {
            if (entity.gameObject.tag == "Player") {
                GameObject miniHPBar = PlayerBar(entity);
                
                miniHPBar.transform.SetParent(gameManager.gridCanvas.transform);

                entity.SetHealthBar(miniHPBar.GetComponent<EnergyBar>());
            } else if (entity.gameObject.tag == "Enemy") {
                GameObject miniHPBar = EnemyBar(entity);

                miniHPBar.transform.SetParent(gameManager.gridCanvas.transform);

                entity.SetHealthBar(miniHPBar.GetComponent<EnergyBar>());
            }
        }

        hpBarsDrawn = true;
    }

    GameObject EnemyBar(Entity enemyMapEntity) {
        GameObject barObject = new GameObject("Enemy HP Bar");

        RectTransform rectTransform = barObject.AddComponent<RectTransform>();
        RectTransformExtensions.SetSize(rectTransform, new Vector2(70, 10));     
        
        Sprite spriteBar = Resources.Load<Sprite>("simple_1_bar");
        Sprite spriteFg  = Resources.Load<Sprite>("simple_1_fg");
        EnergyBarUGUIBase.SpriteTex spriteFgTex = new EnergyBarUGUIBase.SpriteTex();
        spriteFgTex.sprite = spriteFg;
        
        List<EnergyBarUGUIBase.SpriteTex> foregroundSprites = new List<EnergyBarUGUIBase.SpriteTex>();
        foregroundSprites.Add(spriteFgTex);

        FilledRendererUGUI renderer = barObject.AddComponent<FilledRendererUGUI>();
        renderer.spriteBar = spriteBar;
        renderer.spriteBarColor = Color.red;
        renderer.spritesForeground = foregroundSprites;

        EnergyBar bar = barObject.AddComponent<EnergyBar>();

        EnergyBarFollowObject barFollow = barObject.AddComponent<EnergyBarFollowObject>();
        barFollow.followObject = enemyMapEntity.gameObject.transform.Find("HP Bar Anchor").gameObject;
        
        return barObject;
    }

    GameObject PlayerBar(Entity playerMapEntity) {
        GameObject barObject = new GameObject("Player HP Bar");

        RectTransform rectTransform = barObject.AddComponent<RectTransform>();
        RectTransformExtensions.SetSize(rectTransform, new Vector2(70, 10));      

        Sprite spriteBar = Resources.Load<Sprite>("simple_1_bar");
        Sprite spriteFg  = Resources.Load<Sprite>("simple_1_fg");
        EnergyBarUGUIBase.SpriteTex spriteFgTex = new EnergyBarUGUIBase.SpriteTex();
        spriteFgTex.sprite = spriteFg;
        
        List<EnergyBarUGUIBase.SpriteTex> foregroundSprites = new List<EnergyBarUGUIBase.SpriteTex>();
        foregroundSprites.Add(spriteFgTex);

        FilledRendererUGUI renderer = barObject.AddComponent<FilledRendererUGUI>();
        renderer.spriteBar = spriteBar;
        renderer.spriteBarColor = Color.green;
        renderer.spritesForeground = foregroundSprites;
        
        EnergyBar bar = barObject.AddComponent<EnergyBar>();

        EnergyBarFollowObject barFollow = barObject.AddComponent<EnergyBarFollowObject>();
        barFollow.followObject = playerMapEntity.gameObject.transform.Find("HP Bar Anchor").gameObject;
        
        return barObject;
    }
}
