using UnityEngine;

namespace ThetaImageReflectionURP.Scripts
{
    public class ApplySkyboxTexture : MonoBehaviour
    {
        
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        
        [SerializeField]
        private Material skyboxMat;
        [SerializeField]
        private int downSample = 0;
        [SerializeField]
        private FilterMode filterMode = FilterMode.Bilinear;
        [SerializeField]
        private Texture sourceTex;

        private RenderTexture outputTexture;

        public void Set(Texture source)
        {
            sourceTex = source;
        }

        private void Initialize()
        {
            var width = sourceTex.width >> downSample;
            var height = sourceTex.height >> downSample;
            outputTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Default)
                {
                    useMipMap = true,
                    autoGenerateMips = true,
                    filterMode = filterMode
                };

            if (skyboxMat != null)
            {
                RenderSettings.skybox = skyboxMat;
                skyboxMat.SetTexture(MainTex, outputTexture);
            }

            Shader.SetGlobalTexture(MainTex, outputTexture);
        }

        private void LateUpdate()
        {
            if (sourceTex)
            {
                if (outputTexture == null) Initialize();
            }
            else
            {
                Debug.LogWarning($"{nameof(ApplySkyboxTexture)}: Missing Source Texture");
                return;
            }
            
            Graphics.Blit(sourceTex, outputTexture);

            if (skyboxMat != null)
            {
                RenderSettings.skybox = skyboxMat;
                skyboxMat.SetTexture(MainTex, outputTexture);
            }

            Shader.SetGlobalTexture(MainTex, outputTexture);
        }
    }
}
