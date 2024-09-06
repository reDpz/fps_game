using Godot;
using System.Runtime.CompilerServices;


public static class math
{
    // from source sdk
    public static float vec3_normalize(ref Vector3 vec)
    {
        float sqr_len = (vec.X * vec.X + vec.Y * vec.Y + vec.Z * vec.Z) + 1.0e-10f;
        float inv_len = 1.0f / Mathf.Sqrt(sqr_len);
        vec.X *= inv_len;
        vec.Y *= inv_len;
        vec.Z *= inv_len;

        return sqr_len * inv_len;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float xz_length_vec3(Vector3 vec)
    {
        return Mathf.Sqrt(vec.X * vec.X + vec.Z * vec.Z);
    }

}
