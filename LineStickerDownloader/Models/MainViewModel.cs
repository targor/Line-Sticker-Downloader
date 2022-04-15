using LineStickerDownloader.Commands;
using LineStickerDownloader.Stickers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace LineStickerDownloader.Models
{
    internal class MainViewModel:BaseViewModel
    {
        public static MainViewModel Instance;

        public ICommand CloseWindow { get; set; }
        public ICommand MinimizeWindow { get; set; }
        public ICommand SelectSavePathCommand { get; set; }
        public ICommand ShowSettingsCommand { get; set; }
        public ICommand DownloadAllCollectionsCommand { get; set; }
        public ICommand DowenloadCollectionNumber { get; set; }

        private WindowState _curWindowState = WindowState.Normal;
        public WindowState CurWindowState
        {
            get { return _curWindowState; }
            set { _curWindowState = value; InvokePropertyChanged(); }
        }


        private int _multiBlur = 0;
        public int MultiBlur 
        {
            get
            {
                return _multiBlur;
            }
            set
            {
                _multiBlur = value;
                InvokePropertyChanged();
            }
        }

        private Visibility _multiVisibility = Visibility.Collapsed;
        public Visibility MultiVisibility
        {
            get { return _multiVisibility; }
            set 
            { 
                _multiVisibility = value; 
                InvokePropertyChanged(); 
            }  
        }

        public static void SetMultiBlur(int blur)
        {
            App.Current.Dispatcher.Invoke(() => { 
                Instance.MultiBlur = blur;
                if (blur>0)
                {
                    Instance.MultiVisibility = Visibility.Visible;
                }
                else
                {
                    Instance.MultiVisibility = Visibility.Collapsed;
                }
            });
        }

        public static Settings Settings { get; set; } = new Settings();


        private WPFMessageBox _messageBox = null;
        public WPFMessageBox MessageBox
        {
            get
            {
                return _messageBox;
            }
            private set { }
        }


        #region pagination


        private ObservableCollection<StickerCollection> _pageImageList = new ObservableCollection<StickerCollection>();
        public ObservableCollection<StickerCollection> PageImageList 
        {
            get
            {
                return _pageImageList;
            }
            set 
            {
                InvokePropertyChanged();
                _pageImageList = value; 
            }
        }

        int currentPage = 0;
        int showPages = 10;
        private List<Page> internPages = new List<Page>();
        private ObservableCollection<Page> _pages= new ObservableCollection<Page>();
        public ObservableCollection<Page> Pages
        {
            get { return _pages; }
            set 
            {
                InvokePropertyChanged();
                _pages = value; 
            }
        }

        private bool _PaginationEnabled=true;

        public bool PaginationEnabled
        {
            get { return _PaginationEnabled; }
            set 
            {
                if (value == true) 
                {
                    PaginationEnabledOpacity = 0.4; 
                } else 
                {
                    PaginationEnabledOpacity = 1.0; 
                }
                _PaginationEnabled = value; 
                InvokePropertyChanged();
            }
        }

        private double _paginationEnabledOpacity = 1;
        public double PaginationEnabledOpacity
        {
            get
            {
                return _paginationEnabledOpacity;
            }
            set
            {
                InvokePropertyChanged();
                _paginationEnabledOpacity = value;
            }
        }

        public int GetPageNumber(Page p)
        {
            if (p.Value.ToLower().Equals("first"))
            {
                return 1;
            }
            else if (p.Value.ToLower().Equals("last"))
            {
                return internPages.Count;
            }
            else
            {
                return int.Parse(p.Value);
            }
        }

        private void CallPage(object p)
        {
            Page page = (Page)p;
            currentPage = GetPageNumber(page);
            GeneratePageButtons(false);

            // load page images
            PaginationEnabled = false;
            PageImageList.Clear();

            Thread t = new Thread(() =>
            {
                HashSet<StickerCollection> collection = LineStickers.GetPageStickerCollections(currentPage);

                foreach (StickerCollection c in collection)
                {
                    c.BaseViewMessageBox = this.MessageBox;
                    c.LoadImage();
                    c.GetMeta();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        PageImageList.Add(c);
                    });
                }
             
                App.Current.Dispatcher.Invoke(() =>
                {
                    PaginationEnabled = true;
                });
                
            });
            t.IsBackground = true;
            t.Start();
        }

        private void GeneratePageButtons(bool initial = false)
        {

            int from = currentPage - (showPages/2);
            if (from < 0) { from = 0; }
            
            if (initial) { from = 0; }


            int to= currentPage + (showPages/2);
            if (to - from < showPages) { to += showPages - (to - from); }

            if (to > internPages.Count) { to = internPages.Count; }

            if (initial)
            {
                if (internPages.Count >= showPages)
                { 
                    to = showPages;
                }
                else
                {
                    to = internPages.Count;
                }
            }


            Pages.Clear();


            Pages.Add(new Page("First", new RelayCommand(CallPage)));
            for (int i=from;i<to;i++)
            {
                if (internPages[i].Value.Equals(currentPage.ToString()))
                {
                    internPages[i].IsSelected = true;
                }
                else
                {
                    internPages[i].IsSelected = false;
                }
                Pages.Add(internPages[i]);
            }
            Pages.Add(new Page("Last", new RelayCommand(CallPage)));
        }
        #endregion

        private string _collectionNumberTxt = "Put ID here...";
        public string CollectionNumberTxt
        {
            get
            {
                return _collectionNumberTxt;
            }
            set
            {
                InvokePropertyChanged();
                _collectionNumberTxt = value;
            }
        }


        public MainViewModel()
        {

            CloseWindow = new RelayCommand((o) => {
                System.Environment.Exit(1);
            });
            MinimizeWindow = new RelayCommand((o) => {
                CurWindowState = WindowState.Minimized;
            });


            Instance = this;
            Settings.LoadSettings();

            _messageBox = new WPFMessageBox("", "", "");

            DownloadAllCollectionsCommand = new RelayCommand(DownloadCollections,(o)=> { return PaginationEnabled; });
            DowenloadCollectionNumber = new RelayCommand(DownloadSpecificCollection, (o) => { return PaginationEnabled; });
            ShowSettingsCommand= new RelayCommand((o) => { Settings.SettingsVisibility = Visibility.Visible; });
            SelectSavePathCommand= new RelayCommand((o) => 
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = Settings.StickerPath;
                DialogResult dr= fbd.ShowDialog();
                if (dr==DialogResult.OK)
                {
                    Settings.StickerPath = Path.Combine(fbd.SelectedPath,"Sticker");
                }
            });

            MessageBox.OverrideValues(new WPFMessageBox("Retrieving Pagenumbers from Line site", "Please wait, while the page numbers will be received.", "OK", true, true,true));

            Task.Run(() =>
            {
                int pages = 30;// LineStickers.GetPages()+1;
                for (int i = 1; i < pages; i++)
                {
                    Page p = new Page(i.ToString(), new RelayCommand(CallPage));
                    internPages.Add(p);
                }
            }).ContinueWith((o) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    GeneratePageButtons(true);
                    MessageBox.Hide();
                    if (internPages != null && internPages.Count > 0)
                    {
                        CallPage(internPages[0]);
                    }
                });
            });


     //       Helper.DownloadFile("https://sdl-stickershop.line.naver.jp/products/0/0/1/24603/iphone/animation/443645311@2x.png", new System.IO.FileInfo("e:\\imgtest.png"));
            //Helper.ApngToGif(new System.IO.FileInfo("e:\\img.png"), new System.IO.FileInfo("e:\\imgtest.gif"));
            
            int mk = 0;
          /*  StickerCollection cl = new StickerCollection(24603);
            PageImageList.Add(cl);
            cl.LoadImage();
            cl.GetMeta();*/
        }

        private void DownloadSpecificCollection(object o)
        {
            int collectionNumber = -1;
            if (CollectionNumberTxt!=null && CollectionNumberTxt!="")
            {
                if (int.TryParse(CollectionNumberTxt,out collectionNumber))
                {
                    CancellationTokenSource cancelToken = new CancellationTokenSource();
                    StickerCollection c = new StickerCollection(collectionNumber);
                    c.BaseViewMessageBox = this.MessageBox;
                    c.DownloadCollection(null, false, CancellationToken.None, false);
                }
            }
        }
        private void DownloadCollections(object o)
        {
            PaginationEnabled = false;
            CancellationTokenSource cancelToken = new CancellationTokenSource();

          
            string txt = "Please wait, the Collections are being downloaded.";
            MessageBox.OverrideValues(new WPFMessageBox("Retrieving all collections", string.Copy(txt), "Cancel", false, true, true,
              new RelayCommand((o1) =>
              {
                  cancelToken.Cancel();
                  PaginationEnabled = false;
                  MessageBox.Hide();
              })
               ));

            Task.Run(() =>
            {
                foreach (Page p in internPages)
                {
                    MessageBox.MessageBoxText = string.Copy(txt);
                    MessageBox.MessageBoxText += "\r\n Getting Page " + GetPageNumber(p) + " from " + (internPages.Count - 1);

                    if (cancelToken.Token.IsCancellationRequested) {return; }
                    HashSet<StickerCollection> collection = LineStickers.GetPageStickerCollections(GetPageNumber(p));

                    foreach (StickerCollection c in collection)
                    {
                        if (cancelToken.Token.IsCancellationRequested) { return; }
                        c.BaseViewMessageBox = this.MessageBox;
                        c.LoadImage();
                        c.GetMeta();
                        c.DownloadCollection(null, true, cancelToken.Token,true);
                    }
                }
            }).ContinueWith((o1) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    PaginationEnabled = true;
                    this.MessageBox.Hide();
                });
            });
        }

    }
}
