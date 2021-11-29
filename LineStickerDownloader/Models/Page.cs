using LineStickerDownloader.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LineStickerDownloader.Models
{
    internal class Page: BaseViewModel
    {
        public string Value { get; set; }

        public ICommand CallPage { get; set; }

        private bool _isSelected=false;

        public bool IsSelected
        {
            get { return _isSelected; }
            set 
            {
                InvokePropertyChanged();
                _isSelected = value;
            }
        }


        public Page(string numberOrValue,RelayCommand cmd)
        {
            this.CallPage = cmd;
            this.Value = numberOrValue;
        }
    }
}
