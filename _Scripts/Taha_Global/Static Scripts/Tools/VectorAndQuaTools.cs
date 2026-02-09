using UnityEngine;

public static class VectorsAndQuaTools
{
    public static Vector3 _Vector3Maker(Quaternion quaternion)
    {
        return new Vector3(quaternion.x, quaternion.y, quaternion.z);
    }
    public static Vector3 _VectorMultiplayer(Vector3 input, Vector3 scale)
    {
        input.x *= scale.x;
        input.y *= scale.y;
        input.z *= scale.z;
        return input;
    }
    public static Quaternion _QuaternionPlusVector(Quaternion iRotation, Vector3 iVector3)
    {
        iRotation.x += iVector3.x;
        iRotation.y += iVector3.y;
        iRotation.z += iVector3.z;
        return iRotation;
    }
}