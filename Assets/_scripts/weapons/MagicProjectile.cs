using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MagicProjectile : MagicEffect
{
    public GameObject hitFX;

    float _moveSpeed = 6f;
    static Transform _target;
    
    static MagicProjectile _projectileInstance;

    static UnityEvent _magicEffectSpawned;
    static UnityEvent _halfwayToTarget;
    static UnityEvent _reachedTarget;    


    public MagicProjectile Launch(Grimiore _grimiore, Vector3 spawnPoint, Transform target) {
        if (!_movingToTarget) {
            _target  = target;
            grimiore = _grimiore;
          
            _magicEffectSpawned = grimiore.OnParticleEffectSpawn;
            _halfwayToTarget = grimiore.OnParticleEffectHalfway;
            _reachedTarget = grimiore.OnParticleEffectLanded;

            _projectileInstance = Instantiate(this, spawnPoint, Quaternion.Euler(0, 0, 0));
            _projectileInstance.transform.LookAt(_target.transform);
    
            _halfway = false;
            _hitTarget = false;

            _movingToTarget = true;
            
            _magicEffectSpawned.Invoke();
            OnEffectSpawn.Invoke();

            return _projectileInstance;
        }

        throw new UnityException("Unable to spawn Projectile");
    }

   

    // Update is called once per frame
    protected virtual void Update()
    {
        if (_movingToTarget) {
            _projectileInstance.transform.Translate(
                new Vector3(
                    0, 
                    0,
                    _moveSpeed * Time.deltaTime
                )
            );
 
            if(Vector3.Distance(_projectileInstance.transform.position, _target.position) < 5f && !_halfway) {
                _halfway = true;

                _halfwayToTarget.Invoke();

                OnBeforeEffectHit.Invoke();
            }

            if(Vector3.Distance(_projectileInstance.transform.position, _target.position) < 2f && !_hitTarget) {
                _hitTarget = true;
                
                _reachedTarget.Invoke();

                OnEffectHit.Invoke();
            }
        }
    }

    void OnTriggerEnter()
    {
        GameObject expl = Instantiate(hitFX, _projectileInstance.transform.position, Quaternion.identity) as GameObject;
        Destroy(this.gameObject, 0.3f);
        Destroy(expl, 3);           // delete the explosion after 3 seconds
        _movingToTarget = false;
    }
}
