using System.Collections.Generic;
using UnityEngine;

public static class PrototypeRagdollBuilder
{
    public static bool TryBuild(GameObject characterVisual, out Rigidbody[] bodies, out Collider[] colliders)
    {
        bodies = new Rigidbody[0];
        colliders = new Collider[0];
        if (characterVisual == null) return false;

        Transform hips = FindBone(characterVisual, "Hips");
        Transform chest = FindBone(characterVisual, "Chest");
        Transform neck = FindBone(characterVisual, "Neck");
        Transform head = FindBone(characterVisual, "Head");
        if (hips == null || chest == null || neck == null || head == null) return false;

        Dictionary<string, Rigidbody> boneBodies = new Dictionary<string, Rigidbody>();
        List<Collider> boneColliders = new List<Collider>();

        AddCapsuleBody(boneBodies, boneColliders, "Hips", hips, chest, 5f, 0.42f);
        AddCapsuleBody(boneBodies, boneColliders, "Chest", chest, neck, 3.5f, 0.48f);
        AddSphereBody(boneBodies, boneColliders, "Head", head, 1.2f, 0.075f);

        AddLimb(characterVisual, boneBodies, boneColliders, "LeftUpperArm", "LeftLowerArm", 1.1f, 0.34f);
        AddLimb(characterVisual, boneBodies, boneColliders, "LeftLowerArm", "LeftHand", 0.8f, 0.3f);
        AddLimb(characterVisual, boneBodies, boneColliders, "RightUpperArm", "RightLowerArm", 1.1f, 0.34f);
        AddLimb(characterVisual, boneBodies, boneColliders, "RightLowerArm", "RightHand", 0.8f, 0.3f);
        AddLimb(characterVisual, boneBodies, boneColliders, "LeftUpperLeg", "LeftLowerLeg", 2.2f, 0.42f);
        AddLimb(characterVisual, boneBodies, boneColliders, "LeftLowerLeg", "LeftFoot", 1.5f, 0.36f);
        AddLimb(characterVisual, boneBodies, boneColliders, "RightUpperLeg", "RightLowerLeg", 2.2f, 0.42f);
        AddLimb(characterVisual, boneBodies, boneColliders, "RightLowerLeg", "RightFoot", 1.5f, 0.36f);

        Connect(boneBodies, "Chest", "Hips", 25f, 20f);
        Connect(boneBodies, "Head", "Chest", 30f, 25f);
        Connect(boneBodies, "LeftUpperArm", "Chest", 55f, 40f);
        Connect(boneBodies, "LeftLowerArm", "LeftUpperArm", 15f, 60f);
        Connect(boneBodies, "RightUpperArm", "Chest", 55f, 40f);
        Connect(boneBodies, "RightLowerArm", "RightUpperArm", 15f, 60f);
        Connect(boneBodies, "LeftUpperLeg", "Hips", 35f, 35f);
        Connect(boneBodies, "LeftLowerLeg", "LeftUpperLeg", 10f, 65f);
        Connect(boneBodies, "RightUpperLeg", "Hips", 35f, 35f);
        Connect(boneBodies, "RightLowerLeg", "RightUpperLeg", 10f, 65f);

        bodies = new List<Rigidbody>(boneBodies.Values).ToArray();
        colliders = boneColliders.ToArray();
        IgnoreSelfCollisions(colliders);
        foreach (Rigidbody body in bodies)
        {
            body.isKinematic = true;
        }

        foreach (Collider ragdollCollider in colliders)
        {
            ragdollCollider.enabled = false;
        }

        return bodies.Length > 0;
    }

    private static void AddLimb(
        GameObject visual,
        Dictionary<string, Rigidbody> bodies,
        List<Collider> colliders,
        string boneName,
        string childName,
        float mass,
        float radiusScale)
    {
        Transform bone = FindBone(visual, boneName);
        Transform child = FindBone(visual, childName);
        if (bone != null && child != null)
        {
            AddCapsuleBody(bodies, colliders, boneName, bone, child, mass, radiusScale);
        }
    }

