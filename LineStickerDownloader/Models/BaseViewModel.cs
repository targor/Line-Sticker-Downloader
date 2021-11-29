using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LineStickerDownloader.Models
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        [JsonIgnore]
        private WPFMessageBox _baseViewMessageBox = null;
        [JsonIgnore]
        public WPFMessageBox BaseViewMessageBox 
        {
            get
            {
                return _baseViewMessageBox;
            }
            set
            {
                _baseViewMessageBox = value;
                InvokePropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void InvokePropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void PusblishMessageBox(WPFMessageBox box)
        {
            this.BaseViewMessageBox.OverrideValues(box);
        }

        public BaseViewModel(WPFMessageBox messageBox=null)
        {
           this.BaseViewMessageBox = messageBox;
        }

    }
}
