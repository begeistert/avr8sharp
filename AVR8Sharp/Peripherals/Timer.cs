using AVR8Sharp.Cpu;
namespace AVR8Sharp.Peripherals;

public class AvrTimer
{
	// Force Output Compare (FOC) bits
	const int FOCA = 1 << 7;
	const int FOCB = 1 << 6;
	const int FOCC = 1 << 5;
	
	const int TopOCRA = 1;
	const int TopICR = 2;
	
	const int OCToggle = 1;
	
	public static int[] Timer01Dividers = new[] {
		0,
		1,
		8,
		64,
		256,
		1024,
		0, // External clock - see ExternalClockMode
		0, // Ditto
	};
	public static AvrTimerConfig DefaultTimerBits = new AvrTimerConfig () {
		// TIFR bits
		TOV = 1,
		OCFA = 2,
		OCFB = 4,
		OCFC = 0, // Unused
		
		// TIMSK bits
		TOIE = 1,
		OCIEA = 2,
		OCIEB = 4,
		OCIEC = 0, // Unused
	};
	public static AvrTimerConfig Timer0Config = new AvrTimerConfig () {
		Bits = 8,
		Dividers = Timer01Dividers,
		CaptureInterrupt = 0, // Not Available
		ComparatorAInterrupt = 0x1c,
		ComparatorBInterrupt = 0x1e,
		ComparatorCInterrupt = 0,
		OverflowInterrupt = 0x20,
		TIFR = 0x35,
		OCRA = 0x47,
		OCRB = 0x48,
		OCRC = 0, // Not Available
		ICR = 0, // Not Available
		TCNT = 0x46,
		TCCRA = 0x44,
		TCCRB = 0x45,
		TCCRC = 0, // Not Available
		TIMSK = 0x6e,
		ComparatorPortA = AvrIoPort.PortDConfig.PORT,
		ComparatorPinA = 6,
		ComparatorPortB = AvrIoPort.PortDConfig.PORT,
		ComparatorPinB = 5,
		ComparatorPortC = 0, // Not Available
		ComparatorPinC = 0, 
		ExternalClockPort = AvrIoPort.PortDConfig.PORT,
		ExternalClockPin = 4,
		// Apply default bits
		TOV = DefaultTimerBits.TOV,
		OCFA = DefaultTimerBits.OCFA,
		OCFB = DefaultTimerBits.OCFB,
		OCFC = DefaultTimerBits.OCFC,
		TOIE = DefaultTimerBits.TOIE,
		OCIEA = DefaultTimerBits.OCIEA,
		OCIEB = DefaultTimerBits.OCIEB,
		OCIEC = DefaultTimerBits.OCIEC,
	};
	public static AvrTimerConfig Timer1Config = new AvrTimerConfig () {
		Bits = 16,
		Dividers = Timer01Dividers,
		CaptureInterrupt = 0x14,
		ComparatorAInterrupt = 0x16,
		ComparatorBInterrupt = 0x18,
		ComparatorCInterrupt = 0,
		OverflowInterrupt = 0x1a,
		TIFR = 0x36,
		OCRA = 0x88,
		OCRB = 0x8a,
		OCRC = 0, // Not Available
		ICR = 0x86,
		TCNT = 0x84,
		TCCRA = 0x80,
		TCCRB = 0x81,
		TCCRC = 0x82,
		TIMSK = 0x6f,
		ComparatorPortA = AvrIoPort.PortBConfig.PORT,
		ComparatorPinA = 1,
		ComparatorPortB = AvrIoPort.PortBConfig.PORT,
		ComparatorPinB = 2,
		ComparatorPortC = 0, // Not Available
		ComparatorPinC = 0, 
		ExternalClockPort = AvrIoPort.PortDConfig.PORT,
		ExternalClockPin = 5,
		// Apply default bits
		TOV = DefaultTimerBits.TOV,
		OCFA = DefaultTimerBits.OCFA,
		OCFB = DefaultTimerBits.OCFB,
		OCFC = DefaultTimerBits.OCFC,
		TOIE = DefaultTimerBits.TOIE,
		OCIEA = DefaultTimerBits.OCIEA,
		OCIEB = DefaultTimerBits.OCIEB,
		OCIEC = DefaultTimerBits.OCIEC,
	};
	public static AvrTimerConfig Timer2Config = new AvrTimerConfig () {
		Bits = 8,
		Dividers = [
			0,
			1,
			8,
			32,
			64,
			128,
			256,
			1024
		],
		CaptureInterrupt = 0, // Not Available
		ComparatorAInterrupt = 0x0e,
		ComparatorBInterrupt = 0x10,
		ComparatorCInterrupt = 0,
		OverflowInterrupt = 0x12,
		TIFR = 0x37,
		OCRA = 0xb3,
		OCRB = 0xb4,
		OCRC = 0, // Not Available
		ICR = 0, // Not Available
		TCNT = 0xb2,
		TCCRA = 0xb0,
		TCCRB = 0xb1,
		TCCRC = 0, // Not Available
		TIMSK = 0x70,
		ComparatorPortA = AvrIoPort.PortBConfig.PORT,
		ComparatorPinA = 3,
		ComparatorPortB = AvrIoPort.PortDConfig.PORT,
		ComparatorPinB = 3,
		ComparatorPortC = 0, // Not Available
		ComparatorPinC = 0, 
		ExternalClockPort = 0, // Not Available
		ExternalClockPin = 0,
		// Apply default bits
		TOV = DefaultTimerBits.TOV,
		OCFA = DefaultTimerBits.OCFA,
		OCFB = DefaultTimerBits.OCFB,
		OCFC = DefaultTimerBits.OCFC,
		TOIE = DefaultTimerBits.TOIE,
		OCIEA = DefaultTimerBits.OCIEA,
		OCIEB = DefaultTimerBits.OCIEB,
		OCIEC = DefaultTimerBits.OCIEC,
	};
	public static WgmConfig[] WgmModes8Bit = [
		new WgmConfig { Mode = TimerMode.Normal, TimerTopValue = 0xff, OCRUpdateMode = OCRUpdateMode.Immediate, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseCorrect, TimerTopValue = 0xff, OCRUpdateMode = OCRUpdateMode.Top, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = 0, },
		new WgmConfig { Mode = TimerMode.CTC, TimerTopValue = TopOCRA, OCRUpdateMode = OCRUpdateMode.Immediate, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.FastPWM, TimerTopValue = 0xff, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseCorrect, TimerTopValue = TopOCRA, OCRUpdateMode = OCRUpdateMode.Top, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = OCToggle, },
		new WgmConfig { Mode = TimerMode.Reserved, TimerTopValue = 0xff, OCRUpdateMode = OCRUpdateMode.Immediate, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.FastPWM, TimerTopValue = TopOCRA, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Top, Flags = OCToggle, },
	];
	public static WgmConfig[] WgmModes16Bits = [
		new WgmConfig { Mode = TimerMode.Normal, TimerTopValue = 0xffff, OCRUpdateMode = OCRUpdateMode.Immediate, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseCorrect, TimerTopValue = 0x00ff, OCRUpdateMode = OCRUpdateMode.Top, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseCorrect, TimerTopValue = 0x01ff, OCRUpdateMode = OCRUpdateMode.Top, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseCorrect, TimerTopValue = 0x03ff, OCRUpdateMode = OCRUpdateMode.Top, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = 0, },
		new WgmConfig { Mode = TimerMode.CTC, TimerTopValue = TopOCRA, OCRUpdateMode = OCRUpdateMode.Immediate, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.FastPWM, TimerTopValue = 0x00ff, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Top, Flags = 0, },
		new WgmConfig { Mode = TimerMode.FastPWM, TimerTopValue = 0x01ff, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Top, Flags = 0, },
		new WgmConfig { Mode = TimerMode.FastPWM, TimerTopValue = 0x03ff, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Top, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseFrequencyCorrect, TimerTopValue = TopICR, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseFrequencyCorrect, TimerTopValue = TopOCRA, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = OCToggle, },
		new WgmConfig { Mode = TimerMode.PWMPhaseCorrect, TimerTopValue = TopICR, OCRUpdateMode = OCRUpdateMode.Top, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = 0, },
		new WgmConfig { Mode = TimerMode.PWMPhaseCorrect, TimerTopValue = TopOCRA, OCRUpdateMode = OCRUpdateMode.Top, TOVUpdateMode = TOVUpdateMode.Bottom, Flags = OCToggle, },
		new WgmConfig { Mode = TimerMode.CTC, TimerTopValue = TopICR, OCRUpdateMode = OCRUpdateMode.Immediate, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.Reserved, TimerTopValue = 0xffff, OCRUpdateMode = OCRUpdateMode.Immediate, TOVUpdateMode = TOVUpdateMode.Max, Flags = 0, },
		new WgmConfig { Mode = TimerMode.FastPWM, TimerTopValue = TopICR, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Top, Flags = OCToggle, },
		new WgmConfig { Mode = TimerMode.FastPWM, TimerTopValue = TopOCRA, OCRUpdateMode = OCRUpdateMode.Bottom, TOVUpdateMode = TOVUpdateMode.Top, Flags = OCToggle, },
	];
	
