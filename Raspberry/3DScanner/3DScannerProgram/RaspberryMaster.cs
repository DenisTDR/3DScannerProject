using System;
using RaspberryPiDotNet;

namespace csTest
{
	public class RaspberryMaster
	{
		private PinStateListener _pin1Listener;
		private PinStateListener _pin2Listener;
		private PWMHelper _pwmHelper;

		private int _counterLimit = 12; 
		private int _counter = 0;

		public RaspberryMaster ()
		{
			Init ();
		}

		private void Init() {
			_pin1Listener = new PinStateListener(GPIOPins.GPIO_00);
			_pin2Listener = new PinStateListener(GPIOPins.GPIO_00);
			_pwmHelper = new PWMHelper (GPIOPins.GPIO_00, 10, 5);

			_pin1Listener.StateChanged += PinStateChangedHandler;

			_pin1Listener.Start ();
			_pin2Listener.Start ();
		}

		private void PinStateChangedHandler(PinStateListener sender, PinStateChangedEventArgs e) {
			if (e.NewValue == PinState.High) {
				if (_pin2Listener.PinState == PinState.High) {
					_counter++;
				} else {
					_counter--;
				}
				CounterChanged ();
			}
		}

		private void CounterChanged() {
			if (_counter == _counterLimit) {
				_counter = 0;
				_pwmHelper.Stop ();
			}
		}

		public void DoRotation(float angle){
			_counterLimit = (int)(angle * 5);
			_pwmHelper.Start ();
		}
	}
}

