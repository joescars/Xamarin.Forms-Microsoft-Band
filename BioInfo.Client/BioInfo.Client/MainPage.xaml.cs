using Microsoft.Band.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

namespace BioInfo.Client
{
    public partial class MainPage : ContentPage
    {
        private BandClient bandClient;
        private BandDeviceInfo band;
        private BandClientManager bandClientManager;

        public MainPage()
        {
            InitializeComponent();

            getBands();
        }

        void DisableToggles()
        {

        }
        void EnableToggles()
        {
            ToggleHeart.IsEnabled = true;
            ToggleSkin.IsEnabled = true;
            ToggleGSR.IsEnabled = true;
            ToggleAmbientLight.IsEnabled = true;
            ToggleUV.IsEnabled = true;
            TogglePedo.IsEnabled = true;
            ToggleCalories.IsEnabled = true;
        }

        // Band Connections
        private async void getBands()
        {
            bandClientManager = BandClientManager.Instance;
            var bands = await bandClientManager.GetPairedBandsAsync();
            band = bands.FirstOrDefault();
            if (band == null)
            {
                lblStatus.Text = "tried but failed";
                return;
            }

            btnConnect.Text = "Connect to Band: " + band.Name;
        }

        private async void ConnectToBand()
        {
            lblStatus.Text = "Connecting...";
            bandClient = await bandClientManager.ConnectAsync(band);
            lblStatus.Text = String.Format("connected to {0} !", band.Name);
            btnConnect.IsEnabled = false;
            EnableToggles();
        }

        // Button & Toggle Actions
        void OnButtonConnect(object sender, EventArgs args)
        {
            ConnectToBand();
        }

        void OnButtonStop(object sender, EventArgs args)
        {
            StopHR();
            StopSkinTemp();
            StopGSR();
            StopAmbientLight();
            StopUV();
            StopPedometer();
            StopCalories();

            ToggleHeart.IsToggled = false;
            ToggleSkin.IsToggled = false;
            ToggleGSR.IsToggled = false;
            ToggleAmbientLight.IsToggled = false;
            ToggleUV.IsToggled = false;
            TogglePedo.IsToggled = false;
            ToggleCalories.IsToggled = false;
        }

        async void OnToggleHeartRate(object sender, EventArgs args)
        {
            Switch s = (Switch)sender;
            if (s.IsToggled)
            {
                // Turn On
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
                    StartHR();
                    bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
                }
                else
                {
                    lblHeartRate.Text = "No Consent for HR";
                }
            }
            else
            {
                // Turn Off
                StopHR();
            }
        }

        void OnToggleSkinTemp(object sender, EventArgs args)
        {
            Switch s = (Switch)sender;
            if (s.IsToggled)
                StartSkinTemp();
            else
                StopSkinTemp();
            
        }
        void OnToggleGSR(object sender, EventArgs args)
        {
            Switch s = (Switch)sender;
            if (s.IsToggled)
               StartGSR();                
            else
               StopGSR();
        }

        void OnToggleAmbientLight(object sender, EventArgs args)
        {
            Switch s = (Switch)sender;
            if (s.IsToggled)            
                StartAmbientLight();            
            else
                StopAmbientLight();
        }
        void OnToggleUV(object sender, EventArgs args)
        {
            Switch s = (Switch)sender;
            if (s.IsToggled)
                StartUV();
            else
                StopUV();
        }
        void OnToggleCalories(object sender, EventArgs args)
        {
            Switch s = (Switch)sender;
            if (s.IsToggled)
                StartCalories();
            else
                StopCalories();
        }
        void OnTogglePedometer(object sender, EventArgs args)
        {
            Switch s = (Switch)sender;
            if (s.IsToggled)
                StartPedometer();
            else
                StopPedometer();
        }

        // Band Readings
        private async void StartHR()
        {
            await bandClient.SensorManager.HeartRate.StartReadingsAsync();
        }

        private async void StopHR()
        {
            await bandClient.SensorManager.HeartRate.StopReadingsAsync();
        }
        private async void StartSkinTemp()
        {
            await bandClient.SensorManager.SkinTemperature.StartReadingsAsync();
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged;
        }

