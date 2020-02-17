using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnergyBarToolkit;

public class BattleUI : MonoBehaviour
{
    public Battler player;
    public Battler enemy ;
    bool isDisplayed = false;
    bool healthBarsSet = false;
    bool showDodge = false;

    GameObject canvas;
    EnergyBar enemyBar;
    FilledRendererUGUI enemyBarRenderer;
    EnergyBar playerBar;
    FilledRendererUGUI playerBarRenderer;

    void Start() {
        canvas = GameObject.Find("Canvas");
        SetHealthBars();
    }

    void OnGUI() {
        if (isDisplayed) {
            GUI.Box(new Rect(Screen.width - 210, Screen.height - 110, 200, 100), player.gameObject.name);
            GUI.Box(new Rect(10, 10, 200, 100), enemy.gameObject.name);
            
            if (!healthBarsSet) {
                SetHealthBars();
                healthBarsSet = true;
            }

            enemyBar.SetValueCurrent(enemy.HealthPoints);
            enemyBar.valueMax = enemy.entity.BASE_STATS["HEALTH"].CalculateValue();

            playerBar.SetValueCurrent(player.HealthPoints);
            playerBar.valueMax = player.entity.BASE_STATS["HEALTH"].CalculateValue();
            

            if (enemyBarRenderer != null)
                enemyBarRenderer.enabled = true;
            if (playerBarRenderer != null)
                playerBarRenderer.enabled = true;


            if (showDodge)
                StartCoroutine("DisplayDodge");
        }
    }

    public void Show() {
        isDisplayed = true;
    }

    public void Hide() {
        isDisplayed = false;
    }

    public void DestroyBars() {
        if (enemyBar != null)
            DestroyImmediate(enemyBar.gameObject);
        
        if (playerBar != null)
            DestroyImmediate(playerBar.gameObject);
    }

    public void ShowDodge() {
        showDodge = true;
    }

    IEnumerator DisplayDodge() {
        GUI.Label(new Rect((Screen.width - (Screen.width - 300)), (Screen.height - (Screen.height - 30)), 100, 20), "DODGED!");

        yield return new WaitForSecondsRealtime(2.5f);

        showDodge = false;
    }

    void SetHealthBars() {
        GameObject enemyBarObj  = EnemyBar();
        GameObject playerBarObj = PlayerBar();

        enemyBarObj.transform.parent    = canvas.transform;
        playerBarObj.transform.parent   = canvas.transform;

        enemyBar    = enemyBarObj.GetComponent<EnergyBar>();
        playerBar   = playerBarObj.GetComponent<EnergyBar>();
        
        enemyBarRenderer = enemyBarObj.GetComponent<FilledRendererUGUI>();
        playerBarRenderer = playerBarObj.GetComponent<FilledRendererUGUI>();

        enemyBarRenderer.enabled = false;
        playerBarRenderer.enabled = false;
    }

    GameObject EnemyBar() {
        GameObject barObject = new GameObject("Enemy HP Bar");

        RectTransform rectTransform = barObject.AddComponent<RectTransform>();
        RectTransformExtensions.SetSize(rectTransform, new Vector2(150, 20));     
        rectTransform.localPosition = new Vector3((Screen.width - (Screen.width - 110)), Screen.height - 50, 0);
        
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
        
        return barObject;
    }

    GameObject PlayerBar() {
        GameObject barObject = new GameObject("Player HP Bar");

        RectTransform rectTransform = barObject.AddComponent<RectTransform>();
        RectTransformExtensions.SetSize(rectTransform, new Vector2(150, 20));      
        rectTransform.localPosition = new Vector3(Screen.width - 110, (Screen.height - (Screen.height - 65)), 0);

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
        
        return barObject;
    }
}
