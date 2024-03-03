using System;
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
            Quarter = 4,
            Eighth = 8
        }

        [SerializeField]
        private ResolutionPer resolutionPer = ResolutionPer.Half;
        
        private int currentPreviewIndex;
        private List<Texture2D> previewTextureList = new(PoolCount);
        private BinaryReader binaryReader;

        public Texture2D ThetaTexture => previewTextureList[currentPreviewIndex];

        void Start()
        {
            for (var i = 0; i < PoolCount; i++)
            {
                // previewTextureList.Add(new Texture2D(MaxWidth / (int)resolutionPer, MaxHeight / (int)resolutionPer));
                previewTextureList.Add(new Texture2D(128, 128));
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

        private void OnDestroy()
        {
            StopCoroutine(GetLivePreview());
            foreach (var tex in previewTextureList)
            {
                Destroy(tex);
            }
        }

        private IEnumerator GetLivePreview()
        {

            var dataArray = new byte[100000];
            var index = 0;
            var isLoadStart = false;

            while (true)
            {
                var byteData = binaryReader.ReadByte();
                if (!isLoadStart){
                    if (byteData == 0xFF){
                        var nextData = binaryReader.ReadByte();
                        if (nextData == 0xD8){
                            // dataList.Add(byteData);
                            // dataList.Add(nextData);
                            dataArray[index] = byteData;
                            index += 1;
                            dataArray[index] = nextData;
                            index += 1;

                            isLoadStart = true;
                        }
                    }
                }else{
                    // dataList.Add(byteData);
                    dataArray[index] = byteData;
                    index += 1;
                    if (byteData == 0xFF){
                        var nextData = binaryReader.ReadByte();
                        if (nextData == 0xD9){
                            // dataList.Add(nextData);
                            dataArray[index] = nextData;
                            
                            previewTextureList[currentPreviewIndex].LoadImage(dataArray);
                            currentPreviewIndex += 1;
                            if (previewTextureList.Count <= currentPreviewIndex)
                            {
                                currentPreviewIndex = 0;
                            }
                            // dataList.Clear();
                            index = 0;
                            isLoadStart = false;
                            yield return null;
                        }else{
                            // dataList.Add(nextData);
                            dataArray[index] = nextData;
                            index += 1;
                        }
                    }
                }
            }
        }
    
    }
}
