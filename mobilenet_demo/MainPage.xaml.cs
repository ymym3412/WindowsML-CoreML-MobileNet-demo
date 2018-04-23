using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.System.Threading;
using Windows.Storage;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using Windows.Devices.Enumeration;
using mobilenets;

// 空白ページの項目テンプレートについては、https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x411 を参照してください

namespace mobilenet_demo
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1);                     //複数のスレッドで検出しないようにするためのsemaphore
        private ThreadPoolTimer timer;
        private MobilenetModel mobilenetModel = new MobilenetModel();             //AIモデルオブジェクト
        private MobilenetModelInput inputData = new MobilenetModelInput();        //input用オブジェクト（VideoFrame）
        private MobilenetModelOutput outputData = new MobilenetModelOutput();     //output用オブジェクト（List）

        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Hello");
            await InitCameraAsync();

            LoadModel();
        }


        /// <summary>
        /// カメラの初期化及びタイマーの起動
        /// </summary>
        /// <returns></returns>
        private async Task InitCameraAsync()
        {
            try
            {
                //mediaCaptureオブジェクトが有効な時は一度Disposeする
                if (mediaCapture != null)
                {
                    mediaCapture.Dispose();
                    mediaCapture = null;
                }

                //キャプチャーの設定
                var captureInitSettings = new MediaCaptureInitializationSettings();
                captureInitSettings.VideoDeviceId = "";
                captureInitSettings.StreamingCaptureMode = StreamingCaptureMode.Video;

                //カメラデバイスの取得
                var cameraDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

                if (cameraDevices.Count() == 0)
                {
                    Debug.WriteLine("No Camera");
                    return;
                }
                else if (cameraDevices.Count() == 1)
                {
                    Debug.WriteLine("count1\n");
                    captureInitSettings.VideoDeviceId = cameraDevices[0].Id;
                }
                else
                {
                    Debug.WriteLine("countelse\n");
                    captureInitSettings.VideoDeviceId = cameraDevices[1].Id;
                }

                //キャプチャーの準備
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync(captureInitSettings);

                VideoEncodingProperties vp = new VideoEncodingProperties();

                Debug.WriteLine("before camera size\n");
                //RasperryPiでは解像度が高いと映像が乱れるので小さい解像度にしている
                //ラズパイじゃなければ必要ないかも？
                vp.Height = 720;
                vp.Width = 1280;
                vp.Subtype = "RGB24";

                await mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, vp);

                capture.Source = mediaCapture;

                //キャプチャーの開始
                await mediaCapture.StartPreviewAsync();

                Debug.WriteLine("Camera Initialized");

                //指定したFPS毎にタイマーを起動する。
                TimeSpan timerInterval = TimeSpan.FromMilliseconds(1);
                timer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(CurrentVideoFrame), timerInterval);

            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timer"></param>
        private async void CurrentVideoFrame(ThreadPoolTimer timer)
        {
            //複数スレッドでの同時実行を抑制
            if (!semaphore.Wait(0))
            {
                return;
            }

            try
            {
                //AIモデルのインプットデータは解像度224x224,BGRA8にする必要がある。
                using (VideoFrame previewFrame = new VideoFrame(BitmapPixelFormat.Bgra8, 224, 224))
                {
                    await this.mediaCapture.GetPreviewFrameAsync(previewFrame);

                    if (previewFrame != null)
                    {
                        inputData.data = previewFrame;

                        //AIモデルにデータを渡すと推定値の入ったリストが返る
                        outputData = await mobilenetModel.EvaluateAsync(inputData);

                        //UIスレッドに結果を表示
                        var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            string result = "";
                            //予測結果を表示
                            string label = outputData.classLabel[0];
                            result = result + "Class: " + label + ", Prob: " + outputData.prob[label];


                            this.msgTbk.Text = result;
                        });
                    }
                }
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// AIモデルをロード
        /// </summary>
        private async void LoadModel()
        {
            StorageFile modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/mobilenet.onnx"));
            mobilenetModel = await MobilenetModel.CreateMobilenetModel(modelFile);
        }

    }

   
}
