using System;
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

	    public RaspberryHardware(int pwmPeriod, int pwmHigh, int counterLimit)
	    {
	        SetVariables(pwmPeriod, pwmHigh, counterLimit);
	    }

	    public void SetVariables(int pwmPeriod, int pwmHigh, int counterLimit)
        {
            _pwmPeriod = pwmPeriod;
            _pwmHigh = pwmHigh;
            _counterLimit = counterLimit;
        }

		private void Init()
		{
		    if (_init) return;
		    _init = true;
            _fotoResistorListener = new PinStateListener(GPIOPins.GPIO_23);
			_pwmHelper = new PWMHelper (GPIOPins.GPIO_18, _pwmPeriod, _pwmHigh);
            _fotoResistorListener.StateChanged += PinStateChangedHandler;
		}

		private void PinStateChangedHandler(PinStateListener sender, PinStateChangedEventArgs e)
        {
            Console.WriteLine("FotoResistor Value: " + e.NewValue);
            //if (e.NewValue == PinState.High) {
            CounterChanged ();
			//}
		}

		private void CounterChanged()
		{
		    _counter ++;
			if (_counter == _counterLimit)
            {
                _pwmHelper.Stop();
                CompletedAngle?.Invoke(this, _counter);
                _counter = 0;
			   
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
           // _pwmHelper.Start();
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