        private async void StopSkinTemp()
        {
            await bandClient.SensorManager.SkinTemperature.StopReadingsAsync();
        }
        private async void StartGSR()
        {
            await bandClient.SensorManager.Gsr.StartReadingsAsync();
            bandClient.SensorManager.Gsr.ReadingChanged += Gsr_ReadingChanged;
        }

        private async void StopGSR()
        {
            await bandClient.SensorManager.Gsr.StopReadingsAsync();
        }

        private async void StartAmbientLight()
        {
            await bandClient.SensorManager.AmbientLight.StartReadingsAsync();
            bandClient.SensorManager.AmbientLight.ReadingChanged += AmbientLight_ReadingChanged;
        }

        private async void StopAmbientLight()
        {
            await bandClient.SensorManager.AmbientLight.StopReadingsAsync();
        }
        private async void StartUV()
        {
            await bandClient.SensorManager.UltravioletLight.StartReadingsAsync();
            bandClient.SensorManager.UltravioletLight.ReadingChanged += UltravioletLight_ReadingChanged;
        }

        private async void StopUV()
        {
            await bandClient.SensorManager.UltravioletLight.StopReadingsAsync();
        }

        private async void StartCalories()
        {
            await bandClient.SensorManager.Calories.StartReadingsAsync();
            bandClient.SensorManager.Calories.ReadingChanged += Calories_ReadingChanged;
        }
        private async void StopCalories()
        {
            await bandClient.SensorManager.Calories.StopReadingsAsync();
        }
        private async void StartPedometer()
        {
            await bandClient.SensorManager.Pedometer.StartReadingsAsync();
            bandClient.SensorManager.Pedometer.ReadingChanged += Pedometer_ReadingChanged;
        }
        private async void StopPedometer()
        {
            await bandClient.SensorManager.Pedometer.StopReadingsAsync();
        }

        private void HeartRate_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandHeartRateReading> e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lblHeartRate.Text = string.Format("Quality: {0} BPM: {1}",
                    e.SensorReading.Quality, e.SensorReading.HeartRate);
            });
        }

        private void SkinTemperature_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandSkinTemperatureReading> e)
        {
            Device.BeginInvokeOnMainThread(() => {
                lblSkinTemp.Text = string.Format("Skin Temp: {0} °F",
                    ConvertCelsiusToFahrenheit(e.SensorReading.Temperature).ToString("F2"));
            });
        }
        private void Gsr_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandGsrReading> e)
        {
            Device.BeginInvokeOnMainThread(() => {
                lblGSR.Text = string.Format("Resistance: {0} kohms",
                    e.SensorReading.Resistance.ToString("N0"));
            });
        }
        private void AmbientLight_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandAmbientLightReading> e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lblAmbientLight.Text = string.Format("Lux: {0}",
                    e.SensorReading.Brightness.ToString("N0"));
            });
        }
        private void UltravioletLight_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandUltravioletLightReading> e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lblUV.Text = string.Format("UV Today: {0} Level: {1}",
                    e.SensorReading.ExposureToday.ToString(), e.SensorReading.Level);
            });
        }

        private void Calories_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandCaloriesReading> e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lblCalories.Text = string.Format("Calories Today: {0} Total: {1}",
                    e.SensorReading.CaloriesToday.ToString("N0"), e.SensorReading.Calories.ToString("N0"));
            });
        }
        private void Pedometer_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandPedometerReading> e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                lblPedometer.Text = string.Format("Steps Today: {0} Total: {1}",
                    e.SensorReading.StepsToday.ToString("N0"), e.SensorReading.TotalSteps.ToString("N0"));
            });
        }

        // Helper Functions
        private static double ConvertCelsiusToFahrenheit(double c)
        {
            return ((9.0 / 5.0) * c) + 32;
        }

        private static double ConvertFahrenheitToCelsius(double f)
        {
            return (5.0 / 9.0) * (f - 32);
        }

    }
}
