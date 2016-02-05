using System;
using System.Diagnostics;
using NAudio.Dsp;
using NAudio.Wave;

namespace MicTest {
    class SampleAggregator : ISampleProvider {
        // FFT
        public event EventHandler<FftEventArgs> FftCalculated;
        public bool PerformFFT { get; set; }
        private readonly Complex[] fftBuffer;
        private readonly FftEventArgs fftArgs;
        private int fftPos;
        private readonly int fftLength;
        private int m;
        private readonly ISampleProvider source;

        private readonly int channels;

        public SampleAggregator(ISampleProvider source, int fftLength=8192) {
            channels=source.WaveFormat.Channels;
            if(!IsPowerOfTwo(fftLength)) {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            this.m=(int) Math.Log(fftLength, 2.0);
            this.fftLength=fftLength;
            this.fftBuffer=new Complex[fftLength];
            this.fftArgs=new FftEventArgs(fftBuffer);
            this.source=source;
        }

        bool IsPowerOfTwo(int x) {
            return ( x&( x-1 ) )==0;
        }

        private void Add(float value) {
            if(PerformFFT&&FftCalculated!=null) {
                fftBuffer[fftPos].X=(float) ( value*FastFourierTransform.HammingWindow(fftPos, fftLength) );
                fftBuffer[fftPos].Y=0;
                fftPos++;
                if(fftPos>=fftBuffer.Length) {
                    fftPos=0;
                    // 1024 = 2^10
                    FastFourierTransform.FFT(true, m, fftBuffer);
                    FftCalculated(this, fftArgs);
                }
            }
        }

        public WaveFormat WaveFormat { get { return source.WaveFormat; } }

        public int Read(float[] buffer, int offset, int count) {
            var samplesRead=source.Read(buffer, offset, count);

            for(int n=0 ; n<samplesRead ; n+=channels) {
                Add(buffer[n+offset]);
            }
            return samplesRead;
        }
    }

    public class FftEventArgs : EventArgs {
        [DebuggerStepThrough]
        public FftEventArgs(Complex[] result) {
            this.Result=result;
        }
        public Complex[] Result { get; private set; }
    }
}
