using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

//system group corresponding to user input handling, i.e keyboard/mouse inpout
[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
public class InputSystemGroup : ComponentSystemGroup
{

}
