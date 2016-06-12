﻿using System;
using System.Threading.Tasks;
using RaspberryPiDotNet;

namespace _3DScannerProgram
{
	public class RaspberryHardware
	{


        public event CompletedAngleEventListener CompletedAngle;

        private PinStateListener _fotoResistorListener;
		private PWMHelper _pwmHelper;
	    private int _pwmPeriod = 10, _pwmHigh = 5;

		private int _counterLimit;
	    private int _counter;

	    private bool _init = false;
	    private int _delayBetweenScans;

	    public RaspberryHardware(int pwmPeriod, int pwmHigh, int counterLimit, int delayBetweenScans = 0)
	    {
	        SetVariables(pwmPeriod, pwmHigh, counterLimit, delayBetweenScans);
	    }

	    public void SetVariables(int pwmPeriod, int pwmHigh, int counterLimit, int delayBetweenScans = 0)
        {
            _pwmPeriod = pwmPeriod;
            _pwmHigh = pwmHigh;
            _counterLimit = counterLimit;
            _delayBetweenScans = delayBetweenScans;
        }

		private void Init()
		{
		    if (_init) return;
		    _init = true;
            _fotoResistorListener = new PinStateListener(GPIOPins.GPIO_23);
			_pwmHelper = new PWMHelper (GPIOPins.GPIO_18, _pwmPeriod, _pwmHigh);
            _fotoResistorListener.StateChanged += PinStateChangedHandler;
		}

		private void PinStateChangedHandler(PinStateListener sender, PinStateChangedEventArgs e) {
			//if (e.NewValue == PinState.High) {
				CounterChanged ();
			//}
		    Console.WriteLine("FotoResistor Value: " + e.NewValue);
		}

		private void CounterChanged()
		{
		    _counter ++;
			if (_counter == _counterLimit)
            {
                _pwmHelper.Stop();
                CompletedAngle?.Invoke(this, _counter);
                _counter = 0;
                Console.WriteLine("stopped 1");
			    if (_delayBetweenScans != 0)
			    {
			        Task.Run(async () =>
			        {
			            await Task.Delay(_delayBetweenScans);
			            Console.WriteLine("started");
			            this.Start();
			        });
			    }
			}
		}

		public void DoRotation(float angle){
			_counterLimit = (int)(angle * 5);
			_pwmHelper.Start ();
		}

	    public void Start()
	    {
            Init();
            _fotoResistorListener.Start();
            _pwmHelper.Start();
        }

	    public void Stop()
	    {
	        _fotoResistorListener?.Stop();
	        _pwmHelper?.Stop();
	    }

	    public void StartPWM()
	    {
	        _counter = 0;
	        _pwmHelper.Start();
	    }
    }
    public delegate void CompletedAngleEventListener(RaspberryHardware sender, int counter);
}

