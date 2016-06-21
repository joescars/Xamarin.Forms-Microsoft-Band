using Microsoft.Band.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BioInfo.Client
{
    public class MainPage : ContentPage
    {
        ListView lv = new ListView();
        Label myLabel = new Label();
        Label myHeartRate = new Label();
        Label mySkinTemp = new Label();
        Label myGSR = new Label();

        string results = "Nothing Loaded";

        private BandClient bandClient;
       
        public MainPage()
        {
            //load up band info
            getBands();

            this.Padding = new Thickness(20, Device.OnPlatform(40, 20, 20), 20, 20);

            StackLayout panel = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Vertical,
                Spacing = 15
            };

            panel.Children.Add(myLabel = new Label
            {
                Text = results
            });

            panel.Children.Add(myHeartRate = new Label
            {
                Text = "Heart Rate Here"
            });

            panel.Children.Add(mySkinTemp = new Label
            {
                Text = "Skin Temp Here"
            });

            panel.Children.Add(myGSR = new Label
            {
                Text = "GSR in kohm Here"
            });

            this.Content = panel;
        }

        private async void getBands()
        {

            var bands = await BandClientManager.Instance.GetPairedBandsAsync();
            var band = bands.FirstOrDefault();
            if (band == null)
            {
                myLabel.Text = "tried but failed";
                return;
            }

            myLabel.Text = "connecting...";
            bandClient = await BandClientManager.Instance.ConnectAsync(band);
            myLabel.Text = String.Format("connected to {0} !",band.Name);

            // Heart Rate

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
                myHeartRate.Text = "No Consent for HR";
            }

            // Skin Sensor
            StartSkinTemp();
            bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged;

            // GSR sensor
            StartGSR();
            bandClient.SensorManager.Gsr.ReadingChanged += Gsr_ReadingChanged;

        }

        private void Gsr_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandGsrReading> e)
        {
            Device.BeginInvokeOnMainThread(() => {
                myGSR.Text = string.Format("Resistance {0} kohms",
                    e.SensorReading.Resistance);
            });
        }

        private void SkinTemperature_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandSkinTemperatureReading> e)
        {
            Device.BeginInvokeOnMainThread(() => {
                mySkinTemp.Text = string.Format("Skin Temp {0}",
                    ConvertCelsiusToFahrenheit(e.SensorReading.Temperature).ToString());
            });
        }

        private void HeartRate_ReadingChanged(object sender, Microsoft.Band.Portable.Sensors.BandSensorReadingEventArgs<Microsoft.Band.Portable.Sensors.BandHeartRateReading> e)
        {
            Device.BeginInvokeOnMainThread(() => {
            myHeartRate.Text = string.Format("{0} {1}",
                e.SensorReading.Quality, e.SensorReading.HeartRate);
            });

        }

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
        }

        private async void StopSkinTemp()
        {
            await bandClient.SensorManager.SkinTemperature.StopReadingsAsync();
        }

        private async void StartGSR()
        {
            await bandClient.SensorManager.Gsr.StartReadingsAsync();
        }

        private async void StopGSR()
        {
            await bandClient.SensorManager.Gsr.StopReadingsAsync();
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
