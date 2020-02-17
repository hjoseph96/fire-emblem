using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Arrow : MonoBehaviour
{
    public Bow bow;
    public GameObject hitFX;
    protected static bool _movingToTarget;
    protected static bool _halfway;
    protected static bool _hitTarget;

    float _moveSpeed = 10f;
    static Transform _target;    
    static Arrow _arrowInstance;

    public UnityEvent OnArrowSpawned;
    public UnityEvent OnArrowFired;
    public UnityEvent HalfwayToTarget;
    public UnityEvent ReachedTarget;    

    public Arrow SpawnArrow(Bow _bow, Transform spawnPoint) {
        bow = _bow;

        _arrowInstance = Instantiate(this, spawnPoint.position, spawnPoint.rotation, spawnPoint);
        _arrowInstance.bow = bow;

        OnArrowSpawned.Invoke();

        return _arrowInstance;
    }

    public void Shoot(Transform target, Transform launchPoint) {
        if (!_movingToTarget) {
            _target  = target;

            _arrowInstance.transform.position = launchPoint.transform.position;
            _arrowInstance.transform.rotation = launchPoint.transform.rotation;
            _arrowInstance.transform.SetParent(launchPoint);  
            _arrowInstance.transform.LookAt(_target.transform);
            

            _halfway = false;
            _hitTarget = false;
            _movingToTarget = true;

            OnArrowFired.Invoke();    
        }
    }

    public void Reset() {
        _halfway = false;
        _hitTarget = false;
        _movingToTarget = false;
    }


    // Update is called once per frame
    protected virtual void Update()
    {
        if (_movingToTarget) {
            _arrowInstance.transform.position = Vector3.MoveTowards(
                _arrowInstance.transform.position,
                new Vector3(
                    _target.position.x, _target.position.y, _target.position.z + 40f
                ), (_moveSpeed * 1.7f) * Time.deltaTime
            );
            
            if(Vector3.Distance(_arrowInstance.transform.position, _target.position) < 5f && !_halfway) {
                _arrowInstance.GetComponentInChildren<Collider>().enabled = true;
                _halfway = true;

                HalfwayToTarget.Invoke();
            }

            if(Vector3.Distance(_arrowInstance.transform.position, _target.position) < 2f && !_hitTarget) {
                _hitTarget = true;
                
                ReachedTarget.Invoke();
            }
        }
    }

    void OnTriggerEnter()
    {
        GameObject hitEffect = Instantiate(hitFX, _arrowInstance.transform.position, Quaternion.identity) as GameObject;
        
        Destroy(this.gameObject, 0.3f);
        Destroy(hitEffect, 3f);

        _movingToTarget = false;
    }
}
