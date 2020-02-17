using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BattleManager : MonoBehaviour
{
    public Camera battleCamera;
    public UnityEvent OnBattleComplete; 
    public PlayerEntity playerEntity;
    public EnemyEntity enemyEntity;
    public Battler enemy;
    public Battler player;
    public BattleUI battleUI;
    public GameObject attacker = null;

    Camera gridCamera;
    Canvas gridCanvas;
    CameraPathAnimator cameraPathAnimator; 
    bool beginBattle = false;


    GameObject enemyStandTarget;
    GameObject playerStandTarget;
    GameObject defaultAngle;
    GameObject playerActionAngle;
    GameObject enemyActionAngle;
    GameObject enemyMagicAngle;
    GameObject playerMagicDodge;
    GameObject enemyMagicDodge;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (beginBattle) {
            beginBattle = false;

            // SHOW GUI
            battleUI.Show();

            if (!attacker)
                attacker = player.gameObject;

            switch(attacker.tag) {
                case "Player" :
                    player.attackPreview = player.entity.PreviewAttack(enemy.entity);
                    enemy.defencePreview = new Dictionary<string, int>(player.attackPreview);
                    if (enemy.entity.WithinAttackRange(player.entity.currentCellIndex))
                    {
                        enemy.attackPreview = player.entity.PreviewAttack(enemy.entity);
                        player.defencePreview = new Dictionary<string, int>(player.attackPreview);
                    }
                    
                    player.Attack(enemy);
                    player.OnAttackComplete.AddListener(delegate{
                        enemy.OnAttackComplete.AddListener(delegate{
                            if (this.enabled) {
                                battleUI.DestroyBars();
                                
                                OnBattleComplete.Invoke();

                                this.enabled = false;
                            }
                        });
                        
                        enemy.Attack(player);
                    });
                    
                    // Add Player EXP
                    return;
                case "Enemy"  :
                    enemy.attackPreview = enemy.entity.PreviewAttack(player.entity);
                    player.defencePreview = new Dictionary<string, int>(enemy.attackPreview);
                    if (player.entity.WithinAttackRange(enemy.entity.currentCellIndex))
                    {
                        player.attackPreview = player.entity.PreviewAttack(enemy.entity);
                        enemy.defencePreview = new Dictionary<string, int>(player.attackPreview);
                    }
                    
                    enemy.Attack(player);
                    
                    enemy.OnAttackComplete.AddListener(delegate{
                        player.OnAttackComplete.AddListener(delegate{
                            if (this.enabled) {
                                battleUI.DestroyBars();
                                
                                if (enemy.HealthPoints == 0) {
                                    enemy.OnDeathComplete.AddListener(delegate{
                                        OnBattleComplete.Invoke();
                                    });
                                } else {
                                    OnBattleComplete.Invoke();
                                }

                                this.enabled = false; 
                            }
                        });

                        player.Attack(enemy);
                    });
                    
                    // Add Player EXP


                    return;
                case "Other"  :
                    print("Other");
                    return;
            }

        }
    }

    public void InitialCameraPanComplete() {
        print("PAN COMPLETE");

        // Wait for 5 seconds

        beginBattle = true;
    }

    public void InitiateCameraPan(Camera gridCam) {
        gridCamera = gridCam;
        battleCamera = GameObject.Find("Battle Camera").GetComponent<Camera>();
        gridCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        gridCanvas.enabled = true;
        gridCanvas.gameObject.SetActive(true);

        InstantiateBattlers();
        SetPositions();
        
        cameraPathAnimator = GameObject.Find("InitialCameraPan").GetComponent<CameraPathAnimator>();
        battleUI = this.gameObject.GetComponent<BattleUI>();

        battleUI.enemy = enemy;
        battleUI.player = player;

        cameraPathAnimator.Play();
    }

    void InstantiateBattlers() {
        Battler[] battlers = FindObjectsOfType<Battler>();
        foreach(Battler battler in battlers)
            DestroyImmediate(battler.gameObject);

        GameObject playerStart = GameObject.Find("PlayerStart");
        GameObject enemyStart = GameObject.Find("EnemyStart");

        GameObject playerBattler = Instantiate(player.gameObject, playerStart.transform.position, playerStart.transform.rotation);
        GameObject enemyBattler = Instantiate(enemy.gameObject, enemyStart.transform.position, enemyStart.transform.rotation);
        
        GameObject battlerHolder = GameObject.Find("Battlers");
        GameObject playerHolder = battlerHolder.transform.Find("Players").gameObject;
        playerBattler.transform.SetParent(playerHolder.transform);

        GameObject enemyHolder = battlerHolder.transform.Find("Enemies").gameObject;
        enemyBattler.transform.SetParent(enemyHolder.transform);


        player = playerBattler.GetComponent<Battler>();
        enemy = enemyBattler.GetComponent<Battler>();
        player.mainCamera = battleCamera;
        enemy.mainCamera = battleCamera;
        player.entity = playerEntity;
        enemy.entity = enemyEntity;


        player.OnDodgeComplete.AddListener(DisplayDodge);
        enemy.OnDodgeComplete.AddListener(DisplayDodge);
    }

    void DisplayDodge() {
        battleUI.ShowDodge();
    }

    void SetPositions() {
        GameObject startingPositions = GameObject.Find("Starting Positions");
        playerStandTarget = startingPositions.transform.Find("PlayerStart").gameObject;
        enemyStandTarget = startingPositions.transform.Find("EnemyStart").gameObject;

        player.standTarget = playerStandTarget;
        enemy.standTarget = enemyStandTarget;

        GameObject cameraPositions = GameObject.Find("Camera Positions");
        defaultAngle = cameraPositions.transform.Find("Default").gameObject;
        playerActionAngle = cameraPositions.transform.Find("PlayerAction").gameObject;
        enemyActionAngle = cameraPositions.transform.Find("EnemyAction").gameObject;
        enemyMagicAngle = cameraPositions.transform.Find("EnemyMagicHit").gameObject;
        enemyMagicDodge = cameraPositions.transform.Find("EnemyMagicDodge").gameObject;
        playerMagicDodge = cameraPositions.transform.Find("PlayerMagicDodge").gameObject;

        enemy.defaultCameraAngle     = defaultAngle;
        enemy.battlerCameraAngle     = enemyActionAngle;
        enemy.magicCameraAngle       = playerActionAngle;
        enemy.magicTargetCameraAngle = enemyMagicAngle;
        enemy.magicDodgeCameraAngle  = enemyMagicDodge;

        player.defaultCameraAngle     = defaultAngle;
        player.battlerCameraAngle     = playerActionAngle;
        player.magicCameraAngle       = playerActionAngle;
        player.magicTargetCameraAngle = enemyMagicAngle;
        player.magicDodgeCameraAngle  = playerMagicDodge;
    }
}