	private Cpu.Cpu _cpu;
	private AvrTimerConfig _config;

	private readonly int _max;
	private int _lastCycle = 0;
	private ushort _ocrA = 0;
	private ushort _nextOcrA = 0;
	private ushort _ocrB = 0;
	private ushort _nextOcrB = 0;
	private bool _hasOcrC;
	private ushort _ocrC = 0;
	private ushort _nextOcrC = 0;
	private OCRUpdateMode _ocrUpdateMode = OCRUpdateMode.Immediate;
	private TOVUpdateMode _tovUpdateMode = TOVUpdateMode.Max;
	private ushort _icr = 0; // Only for 16-bit timers
	private TimerMode _timerMode;
	private int _topValue;
	private ushort _tcnt = 0;
	private ushort _tcntNext = 0;
	private byte _compA = 0;
	private byte _compB = 0;
	private byte _compC = 0;
	private bool _tcntUpdated = false;
	private bool _updateDivider = false;
	private bool _countingUp = true;
	private int _divider = 0;
	private AvrIoPort? _externalClockPort;
	private bool _externalClockRisingEdge = false;
	
	// This is the temporary register used to access 16-bit registers (section 16.3 of the datasheet)
	private byte _highByteTemp = 0;
	
	// Interrupts
	private AvrInterruptConfig _ovf;
	private AvrInterruptConfig _ocfa;
	private AvrInterruptConfig _ocfb;
	private AvrInterruptConfig _ocfc;

