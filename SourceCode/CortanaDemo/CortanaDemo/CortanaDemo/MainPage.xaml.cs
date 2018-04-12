using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Plugin.Media;
using Plugin.Media.Abstractions;
using CortanaDemo.Helpers;
using Microsoft.ProjectOxford.Vision;

namespace CortanaDemo
{
    public partial class MainPage : ContentPage
    {
        VisionServiceClient visionClient;
        MediaFile photo;

        public MainPage()
        {
            InitializeComponent();
            visionClient = new VisionServiceClient(Constants.VisionApiKey, "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0");
        }

        private async void OnTakePhotoButtonClicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            // Take photo
            if (CrossMedia.Current.IsCameraAvailable || CrossMedia.Current.IsTakePhotoSupported)
            {
                photo = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    Name = "image.jpg",
                    PhotoSize = PhotoSize.Small
                });

                if (photo != null)
                {
                    image.Source = ImageSource.FromStream(photo.GetStream);
                }
            }
            else
            {
                await DisplayAlert("No Camera", "Camera unavailable.", "OK");
            }

            ((Button)sender).IsEnabled = false;
            activityIndicator.IsRunning = true;

            // Recognize emotion
            try
            {
                if (photo != null)
                {
                    using (var photoStream = photo.GetStream())
                    {
                        var results = await visionClient.DescribeAsync(photoStream, 1);
                        if (results != null)
                        {
                            emotionResultLabel.Text = results.Description.Captions.FirstOrDefault().Text;
                        }
                        photo.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                photo.Dispose();
            }

            activityIndicator.IsRunning = false;
            ((Button)sender).IsEnabled = true;
        }
    }
}
