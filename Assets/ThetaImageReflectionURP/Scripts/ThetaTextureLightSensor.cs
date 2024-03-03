using System.Linq;
using UnityEngine;

namespace ThetaImageReflectionURP.Scripts
{
    public class ThetaTextureLightSensor : MonoBehaviour
    {

        [SerializeField]
        private ThetaGetLivePreview preview;
        [SerializeField]
        private Light light;

        private void Update()
        {
            ApplyIntensityValue();
        }

        private void ApplyIntensityValue()
        {
            var tex = preview.ThetaTexture;
            if (tex == null)
            {
                return;
            }
            var cols = tex.GetPixels();
            var avg = Color.black;
            avg = cols.Aggregate(avg, (current, col) => current + col);
            avg /= cols.Length;
            light.color = avg;
            light.intensity = 0.299f * avg.r + 0.587f * avg.g + 0.114f * avg.b;
        }
    }
}
