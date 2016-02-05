using System;
using System.Diagnostics;
using NAudio.Dsp;
using NAudio.Wave;

namespace MicTest {
    class SampleAggregator {
        // FFT
        private readonly Complex[] fftBuffer;
        private int fftBufferCurPossition;
        private readonly int fftBufferLength;
        private int m;

        private readonly int channels = 2;

        public SampleAggregator(int fftLength = 8192) {
            if (!IsPowerOfTwo(fftLength)) {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            this.m = (int)Math.Log(fftLength, 2.0);
            this.fftBufferLength = fftLength;
            this.fftBuffer = new Complex[fftLength];
        }

        private bool IsPowerOfTwo(int x) { return (x & (x - 1)) == 0; }

        public delegate void FFTCalculated(Complex[] fftBuffer);
        public void Read(ISampleProvider waveSampleProvider, FFTCalculated FFTCalculated) {
            float[] dummyFftArray = new float[fftBufferLength];
            var dummyFftArrayLength = waveSampleProvider.Read(dummyFftArray, 0, fftBufferLength);

            for (int i = 0; i < dummyFftArrayLength; i += channels) {
                fftBuffer[fftBufferCurPossition].X = (float)(dummyFftArray[i] * FastFourierTransform.HammingWindow(fftBufferCurPossition, fftBufferLength));
                fftBuffer[fftBufferCurPossition].Y = 0;
                fftBufferCurPossition++;
                if (fftBufferCurPossition >= fftBufferLength) {
                    fftBufferCurPossition = 0;
                    // 1024 = 2^10
                    FastFourierTransform.FFT(true, m, fftBuffer);
                    FFTCalculated(fftBuffer);
                }
            }
        }
    }
}
