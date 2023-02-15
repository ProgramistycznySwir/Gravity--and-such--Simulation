using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct PlanetJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<float> Masses;
    [ReadOnly] public float GravitationalConstant;
    [ReadOnly] public float RepelingForceConstant;
    [ReadOnly] public NativeArray<float3> CurrPositions;
    public NativeArray<float3> Forces;

    public PlanetJob(NativeArray<float> masses, float gravitationalConstant, float repelingForceConstant, NativeArray<float3> currPositions, NativeArray<float3> forces)
    {
        Masses = masses;
        GravitationalConstant = gravitationalConstant;
        RepelingForceConstant = repelingForceConstant;
        CurrPositions = currPositions;
        Forces = forces;
    }

    public void Execute(int index)
    {
        float3 pos = CurrPositions[index];
        float mass = Masses[index];

        float3 force = 0;
        for(int i  = 0; i < CurrPositions.Length; i++)
        {
            // Ignore itself.
            if(i == index)
                continue;

            float3 otherPos = CurrPositions[i];
            float otherMass = Masses[i];

            float3 dirr = otherPos - pos;
            float distSq = math.distancesq(pos, otherPos);

            float atractingForce = (mass * otherMass) / distSq;
            // float repelingForce = atractingForce / distSq;
            // force += math.normalize(dirr) * (atractingForce * GravitationalConstant - repelingForce * RepelingForceConstant);
            force += math.normalize(dirr) * (atractingForce * GravitationalConstant);
        }

        Forces[index] = force;
    }
}