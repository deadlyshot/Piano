using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using NAudio.Dsp;
using NAudio.CoreAudioApi;
using System.Threading;

namespace MicTest {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private MainWindowViewModel viewModel;

        SampleAggregator aggregator;

        public DispatcherTimer t = new DispatcherTimer();
        public event EventHandler<FftEventArgs> FftCalculated;
        private int fftlenght = 8192;

        public MainWindow() {
            InitializeComponent();
            viewModel = new MainWindowViewModel();
            this.DataContext = viewModel;
            //GenerateSpectogram();
        }

        private void GenerateSpectogram() {
            for (int i = 0; i < fftlenght / 2; i++) {
                ProgressBar newPb = new ProgressBar();
                newPb.Minimum = -80;
                newPb.Maximum = 0;
                spectogram.Children.Add(newPb);
            }
        }

        private void Record_Click(object sender, RoutedEventArgs e) {
            Console.WriteLine("Recording");
            viewModel.Record();
        }

        //private void InitAggregator() {
        //    aggregator = new SampleAggregator(waveProvider.ToSampleProvider());
        //    aggregator.NotificationCount = waveProvider.WaveFormat.SampleRate / 100;
        //    aggregator.PerformFFT = true;
        //    aggregator.FftCalculated += OnFftCalculated;
        //    waveOut.Init(aggregator);//TODO:DELETE 
        //}

        //protected virtual void OnFftCalculated(object sender,FftEventArgs e) {
        //    EventHandler<FftEventArgs> handler = FftCalculated;
        //    if (handler != null) handler(this, e);
        //    CalculateMagnitude(e.Result);
        //}

        //private void CalculateMagnitude(Complex[] complex) {
        //    List<double> magnitudes = new List<double>();
        //    for (int i = 1; i < complex.Length / 2; i += 1) {
        //        double magnitude = 20 * Math.Log10(Math.Sqrt(complex[i].X * complex[i].X + complex[i].Y * complex[i].Y));
        //        magnitudes.Add(magnitude);
        //        ProgressBar tempPb;
        //        tempPb = (ProgressBar)spectogram.Children[magnitudes.Count - 1];
        //        tempPb.Value = (int)Math.Round(magnitudes[magnitudes.Count - 1]);
        //    }
        //    CalculateMaxFreaquency(magnitudes);
        //}

        //private void CalculateMaxFreaquency(List<double> magnitudes) {
        //    double freaquency;
        //    freaquency = (magnitudes.IndexOf(magnitudes.Max()) * viewModel.SampleRate / 2) / (fftlenght / 2);
        //    textBox.AppendText("Magnitude:" + magnitudes.Max() + " Freaquency:" + freaquency + "\n");
        //    textBox.ScrollToEnd();
        //}
    }
}
