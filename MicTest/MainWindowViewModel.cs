using NAudio.CoreAudioApi;
using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MicTest {
    class MainWindowViewModel : ViewModelBase {
        private MMDevice selectedDevice;
        private int sampleRate;
        private int bitDepth;
        private int channelCount;
        private int sampleTypeIndex;
        //private WasapiCapture capture;
        private WaveIn waveIn;
        private readonly SynchronizationContext synchronizationContext;
        private float peak;
        public event EventHandler<FftEventArgs> FftCalculated;

        WaveFormat recordingFormat;
        BufferedWaveProvider waveProvider;
        SampleAggregator aggregator;

        public MainWindowViewModel() {
            synchronizationContext = SynchronizationContext.Current;
            var enumerator = new MMDeviceEnumerator();
            CaptureDevices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToArray();
            var defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            SelectedDevice = CaptureDevices.FirstOrDefault(c => c.ID == defaultDevice.ID);
        }

        public void Record() {
            //capture = new WasapiCapture(selectedDevice);
            //capture.StartRecording();
            //capture.DataAvailable += CaptureOnDataAvabile;
            waveIn = new WaveIn();
            waveIn.BufferMilliseconds = 200;
            waveIn.DataAvailable += CaptureOnDataAvabile;
            InitAggregator();
            waveIn.StartRecording();
        }

        private void CaptureOnDataAvabile(object sender, WaveInEventArgs e) {
            waveProvider.AddSamples(e.Buffer,0,e.Buffer.Length);
            UpdatePeakMeter();
        }

        private void InitAggregator() {
            aggregator = new SampleAggregator(waveProvider.ToSampleProvider());
            aggregator.PerformFFT = true;
            aggregator.FftCalculated += OnFftCalculated;
           // waveOut.Init(aggregator);//TODO:DELETE 
        }

        protected virtual void OnFftCalculated(object sender, FftEventArgs e) {
            EventHandler<FftEventArgs> handler = FftCalculated;
            if (handler != null) handler(this, e);
            CalculateMagnitude(e.Result);
        }

        private void CalculateMagnitude(Complex[] complex) {
            List<double> magnitudes = new List<double>();
            for (int i = 1; i < complex.Length / 2; i += 1) {
                double magnitude = 20 * Math.Log10(Math.Sqrt(complex[i].X * complex[i].X + complex[i].Y * complex[i].Y));
                magnitudes.Add(magnitude);
                //ProgressBar tempPb;
                //tempPb = (ProgressBar)spectogram.Children[magnitudes.Count - 1];
                //tempPb.Value = (int)Math.Round(magnitudes[magnitudes.Count - 1]);
            }
            CalculateMaxFreaquency(magnitudes);
        }

        private void CalculateMaxFreaquency(List<double> magnitudes) {
            double freaquency;
            freaquency = (magnitudes.IndexOf(magnitudes.Max()) * SampleRate / 2) / (8192 / 2);
            //textBox.AppendText("Magnitude:" + magnitudes.Max() + " Freaquency:" + freaquency + "\n");
            //textBox.ScrollToEnd();
            Console.WriteLine("Magnitude:" + magnitudes.Max() + " Freaquency:" + freaquency + "\n");
        }

        void UpdatePeakMeter() {
            // can't access this on a different thread from the one it was created on, so get back to GUI thread
            synchronizationContext.Post(s => Peak = SelectedDevice.AudioMeterInformation.MasterPeakValue, null);
        }

        public IEnumerable<MMDevice> CaptureDevices { get; private set; }

        public float Peak {
            get { return peak; }
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (peak != value) {
                    peak = value;
                    OnPropertyChanged("Peak");
                }
            }
        }

        public BufferedWaveProvider WaveProvider {
            get { return waveProvider; }
        }

        public MMDevice SelectedDevice {
            get { return selectedDevice; }
            set {
                if (selectedDevice != value) {
                    selectedDevice = value;
                    OnPropertyChanged("SelectedDevice");
                    GetDefaultRecordingFormat(value);
                }
            }
        }

        private void GetDefaultRecordingFormat(MMDevice value) {
            using (var c = new WasapiCapture(value)) {
                SampleTypeIndex = c.WaveFormat.Encoding == WaveFormatEncoding.IeeeFloat ? 0 : 1;
                SampleRate = c.WaveFormat.SampleRate;
                BitDepth = c.WaveFormat.BitsPerSample;
                ChannelCount = c.WaveFormat.Channels;
                //Message = "";
                recordingFormat = new WaveFormat(SampleRate, ChannelCount);
                waveProvider = new BufferedWaveProvider(recordingFormat);
            }
        }
        public int SampleTypeIndex {
            get { return sampleTypeIndex; }
            set {
                if (sampleTypeIndex != value) {
                    sampleTypeIndex = value;
                    OnPropertyChanged("SampleTypeIndex");
                    BitDepth = sampleTypeIndex == 1 ? 16 : 32;
                    OnPropertyChanged("IsBitDepthConfigurable");
                }
            }
        }

        public int SampleRate {
            get {
                return sampleRate;
            }
            set {
                if (sampleRate != value) {
                    sampleRate = value;
                    OnPropertyChanged("SampleRate");
                }
            }
        }

        public int BitDepth {
            get { return bitDepth; }
            set {
                if (bitDepth != value) {
                    bitDepth = value;
                    OnPropertyChanged("BitDepth");
                }
            }
        }

        public int ChannelCount {
            get { return channelCount; }
            set {
                if (channelCount != value) {
                    channelCount = value;
                    OnPropertyChanged("ChannelCount");
                }
            }
        }
    }
}
