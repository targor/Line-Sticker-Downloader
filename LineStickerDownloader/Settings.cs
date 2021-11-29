using LineStickerDownloader.Commands;
using LineStickerDownloader.Models;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace LineStickerDownloader
{
    public class Settings:BaseViewModel
    {
        [JsonIgnore]
        private bool _convertAPNG = true;
        public bool ConvertAPNG
        {
            get { return _convertAPNG; }
            set 
            {
                _convertAPNG = value;
                InvokePropertyChanged();
                SaveSettings();
            }
        }

        [JsonIgnore]
        private bool _saveMainImage = true;
        public bool SaveMainImage
        {
            get { return _saveMainImage; }
            set 
            {
                _saveMainImage = value;
                InvokePropertyChanged();
                SaveSettings();
            }
        }

        [JsonIgnore]
        private int _gifLoopCount = 10;
        public int GifLoopCount
        {
            get { return _gifLoopCount; }
            set 
            {
                _gifLoopCount = value;
                InvokePropertyChanged();
                SaveSettings();
            }
        }

        [JsonIgnore]
        private int _settingsBlur = 1;
        [JsonIgnore]
        public int SettingsBlur
        {
            get { return _settingsBlur; }
            set
            {
                _settingsBlur = value;
                MainViewModel.SetMultiBlur(value);
                InvokePropertyChanged();
            }
        }
        

        public string StickerPath { get; set; } = Path.Combine(Helper.GetCurrentPath().FullName, "Sticker");
        public string CachePath { get; set; } = Path.Combine(Helper.GetCurrentPath().FullName, "Cache");


        [JsonIgnore]
        public FileInfo StickerPathFileInfo
        {
            get
            {
                DirectoryInfo di = new DirectoryInfo(StickerPath);
                if (!di.Exists) { di.Create(); }

                return new FileInfo(StickerPath);
            }
            set
            {
                StickerPath = value.FullName;
            }
        }

        [JsonIgnore]
        public FileInfo CachePathFileInfo
        {
            get
            {
                DirectoryInfo di = new DirectoryInfo(CachePath);
                if (!di.Exists) { di.Create(); }

                return new FileInfo(CachePath);
            }
            set
            {
                CachePath = value.FullName;
            }
        }


        [JsonIgnore]
        private ICommand _hideSettingsCommand;
        [JsonIgnore]
        public ICommand HideSettingsCommand
        {
            get { return _hideSettingsCommand;}
            set 
            {
                _hideSettingsCommand = value;
                InvokePropertyChanged();
            }
        }

        [JsonIgnore]
        private Visibility _settingsVisibility = Visibility.Collapsed;
        [JsonIgnore]
        public Visibility SettingsVisibility
        {
            get
            {
                return _settingsVisibility;
            }
            set
            {
                if (value==Visibility.Visible)
                {
                    SettingsBlur = 5;
                }
                else
                {
                    SettingsBlur = 0;
                }
                _settingsVisibility = value;
                InvokePropertyChanged();
            }
        }


        [JsonIgnore]
        private FileInfo SettingsPath = new FileInfo(Path.Combine(Helper.GetCurrentPath().FullName, "settings.json"));


        public Settings()
        {
            HideSettingsCommand = new RelayCommand((o) => {
                SettingsVisibility = Visibility.Collapsed;
            });
        }

        public void LoadSettings()
        {
            if (SettingsPath.Exists)
            {
                Settings s = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(SettingsPath.FullName));
                this.ConvertAPNG = s.ConvertAPNG;
                this.GifLoopCount = s.GifLoopCount;
                this.SaveMainImage = s.SaveMainImage;
            }
            else
            {
                SaveSettings();
            }
        }

        public void SaveSettings()
        {
            File.WriteAllText(SettingsPath.FullName, JsonConvert.SerializeObject(this));
        }


    }
}