	public byte TCCRA {
		get {
			return _cpu.Data[_config.TCCRA];
		}
	}
	public byte TCCRB {
		get {
			return _cpu.Data[_config.TCCRB];
		}
	}
	public byte TIMSK {
		get {
			return _cpu.Data[_config.TIMSK];
		}
	}
	public int CS {
		get {
			return TCCRB & 0x7;
		}
	}
	public int WGM {
		get {
			var mask = _config.Bits == 16 ? 0x18 : 0x8;
			return ((TCCRB & mask) >> 1) | (TCCRA & 0x3);
		}
	}
	public int TOP {
		get {
			switch (_topValue) {
				case TopOCRA:
					return _ocrA;
				case TopICR:
					return _icr;
				default:
					return _topValue;
			}
		}
	}
	public int OcrMask {
		get {
			switch (_topValue) {
				case TopOCRA:
				case TopICR:
					return 0xffff;
				default:
					return _topValue;
			}
		}
	}
	public int DebugTCNT {
		get {
			return _tcnt;
		}
	}

	public AvrTimer (Cpu.Cpu cpu, AvrTimerConfig config)
	{
		_cpu = cpu;
		_config = config;
		
		_max =config.Bits == 16 ? 0xffff : 0xff;
		_hasOcrC = config.OCRC != 0;
		
		_ovf = new AvrInterruptConfig {
			Address = config.OverflowInterrupt,
			EnableRegister = config.TIMSK,
			EnableMask = config.TOIE,
			FlagRegister = config.TIFR,
			FlagMask = config.TOV,
		};
		
		_ocfa = new AvrInterruptConfig {
			Address = config.ComparatorAInterrupt,
			EnableRegister = config.TIMSK,
			EnableMask = config.OCIEA,
			FlagRegister = config.TIFR,
			FlagMask = config.OCFA,
		};
		
		_ocfb = new AvrInterruptConfig {
			Address = config.ComparatorBInterrupt,
			EnableRegister = config.TIMSK,
			EnableMask = config.OCIEB,
			FlagRegister = config.TIFR,
			FlagMask = config.OCFB,
		};
		
		_ocfc = new AvrInterruptConfig {
			Address = config.ComparatorCInterrupt,
			EnableRegister = config.TIMSK,
			EnableMask = config.OCIEC,
			FlagRegister = config.TIFR,
			FlagMask = config.OCFC,
		};

		UpdateWgmConfig ();
	}

