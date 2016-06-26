using BioInfo.Client.ValueConverters;
using Microsoft.Band.Portable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BioInfo.Client.Services
{
    public class BandService : INotifyPropertyChanged
    {
        private BandClient bandClient;
        private BandDeviceInfo band;
        private BandClientManager bandClientManager;

        private string currentSkinTemp = "Nothing to see here";
        public string CurrentSkinTemp
        {
            get { return currentSkinTemp; }
            set
            {
                currentSkinTemp = value;
                OnPropertyChanged();
            }
        }

        private string currentAmbientLight;
        public string CurrentAmbientLight
        {
            get { return currentAmbientLight; }
            set
            {
                currentAmbientLight = value;
                OnPropertyChanged();
            }
        }

        private string currentHeartRate;
        public string CurrentHeartRate
        {
            get { return currentHeartRate; }
            set
            {
                currentHeartRate = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public BandService()
        {
            bandClientManager = BandClientManager.Instance;
        }
        public async Task<IEnumerable<BandDeviceInfo>> getBands()
        {            
            var bands = await bandClientManager.GetPairedBandsAsync();
            return bands;
        }

        public async Task<String> ConnectToBand(BandDeviceInfo b)
        {
            band = b;
            bandClient = await bandClientManager.ConnectAsync(band);
            return String.Format("connected to {0} !", band.Name);            
        }

        public async Task StartReadingSkinTemp()
        {
            await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged;
        }
        public async Task StopReadingSkinTemp()
        {
            await bandClient.SensorManager.SkinTemperature.StopReadingsAsync();
            bandClient.SensorManager.SkinTemperature.ReadingChanged -= SkinTemperature_ReadingChanged;
        }
        private async void SkinTemperature_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandSkinTemperatureReading> e)
        {
            await Task.Run(() => {
                this.CurrentSkinTemp = CelsiusToFahrenheitConverter.Convert(e.SensorReading.Temperature).ToString();
            });
        }
        
        public async Task StartReadingAmbientLight()
        {
            await bandClient.SensorManager.AmbientLight.StartReadingsAsync();
            bandClient.SensorManager.AmbientLight.ReadingChanged += AmbientLight_ReadingChanged;
        }
        public async Task StopReadingAmbientLight()
        {
            await bandClient.SensorManager.AmbientLight.StopReadingsAsync();
            bandClient.SensorManager.AmbientLight.ReadingChanged -= AmbientLight_ReadingChanged;
        }
        private async void AmbientLight_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandAmbientLightReading> e)
        {
            await Task.Run(() =>
            {
                this.CurrentAmbientLight = e.SensorReading.Brightness.ToString();
            });
        }

        public async Task StartReadingHeartRate()
        {
            bool hrConsentGranted;
            switch (bandClient.SensorManager.HeartRate.UserConsented)
            {
                case UserConsent.Declined:
                    hrConsentGranted = false;
                    break;

                case UserConsent.Granted:
                    hrConsentGranted = true;
                    break;

                default:
                case UserConsent.Unspecified:
                    hrConsentGranted = await bandClient.SensorManager.HeartRate.RequestUserConsent();
                    break;
            }
            if (hrConsentGranted)
            {
                await bandClient.SensorManager.HeartRate.StartReadingsAsync();
                bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
            }
            else
            {
                this.CurrentHeartRate = "No Consent for HR";
            }

        }
        public async Task StopReadingHeartRate()
        {
            await bandClient.SensorManager.HeartRate.StopReadingsAsync();
            bandClient.SensorManager.HeartRate.ReadingChanged -= HeartRate_ReadingChanged;
        }
        private async void HeartRate_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandHeartRateReading> e)
        {
            await Task.Run(() =>
            {
                this.CurrentHeartRate = e.SensorReading.HeartRate.ToString();
            });
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
