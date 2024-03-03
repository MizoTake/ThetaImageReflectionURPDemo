using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

namespace ThetaImageReflectionURP.Scripts
{
    public class ThetaGetLivePreview : MonoBehaviour
    {

        private const int MaxWidth = 3840;
        private const int MaxHeight = 1920;
        private const int PoolCount = 10;
        private const string Url = "http://192.168.1.1:80/osc/commands/execute";

        private enum ResolutionPer
        {
            Full = 1,
            Half = 2,
        }

        [SerializeField]
        private ResolutionPer resolutionPer = ResolutionPer.Half;
        
        private int currentPreviewIndex;
        private List<Texture2D> previewTextureList = new(PoolCount);
        private BinaryReader binaryReader;

        public Texture ThetaTexture => previewTextureList[currentPreviewIndex];

        void Start()
        {
            for (var i = 0; i < PoolCount; i++)
            {
                previewTextureList.Add(new Texture2D(MaxWidth / (int)resolutionPer, MaxHeight / (int)resolutionPer));
            }

            var request = WebRequest.Create(Url);
            request.Method = "POST";
            request.Timeout = 300000;
            request.ContentType = "application/json;charset=utf-8";
            var postBody = Encoding.UTF8.GetBytes("{\"name\":\"camera.getLivePreview\"}");
            request.ContentLength = postBody.Length;
        
            var requestStream = request.GetRequestStream();
            requestStream.Write(postBody, 0, postBody.Length);
            requestStream.Close();
            var responseStream = request.GetResponse().GetResponseStream();

            binaryReader = new BinaryReader(new BufferedStream(responseStream), new ASCIIEncoding());

            StartCoroutine(GetLivePreview());
        }

        private IEnumerator GetLivePreview()
        {
            var dataList = new List<byte>();
            var isLoadStart = false;

            while (true)
            {
                var byteData = binaryReader.ReadByte();
                if (!isLoadStart){
                    if (byteData == 0xFF){
                        var nextData = binaryReader.ReadByte();
                        if (nextData == 0xD8){
                            dataList.Add(byteData);
                            dataList.Add(nextData);

                            isLoadStart = true;
                        }
                    }
                }else{
                    dataList.Add(byteData);
                    if (byteData == 0xFF){
                        var nextData = binaryReader.ReadByte();
                        if (nextData == 0xD9){
                            dataList.Add(nextData);

                            // Destroy(previewTextureList[currentPreviewIndex]);
                            previewTextureList[currentPreviewIndex].LoadImage(dataList.ToArray());
                            currentPreviewIndex += 1;
                            if (previewTextureList.Count <= currentPreviewIndex)
                            {
                                currentPreviewIndex = 0;
                            }
                            dataList.Clear();
                            isLoadStart = false;
                            yield return null;
                        }else{
                            dataList.Add(nextData);
                        }
                    }
                }
            }
        }
    
    }
}
