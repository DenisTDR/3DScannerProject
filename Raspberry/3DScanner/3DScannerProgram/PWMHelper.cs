using System;
using System.Diagnostics;
using System.Threading;
using RaspberryPiDotNet;
namespace csTest
{
	public class PWMHelper
	{
		private bool _working = false;
		private GPIOFile _pinObject;
		public GPIOPins Pin { get; private set;}
		public int Period { get; private set; }
		public int HighDuration { get; private set; }
		public int LowDuration => Period - HighDuration;

		public PinState DefaultValue = PinState.Low;
			
		public PWMHelper (GPIOPins pin, int period, int highDuration)
		{
			Pin = pin;
			if (highDuration > period) {
				throw new Exception ("highDuration > Period");
			}
			HighDuration = highDuration;
			Period = period;
		}

		public bool Start() {
			if (_working) {
				return false;
			}
			if (_pinObject != null) {
				if (_pinObject.Pin != Pin) {
					if (!_pinObject.IsDisposed) {
						_pinObject.Dispose ();
					}
					_pinObject = new GPIOFile (Pin);
				}
			}

			_working = true;
			new Thread (LoopMethod).Start();
			return true;
		}

		public void Stop() {
			if (_working) {
				_working = false;

			}
		}

		public void LoopMethod() {
			int lowDuration = LowDuration;

			while (_working) {
				SetPinState (true);
				Thread.Sleep (HighDuration);
				SetPinState (false);
				Thread.Sleep (lowDuration);
			}
			SetPinState (DefaultValue);
		}

		private void SetPinState(bool state){
			if (_pinObject != null && !_pinObject.IsDisposed) {
				_pinObject.Write(state);
			}
		}


		private void SetPinState(PinState state){
			SetPinState(state == PinState.High);
		}
	}
}