	private void UpdateWgmConfig ()
	{
		var wgmModes = _config.Bits == 16 ? WgmModes16Bits : WgmModes8Bit;
		var tccra = _cpu.Data[_config.TCCRA];
		var wgmConfig = wgmModes[WGM];
		_timerMode = wgmConfig.Mode;
		_topValue = wgmConfig.TimerTopValue;
		_ocrUpdateMode = wgmConfig.OCRUpdateMode;
		_tovUpdateMode = wgmConfig.TOVUpdateMode;
		var flags = wgmConfig.Flags;
		
		var pwmMode = _timerMode == TimerMode.FastPWM ||
			_timerMode == TimerMode.PWMPhaseCorrect ||
			_timerMode == TimerMode.PWMPhaseFrequencyCorrect;
		
		var prevCompA = _compA;
		_compA = (byte)((TCCRA >> 6) & 0x3);
		if (_compA == 1 && pwmMode && (flags & OCToggle) == 0) {
			_compA = 0;
		}
		// TODO: Check if this is correct
		// if (!!prevCompA !== !!this.compA) {
		if (prevCompA != _compA) {
			UpdateCompA (_compA != 0 ? PinOverrideMode.Enable : PinOverrideMode.None);
		}
		
		var prevCompB = _compB;
		_compB = (byte)((TCCRA >> 4) & 0x3);
		if (_compB == 1 && pwmMode) {
			_compB = 0; // Reserved, according to the datasheet
		}
		if (prevCompB != _compB) {
			UpdateCompB (_compB != 0 ? PinOverrideMode.Enable : PinOverrideMode.None);
		}

		if (!_hasOcrC) return;
		var prevCompC = _compC;
		_compC = (byte)((TCCRA >> 2) & 0x3);
		if (_compC == 1 && pwmMode) {
			_compC = 0; // Reserved, according to the datasheet
		}
		if (prevCompC != _compC) {
			UpdateCompC (_compC != 0 ? PinOverrideMode.Enable : PinOverrideMode.None);
		}
	}

	// original count function
	public void Count (bool reschedule = true, bool external = false)
	{
		var delta = _cpu.Cycles - _lastCycle;
		
		if (_divider != 0 && delta >= _divider || external) {
			var counterDelta = external ? 1 : delta / _divider;
			_lastCycle += counterDelta * _divider;
			var val = _tcnt;
			var phasePwm = _timerMode == TimerMode.PWMPhaseCorrect || _timerMode == TimerMode.PWMPhaseFrequencyCorrect;
			var newVal = phasePwm ? PhasePwmCount (val, (byte)counterDelta) : (val + counterDelta) % (TOP + 1);
			var overflow = val + counterDelta > TOP;
			// A CPU write overrides (has priority over) all counter clear or count operations.
			if (!_tcntUpdated) {
				_tcnt = (ushort)newVal;
				if (!phasePwm) {
					TimerUpdated (newVal, val);
				}
			}

			if (!phasePwm) {
				if (_timerMode == TimerMode.FastPWM && overflow) {
					if (_compA != 0) {
						UpdateCompPin (_compA, 'A', true);
					}
					if (_compB != 0) {
						UpdateCompPin (_compB, 'B', true);
					}
				}
				if (_ocrUpdateMode == OCRUpdateMode.Bottom && overflow) {
					// OCRUpdateMode.Top only occurs in Phase Correct modes, handled by phasePwmCount()
					_ocrA = _nextOcrA;
					_ocrB = _nextOcrB;
					_ocrC = _nextOcrC;
				}
				
				// OCRUpdateMode.Bottom only occurs in Phase Correct modes, handled by phasePwmCount().
				// Thus we only handle TOVUpdateMode.Top or TOVUpdateMode.Max here.
				if (overflow && (_tovUpdateMode == TOVUpdateMode.Top || TOP == _max)) {
					_cpu.SetInterruptFlag (_ovf);
				}
			}
		}

		if (_tcntUpdated) {
			_tcnt = _tcntNext;
			_tcntUpdated = false;
			if (_tcnt == 0 && _ocrUpdateMode == OCRUpdateMode.Bottom || _tcnt == TOP && _ocrUpdateMode == OCRUpdateMode.Top) {
				_ocrA = _nextOcrA;
				_ocrB = _nextOcrB;
				_ocrC = _nextOcrC;
			}
		}

		if (_updateDivider) {
			var newDivider = _config.Dividers[CS];
			_lastCycle = newDivider != 0 ? _cpu.Cycles : 0;
			_updateDivider = false;
			_divider = newDivider;
			if (_config.ExternalClockPort != 0 && _externalClockPort == null) {
				_externalClockPort = _cpu.GpioByPort[_config.ExternalClockPort];
			}
			if (_externalClockPort != null) {
				_externalClockPort.ExternalClockListeners[_config.ExternalClockPin] = null;
			}
			if (newDivider != 0) {
				_cpu.AddClockEvent (() => Count(), _lastCycle + newDivider - _cpu.Cycles);
			} else if (_externalClockPort != null && (CS == (int)ExternalClockMode.FallingEdge || CS == (int)ExternalClockMode.RisingEdge)) {
				_externalClockPort.ExternalClockListeners[_config.ExternalClockPin] = ExternalClockCallback;
				_externalClockRisingEdge = CS == (int)ExternalClockMode.RisingEdge;
			}
		}
		
		if (reschedule && _divider != 0) {
			_cpu.AddClockEvent (() => Count(), _lastCycle + _divider - _cpu.Cycles);
		}
	}
	
