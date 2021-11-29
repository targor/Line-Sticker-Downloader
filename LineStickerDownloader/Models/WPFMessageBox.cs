using LineStickerDownloader.Commands;
using System.Windows;
using System.Windows.Input;

namespace LineStickerDownloader.Models
{
    public class WPFMessageBox: BaseViewModel
    {

        private string _messageBoxTitle="";
        public string MessageBoxTitle
        {
            get { return _messageBoxTitle; }
            set 
            {
                _messageBoxTitle = value;
                InvokePropertyChanged();
            }
        }

        private string _messageBoxText = "";
        public string MessageBoxText
        {
            get { return _messageBoxText; }
            set 
            {
                _messageBoxText = value;
                InvokePropertyChanged();
            }
        }

        private string _messageBoxButtonText = "";
        public string MessageBoxButtonText
        {
            get { return _messageBoxButtonText; }
            set 
            {
                _messageBoxButtonText = value;
                InvokePropertyChanged();
            }
        }

        private int _messageBoxBlur =0;
        public int MessageBoxBlur
        {
            get { return _messageBoxBlur; }
            set 
            {
                _messageBoxBlur = value;
                MainViewModel.SetMultiBlur(value);
                InvokePropertyChanged();
            }
        }

        private Visibility _messageBoxVisibility = Visibility.Collapsed;
        public Visibility MessageBoxVisibility
        {
            get { return _messageBoxVisibility; }
            set 
            {
                _messageBoxVisibility = value; 
                if (value==Visibility.Visible)
                {
                    MessageBoxBlur = 5;
                }
                else
                {
                    MessageBoxBlur = 0;
                }
                InvokePropertyChanged();
            }
        }

        private Visibility _messageBoxButtonsVisibility = Visibility.Visible;
        public Visibility MessageBoxButtonsVisibility
        {
            get { return _messageBoxButtonsVisibility; }
            set
            {
                _messageBoxButtonsVisibility = value;
                InvokePropertyChanged();
            }
        }

        private Visibility _progressVisibility = Visibility.Collapsed;
        public Visibility ProgressVisibility
        {
            get { return _progressVisibility; }
            set
            {
                _progressVisibility = value;
                InvokePropertyChanged();
            }
        }

        private int _progressBarValue = 0;
        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                InvokePropertyChanged();
            }
        }

        private int _progressBarMin = 0;
        public int ProgressBarMin
        {
            get { return _progressBarMin; }
            set
            {
                _progressBarMin = value;
                InvokePropertyChanged();
            }
        }

        private int _progressBarMax = 100;
        public int ProgressBarMax
        {
            get { return _progressBarMax; }
            set
            {
                _progressBarMax = value;
                InvokePropertyChanged();
            }
        }


        private bool _intermediate = false;
        public bool Intermediate
        {
            get { return _intermediate; }
            set
            {
                _intermediate = value;
                InvokePropertyChanged();
            }
        }

        public bool IsModal { get; set; } = false;
        public bool HasProgressBar { get; set; } = false;

        private ICommand _messageBoxButtonCommand;
        public ICommand MessageBoxButtonCommand 
        {
            get
            {
                return _messageBoxButtonCommand;
            }
            set
            {
                _messageBoxButtonCommand = value;
                InvokePropertyChanged();
            }
        }

        public void Hide() 
        {
            App.Current.Dispatcher.Invoke(() => { 
                MessageBoxVisibility = Visibility.Collapsed;
            });
        }
        public void Show() 
        {
            App.Current.Dispatcher.Invoke(() => {
                MessageBoxVisibility = Visibility.Visible;
            });
        }
        public WPFMessageBox(string title, string description, string buttonText,bool isModal=false,bool hasProgressbar=false,bool progressbarIntermediate=false, RelayCommand ProcessCommand=null)
        {
            this.MessageBoxTitle = title;
            this.MessageBoxText = description;
            this.MessageBoxButtonText = buttonText;
            this.MessageBoxButtonCommand = ProcessCommand;
            this.MessageBoxVisibility=Visibility.Visible;
            this.IsModal = isModal;
            this.HasProgressBar = hasProgressbar;
            this.Intermediate = progressbarIntermediate;

            if (this.IsModal)
            {
                MessageBoxButtonsVisibility = Visibility.Collapsed;
            }

            if(this.HasProgressBar)
            {
                ProgressVisibility=Visibility.Visible;
            }
            this.Intermediate = progressbarIntermediate;
        }

        public void OverrideValues(WPFMessageBox box)
        {
            this.MessageBoxBlur = box.MessageBoxBlur;
            this.MessageBoxButtonCommand = box.MessageBoxButtonCommand;
            this.MessageBoxButtonsVisibility = box.MessageBoxButtonsVisibility;
            this.MessageBoxButtonText = box.MessageBoxButtonText;
            this.MessageBoxText = box.MessageBoxText;
            this.MessageBoxTitle = box.MessageBoxTitle;
            this.MessageBoxVisibility = box.MessageBoxVisibility;
            this.ProgressBarMax = box.ProgressBarMax;
            this.ProgressBarMin = box.ProgressBarMin;
            this.ProgressBarValue = box.ProgressBarValue;
            this.ProgressVisibility = box.ProgressVisibility;

            this.IsModal = box.IsModal;
            this.HasProgressBar = box.HasProgressBar;
            this.Intermediate = box.Intermediate;

            if (this.IsModal)
            {
                MessageBoxButtonsVisibility = Visibility.Collapsed;
            }

            if (this.HasProgressBar)
            {
                ProgressVisibility = Visibility.Visible;
            }

            this.Intermediate = box.Intermediate;
        }

    }
}
