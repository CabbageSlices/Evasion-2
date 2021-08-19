using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class RNGManagerSystem : SystemBase
{
    public NativeArray<Unity.Mathematics.Random> randomNumberGenerators {
        get; private set;
    }

    protected override void OnCreate()
    {
        base.OnCreate();

        //just randomly adding two because nativeThreadIndex in a job is exceeding the number of max workers.
        //it only exceeds by 2 so far, but it could exceed by more. If that is the case, then a different solution will be needed
        int maxJobWorkers = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerMaximumCount + 2;
        Unity.Mathematics.Random[] rngs = new Unity.Mathematics.Random[maxJobWorkers];

        for(int i = 0; i < maxJobWorkers; ++i) {
            rngs[i] = new Unity.Mathematics.Random( (uint)(i + 1) * 134);
        }

        randomNumberGenerators = new NativeArray<Unity.Mathematics.Random>(rngs, Allocator.Persistent);
        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        randomNumberGenerators.Dispose();
    }
    protected override void OnUpdate()
    {

    }
}
