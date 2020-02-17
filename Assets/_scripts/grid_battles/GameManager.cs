using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public int turn;
    public string currentPhase;
    public Dictionary<string, int> PHASES;
    public List<Entity> entities;
    public List<PlayerEntity> playerEntities;
    public List<EnemyEntity> enemyEntities;
    public List<OtherEntity> otherEntities = new List<OtherEntity>();

    public Light gridDirectionalLight;
    public Canvas gridCanvas;


    private Camera gridCamera;
    private Scene battleScene;
    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    private InMapGUI inMapGUI;
    private TGSCellSelect gridSelector;
    private BattleManager battleManager;
    private EnemyEntity targetedEnemy;
    
    private EnemyEntity movingEnemy;
    private OtherEntity movingOtherEntity; 

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 20;

        turn = 0;

        // PHASES["Player"] = 0;
        // PHASES["Other"] = 1;
        // PHASES["Enemy"] = 2;
        
        Entity[] mapEntities = FindObjectsOfType<Entity>();
        PopulateEntities(mapEntities);   
        
        gridCamera = this.gameObject.GetComponent<Camera>();
        inMapGUI = this.gameObject.GetComponent<InMapGUI>();
        gridSelector = this.gameObject.GetComponent<TGSCellSelect>();
        MeshRenderer[] renderers = FindObjectsOfType<MeshRenderer>();
        foreach(MeshRenderer renderer in renderers) {
            if (renderer.gameObject.tag != "Grid")
                meshRenderers.Add(renderer);
        }

        StartPhase("Player");
    }

    // Update is called once per frame
    void Update()
    {   
        ProcessPhase();
    }

    public List<PlayerEntity> UnmovedPlayers() {
        List<PlayerEntity> unmovedPlayers = new List<PlayerEntity>();

        foreach(PlayerEntity player in playerEntities) {
            if (!player.HasMoved)
                unmovedPlayers.Add(player);
        }

        unmovedPlayers.Sort(delegate(PlayerEntity player1, PlayerEntity player2) {
            return player1.moveSpeed.CompareTo(player2.moveSpeed);
        });

        return unmovedPlayers;
    }

    
    public List<PlayerEntity> MovedPlayers() {
        List<PlayerEntity> movedPlayers = new List<PlayerEntity>();

        foreach(PlayerEntity player in playerEntities) {
            if (player.HasMoved)
                movedPlayers.Add(player);
        }

        return movedPlayers;
    }

    
    public List<EnemyEntity> UnmovedEnemies() {
        List<EnemyEntity> unmovedEnemies = new List<EnemyEntity>();

        foreach(EnemyEntity enemy in enemyEntities) {
            if (!enemy.HasMoved)
                unmovedEnemies.Add(enemy);
        }

        unmovedEnemies.Sort(delegate(EnemyEntity enemy1, EnemyEntity enemy2) {
            return enemy1.moveSpeed.CompareTo(enemy2.moveSpeed);
        });

        return unmovedEnemies;
    }

    
    public List<EnemyEntity> MovedEnemies() {
        List<EnemyEntity> movedEnemies = new List<EnemyEntity>();

        foreach(EnemyEntity enemy in enemyEntities) {
            if (enemy.HasMoved)
                movedEnemies.Add(enemy);
        }

        return movedEnemies;
    }

    public void NextPhase() {
        switch (currentPhase) {
            case "Player" :
                if (UnmovedPlayers().Count == 0)
                    StartPhase("Other");
                return;
            case "Other"  :
                if (otherEntities.Count == 0)
                    StartPhase("Enemy");
                return;
            case "Enemy"  :
                if (enemyEntities.Count > 0 && UnmovedEnemies().Count == 0) {
                    StartPhase("Player");
                }
                return;
        }
    }

    void StartPhase(string newPhase) {
        switch(newPhase) {
            case "Player" :
                turn += 1;
                currentPhase = "Player";

                // TODO: Some sort of TURN # transition
                // TODO: Some sort of PLAYER PHASE transition
                
                SetPlayersToMove();
                gridSelector.NextPlayer( UnmovedPlayers()[0] );
                             
                return;
            case "Enemy"  :
                SetEnemiesToMove();
                
                currentPhase = "Enemy";
                
                return;
            case "Other"  :
                currentPhase = "Other";
                
                return;
        }
    }

    void ProcessPhase() {        
        switch(currentPhase) {
            case "Other" :
                if (otherEntities.Count == 0)
                    NextPhase();
                return;
            case "Enemy" :
                inMapGUI.isDisplayed = false;

                 if (UnmovedEnemies().Count > 0) {
                    movingEnemy = UnmovedEnemies()[0];
                    
                    movingEnemy.turnToMove = true;
                } else {
                    NextPhase();
                }
                return;
        }
    }

    void SetPlayersToMove() {
        foreach(PlayerEntity player in playerEntities) {
            player.HasMoved = false;
            player.hasAttacked = false;
        }
    }

    void SetEnemiesToMove() {
        foreach(EnemyEntity enemy in enemyEntities) {
            enemy.HasMoved = false;
            enemy.hasAttacked = false;
        }
    }

    bool HideMeshes() {
        bool allInactive = true;

        foreach (MeshRenderer renderer in meshRenderers)
            renderer.gameObject.SetActive(false);

        foreach (MeshRenderer renderer in meshRenderers) {
            if (renderer.gameObject.activeSelf)
                allInactive = false;
        }

        return allInactive;
    }



    bool ShowMeshes() {
        bool allActive = true;

        foreach (MeshRenderer renderer in meshRenderers)
            renderer.gameObject.SetActive(true);

        foreach (MeshRenderer renderer in meshRenderers) {
            if (!renderer.gameObject.activeSelf)
                allActive = false;
        }

        return allActive;
    }
    void PopulateEntities(Entity[] mapEntities) {
        foreach(Entity entity in mapEntities) {
            entities.Add(entity);

            // Populate Entity Lists
            if (entity.EntityType == "player") {
                PlayerEntity player = entity.gameObject.GetComponent<PlayerEntity>();
                
                playerEntities.Add(player);
            } else if (entity.EntityType == "enemy") {
                EnemyEntity enemy = entity.gameObject.GetComponent<EnemyEntity>();
                
                enemyEntities.Add(enemy);
            }

            // Remove Entity upon Death
            entity.OnDeathEvent.AddListener(delegate{
                entity.DestroyHealthBar();
                
                entities.Remove(entity);

                if (entity.EntityType == "player") {
                    playerEntities.Remove(entity.gameObject.GetComponent<PlayerEntity>());
                } else if (entity.EntityType == "enemy") {
                    enemyEntities.Remove(entity.gameObject.GetComponent<EnemyEntity>());
                }
                
                DestroyImmediate(entity.gameObject);
            });
        }
    }

    // Player Version
    public void LoadBattleScene(EnemyEntity enemyEntity) {
        gridCanvas.enabled = false;
        gridDirectionalLight.enabled = false;

        gridSelector.GetSelectedPlayer().hasAttacked = true;

        targetedEnemy = enemyEntity;
        if (SceneManager.sceneCount == 1) {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive);
        }
    }
    
    // AI Version
    public void LoadBattleScene(EnemyEntity enemyEntity, PlayerEntity playerEntity) {
        gridCamera.gameObject.SetActive(false);
        gridCanvas.enabled = false;
        gridDirectionalLight.enabled = false;

        enemyEntity.hasAttacked = true;

        targetedEnemy = enemyEntity;
        
        if (SceneManager.sceneCount == 1) {
            SceneManager.sceneLoaded += delegate(Scene scene, LoadSceneMode mode) {
                OnSceneLoaded(scene, mode, playerEntity);
            };

            SceneManager.LoadScene("BattleScene", LoadSceneMode.Additive);
        }
    }

    // Player Version
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Switch cameras & turn off meshes
        gridCamera.gameObject.SetActive(false);
        HideMeshes();

        battleScene = scene;
        SceneManager.SetActiveScene(scene);

        GameObject battleManagerObj = GameObject.FindGameObjectWithTag("GameController"); 
        battleManager = battleManagerObj.GetComponent<BattleManager>();
        
        PlayerEntity selectedPlayer = gridSelector.GetSelectedPlayer();  
        battleManager.playerEntity = selectedPlayer;
        battleManager.player = selectedPlayer.battler;

        battleManager.enemyEntity = targetedEnemy;
        battleManager.enemy = targetedEnemy.battler;

        battleManager.attacker = selectedPlayer.gameObject;
        
        inMapGUI.HideHealthBars();
        battleManager.OnBattleComplete.AddListener(UnloadBattleScene);
        battleManager.InitiateCameraPan(gridCamera);
    }

    // AI Version
    void OnSceneLoaded(Scene scene, LoadSceneMode mode, PlayerEntity targetedPlayer) {
        // Switch cameras & turn off meshes
        gridCamera.gameObject.SetActive(false);
        HideMeshes();

        battleScene = scene;
        SceneManager.SetActiveScene(scene);

        GameObject battleManagerObj = GameObject.FindGameObjectWithTag("GameController"); 
        battleManager = battleManagerObj.GetComponent<BattleManager>();
        
        PlayerEntity selectedPlayer = targetedPlayer;  
        battleManager.playerEntity = selectedPlayer;
        battleManager.player = selectedPlayer.battler;

        battleManager.enemyEntity = targetedEnemy;
        battleManager.enemy = targetedEnemy.battler;

        battleManager.attacker = targetedEnemy.gameObject;

        inMapGUI.HideHealthBars();

        Entity entity = battleManager.attacker.GetComponent<Entity>();
        battleManager.OnBattleComplete.AddListener(delegate {
            UnloadBattleScene(entity);
        });

        battleManager.InitiateCameraPan(gridCamera);
    }

    // Player Version
    void UnloadBattleScene() {
        // Maybe add a fade out/in or transition?
        ShowMeshes();
        
        gridDirectionalLight.enabled = true;
        gridCanvas.enabled = true;
        inMapGUI.ClearBattleMode();
        
        gridCamera.gameObject.SetActive(true);

        Entity entity = battleManager.attacker.GetComponent<Entity>();
        entity.hasAttacked = true;
        entity.HasMoved = true;

        NextPlayer();

        SceneManager.UnloadSceneAsync(battleScene, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
    }
    
    // Enemy Version
    void UnloadBattleScene(Entity attacker) {
        // Maybe add a fade out/in or transition?
        ShowMeshes();

        gridDirectionalLight.enabled = true;
        gridCanvas.enabled = true;
        inMapGUI.ClearBattleMode();

        gridCamera.gameObject.SetActive(true);
        
        attacker.hasAttacked = true;
        attacker.HasMoved = true;

        SceneManager.UnloadSceneAsync(battleScene);
    }

    public void NextPlayer() {
        gridSelector.ClearActionSelectMode();
        
        List<PlayerEntity> unmovedPlayers = UnmovedPlayers(); 
        
        if (unmovedPlayers.Count > 0) {
            gridSelector.NextPlayer(unmovedPlayers[0]);
        } else {
            NextPhase();
        }
    }
}
