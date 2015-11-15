using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NAudio.Dsp;

namespace MicTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public DispatcherTimer t = new DispatcherTimer();
        WaveFormat recordingFormat;
        BufferedWaveProvider waveProvider;
        WaveIn waveIn = new WaveIn();
        WaveOut waveOut = new WaveOut();
        SampleAggregator aggregator;
        public event EventHandler<FftEventArgs> FftCalculated;

        public MainWindow() {
            InitializeComponent();
            GenerateSpectogram();
            InitializeWaveIn();
            Record();
        }

        private void GenerateSpectogram() {
            for (int i = 0; i < 1024; i++) {
                ProgressBar newPb = new ProgressBar();
                newPb.Minimum = -80;
                newPb.Maximum = 0;
                spectogram.Children.Add(newPb);
            }
        }

        private void InitializeWaveIn() {
            waveIn.BufferMilliseconds = 200;
            waveIn.DataAvailable += OnDataAvailable;
        }

        private void Record() {
            List<WaveInCapabilities> sources = new List<WaveInCapabilities>();
            for (int i = 0; i < WaveIn.DeviceCount; i++) { sources.Add(WaveIn.GetCapabilities(i)); }
            foreach (var source in sources) { comboBox.Items.Add(source.ProductName); }
        }

        private void Record_Click(object sender, RoutedEventArgs e) {
            Console.WriteLine("Recording");
            waveIn.StartRecording();
            waveOut.Play();// TODO:DELETE
        }

        void OnDataAvailable(object sender, WaveInEventArgs e) {
            waveProvider.AddSamples(e.Buffer, 0, e.Buffer.Length);

        }

        private void InitAggregator() {
            aggregator = new SampleAggregator(waveProvider.ToSampleProvider());
            aggregator.NotificationCount = waveProvider.WaveFormat.SampleRate / 100;
            aggregator.PerformFFT = true;
            aggregator.FftCalculated += (s, a) => OnFftCalculated(a);
            waveOut.Init(aggregator);//TODO:DELETE 
        }

        protected virtual void OnFftCalculated(FftEventArgs e) {
            EventHandler<FftEventArgs> handler = FftCalculated;
            if (handler != null) handler(this, e);
            CalculateFreaquency(e.Result);
        }

        private void CalculateFreaquency(Complex[] complex) {
            List<double> buffer = new List<double>();
            for (int i = 2; i < complex.Length / 2; i += 1) {
                buffer.Add(20 * Math.Log10(Math.Sqrt(complex[i].X * complex[i].X + complex[i].Y * complex[i].Y)) + Double.Epsilon);
                ProgressBar tempPb;
                tempPb = (ProgressBar)spectogram.Children[buffer.Count - 1];
                tempPb.Value = (int)Math.Round(buffer[buffer.Count - 1]);
            }

            double freaquency;
            freaquency = (buffer.IndexOf(buffer.Max()) * waveProvider.WaveFormat.SampleRate / 2) / (complex.Length / 2);
            Console.WriteLine(buffer.Max() + " " + freaquency);
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (comboBox.SelectedItem == null) return;
            ChangeInputDevice(comboBox.SelectedIndex);
            InitializeWaveProvider(comboBox.SelectedIndex);
            InitAggregator();
        }

        private void ChangeInputDevice(int deviceNumber) {
            waveIn.DeviceNumber = deviceNumber;
        }

        private void InitializeWaveProvider(int deviceNumber) {
            recordingFormat = new WaveFormat(44100, WaveIn.GetCapabilities(deviceNumber).Channels);
            waveIn.WaveFormat = recordingFormat;
            waveProvider = new BufferedWaveProvider(recordingFormat);
        }
    }
}
