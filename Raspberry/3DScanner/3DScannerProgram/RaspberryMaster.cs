using System;
using System.Threading.Tasks;
using RaspberryPiDotNet;

namespace csTest
{
	public class RaspberryMaster
	{
		private PinStateListener _fotoResistorListener;
		private PWMHelper _pwmHelper;
	    private int _pwmPeriod = 10, _pwmHigh = 5;

		private int _counterLimit; 
		private int _counter ;

		public RaspberryMaster (int pwmPeriod, int pwmHigh, int counterLimit)
		{
		    _pwmPeriod = pwmPeriod;
		    _pwmHigh = pwmHigh;
		    _counterLimit = counterLimit;
            Init ();
		}

		private void Init() {
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
			if (_counter == _counterLimit) {
				_counter = 0;
				_pwmHelper.Stop ();
                Console.WriteLine("stopped 1");
			    Task.Run(async () =>
			    {
			        await Task.Delay(3000);
                    Console.WriteLine("started");
                    this.Start();
			    }).Start();
                Console.WriteLine("stopped 2");
            }
		}

		public void DoRotation(float angle){
			_counterLimit = (int)(angle * 5);
			_pwmHelper.Start ();
		}

	    public void Start()
	    {
            _fotoResistorListener.Start();
            _pwmHelper.Start();
        }

	    public void Stop()
	    {
	        _fotoResistorListener.Stop();
	        _pwmHelper.Stop();
	    }

	    public void StartPWM()
	    {
	        _counter = 0;
	        _pwmHelper.Start();
	    }
	}
}

