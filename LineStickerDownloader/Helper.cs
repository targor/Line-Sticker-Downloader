using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace LineStickerDownloader
{
    public static class Helper
    {

        [DllImport(@"Lib\apng2GifManaged.dll", EntryPoint = "ConvertFile", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int ConvertFile([MarshalAs(UnmanagedType.LPStr)] string path, int numLoops);
        public static void ApngToGif(FileInfo inFile, FileInfo outFile,int numOfLoops=10)
        {
            int result = ConvertFile(inFile.FullName, numOfLoops);
            if (result == 0)
            {
                FileInfo newFile = new FileInfo(inFile.FullName.Replace(inFile.Extension, ".gif"));
                inFile.Delete();
                if (!newFile.FullName.Equals(outFile.FullName))
                {
                    newFile.MoveTo(outFile.FullName);
                }
            }
            else
            {
                throw new Exception("something went wrong with converting apng to gif with file:"+inFile.FullName);
            }


        }

        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private static async Task<string> GetUrlIntern(string url)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    return await httpClient.GetStringAsync(url);
                }
            }
            catch (HttpRequestException exc)
            {
                if (exc.Message.Contains("404"))
                {
                    return "HTTP404Err";
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public static void SaveBitmapImage(BitmapImage image, string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public static string GetUrl(string url)
        {
            return Task.Run<dynamic>(async () => await GetUrlIntern(url)).Result;
        }

        public static async Task<bool> DownloadFileAsync(string url,FileInfo destination)
        {
            var httpClient = new HttpClient();

            // Download the image and write to the file
            var imageBytes = await httpClient.GetByteArrayAsync(url);
            File.WriteAllBytes(destination.FullName, imageBytes);
            return true;
        }

        public static bool DownloadFile(string url, FileInfo destination)
        {
            return Task.Run<bool>(async () => await DownloadFileAsync(url, destination)).Result;
        }


        public static async Task<Bitmap> DownloadImageAsBitmapAsync(string url)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);
            return new System.Drawing.Bitmap(response.Content.ReadAsStreamAsync().Result);
        }

        public static async Task<BitmapImage> DownloadImageAsyncAsBitmapImage(string url)
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpResponseMessage response = await client.GetAsync(url);

                Bitmap b = new Bitmap(response.Content.ReadAsStreamAsync().Result);
                return ToBitmapImage(b);
            }catch(Exception)
            {
                return null;
            }
        }

        public static BitmapImage DownloadImageAsBitmapImage(string url)
        {
            return Task.Run<BitmapImage>(async () => await DownloadImageAsyncAsBitmapImage(url)).Result;
        }

        
        public static Bitmap DownloadImageAsBitmap(string url)
        {
            return Task.Run<Bitmap>(async () => await DownloadImageAsBitmapAsync(url)).Result;
        }


        private static Bitmap CropImage(Bitmap img, System.Drawing.Rectangle cropArea)
        {
            return img.Clone(cropArea, img.PixelFormat);
        }

     /*   public static Bitmap ToBitmap(IMagickImage mimg, MagickFormat fmt = MagickFormat.Png24)
        {
            Bitmap bmp = null;
            using (MemoryStream ms = new MemoryStream())
            {
                mimg.Write(ms, fmt);
                ms.Position = 0;
                bmp = (Bitmap)Bitmap.FromStream(ms);
            }
            return bmp;
        }

        public static IMagickImage ToMagickImage(Bitmap bmp)
        {
            IMagickImage img = null;
            MagickFactory f = new MagickFactory();
            using (MemoryStream ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Bmp);
                ms.Position = 0;
                img = new MagickImage(f.Image.Create(ms));
            }
            return img;
        }*/


        public static Bitmap ByteToImage(byte[] blob)
        {
            using (MemoryStream mStream = new MemoryStream())
            {
                mStream.Write(blob, 0, blob.Length);
                mStream.Seek(0, SeekOrigin.Begin);

                Bitmap bm = new Bitmap(mStream);
                return bm;
            }
        }

        private static DirectoryInfo programPath = null;
        public static DirectoryInfo GetCurrentPath()
        {
            if (programPath != null) { return programPath; }

            FileInfo fi = new FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            DirectoryInfo di= fi.Directory;
            programPath = di;
            return di;
        }

    }
}
