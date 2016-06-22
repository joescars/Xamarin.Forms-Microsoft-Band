using Microsoft.Band.Portable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BioInfo.Client
{
    public class MainPageOld : ContentPage
    {
        private BandClient bandClient;
        private BandDeviceInfo band;
        private BandClientManager bandClientManager;

        Label myLabel = new Label();
        Label myHeartRate = new Label();
        Label mySkinTemp = new Label();
        Label myGSR = new Label();
        Label mainHeading = new Label();
        Button clickMe;
        Button btnHeartToggle;
        Button btnSkinTempToggle;
        Button btnGSRToggle;
        Button btnConnect;        

        string results = "Nothing Loaded";
        bool HeartRateActive = false;
        bool SkinTempActive = false;
        bool GSRActive = false;

        public MainPageOld()
        {
 
            this.Padding = new Thickness(20, Device.OnPlatform(40, 20, 20), 20, 20);

            StackLayout panel = new StackLayout
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Orientation = StackOrientation.Vertical,
                Spacing = 15
            };

            panel.Children.Add(mainHeading = new Label
            {
                Text = "BioInformatics",
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.CenterAndExpand
            });

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

            panel.Children.Add(btnConnect = new Button
            {
                Text = "Connect to Band"
            });

            //panel.Children.Add(clickMe = new Button
            //{
            //    Text = "Show Connected Band"
            //});

            panel.Children.Add(btnHeartToggle = new Button
            {
                Text = "Start Heart Rate Monitor"
            });

            panel.Children.Add(btnSkinTempToggle = new Button
            {
                Text = "Start Skin Temp Monitor"
            });

            panel.Children.Add(btnGSRToggle = new Button
            {
                Text = "Start GSR Monitor"
            });

            //clickMe.Clicked += ClickMe_Clicked;
            btnHeartToggle.Clicked += BtnHeartToggle_Clicked;
            btnConnect.Clicked += BtnConnect_Clicked;
            btnSkinTempToggle.Clicked += BtnSkinTempToggle_Clicked;
            btnGSRToggle.Clicked += BtnGSRToggle_Clicked;

            this.Content = panel;

            //load up band info
            getBands();
        }

        private void BtnConnect_Clicked(object sender, EventArgs e)
        {
            ConnectToBand();
        }

        private void BtnGSRToggle_Clicked(object sender, EventArgs e)
        {
            if (GSRActive)
            {
                StopGSR();
                GSRActive = !GSRActive;
                btnGSRToggle.Text = "Start GSR Monitor";
            }
            else
            {
                StartGSR();
                bandClient.SensorManager.Gsr.ReadingChanged += Gsr_ReadingChanged;
                GSRActive = !GSRActive;
                btnGSRToggle.Text = "Stop GSR Monitor";
            }

        }

        private void BtnSkinTempToggle_Clicked(object sender, EventArgs e)
        {
            if (SkinTempActive)
            {
                StopSkinTemp();
                SkinTempActive = !SkinTempActive;
                btnSkinTempToggle.Text = "Start Skin Temp Monitor";
            }
            else
            {
                StartSkinTemp();
                bandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged;
                SkinTempActive = !SkinTempActive;
                btnSkinTempToggle.Text = "Stop Skin Temp Monitor";
            }

        }
        
        private async void BtnHeartToggle_Clicked(object sender, EventArgs e)
        {
            if (HeartRateActive)
            {
                StopHR();
                btnHeartToggle.Text = "Start Heart Rate Monitor";
                myHeartRate.Text = "Heart rate not active";
                HeartRateActive = !HeartRateActive;
            }
            else
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
                    StartHR();
                    bandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
                    btnHeartToggle.Text = "Stop Heart Rate Monitor";
                    HeartRateActive = !HeartRateActive;
                }
                else
                {
                    myHeartRate.Text = "No Consent for HR";
                }
            }
        }

        //private async void ClickMe_Clicked(object sender, EventArgs e)
        //{
        //    string myMessage;
        //    if (band != null)
        //    {
        //        myMessage = band.Name;                
        //    }
        //    else
        //    {
        //        myMessage = "Nothing Yet";
        //    }
        //    await this.DisplayAlert("Message", myMessage, "Dismiss");
        //}

        private async void getBands()
        {
            bandClientManager = BandClientManager.Instance;
            //var bands = await BandClientManager.Instance.GetPairedBandsAsync();
            var bands = await bandClientManager.GetPairedBandsAsync();
            band = bands.FirstOrDefault();
            if (band == null)
            {
                myLabel.Text = "tried but failed";
                return;
            }

            btnConnect.Text = "Connect to Band: " + band.Name;

        }

        private async void ConnectToBand()
        {
            myLabel.Text = "connecting...";
            //bandClient = await BandClientManager.Instance.ConnectAsync(band);'
            bandClient = await bandClientManager.ConnectAsync(band);
            myLabel.Text = String.Format("connected to {0} !", band.Name);
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
            Device.BeginInvokeOnMainThread(() =>
            {
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
