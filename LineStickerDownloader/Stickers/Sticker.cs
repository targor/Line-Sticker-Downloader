using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LineStickerDownloader.Stickers
{
    public class Sticker
    {
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;
        public int Id { get; set; }
        public int PackageId { get; set; }
        public bool HasAnimation { get; set; } = false;

        public Sticker() { }
        public Sticker(int strickerId,int packageId,bool hasAnimation)
        {
            this.HasAnimation = hasAnimation;
            this.Id = strickerId;
            this.PackageId = packageId; 
        }


        // mit animation -> https://sdl-stickershop.line.naver.jp/products/0/0/1/10306/iphone/animation/27533209@2x.png
        // Ohne animation, nur der sticker -> https://stickershop.line-scdn.net/stickershop/v1/sticker/27533209/iPhone/sticker@2x.png

        //data-preview='{ "type" : "static", "id" : "24306",
        //"staticUrl" :         "https://stickershop.line-scdn.net/stickershop/v1/sticker/24306/android/sticker.png",
        //"fallbackStaticUrl" : "https://stickershop.line-scdn.net/stickershop/v1/sticker/24306/android/sticker.png", "animationUrl" : "", "popupUrl" : "", "soundUrl" : "" }'

        public void DownloadSticker(FileInfo savePath)
        {
            if (HasAnimation)
            {
                if (!Helper.DownloadFile("https://sdl-stickershop.line.naver.jp/products/0/0/1/"+this.PackageId+"/iphone/animation/"+this.Id+"@2x.png",savePath))
                {

                }
            }
            else
            {
                Helper.DownloadFile("https://stickershop.line-scdn.net/stickershop/v1/sticker/"+this.Id+"/iPhone/sticker@2x.png", savePath);
            }
        }
    }
}
