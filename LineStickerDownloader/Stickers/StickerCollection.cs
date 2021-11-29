using LineStickerDownloader.Commands;
using LineStickerDownloader.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace LineStickerDownloader.Stickers
{
    public class StickerCollection: BaseViewModel
    {
        [JsonIgnore]
        FileInfo TempPath = null;
        
        [JsonIgnore]
        FileInfo TempImagePath = null;
        
        [JsonIgnore]
        public string _name { get; set; }
        public string Name
        {
            get 
            {
                return _name; 
            }
            set
            {
                InvokePropertyChanged();
                _name = value;
            }
        }

        [JsonIgnore]
        private bool _hasAnimation=false;
        public bool HasAnimation
        {
            get
            {
                return _hasAnimation;
            }
            set
            {
                InvokePropertyChanged();
                _hasAnimation = value;
            }
        }

        [JsonIgnore]
        private int _packageId;
        public int PackageId
        {
            get
            {
                return _packageId;
            }
            set
            {
                _packageId = value;
            }
        }

        [JsonIgnore]
        public double OpenFolderOpacity
        {
            get
            {
                if (CanOpenCollectionFolder())
                {
                    return 1.0;
                }
                return 0.3;
            }
            private set { }
        }
        

        public string ImageLink { get; set; }


        [JsonIgnore]
        public ICommand DownloadCollectionCommand { get; set; }
        [JsonIgnore]
        public ICommand OpenCollectionFolderCommand { get; set; }

        public List<Sticker> StickerList = new List<Sticker>();

        [JsonIgnore]
        private BitmapImage _image = null;

        [JsonIgnore]
        public BitmapImage Image
        {
            get
            {
                return _image;
            }
            set
            {
                InvokePropertyChanged();
                _image = value;
            }
        }

        public bool LoadCollectionFromCache()
        {
            if (TempPath.Exists)
            {
                StickerCollection sc= JsonConvert.DeserializeObject<StickerCollection>(File.ReadAllText(this.TempPath.FullName));
                if (sc!=null)
                {
                    this.Name = sc.Name;
                    this.HasAnimation = sc.HasAnimation;
                    this.ImageLink = sc.ImageLink;
                    this.GotMeta = sc.GotMeta;
                    this.StickerList = sc.StickerList;
                    if (this.TempImagePath.Exists)
                    {
                        _image = new BitmapImage(new Uri(this.TempImagePath.FullName));
                        _image.Freeze();
                    }
                }
                else
                {
                    TempPath.Delete();
                }
            }
            
            return false;
        }
        
        public void SaveToCache()
        {
            File.WriteAllText(this.TempPath.FullName, JsonConvert.SerializeObject(this));
            if (_image!=null && !TempImagePath.Exists)
            {
                Helper.SaveBitmapImage(_image, TempImagePath.FullName);
            }
        }


        public StickerCollection() { } 
        public StickerCollection(int packageId)
        {

            this.PackageId = packageId;

           

            this.TempPath = new FileInfo(Path.Combine(MainViewModel.Settings.CachePathFileInfo.FullName, this.PackageId + ".json"));
            this.TempImagePath = new FileInfo(Path.Combine(MainViewModel.Settings.CachePathFileInfo.FullName, this.PackageId + ".png"));
            this.ImageLink = "https://stickershop.line-scdn.net/stickershop/v1/product/" + packageId + "/LINEStorePC/main.png;compress=true";
       

            LoadCollectionFromCache();
            this.DownloadCollectionCommand = new RelayCommand((o)=> { DownloadCollection(o,false, CancellationToken.None); });

            OpenCollectionFolderCommand = new RelayCommand((o) => {
                DirectoryInfo CollectionPath = GetCollectionPath();
                if (CollectionPath != null)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "explorer";
                    p.StartInfo.Arguments = "\"" + CollectionPath.FullName + "\"";
                    p.Start();
                }
            }, 
            
            (o) => {
                return CanOpenCollectionFolder();
            });
        }


        public DirectoryInfo GetCollectionPath()
        {
            if (this.Name!=null && this.Name !="")
            {
                return new DirectoryInfo(Path.Combine(MainViewModel.Settings.StickerPathFileInfo.FullName, string.Join("_", this.Name.Split(Path.GetInvalidFileNameChars()))));
            }
            return null;
        }



        private bool CanOpenCollectionFolder()
        {
            if (Name != null)
            {
                DirectoryInfo di = GetCollectionPath();
                if (di!=null && di.Exists)
                {
                    return true;
                }
            }
            return false;
        }


        public void LoadImage()
        {
            if (Image == null)
            {
                this.Image = Helper.DownloadImageAsBitmapImage(this.ImageLink);
                this.Image.Freeze();
                SaveToCache();
            }
        }

        [JsonIgnore]
        public bool GotMeta
        {
            get
            {
                if (
                    !String.IsNullOrEmpty(this.Name) && !String.IsNullOrWhiteSpace(this.Name) &&
                    this.StickerList!=null && StickerList.Count>0
                   )
                {
                    return true;
                }
                return false;
            }
            private set { }
        }

        public bool GetMeta(bool force=false)
        {
            if (!force && GotMeta) { return true; }
            dynamic result = LineStickers.GetStickerMeta(this.PackageId);
            if (result != null)
            {
                this.Name = result.title.en;

                if (result.hasAnimation != null)
                {
                    this.HasAnimation = result.hasAnimation;
                }
                else
                {
                    this.HasAnimation = false;
                }

                foreach (dynamic sx in result.stickers)
                {
                    int id = ParseJsonInt(sx.id.ToString());

                    Sticker s = new Sticker(id, this.PackageId, this.HasAnimation);

                    if (sx.height != null)
                    {
                        s.Height = ParseJsonInt(sx.height.ToString());
                    }
                    if (sx.width!=null)
                    {
                        s.Width = ParseJsonInt(sx.width.ToString());
                    }

                    this.StickerList.Add(s);
                }
                SaveToCache();
                return true;
            }
            return false;
        }

        public void DownloadCollection(object o, bool UpdateTextOnly, CancellationToken cancelToken, bool synchron = false)
        {
            if (!UpdateTextOnly)
            {
                BaseViewMessageBox.OverrideValues(new WPFMessageBox("Retrieving Collection", "Please wait, the Collection is being downloaded.", "OK", true, true, true));
            }

            Task t = Task.Run(() =>
             {
                 if (GetMeta())
                 {
                     if (cancelToken.IsCancellationRequested) { return; }
                     BaseViewMessageBox.MessageBoxText += "\r\n Getting Data for collection: " + Name;
                     DirectoryInfo CollectionPath = GetCollectionPath();
                     if (!CollectionPath.Exists)
                     {
                         BaseViewMessageBox.MessageBoxText += "\r\n Creating Collection directory.";
                         CollectionPath.Create();
                     }


                     BaseViewMessageBox.MessageBoxText += "\r\n Downloading Stickers:";
                     BaseViewMessageBox.MessageBoxText += "\r\n Downloaded Sticker {0} from {1} {2}";
                     string txt = BaseViewMessageBox.MessageBoxText;
                     if (StickerList == null || StickerList.Count == 0) { GetMeta(true); }
                     int count = 0;


                    // save collection image
                    if (MainViewModel.Settings.SaveMainImage)
                     {
                         FileInfo mainImage = new FileInfo(CollectionPath.FullName + "\\000AAA_" + this.PackageId + "_main.png");
                         Helper.SaveBitmapImage(Image, mainImage.FullName);
                     }

                     foreach (Sticker s in this.StickerList)
                     {
                         if (cancelToken.IsCancellationRequested) { return; }
                         count++;
                         BaseViewMessageBox.MessageBoxText = String.Format(txt, count, this.StickerList.Count, "");

                         if (CollectionPath != null)
                         {
                             FileInfo fi = new FileInfo(Path.Combine(CollectionPath.FullName, this.PackageId + "_" + s.Id + ".png"));

                             s.DownloadSticker(fi);

                             if (HasAnimation && MainViewModel.Settings.ConvertAPNG)
                             {
                                 BaseViewMessageBox.MessageBoxText = String.Format(txt, count, this.StickerList.Count, "Converting APNG animation to Gif");

                                 FileInfo dest = new FileInfo(Path.Combine(CollectionPath.FullName, this.PackageId + "_" + s.Id + ".gif"));
                                 Helper.ApngToGif(fi, dest, MainViewModel.Settings.GifLoopCount);
                             }
                         }
                     }
                 }
             }).ContinueWith((a) =>
             {
                 if (!UpdateTextOnly)
                 {
                     BaseViewMessageBox.Hide();
                 }
                 InvokePropertyChanged("OpenFolderOpacity");
             });

            if (synchron)
            {
                t.Wait();
            }


        }

        
        public int ParseJsonInt(string val)
        {
            val = val.Replace("{", String.Empty);
            val = val.Replace("}", String.Empty);
            return int.Parse(val);
        }


    }
}
