using BowlFrame.Target;
using BowlFrame.Tools;
using Newtonsoft.Json.Linq;
using System.Buffers.Text;
using System.Net.Http.Json;
using static BowlFrame.Tools.Tools;

namespace BowlFrame.Adapter.TencentQQAdapter.Tools
{
    internal static class UploadMedia
    {
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="tencentQQ"></param>
        /// <param name="bytes"></param>
        /// <param name="fileType">媒体类型：1 图片，2 视频，3 语音，4 文件（暂不开放）图片：png/jpg，视频：mp4，语音：silk</param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static async Task<Files?> GetFiles(TencentQQ tencentQQ, byte[] bytes, short fileType, ITarget target)
        {
            //S3Helper s3 = new();

            //string filename = GetMD5Hex(bytes);

            //string? url = await s3.GetTempLink(filename);

            //if (url is null || url == "") {
            //    if (await s3.UploadFile(filename, bytes) != true)
            //        return null;
            //    url = await s3.GetTempLink(filename);
            //}

            HttpRequestMessage request = new(HttpMethod.Post, new Uri(tencentQQ.BaseUrl, target is Group ? $"/v2/groups/{target.ID}/files" : target is User ? $"/v2/users/{target.ID}/files" : throw new NotSupportedException()))
            {
                Content = JsonContent.Create(new
                {
                    file_type = fileType,
                    file_data = Convert.ToBase64String(bytes),
                    srv_send_msg = false,
                })
            };

            HttpResponseMessage? responseMessage = await tencentQQ.Send(request);
            if (responseMessage is null || !responseMessage.IsSuccessStatusCode)
                return null;

            Files files = JObject.Parse(await responseMessage.Content.ReadAsStringAsync()).ToObject<Files>();

            files.TTLTime = files.TTL == 0 ? 0 : new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds() + files.TTL - 10;

            return files;
        }
    }
}