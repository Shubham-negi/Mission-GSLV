// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Required for compatibility between Unity 5.2, 5.3.3 and 5.4.

// Tranforms position from object to homogenous space
inline float4 QwrUnityObjectToClipPos(in float3 pos) {
#if defined(UNITY_5_4_OR_NEWER)
    return UnityObjectToClipPos(pos);
#else

#if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)
    // More efficient than computing M*VP matrix product
    return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
#else
    return UnityObjectToClipPos(float4(pos, 1.0));
#endif  // defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)

#endif  // defined(UNITY_5_4_OR_NEWER)
}
