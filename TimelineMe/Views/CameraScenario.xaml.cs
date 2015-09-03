using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using TimelineMe;
using Windows.Storage;
using Windows.Devices.Enumeration;
using TimelineMe.Models;
using TimelineMe.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TimelineMe.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CameraScenario : Page
    {
        /// <summary>   
        /// Brush for drawing the bounding box around each detected face.
        /// </summary>
        //private readonly SolidColorBrush lineBrush = new SolidColorBrush(Windows.UI.Colors.Yellow);

        /// <summary>
        /// Thickness of the face bounding box lines.
        /// </summary>
        //private readonly double lineThickness = 2.0;

        /// <summary>
        /// Transparent fill for the bounding box.
        /// </summary>
        //private readonly SolidColorBrush fillBrush = new SolidColorBrush(Windows.UI.Colors.Transparent);

        /// <summary>
        /// Reference back to the "root" page of the app.
        /// </summary>
        private MainPage rootPage;

        /// <summary>
        /// Holds the current scenario state value.
        /// </summary>
        private ScenarioState currentState;

        /// <summary>
        /// References a MediaCapture instance; is null when not in Streaming state.
        /// </summary>
        private MediaCapture mediaCapture;

        private MediaViewModel media;

        private MediaCaptureInitializationSettings MediaCaptureSettings;
        private DeviceInformation frontWebcam;
        private DeviceInformation backWebcam;
        private StorageFolder folder = ApplicationData.Current.LocalFolder;
        private StorageFile captureFile;
        // True for back camera
        private bool whichCameraToInit = true;
        // True for video mode :)
        private bool videoOrPic = true;
        
        /// <summary>
        /// Cache of properties from the current MediaCapture device which is used for capturing the preview frame.
        /// </summary>
        private VideoEncodingProperties videoProperties;
        public CameraScenario()
        {
            this.InitializeComponent();
            this.currentState = ScenarioState.Idle;
            App.Current.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Values for identifying and controlling scenario states.
        /// </summary>
        private enum ScenarioState
        {
            /// <summary>
            /// Display is blank - default state.
            /// </summary>
            Idle,

            /// <summary>
            /// Webcam is actively engaged and a live video stream is displayed.
            /// </summary>
            Streaming,

            /// <summary>
            /// Snapshot image has been captured and is being displayed along with detected faces; webcam is not active.
            /// </summary>
            Snapshot
        }

      
        private async Task ConfigureCamera()
        {
            
            DeviceInformationCollection webcamList = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);


            frontWebcam = (from webcam in webcamList
                                 where webcam.EnclosureLocation != null
                                 && webcam.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Front
                                 select webcam).FirstOrDefault();
                
            
            backWebcam = (from webcam in webcamList
                                            where webcam.EnclosureLocation != null
                                            && webcam.EnclosureLocation.Panel == Windows.Devices.Enumeration.Panel.Back
                                            select webcam).FirstOrDefault();


        }



        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                this.rootPage = MainPage.Current;
                CameraStreamingButton.IsEnabled = false;
                await ConfigureCamera();
                if (backWebcam == null)
                    backCamComboItem.IsEnabled = false;
                if (frontWebcam == null)
                    frontCamComboItem.IsEnabled = false;
            }
            catch(Exception ex)
            {
                // TODO: inform user of error
            }
            


        }

        /// <summary>
        /// Responds to App Suspend event to stop/release MediaCapture object if it's running and return to Idle state.
        /// </summary>
        /// <param name="sender">The source of the Suspending event</param>
        /// <param name="e">Event data</param>
        private void OnSuspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            if (this.currentState == ScenarioState.Streaming)
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                try
                {
                    this.ChangeScenarioState(ScenarioState.Idle);
                }
                finally
                {
                    deferral.Complete();
                }
            }
        }

        /// <summary>
        /// Initializes a new MediaCapture instance and starts the Preview streaming to the CamPreview UI element.
        /// </summary>
        /// <returns>Async Task object returning true if initialization and streaming were successful and false if an exception occurred.</returns>
        private async Task<bool> StartWebcamStreaming()
        {
            bool successful = true;

            try
            {
                this.mediaCapture = new MediaCapture();
                
                MediaCaptureSettings = new MediaCaptureInitializationSettings();
                MediaCaptureSettings.StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo;

                if(whichCameraToInit)
                {
                    MediaCaptureSettings.VideoDeviceId = backWebcam.Id;
                }
                else
                {
                    MediaCaptureSettings.VideoDeviceId = frontWebcam.Id;
                }
                await this.mediaCapture.InitializeAsync(MediaCaptureSettings);
                this.mediaCapture.CameraStreamStateChanged += this.MediaCapture_CameraStreamStateChanged;

                // Cache the media properties as we'll need them later.
                var deviceController = this.mediaCapture.VideoDeviceController;
                this.videoProperties = deviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                // Immediately start streaming to our CaptureElement UI.
                // NOTE: CaptureElement's Source must be set before streaming is started.
                this.CamPreview.Source = this.mediaCapture;
                await this.mediaCapture.StartPreviewAsync();
            }
            catch (System.UnauthorizedAccessException)
            {
                // If the user has disabled their webcam this exception is thrown; provide a descriptive message to inform the user of this fact.
                this.rootPage.NotifyUser("Webcam is disabled or access to the webcam is disabled for this app.\nEnsure Privacy Settings allow webcam usage.", NotifyType.ErrorMessage);
                successful = false;
            }
            catch (Exception ex)
            {
                this.rootPage.NotifyUser(ex.ToString(), NotifyType.ErrorMessage);
                successful = false;
            }

            return successful;
        }

        /// <summary>
        /// Safely stops webcam streaming (if running) and releases MediaCapture object.
        /// </summary>
        private async void ShutdownWebCam()
        {
            if (this.mediaCapture != null)
            {
                if (this.mediaCapture.CameraStreamState == Windows.Media.Devices.CameraStreamState.Streaming)
                {
                    await this.mediaCapture.StopPreviewAsync();
                }

                this.mediaCapture.Dispose();
            }

            this.CamPreview.Source = null;
            this.mediaCapture = null;
        }

        /// <summary>
        /// Captures a single frame from the running webcam stream and executes the FaceDetector on the image. If successful calls SetupVisualization to display the results.
        /// </summary>
        /// <returns>Async Task object returning true if the capture was successful and false if an exception occurred.</returns>
        private async Task<bool> TakeSnapshotAndFindFaces()
        {
            bool successful = true;

            try
            {
                if (this.currentState != ScenarioState.Streaming)
                {
                    return false;
                }

                // TODO : photo and video option.

                if(videoOrPic)
                {
                    captureFile = await folder.CreateFileAsync("timeline.mp4", CreationCollisionOption.GenerateUniqueName);
                    await mediaCapture.StartRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p), captureFile);

                    //TODO : Link ViewModel
                    
                    media = new MediaViewModel();
                    media.SaveItem(new MediaViewModel
                    {
                        Name = captureFile.DisplayName,
                        CreationDate = captureFile.DateCreated.DateTime,
                        VidOrPic = true
                    });
                   
                }
                else
                {
                    captureFile = await folder.CreateFileAsync("timeline.jpeg", CreationCollisionOption.GenerateUniqueName);
                    await mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg(), captureFile);
                    //TODO : link ViewMOdel
                    media = new MediaViewModel();
                    media.SaveItem(new MediaViewModel
                    {
                        Name = captureFile.DisplayName,
                        CreationDate = captureFile.DateCreated.DateTime,
                        VidOrPic = false
                    });


                }

                // TODO
            }
            catch (Exception ex)
            {
                this.rootPage.NotifyUser(ex.ToString(), NotifyType.ErrorMessage);
                successful = false;
            }

            return successful;
        }

        private async void ChangeScenarioState(ScenarioState newState)
        {
            switch (newState)
            {
                case ScenarioState.Idle:

                    this.ShutdownWebCam();
                    //this.SnapshotCanvas.Background = null;
                    //this.SnapshotCanvas.Children.Clear();
                    this.CameraSnapshotButton.IsEnabled = false;
                    this.CameraStreamingButton.Content = "Start Cam";
                    this.CameraSnapshotButton.Content = "Capture";
                    this.currentState = newState;
                    break;

                case ScenarioState.Streaming:

                    if (!await this.StartWebcamStreaming())
                    {
                        this.ChangeScenarioState(ScenarioState.Idle);
                        break;
                    }

                   
                    
                    
                    //this.SnapshotCanvas.Children.Clear();
                    this.CameraSnapshotButton.IsEnabled = true;
                    this.CameraStreamingButton.Content = "Stop Cam";
                    this.CameraSnapshotButton.Content = "Capture";
                    this.currentState = newState;
                    break;


                case ScenarioState.Snapshot:

                    if (!await this.TakeSnapshotAndFindFaces())
                    {
                        this.ChangeScenarioState(ScenarioState.Idle);
                        break;
                    }

                    // this.ShutdownWebCam();
                    // I don't really know what happened here, it just seems to work \w/

                    this.CameraSnapshotButton.IsEnabled = true;
                    this.CameraStreamingButton.Content = "Start Cam";
                    this.CameraSnapshotButton.Content = "Clear Display";
                    this.currentState = newState;
                    break;
            }
        }

        /// <summary>
        /// Handles MediaCapture changes by shutting down streaming and returning to Idle state.
        /// </summary>
        /// <param name="sender">The source of the event, i.e. our MediaCapture object</param>
        /// <param name="args">Event data</param>
        private void MediaCapture_CameraStreamStateChanged(MediaCapture sender, object args)
        {
            // MediaCapture is not Agile and so we cannot invoke it's methods on this caller's thread
            // and instead need to schedule the state change on the UI thread.
            var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ChangeScenarioState(ScenarioState.Idle);
            });
        }

        /// <summary>
        /// Handles "streaming" button clicks to start/stop webcam streaming.
        /// </summary>
        /// <param name="sender">Button user clicked</param>
        /// <param name="e">Event data</param>
        private void CameraStreamingButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentState == ScenarioState.Streaming)
            {
                this.rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                this.ChangeScenarioState(ScenarioState.Idle);
            }
            else
            {
                this.rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                this.ChangeScenarioState(ScenarioState.Streaming);
            }
        }

        /// <summary>
        /// Handles "snapshot" button clicks to take a snapshot or clear the current display.
        /// </summary>
        /// <param name="sender">Button user clicked</param>
        /// <param name="e">Event data</param>
        private void CameraSnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentState == ScenarioState.Streaming)
            {
                this.rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                this.ChangeScenarioState(ScenarioState.Snapshot);
            }
            else
            {
                this.rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                this.ChangeScenarioState(ScenarioState.Idle);
            }
        }

        private void backCamComboItem_Tapped(object sender, TappedRoutedEventArgs e)
        {  
            whichCameraToInit = true;
            CameraStreamingButton.IsEnabled = true;
        }

        private void frontCamComboItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            whichCameraToInit = false;
            CameraStreamingButton.IsEnabled = true;
        }

        private void vidComboBoxItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            videoOrPic = true;
        }

        private void picComboBoxItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            videoOrPic = false;
        }
    }
}
