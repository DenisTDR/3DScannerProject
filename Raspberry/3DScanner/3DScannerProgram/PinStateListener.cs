using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RaspberryPiDotNet;

namespace csTest
{
	public class PinStateListener
	{
		public delegate void StateChangedEventHandler(PinStateListener sender, PinStateChangedEventArgs e);

		public event StateChangedEventHandler StateChanged;
		public PinState PinState{ get; private set; }

		private GPIO _input;
		private bool _running = false;
		private Thread _thread;
		private long _counter = 0;

		public long ChangeCounter
		{
			get
			{
				var val = Interlocked.Read(ref _counter);
				return val;
			}
		}

		public long PosedgeCounter => ChangeCounter/2;

		public PinStateListener(GPIOPins pin)
		{
			Console.WriteLine("constructor");
			if (!SetPin(pin))
			{
				throw LastException;
			}
		}

		public void Start()
		{
			if (!_running)
			{
				Console.WriteLine("Starting ...");
				(_thread = new Thread(Loop)).Start();
			}
		}

		public void Stop()
		{
			_running = false;
		}

		public void ResetCounter()
		{
			Interlocked.Exchange(ref _counter, 0);
		}

		public bool SetPin(GPIOPins pin)
		{
			try
			{
				if (_running) return false;
				//                _input = GPIO.CreatePin(pin, GPIODirection.In);
				_input = new GPIOFile(pin, GPIODirection.In);
				return true;
			}
			catch (Exception exc)
			{
				Console.WriteLine("ceva a crapat: "+exc.Message);
				LastException = exc;
				return false;
			}
		}
		private void Loop()
		{
			var oldValue = _input.Read();
			_running = true;
			while (_running)
			{
				var newValue = _input.Read();
				if (newValue == oldValue) continue;
				Interlocked.Increment(ref _counter);
				if (StateChanged != null)
				{
					var oldTmp = oldValue;
					Task.Factory.StartNew(
						() =>
						{
							StateChanged(this,
								new PinStateChangedEventArgs(newValue, oldTmp, _input));
						});
				}
				oldValue = newValue;
				PinState = newValue;
				Thread.Sleep(1);
			}
		}
		public Exception LastException { get; private set; }
	}

	public class PinStateChangedEventArgs
	{
		public readonly PinState OldValue;
		public readonly PinState NewValue;
		public readonly GPIO Pin;

		public PinStateChangedEventArgs(PinState newValue, PinState oldValue, GPIO pin)
		{
			NewValue = newValue;
			OldValue = oldValue;
			Pin = pin;
		}
	}

}

