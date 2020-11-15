using UnityEngine;

namespace UGUIDOTS.Render {

    public static class ShaderIDConstants {
        public static readonly int MainTex   = Shader.PropertyToID("_MainTex");
        public static readonly int ColorMask = Shader.PropertyToID("_ColorMask");
        public static readonly int Color     = Shader.PropertyToID("_Color");
        public static readonly int Fill      = Shader.PropertyToID("_Fill");
        public static readonly int FillType  = Shader.PropertyToID("_FillType");
        public static readonly int Axis      = Shader.PropertyToID("_Axis");
        public static readonly int Flip      = Shader.PropertyToID("_Flip");
    }
}
