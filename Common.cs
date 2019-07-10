using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace XiguaDanmakuHelper
{
    public class Common
    {
        public static string HttpGet(string url)
        {
            HttpWebRequest myRequest = null;
            HttpWebResponse myHttpResponse = null;
            myRequest = (HttpWebRequest)WebRequest.Create(url);
            myRequest.Method = "GET";
            myHttpResponse = (HttpWebResponse)myRequest.GetResponse();
            var reader = new StreamReader(myHttpResponse.GetResponseStream());
            var json = reader.ReadToEnd();
            reader.Close();
            myHttpResponse.Close();
            return json;
        }

        public static string HttpPost(string url, string data)
        {
            var myRequest = (HttpWebRequest)WebRequest.Create(url);
            byte[] ba = Encoding.Default.GetBytes(data);
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            myRequest.ContentLength = ba.Length;
            var pStream = myRequest.GetRequestStream();
            pStream.Write(ba, 0, ba.Length);
            pStream.Close();

            var myHttpResponse = (HttpWebResponse)myRequest.GetResponse();
            var reader = new StreamReader(myHttpResponse.GetResponseStream());
            var json = reader.ReadToEnd();
            reader.Close();
            myHttpResponse.Close();
            return json;
        }

        public static async Task<string> HttpGetAsync(string url)
        {
            var request = WebRequest.Create(url);
            request.Method = "GET";
            var response = request.GetResponse();

            using (var stream = response.GetResponseStream())
            using (var sr = new StreamReader(stream))
            {
                var json = await sr.ReadToEndAsync();
                return json;
            }
        }

        public static async Task<string> HttpPostAsync(string url, string data)
        {
            var request = WebRequest.Create(url);
            byte[] ba = Encoding.Default.GetBytes(data);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.ContentLength = ba.Length;
            var pStream = request.GetRequestStream();
            await pStream.WriteAsync(ba, 0, ba.Length);
            pStream.Close();
            var response = request.GetResponse();

            using (var stream = response.GetResponseStream())
            using (var sr = new StreamReader(stream))
            {
                var json = await sr.ReadToEndAsync();
                return json;
            }
        }
        /// <summary>
        /// Http方式下载文件
        /// </summary>
        /// <param name="url">http地址</param>
        /// <param name="localfile">本地文件</param>
        /// <returns></returns>
        public static bool Download(string url, string localfile)
        {
            bool flag = false;
            long startPosition = 0; // 上次下载的文件起始位置
            FileStream writeStream; // 写入本地文件流对象

            long remoteFileLength = GetHttpLength(url);// 取得远程文件长度
            System.Console.WriteLine("remoteFileLength=" + remoteFileLength);
            if (remoteFileLength == 745)
            {
                System.Console.WriteLine("远程文件不存在.");
                return false;
            }

            // 判断要下载的文件夹是否存在
            if (File.Exists(localfile))
            {

                writeStream = File.OpenWrite(localfile);             // 存在则打开要下载的文件
                startPosition = writeStream.Length;                  // 获取已经下载的长度

                if (startPosition >= remoteFileLength)
                {
                    System.Console.WriteLine("本地文件长度" + startPosition + "已经大于等于远程文件长度" + remoteFileLength);
                    writeStream.Close();

                    return false;
                }
                else
                {
                    writeStream.Seek(startPosition, SeekOrigin.Current); // 本地文件写入位置定位
                }
            }
            else
            {
                writeStream = new FileStream(localfile, FileMode.Create);// 文件不保存创建一个文件
                startPosition = 0;
            }


            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接

                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);// 设置Range值,与上面的writeStream.Seek用意相同,是为了定义远程文件读取位置
                }


                Stream readStream = myRequest.GetResponse().GetResponseStream();// 向服务器请求,获得服务器的回应数据流


                byte[] btArray = new byte[512];// 定义一个字节数据,用来向readStream读取内容和向writeStream写入内容
                int contentSize = readStream.Read(btArray, 0, btArray.Length);// 向远程文件读第一次

                long currPostion = startPosition;

                while (contentSize > 0)// 如果读取长度大于零则继续读
                {
                    currPostion += contentSize;
                    int percent = (int)(currPostion * 100 / remoteFileLength);
                    System.Console.WriteLine("percent=" + percent + "%");

                    writeStream.Write(btArray, 0, contentSize);// 写入本地文件
                    contentSize = readStream.Read(btArray, 0, btArray.Length);// 继续向远程文件读取
                }

                //关闭流
                writeStream.Close();
                readStream.Close();

                flag = true;        //返回true下载成功
            }
            catch (Exception)
            {
                writeStream.Close();
                flag = false;       //返回false下载失败
            }

            return flag;
        }

        // 从文件头得到远程文件的长度
        private static long GetHttpLength(string url)
        {
            long length = 0;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);// 打开网络连接
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                if (rsp.StatusCode == HttpStatusCode.OK)
                {
                    length = rsp.ContentLength;// 从文件头得到远程文件的长度
                }

                rsp.Close();
                return length;
            }
            catch (Exception)
            {
                return length;
            }

        }
    }
}