	private void ExternalClockCallback (bool value)
	{
		if (value == _externalClockRisingEdge) {
			Count (false, true);
		}
	}

	private int PhasePwmCount (ushort value, byte delta)
	{
		if (value == 0 && TOP == 0) {
			delta = 0;
			if (_ocrUpdateMode == OCRUpdateMode.Top) {
				_ocrA = _nextOcrA;
				_ocrB = _nextOcrB;
				_ocrC = _nextOcrC;
			}
		}

		while (delta > 0) {
			if (_countingUp) {
				value++;
				if (value == TOP && !_tcntUpdated) {
					_countingUp = false;
					if (_ocrUpdateMode == OCRUpdateMode.Top) {
						_ocrA = _nextOcrA;
						_ocrB = _nextOcrB;
						_ocrC = _nextOcrC;
					}
				}
			} else {
				value--;
				if (value == 0 && !_tcntUpdated) {
					_countingUp = true;
					_cpu.SetInterruptFlag (_ovf);
					if (_ocrUpdateMode == OCRUpdateMode.Bottom) {
						_ocrA = _nextOcrA;
						_ocrB = _nextOcrB;
						_ocrC = _nextOcrC;
					}
				}
			}
			if (!_tcntUpdated) {
				if (value == _ocrA) {
					_cpu.SetInterruptFlag (_ocfa);
					if (_compA != 0) {
						UpdateCompPin (_compA, 'A');
					}
				}
				if (value == _ocrB) {
					_cpu.SetInterruptFlag (_ocfb);
					if (_compB != 0) {
						UpdateCompPin (_compB, 'B');
					}
				}
				if (_hasOcrC && value == _ocrC) {
					_cpu.SetInterruptFlag (_ocfc);
					if (_compC != 0) {
						UpdateCompPin (_compC, 'C');
					}
				}
			}
			delta--;
		}
		return value & _max;
	}

	private void TimerUpdated (int value, int prevNumber)
	{
		var overflow = prevNumber > value;
		if ((prevNumber < _ocrA || overflow) && value >= _ocrA || prevNumber < _ocrA && overflow) {
			_cpu.SetInterruptFlag (_ocfa);
			if (_compA != 0) {
				UpdateCompPin (_compA, 'A');
			}
		}
		if ((prevNumber < _ocrB || overflow) && value >= _ocrB || prevNumber < _ocrB && overflow) {
			_cpu.SetInterruptFlag (_ocfb);
			if (_compB != 0) {
				UpdateCompPin (_compB, 'B');
			}
		}
		if (_hasOcrC && ((prevNumber < _ocrC || overflow) && value >= _ocrC || prevNumber < _ocrC && overflow)) {
			_cpu.SetInterruptFlag (_ocfc);
			if (_compC != 0) {
				UpdateCompPin (_compC, 'C');
			}
		}
	}

	private void CheckForceCompare (int value)
	{
		if (_timerMode == TimerMode.FastPWM || _timerMode == TimerMode.PWMPhaseCorrect || _timerMode == TimerMode.PWMPhaseFrequencyCorrect) {
			// The FOCnA/FOCnB/FOCnC bits are only active when the WGMn3:0 bits specifies a non-PWM mode
			return;
		}
		if ((value & FOCA) != 0) {
			UpdateCompPin (_compA, 'A');
		}
		if ((value & FOCB) != 0) {
			UpdateCompPin (_compB, 'B');
		}
		if (_config.ComparatorPortC != 0 && (value & FOCC) != 0) {
			UpdateCompPin (_compC, 'C');
		}
	}

