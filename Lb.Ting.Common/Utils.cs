using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;

namespace Lb.Ting.Common
{
    public class Utils
    {
        public static void WriteFile(String filename, String content)
        {
            IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            //如果是复写文件,那么在写之前最好把文件删掉,不然如果这次写入的文件大小,小于文件本身的大小,那么之前文件的数据还是存在的,在读取的时候就会出问题.
            if (isolatedStorage.FileExists(filename) == true)
            {
                isolatedStorage.DeleteFile(filename);
            }
            using (IsolatedStorageFileStream stream = isolatedStorage.OpenFile(filename, FileMode.CreateNew))
            {
                using (var str = new StreamWriter(stream))
                {
                    str.Write(content);
                }
            }
        }

        public static String ReadFile(String filename)
        {
            IsolatedStorageFile isolatedStorage = IsolatedStorageFile.GetUserStoreForApplication();
            //isolatedStorage.GetLastWriteTime
            if (isolatedStorage.FileExists(filename) == true)
            {
                //打开文件
                IsolatedStorageFileStream stream = isolatedStorage.OpenFile(filename, FileMode.Open);
                try
                {
                    using (var sr = new StreamReader(stream))
                    {
                        //读取文件
                        string result = sr.ReadToEnd();
                        result = result.Trim();
                        sr.Close();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                finally
                {
                    stream.Close();
                }
            }
            return "";
        }

        public static void WebClientAsync(WebClient client, string url)
        {
            if (client.IsBusy)
                client.CancelAsync();
            client.DownloadStringAsync(new Uri(url, UriKind.Absolute));
        }

        public static void SetImageUrl(Image img, Uri uri, Uri defaultUri)
        {
            WebClient wc = new WebClient();
            wc.Headers["Referer"] = "http://www.baidu.com";
            BitmapImage bi = new BitmapImage();
            img.Source = bi;
            wc.OpenReadCompleted += (s, e) =>
            {
                try
                {
                    bi.SetSource(e.Result);
                }
                catch (Exception)
                {
                    //bi.UriSource = defaultUri;
                }
            };
            wc.OpenReadAsync(uri);
        }

        public static void SetImageUrl(ImageBrush img, Uri uri, Uri defaultUri)
        {
            WebClient wc = new WebClient();
            wc.Headers["Referer"] = "http://www.baidu.com";
            BitmapImage bi = new BitmapImage();
            img.ImageSource = bi;
            wc.OpenReadCompleted += (s, e) =>
            {
                try
                {
                    bi.SetSource(e.Result);
                }
                catch (Exception)
                {
                    //bi.UriSource = defaultUri;
                }
            };
            wc.OpenReadAsync(uri);
        }


    }
}
