using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using System.IO;
using System.Threading;

namespace MicTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DispatcherTimer t=new DispatcherTimer ();
       // private FftPitchDetector pitchDetector;
        private AutoCorrelator pitchDetector;
        private WaveBuffer waveBuffer;
        readonly SampleAggregator sampleAggregator=new SampleAggregator ();
        WaveFileWriter writer;
        WaveFormat recordingFormat;
        string waveFileName;

        public MainWindow () {
            InitializeComponent ();
            // Volume();
            Record ();
        }

        private void Record () {
            List<WaveInCapabilities> sources=new List<WaveInCapabilities> ();
            for (int i=0; i<WaveIn.DeviceCount; i++) {
                sources.Add (WaveIn.GetCapabilities (i));
            }
            foreach (var source in sources) {
                sourceList.Items.Add (source.ProductName);
            }
        }

        private void Volume () {
            MMDeviceEnumerator enumerator=new MMDeviceEnumerator ();
            var devices=enumerator.EnumerateAudioEndPoints (DataFlow.All, DeviceState.Active);
            foreach (var dev in devices) {
                comboBox.Items.Add (dev);
            }
            t.Interval=new TimeSpan (0, 0, 1);
            t.Tick+=t_Tick;
            t.Start ();
        }

        void t_Tick (object sender, EventArgs e) {
            if (comboBox.SelectedItem!=null) {
                var device=(MMDevice) comboBox.SelectedItem;
                progressBar.Value=(int) Math.Round (device.AudioMeterInformation.MasterPeakValue*100);

            }
        }

        private WaveIn waveIn=null;
        private DirectSoundOut waveOut=null;

        private void Button_Click (object sender, RoutedEventArgs e) {
            if (sourceList.SelectedItems.Count==0) return;

            int deviceNumber=sourceList.SelectedIndex;

            waveIn=new WaveIn ();
            waveIn.DeviceNumber=deviceNumber;
            waveIn.DataAvailable+=OnDataAvailable;
            waveIn.RecordingStopped+=OnRecordingStopped;
            recordingFormat=new WaveFormat (44100, WaveIn.GetCapabilities (deviceNumber).Channels);
            waveIn.WaveFormat=recordingFormat;
            waveFileName=Path.Combine (Path.GetTempPath (), Guid.NewGuid ()+".wav");

            writer=new WaveFileWriter (waveFileName, recordingFormat);

            WaveInProvider waveInProvider=new WaveInProvider (waveIn);

            waveOut=new DirectSoundOut ();
            waveOut.Init (waveInProvider);

            waveIn.StartRecording ();
            waveOut.Play ();
        }

        void OnDataAvailable (object sender, WaveInEventArgs e) {
            byte[] buffer=e.Buffer;
            int bytesRecorded=e.BytesRecorded;
            WriteToFile (buffer, bytesRecorded);

            for (int index=0; index<e.BytesRecorded; index+=2) {
                short sample=(short) ((buffer[index+1]<<8)|
                                        buffer[index+0]);
                float sample32=sample/32768f;
                sampleAggregator.Add (sample32);
            }
        }

        private void WriteToFile (byte[] buffer, int bytesRecorded) {
            long maxFileLength=this.recordingFormat.AverageBytesPerSecond*2;

            //if(recordingState==RecordingState.Recording
            //    ||recordingState==RecordingState.RequestedStop) {
            var toWrite=(int) Math.Min (maxFileLength-writer.Length, bytesRecorded);
            if (toWrite>0) {
                writer.Write (buffer, 0, bytesRecorded);
            } else {
                Stop ();
            }
            // }
        }

        public void Stop () {
            //if(recordingState==RecordingState.Recording) {
            //    recordingState=RecordingState.RequestedStop;
            waveIn.StopRecording ();
            //}
        }

        private void OnRecordingStopped (object sender, StoppedEventArgs e) {
            //recordingState=RecordingState.Stopped;
            writer.Dispose ();
            //  Stopped(this, EventArgs.Empty);
        }

        public AutoTuneSettings autoTuneSettings=new AutoTuneSettings ();
        private void Play_Click (object sender, RoutedEventArgs e) {
            WaveOut waveOut1=new WaveOut ();
            WaveStream source=new WaveFileReader (waveFileName);

            if (waveOut!=null) {
                waveOut.Stop ();
                waveOut.Dispose ();
                waveOut=null;
            }
            //if(waveIn!=null) {
            //    waveIn.StopRecording();
            //    waveIn.Dispose();
            //    waveIn=null;
            //}

            string tempFile = Path.Combine (Path.GetTempPath (), Guid.NewGuid ()+".wav");
            AutoTuneUtils.ApplyAutoTune (waveFileName, tempFile, autoTuneSettings);


            waveOut1.Init (source);
            waveOut1.Play ();
        }
    }
}