	private void UpdateCompPin (byte compValue, char pinName, bool bottom = false)
	{
		var newValue = PinOverrideMode.None;
		var invertingMode = compValue == 3;
		var isSet = _countingUp && invertingMode;
		switch (_timerMode) {
			case TimerMode.Normal:
			case TimerMode.CTC:
				newValue = CompToOverride (compValue);
				break;
			case TimerMode.FastPWM:
				if (compValue == 1) 
					newValue = bottom ? PinOverrideMode.None : PinOverrideMode.Toggle;
				else 
					newValue = isSet ? PinOverrideMode.Set : PinOverrideMode.Clear;
				break;
			case TimerMode.PWMPhaseCorrect:
			case TimerMode.PWMPhaseFrequencyCorrect:
				if (compValue == 1) 
					newValue = PinOverrideMode.Toggle;
				else 
					newValue = isSet ? PinOverrideMode.Set : PinOverrideMode.Clear;
				break;
		}

		if (newValue != PinOverrideMode.None) {
			switch (pinName) {
				case 'A':
					UpdateCompA (newValue);
					break;
				case 'B':
					UpdateCompB (newValue);
					break;
				case 'C':
					UpdateCompC (newValue);
					break;
			}
		}
	}
	
	private void UpdateCompA (PinOverrideMode mode)
	{
		var port = _cpu.GpioByPort[_config.ComparatorPortA];
		port.TimerOverridePin (_config.ComparatorPinA, mode);
	}
	
	private void UpdateCompB (PinOverrideMode mode)
	{
		var port = _cpu.GpioByPort[_config.ComparatorPortB];
		port.TimerOverridePin (_config.ComparatorPinB, mode);
	}
	
	private void UpdateCompC (PinOverrideMode mode)
	{
		var port = _cpu.GpioByPort[_config.ComparatorPortC];
		port.TimerOverridePin (_config.ComparatorPinC, mode);
	}

	private PinOverrideMode CompToOverride (byte comp)
	{
		switch (comp) {
			case 1:
				return PinOverrideMode.Toggle;
			case 2:
				return PinOverrideMode.Clear;
			case 3:
				return PinOverrideMode.Set;
			default:
				return PinOverrideMode.Enable;
		}
	}
}

public class AvrTimerConfig
{
	public byte Bits;
	public int[] Dividers = [];
	
	// Interrupt Vectors
	public byte CaptureInterrupt;
	public byte ComparatorAInterrupt;
	public byte ComparatorBInterrupt;
	public byte ComparatorCInterrupt; // Optional: 0 if not used
	public byte OverflowInterrupt;
	
	// Register Addresses
	public byte TIFR;
	public byte OCRA;
	public byte OCRB;
	public byte OCRC; // Optional: 0 if not used
	public byte ICR;
	public byte TCNT;
	public byte TCCRA;
	public byte TCCRB;
	public byte TCCRC;
	public byte TIMSK;
	
	// TIFR bits
	public byte TOV;
	public byte OCFA;
	public byte OCFB;
	public byte OCFC; // Optional: Only if CompareCInterrupt is != 0
	
	// TIMSK bits
	public byte TOIE;
	public byte OCIEA;
	public byte OCIEB;
	public byte OCIEC; // Optional: Only if CompareCInterrupt is != 0
	
	// Output Compare Inputs
	public ushort ComparatorPortA;
	public byte ComparatorPinA;
	public ushort ComparatorPortB;
	public byte ComparatorPinB;
	public ushort ComparatorPortC; // Optional: 0 if not used
	public byte ComparatorPinC; 
	
	// External clock pin
	public ushort ExternalClockPort;
	public byte ExternalClockPin;
}

public class WgmConfig
{
	public TimerMode Mode;
	public int TimerTopValue;
	public OCRUpdateMode OCRUpdateMode;
	public TOVUpdateMode TOVUpdateMode;
	public int Flags;
}

public enum ExternalClockMode
{
	FallingEdge = 6,
	RisingEdge = 7,
}

public enum TimerMode
{
	Normal,
	PWMPhaseCorrect,
	CTC,
	FastPWM,
	PWMPhaseFrequencyCorrect,
	Reserved,
}

public enum TOVUpdateMode {
	Max,
	Top,
	Bottom,
}

public enum OCRUpdateMode {
	Immediate,
	Top,
	Bottom,
}
