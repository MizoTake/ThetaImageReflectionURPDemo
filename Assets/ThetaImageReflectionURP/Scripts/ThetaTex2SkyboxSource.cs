using UnityEngine;

namespace ThetaImageReflectionURP.Scripts
{
    [RequireComponent(typeof(ThetaGetLivePreview))]
    [RequireComponent(typeof(ApplySkyboxTexture))]
    public class ThetaTex2SkyboxSource : MonoBehaviour
    {

        private ThetaGetLivePreview getLivePreview;
        private ApplySkyboxTexture skyboxTexture;

        private void Update()
        {
            if (getLivePreview == null || skyboxTexture == null)
            {
                getLivePreview = GetComponent<ThetaGetLivePreview>();
                skyboxTexture = GetComponent<ApplySkyboxTexture>();
            }

            var sourceTex = getLivePreview.ThetaTexture;
            if (sourceTex == null) return;
            skyboxTexture.Set(sourceTex);
        }
    }
}
