﻿using System;
using UnityEngine;
namespace cylvester
{

    public interface IPdBackend
    {
        IPdArrayContainer SpectrumArrayContainer{ get; }
        IPdArrayContainer WaveformArrayContainer{ get; }
    }
    
    public class PdBackend : MonoBehaviour, IPdBackend
    {
        [SerializeField] UnityControlEvent onControlMessageReceived = null;
        
        public int samplePlayback;

        private IChangeObserver<int> samplePlaybackObserver_;
        
        private IPdSender pdSender_;
        private IPdReceiver pdReceiver_;
        private IMidiParser midiParser_;
        private IDspController dspController_;
        
        public IPdArrayContainer SpectrumArrayContainer { get; private set; }
        private IUpdater spectrumArrayUpdater_;
        public IPdArrayContainer WaveformArrayContainer { get; private set; }
        private IUpdater waveformArrayUpdater_;
        
        private Action onSamplePlaybackChanged_;
        private Action<ControlMessage> onControlMessageReceived_;

        private void Awake()
        {
            SpectrumArrayContainer = new PdArrayContainer("fft_");
            WaveformArrayContainer = new PdArrayContainer("wave_");
            
            spectrumArrayUpdater_ = (IUpdater) SpectrumArrayContainer;
            waveformArrayUpdater_ = (IUpdater) WaveformArrayContainer;

            pdSender_ = new PdSender(PdConstant.ip, PdConstant.sendPort);
            pdReceiver_ = new PdReceiver(PdConstant.receivedPort);
            midiParser_ = new MidiParser(pdReceiver_);
            
            dspController_ = new DspController(pdSender_);

            samplePlaybackObserver_ = new ChangeObserver<int>(samplePlayback);

            onSamplePlaybackChanged_ = () =>
            {
                pdSender_.Send(new[]{(byte)PdMessage.SampleSound, (byte)samplePlayback});
            };

            onControlMessageReceived_ = (message) =>
            {
                onControlMessageReceived.Invoke(message);
            };
            
            samplePlaybackObserver_.ValueChanged += onSamplePlaybackChanged_;
            midiParser_.ControlMessageReceived += onControlMessageReceived_;

            dspController_.State = true;
        }
        
        private void OnDestroy()
        {
            dspController_.State = false;
            pdSender_?.Dispose();
            samplePlaybackObserver_.ValueChanged -= onSamplePlaybackChanged_;
            midiParser_.ControlMessageReceived -= onControlMessageReceived_;
        }

        public void Update()
        {
            pdReceiver_.Update();
            spectrumArrayUpdater_.Update();
            waveformArrayUpdater_.Update();
            samplePlaybackObserver_.Value = samplePlayback;
        }
    }
}