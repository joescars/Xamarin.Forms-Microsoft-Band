using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BioInfo.Client.ValueConverters;
using Microsoft.Band.Portable;
using Xamarin.Forms;
using BioInfo.Client.Services;

namespace BioInfo.Client.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Band Setup

        //local band that we will connect to
        private BandDeviceInfo band;

        private BandService bandService;

        // Interface

        private string pageTitle;
        private string currentStatus;
        private string connectButtonText;
        private bool connectButtonEnabled;

        // Sensor Stats

        private string readHR;
        private string readHRQuality;
        private string readSkinTemp;
        private string readAmbientLight;
        private bool isToggleSkinTemp;
        private bool isToggleAmbientLight;
        private bool isToggleHeartRate;

        public bool IsToggleSkinTemp
        {
            get { return isToggleSkinTemp; }
            set
            {
                if (isToggleSkinTemp != value)
                {
                    isToggleSkinTemp = value;
                    OnPropertyChanged();
                    ToggleSkinTemp();
                }
            }
        }

        public bool IsToggleAmbientLight
        {
            get { return isToggleAmbientLight; }
            set
            {
                if (isToggleAmbientLight != value)
                {
                    isToggleAmbientLight = value;
                    OnPropertyChanged();
                    ToggleAmbientLight();
                }
            }
        }

        public bool IsToggleHeartRate
        {
            get { return isToggleHeartRate; }
            set
            {
                if (isToggleHeartRate != value)
                {
                    isToggleHeartRate = value;
                    OnPropertyChanged();
                    ToggleHeartRate();
                }
            }
        }

        public string PageTitle
        {
            get { return pageTitle; }
            set
            {
                pageTitle = value;
                OnPropertyChanged();
            }
        }

        public string CurrentStatus
        {
            get { return currentStatus; }
            set
            {
                currentStatus = value;
                OnPropertyChanged();
            }
        }

        public string ConnectButtonText
        {
            get { return connectButtonText; }
            set
            {
                connectButtonText = value;
                OnPropertyChanged();
            }
        }

        public bool ConnectButtonEnabled
        {
            get { return connectButtonEnabled; } 
            set
            {
                connectButtonEnabled = value;
                OnPropertyChanged();
            }
        }

        public string ReadHR
        {
            get { return readHR; }
            set
            {
                readHR = value;
                OnPropertyChanged();
            }
        }

        public string ReadHRQuality
        {
            get { return readHRQuality; }
            set
            {
                readHRQuality = value;
                OnPropertyChanged();
            }
        }
        public string ReadSkinTemp
        {
            get { return readSkinTemp; }
            set
            {
                readSkinTemp = value;
                OnPropertyChanged();
            }
        }

        public string ReadAmbientLight
        {
            get { return readAmbientLight; }
            set
            {
                readAmbientLight = value;
                OnPropertyChanged();
            }
        }

        public MainPageViewModel()
        {
            pageTitle = "BioInformatics";
            currentStatus = "Current Status";
            connectButtonText = "Connect to Band";
            readHR = "25";
            readSkinTemp = "Not Connected";
            readAmbientLight = "Not Connected";
            isToggleSkinTemp = false;
            isToggleAmbientLight = false;
            connectButtonEnabled = true;           

            bandService = new BandService();           

            getBands();
        }
        
        private async void getBands()
        {
            var bands = await bandService.getBands();
            band = bands.FirstOrDefault();
            if (band == null)
            {
                this.CurrentStatus = "tried but failed";
                return;
            }
            this.ConnectButtonText = "Connect to Band: " + band.Name;
        }

        private async void ConnectToBand()
        {
            this.CurrentStatus = "Connecting...";
            var result = await bandService.ConnectToBand(band);             
            this.CurrentStatus = result;
            this.ConnectButtonEnabled = false;
            bandService.PropertyChanged += BandService_PropertyChanged;
            //EnableToggles();
        }

        private async void BandService_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await Task.Run(() =>
            {
                this.ReadSkinTemp = bandService.CurrentSkinTemp;
                this.ReadAmbientLight = bandService.CurrentAmbientLight;
                this.ReadHR = bandService.CurrentHeartRate;
            });            
        }

        public Command Connect {
            get
            {
                return new Command(() =>
                {
                    ConnectToBand();
                });
            }                
        }

        // Band Readings
        private async void ToggleSkinTemp()
        {
            if (isToggleSkinTemp)
                await bandService.StartReadingSkinTemp();
            else
                await bandService.StopReadingSkinTemp();
            
        }
        private async void ToggleAmbientLight()
        {
            if (isToggleAmbientLight)
                await bandService.StartReadingAmbientLight();
            else
                await bandService.StopReadingAmbientLight();
        }

        private async void ToggleHeartRate()
        {
            if (isToggleHeartRate)
                await bandService.StartReadingHeartRate();
            else
                await bandService.StartReadingHeartRate();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
