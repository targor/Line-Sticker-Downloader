using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace LineStickerDownloader.Stickers
{
    //https://isamunoheya.blogspot.com/2016/10/how-to-download-line-stickers-on-pc.html
    public static class LineStickers
    {


        // http://dl.stickershop.line.naver.jp/stickershop/v1/sticker/2768077/iphone/sticker@2x.png

        // https://sdl-stickershop.line.naver.jp/products/0/0/1/3552/iphone/animation/2768077@2x.png
        // https://sdl-stickershop.line.naver.jp/products/0/0/1/3552/iphone/2768077@2x.png

        //https://stickershop.line-scdn.net/stickershop/v1/sticker/2768077/iphone/stickers@2x.png


        // bild zu dem paket für paketübersicht -> https://stickershop.line-scdn.net/stickershop/v1/product/10306/LINEStorePC/main.png;compress=true
        // mit animation -> https://sdl-stickershop.line.naver.jp/products/0/0/1/10306/iphone/animation/27533209@2x.png
        // Ohne animation, nur der sticker -> https://stickershop.line-scdn.net/stickershop/v1/sticker/27533209/iPhone/sticker@2x.png

        // meta info https://stickershop.line-scdn.net/stickershop/v1/product/7488/android/productInfo.meta
        // old meta info http://dl.stickershop.line.naver.jp/products/0/0/1/7488/android/productInfo.meta

        public static dynamic GetStickerMeta(int id)
        {
            string text = null;
            int count = 0;
            do
            {
                text= Helper.GetUrl("https://stickershop.line-scdn.net/stickershop/v1/product/" + id + "/android/productInfo.meta");
                //text = Helper.GetUrl("http://dl.stickershop.line.naver.jp/products/0/0/1/" + id + "/android/productInfo.meta");
                if (text != null && !text.Contains("MdMN05Error") && !text.Contains("HTTP404Err"))
                {
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                    text = null;
                }
                count++;
            } while (count < 10);

            if (text != null)
            {
                return JsonConvert.DeserializeObject(text);
            }
            return null;
        }


        private static Regex regex = new Regex(@"/product/(\d.+?)/+?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Multiline | RegexOptions.Singleline);
        public static HashSet<StickerCollection> GetPageStickerCollections(int pageNumber)
        {
            HashSet<int> ids = new HashSet<int>();

            HashSet<StickerCollection> collections = new HashSet<StickerCollection>();

            string document = Helper.GetUrl("https://store.line.me/stickershop/showcase/top/de?page=" + pageNumber);

            MatchCollection matchCollection = regex.Matches(document);
            
            foreach (Match match in matchCollection)
            {
                if (match.Groups!=null && match.Groups.Count==2)
                {
                    int imageId = -1;
                    if (int.TryParse(match.Groups[1].Value, out imageId))
                    {
                        if (ids.Add(imageId))
                        {
                            StickerCollection col = new StickerCollection(imageId);
                            collections.Add(col);
                        }
                    }
                }
            }

            return collections;
        }

        /// <summary>
        /// This will check how many pages are availabe on the line sticker website
        /// </summary>
        /// <returns></returns>
        public static int GetPages()
        {
            bool found = false;
            int i = 29;
            while (!found)
            {
                string document = null;
                i += 10;
                do
                {
                    
                    document = Helper.GetUrl("https://store.line.me/stickershop/showcase/top/de?page=" + i);
                    if (document == null) { Thread.Sleep(1000); }
                } while (document == null);
                
                // at this point, there are no more pages, so we can check up to the last page
                if (document.Contains("MdMN05Error") || document.Contains("HTTP404Err"))
                {
                    i -= 10;
                    for (int a=i;a<(i+11);a++)
                    {
                        string document2 = Helper.GetUrl("https://store.line.me/stickershop/showcase/top/de?page=" + a);
                        if (document2.Contains("MdMN05Error") || document2.Contains("HTTP404Err"))
                        {
                            return (a -1);
                        }
                    }
                }
            }

            return -1;
            
           /* var parser = new HtmlParser();

            //MdMN05Error mdMN05404
            var doc = parser.ParseDocument(document);
            var element=doc.QuerySelector(".MdCMN14Pagination a[class=stoksPrice]"");
           
            */
        }    
    }
}
