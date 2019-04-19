using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager : MonoBehaviour
{
    [SerializeField] ParticleSystem knockbackTrail;
    [SerializeField] ParticleSystem dashPoof;
    [SerializeField] ParticleSystem hardLandPoof;
    [SerializeField] GameObject swordHitPrefab;
    [SerializeField] GameObject lightHitPrefab;
    [SerializeField] GameObject bigHitPrefab;
    [SerializeField] GameObject ringOutPrefab;
    [SerializeField] CameraShake camShake;

    Avatar avatar;

    private void Start()
    {
        avatar = GetComponent<Avatar>();
    }

    public void ShakeCamera(float seconds)
    {
        camShake.ShakeForTime(seconds);
    }

    public void PlayTrail(bool play)
    {
        if (play)
        {
            if (!knockbackTrail.isPlaying)
            {
                knockbackTrail.Play();
            }
        }
        else
        {
            knockbackTrail.Stop();
        }
    }

    public void PlayDashPoof()
    {
        dashPoof.Play();
    }

    public void PlayHardLandPoof()
    {
        hardLandPoof.Play();
    }

    public void SpawnHitEffect(GameObject prefab, Vector3 position)
    {
        var go = Instantiate(prefab);
        go.transform.position = position;
    }

    public void SpawnSwordHit(Vector3 position)
    {
        var go = Instantiate(swordHitPrefab);
        go.transform.position = position;
    }

    public void SpawnLightHit(Vector3 position)
    {
        var go = Instantiate(lightHitPrefab);
        go.transform.position = position;
    }
    public void SpawnBigHit(Vector3 position)
    {
        var go = Instantiate(bigHitPrefab);
        go.transform.position = position;
    }

    public void SpawnRingOutSplash(Vector3 position, Quaternion rotation)
    {
        var go = Instantiate(ringOutPrefab);
        go.transform.position = position;
        go.transform.rotation = rotation;
    }
}
