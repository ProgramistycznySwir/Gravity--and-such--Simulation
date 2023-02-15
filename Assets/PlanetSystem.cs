using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PlanetSystem : MonoBehaviour
{
    #region >>> Job Data <<<
    NativeArray<float> _masses;
    NativeArray<float3> _currPositions;
    NativeArray<float3> _forces;
    #endregion >>> Job Data <<<

    // List<Planet> _planets;
    List<Transform> _planetTransforms;
    List<Rigidbody> _planetRigidBodies;

    public int planetCount = 100;
    public Vector3 spawnArea = Vector3.one * 100f;
    public float mass_min = 1;
    public float mass_max = 1;
    public float gravitationalConstant = 1;
    public float repelingForceConstant = 4;

    public GameObject planetPrefab;

    public void Awake()
    {
        SpawnPlanets();
        InitArrays();
    }

    void InitArrays()
    {
        _masses        = new(_planetTransforms.Count, Allocator.Persistent);
        _currPositions = new(_planetTransforms.Count, Allocator.Persistent);
        _forces        = new(_planetTransforms.Count, Allocator.Persistent);
    }

    static Vector3 RandVec3(Vector3 bounds)
        => new Vector3(
                x: UnityEngine.Random.Range(-bounds.x, bounds.x),
                y: UnityEngine.Random.Range(-bounds.y, bounds.y),
                z: UnityEngine.Random.Range(-bounds.z, bounds.z)
            );

    void SpawnPlanets()
    {
        _planetTransforms =
            Enumerable.Range(0, planetCount)
            .Select(_ => GameObject.Instantiate(planetPrefab, RandVec3(spawnArea), Quaternion.identity))
            .Select(gameObject => gameObject.transform)
            .ToList();
        _planetRigidBodies =
            _planetTransforms
            .Select(transform => transform.GetComponent<Rigidbody>())
            .ToList();
        foreach(var rb in _planetRigidBodies)
            rb.mass = UnityEngine.Random.Range(mass_min, mass_max);

        // _planets =
        //     _planetTransforms
        //     .Select(transform => transform.GetComponent<Planet>())
        //     .ToList();
        // foreach(var planet in _planets)
        //     planet.OnCollidedWith =
    }

    public void OnDestroy()
    {
        _masses.Dispose();
        _currPositions.Dispose();
        _forces.Dispose();
    }

    public void Update()
    {
        // MergePlanets();

        for (int i = 0; i < _planetTransforms.Count; i++)
        {
            _currPositions[i] = _planetRigidBodies[i].position;
            _masses[i] = _planetRigidBodies[i].mass;
        }

        new PlanetJob (
                masses: _masses,
                gravitationalConstant: gravitationalConstant,
                repelingForceConstant: repelingForceConstant,
                currPositions: _currPositions,
                forces: _forces
            )
            .Schedule(_planetTransforms.Count, 12)
            .Complete();

        for (int i = 0; i < _planetTransforms.Count; i++)
        {
            _planetRigidBodies[i].AddForce(_forces[i], ForceMode.Force);
        }
    }

    // List<(Transform self, Transform other)> toMerge = new();

    // public void AddCollided(Transform self, Transform other)
    // {
    //     lock(toMerge)
    //         toMerge.Add((self, other));
    // }

    // public void MergePlanets()
    // {
    //     // while(toMerge.Count is not 0)
    //     // {
    //     //     var pair = toMerge.Last();
    //     //     toMerge.RemoveAt(toMerge.Count-1);
    //     //     while(toMerge.FindIndex(item => item.other == pair.self && item.self == pair.other || item.other == pair.other && item.self == item.other) is int idx && idx is not -1)
    //     //         toMerge.RemoveAt(idx);

    //     //     // Destroy(pair.self.GetComponent<Planet>());
    //     //     // Destroy(pair.other.GetComponent<Planet>());

    //     //     var selfRB = pair.self.GetComponent<Rigidbody>();
    //     //     var otherRB = pair.other.GetComponent<Rigidbody>();



    //     //     float summaryMass
    //     // }
    // }
}