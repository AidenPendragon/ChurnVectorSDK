using UnityEngine;

namespace NeedStations
{
    public interface IFuckSimulator
    {
        Vector3 GetHipPosition();
        Vector3 GetCorrectiveForce();
        void SimulateStep(float dt);
    }
}
