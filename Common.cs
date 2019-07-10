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
        /// Http��ʽ�����ļ�
        /// </summary>
        /// <param name="url">http��ַ</param>
        /// <param name="localfile">�����ļ�</param>
        /// <returns></returns>
        public static bool Download(string url, string localfile)
        {
            bool flag = false;
            long startPosition = 0; // �ϴ����ص��ļ���ʼλ��
            FileStream writeStream; // д�뱾���ļ�������

            long remoteFileLength = GetHttpLength(url);// ȡ��Զ���ļ�����
            System.Console.WriteLine("remoteFileLength=" + remoteFileLength);
            if (remoteFileLength == 745)
            {
                System.Console.WriteLine("Զ���ļ�������.");
                return false;
            }

            // �ж�Ҫ���ص��ļ����Ƿ����
            if (File.Exists(localfile))
            {

                writeStream = File.OpenWrite(localfile);             // �������Ҫ���ص��ļ�
                startPosition = writeStream.Length;                  // ��ȡ�Ѿ����صĳ���

                if (startPosition >= remoteFileLength)
                {
                    System.Console.WriteLine("�����ļ�����" + startPosition + "�Ѿ����ڵ���Զ���ļ�����" + remoteFileLength);
                    writeStream.Close();

                    return false;
                }
                else
                {
                    writeStream.Seek(startPosition, SeekOrigin.Current); // �����ļ�д��λ�ö�λ
                }
            }
            else
            {
                writeStream = new FileStream(localfile, FileMode.Create);// �ļ������洴��һ���ļ�
                startPosition = 0;
            }


            try
            {
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(url);// ����������

                if (startPosition > 0)
                {
                    myRequest.AddRange((int)startPosition);// ����Rangeֵ,�������writeStream.Seek������ͬ,��Ϊ�˶���Զ���ļ���ȡλ��
                }


                Stream readStream = myRequest.GetResponse().GetResponseStream();// �����������,��÷������Ļ�Ӧ������


                byte[] btArray = new byte[512];// ����һ���ֽ�����,������readStream��ȡ���ݺ���writeStreamд������
                int contentSize = readStream.Read(btArray, 0, btArray.Length);// ��Զ���ļ�����һ��

                long currPostion = startPosition;

                while (contentSize > 0)// �����ȡ���ȴ������������
                {
                    currPostion += contentSize;
                    int percent = (int)(currPostion * 100 / remoteFileLength);
                    System.Console.WriteLine("percent=" + percent + "%");

                    writeStream.Write(btArray, 0, contentSize);// д�뱾���ļ�
                    contentSize = readStream.Read(btArray, 0, btArray.Length);// ������Զ���ļ���ȡ
                }

                //�ر���
                writeStream.Close();
                readStream.Close();

                flag = true;        //����true���سɹ�
            }
            catch (Exception)
            {
                writeStream.Close();
                flag = false;       //����false����ʧ��
            }

            return flag;
        }

        // ���ļ�ͷ�õ�Զ���ļ��ĳ���
        private static long GetHttpLength(string url)
        {
            long length = 0;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);// ����������
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                if (rsp.StatusCode == HttpStatusCode.OK)
                {
                    length = rsp.ContentLength;// ���ļ�ͷ�õ�Զ���ļ��ĳ���
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