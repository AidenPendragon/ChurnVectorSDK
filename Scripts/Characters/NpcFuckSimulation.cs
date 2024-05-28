using DPG;
using NeedStations;
using UnityEngine;

public class NpcFuckSimulation: IFuckSimulator
{
    private CatmullSpline cachedSpline;
    public Vector3 GetHipPosition()
    {
        return currentFrame.hipPosition;
    }
    public Vector3 GetCorrectiveForce()
    {
        return currentFrame.correctiveForce;
    }

    private class DickData
    {
        private Transform hipTransform;
        public Vector3 hipPosition;
        public Vector3 hipVelocity;
        private CatmullSpline cachedSpline;
        public bool thrustingIn = true;

        public float length => penetrator.GetSquashStretchedWorldLength();
        public Vector3 dickPosition => hipPosition + hipTransform.TransformVector(hipTransform.InverseTransformPoint(penetrator.GetRootTransform().position));

        public float GetKnotForce()
        {
            return penetrator.GetPenetrationData()?.knotForce ?? 0f;
        }
        public bool isInside => penetrator.GetPenetrationData()?.tipIsInside ?? false;

        public float GetDistanceFromHole(Penetrable hole)
        {
            cachedSpline ??= new CatmullSpline(new[] { Vector3.zero, Vector3.one });
            penetrator.GetFinalizedSpline(ref cachedSpline, out var distanceAlongSpline, out var insertionLerp, out var penetrationArgs);
            if (penetrationArgs.HasValue)
            {
                return penetrationArgs.Value.penetratorData.GetWorldLength() - penetrationArgs.Value.penetrationDepth;
            }
            return Vector3.Distance(dickPosition, hole.GetPoints()[0]);
        }
        private Penetrator penetrator;
        public DickData(Transform hipTransform, Penetrator penetrator)
        {
            this.penetrator = penetrator;
            this.hipTransform = hipTransform;
            hipPosition = hipTransform.position;
            hipVelocity = Vector3.zero;
        }
    }

    private struct Frame
    {
        public Vector3 hipPosition;
        public Vector3 correctiveForce;
    }

    private Frame currentFrame;

    // Can get 50% in by default, each time it's hit it increases
    private Penetrable hole;
    private DickData dick;
    private Animator penetratorAnimator;

    public NpcFuckSimulation(Transform hipTransform, Penetrable targetHole, Penetrator targetPenetrator, Animator penetratorAnimator)
    {
        this.penetratorAnimator = penetratorAnimator;
        hole = targetHole;
        dick = new DickData(hipTransform, targetPenetrator);
        if (targetPenetrator is PenetratorJiggleDeform jiggleDeformPenetrator)
        {
            jiggleDeformPenetrator.SetLinkedPenetrable(targetHole);
        }
        else
        {
            throw new UnityException("Don't support that type of penetrator!");
        }

        currentFrame = new Frame { hipPosition = hipTransform.position };
    }

    public void SimulateStep(float dt)
    {
        SubStep(dt, Time.time);
    }

    private void SubStep(float dt, double time)
    {
        Vector3 correctiveForce = Vector3.zero;
        float friction = 0.5f;
        dick.hipVelocity *= 1f - (friction * friction);

        hole.GetHole(out var holePosition, out var holeNormal);
        holeNormal *= -1;
        float splineDistanceToHole = dick.GetDistanceFromHole(hole);

        // Hip alignment onto XY-camera plane with the hole.
        Vector3 alignmentVector = penetratorAnimator.transform.right;//cam.transform.forward;
        Vector3 dickToHole = holePosition - dick.dickPosition;
        Vector3 correction = Vector3.Project(dickToHole, alignmentVector);
        dick.hipVelocity += correction * (dt * 10f);

        //float gradientAdjustment = Mathf.Clamp01((dick.length * 1.25f - splineDistanceToHole) * 2.5f);

        float arbitraryMouseForceAdjustment = 90f;
        Vector3 thrustingVector;
        if(!dick.thrustingIn)
        {
            thrustingVector = new Vector3(0f, 0f, 1.5f);
        }
        else
        {
            thrustingVector = new Vector3(0f, 0f, -1.5f);
        }

        Vector3 diff = (thrustingVector - dick.hipPosition);

        float force = diff.magnitude;

        if (dick.isInside)
        {
            Vector3 holeToMouse = thrustingVector - holePosition;
            float dot = Vector3.Dot(holeToMouse.normalized, holeNormal);
            float newForce = Mathf.Min(force, Mathf.Lerp(Mathf.Max(dot, 0f), 1f, Vector3.Distance(dick.hipPosition, holePosition) * 10f));
            //correctiveForce += diff.normalized * ((newForce-force) * (dt * arbitraryMouseForceAdjustment));
            force = newForce;
        }

        dick.hipVelocity += diff.normalized * (force * (dt * arbitraryMouseForceAdjustment));

        // Knot forces
        if (dick.isInside)
        {
            Vector3 holeToDickDir = Vector3.Normalize(dick.hipPosition - holePosition);
            float arbitraryKnotForceAdjustment = 14f;
            float knotForce = dick.GetKnotForce() * dt * arbitraryKnotForceAdjustment;
            correctiveForce -= holeToDickDir * knotForce;
            dick.hipVelocity -= holeToDickDir * knotForce;
        }

        // Hip to hole distance limit
        if (!dick.isInside)
        {
            // Sphere constraint when outside
            Vector3 hipToHole = (holePosition + holeNormal * 0.25f) - dick.hipPosition;
            float hipToHoleDistance = hipToHole.magnitude;
            float distanceCorrection = Mathf.Max(hipToHoleDistance - 0.75f, 0f);
            if (distanceCorrection > 0f)
            {
                dick.hipVelocity = Vector3.ProjectOnPlane(dick.hipVelocity, hipToHole.normalized);
            }
            dick.hipPosition += hipToHole.normalized * (distanceCorrection);
        }
        else
        {
            //Vector3 adjustedHolePosition = holePosition + holeNormal * 0.020f;
            // Cone constraint when inside
            Vector3 holeToDick = dick.dickPosition - holePosition;
            float dot = Vector3.Dot(holeToDick.normalized, holeNormal);
            float radius = holeToDick.magnitude * 0.5f * Mathf.Max(dot, 0f);
            Vector3 projectedPoint = Vector3.Project(Mathf.Sign(dot) * holeToDick, holeNormal) + holePosition;
            Vector3 dickToProjectedPoint = projectedPoint - dick.dickPosition;
            float distanceCorrection = Mathf.Max(dickToProjectedPoint.magnitude - radius, 0f);
            if (distanceCorrection > 0f)
            {
                dick.hipVelocity = Vector3.ProjectOnPlane(dick.hipVelocity, dickToProjectedPoint.normalized);
            }
            correctiveForce += dickToProjectedPoint.normalized * distanceCorrection;
            dick.hipPosition += dickToProjectedPoint.normalized * distanceCorrection;
        }

        dick.hipPosition += dick.hipVelocity * dt;
        if(dick.hipVelocity.z <= 0.100)
        {
            dick.thrustingIn = !dick.thrustingIn;
        }

//        Debug.Log("HipVel = " + (dick.hipVelocity).ToString());
//        Debug.Log("HipPos = " + (dick.hipPosition).ToString());
//        Debug.Log("thrusting In = " + (dick.thrustingIn).ToString());

        currentFrame = new Frame { hipPosition = dick.hipPosition, correctiveForce = correctiveForce };
    }
}