    private static void AddCapsuleBody(
        Dictionary<string, Rigidbody> bodies,
        List<Collider> colliders,
        string name,
        Transform bone,
        Transform endpoint,
        float mass,
        float radiusScale)
    {
        Rigidbody body = AddBody(bone, mass);
        Vector3 endpointLocal = bone.InverseTransformPoint(endpoint.position);
        float length = Mathf.Max(0.04f, endpointLocal.magnitude);
        int direction = DominantAxis(endpointLocal);

        CapsuleCollider capsule = bone.gameObject.AddComponent<CapsuleCollider>();
        capsule.direction = direction;
        capsule.center = endpointLocal * 0.5f;
        capsule.radius = Mathf.Max(0.018f, length * radiusScale);
        capsule.height = Mathf.Max(length, capsule.radius * 2f);

        bodies[name] = body;
        colliders.Add(capsule);
    }

    private static void AddSphereBody(
        Dictionary<string, Rigidbody> bodies,
        List<Collider> colliders,
        string name,
        Transform bone,
        float mass,
        float radius)
    {
        Rigidbody body = AddBody(bone, mass);
        SphereCollider sphere = bone.gameObject.AddComponent<SphereCollider>();
        sphere.radius = radius;
        sphere.center = Vector3.up * radius * 0.35f;
        bodies[name] = body;
        colliders.Add(sphere);
    }

    private static Rigidbody AddBody(Transform bone, float mass)
    {
        Rigidbody body = bone.GetComponent<Rigidbody>();
        if (body == null)
        {
            body = bone.gameObject.AddComponent<Rigidbody>();
        }

        body.mass = mass;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        body.maxAngularVelocity = 20f;
        body.solverIterations = 12;
        body.solverVelocityIterations = 4;
        return body;
    }

    private static void Connect(
        Dictionary<string, Rigidbody> bodies,
        string childName,
        string parentName,
        float swingLimit,
        float bendLimit)
    {
        if (!bodies.TryGetValue(childName, out Rigidbody child)) return;
        if (!bodies.TryGetValue(parentName, out Rigidbody parent)) return;

        ConfigurableJoint joint = child.gameObject.AddComponent<ConfigurableJoint>();
        joint.connectedBody = parent;
        joint.autoConfigureConnectedAnchor = false;
        joint.anchor = Vector3.zero;
        joint.connectedAnchor = parent.transform.InverseTransformPoint(child.transform.position);
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Limited;
        joint.angularYMotion = ConfigurableJointMotion.Limited;
        joint.angularZMotion = ConfigurableJointMotion.Limited;
        joint.lowAngularXLimit = new SoftJointLimit { limit = -bendLimit * 0.35f };
        joint.highAngularXLimit = new SoftJointLimit { limit = bendLimit };
        joint.angularYLimit = new SoftJointLimit { limit = swingLimit };
        joint.angularZLimit = new SoftJointLimit { limit = swingLimit };
        joint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.projectionDistance = 0.015f;
        joint.projectionAngle = 4f;
        joint.enablePreprocessing = true;
        joint.enableCollision = false;
    }

    private static void IgnoreSelfCollisions(Collider[] colliders)
    {
        for (int first = 0; first < colliders.Length; first++)
        {
            if (colliders[first] == null) continue;

            for (int second = first + 1; second < colliders.Length; second++)
            {
                if (colliders[second] != null)
                {
                    Physics.IgnoreCollision(colliders[first], colliders[second], true);
                }
            }
        }
    }

    private static int DominantAxis(Vector3 value)
    {
        Vector3 absolute = new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        if (absolute.x >= absolute.y && absolute.x >= absolute.z) return 0;
        return absolute.z >= absolute.y ? 2 : 1;
    }

    private static Transform FindBone(GameObject visual, string boneName)
    {
        foreach (Transform candidate in visual.GetComponentsInChildren<Transform>(true))
        {
            if (candidate.name == boneName || candidate.name.EndsWith(":" + boneName) || candidate.name.EndsWith("|" + boneName))
            {
                return candidate;
            }
        }

        return null;
    }
}
