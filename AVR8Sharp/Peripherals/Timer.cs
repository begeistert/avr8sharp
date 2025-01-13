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
	public static AvrTimerConfig DefaultTimerBits = new AvrTimerConfig (
		// TIFR bits
		tov: 1,
		ocfa: 2,
		ocfb: 4,
		ocfc: 0, // Unused
		
		// TIMSK bits
		toie: 1,
		ociea: 2,
		ocieb: 4,
		ociec: 0 // Unused
	);
	public static AvrTimerConfig Timer0Config = new AvrTimerConfig (
		bits: 8,
		dividers: Timer01Dividers,
		captureInterrupt: 0, // Not Available
		comparatorAInterrupt:0x1c,
		comparatorBInterrupt: 0x1e,
		comparatorCInterrupt: 0,
		overflowInterrupt: 0x20,
		tifr: 0x35,
		ocra: 0x47,
		ocrb: 0x48,
		ocrc: 0, // Not Available
		icr: 0, // Not Available
		tcnt: 0x46,
		tccra: 0x44,
		tccrb: 0x45,
		tccrc: 0, // Not Available
		timsk: 0x6e,
		comparatorPortA: AvrIoPort.PortDConfig.PORT,
		comparatorPinA: 6,
		comparatorPortB: AvrIoPort.PortDConfig.PORT,
		comparatorPinB: 5,
		comparatorPortC: 0, // Not Available
		comparatorPinC: 0, 
		externalClockPort: AvrIoPort.PortDConfig.PORT,
		externalClockPin: 4,
		// Apply default bits
		tov: DefaultTimerBits.TOV,
		ocfa: DefaultTimerBits.OCFA,
		ocfb: DefaultTimerBits.OCFB,
		ocfc: DefaultTimerBits.OCFC,
		toie: DefaultTimerBits.TOIE,
		ociea: DefaultTimerBits.OCIEA,
		ocieb: DefaultTimerBits.OCIEB,
		ociec: DefaultTimerBits.OCIEC
	);
	public static AvrTimerConfig Timer1Config = new AvrTimerConfig (
		bits: 16,
		dividers: Timer01Dividers,
		captureInterrupt: 0x14,
		comparatorAInterrupt: 0x16,
		comparatorBInterrupt: 0x18,
		comparatorCInterrupt: 0,
		overflowInterrupt: 0x1a,
		tifr: 0x36,
		ocra: 0x88,
		ocrb: 0x8a,
		ocrc: 0, // Not Available
		icr: 0x86,
		tcnt: 0x84,
		tccra: 0x80,
		tccrb: 0x81,
		tccrc: 0x82,
		timsk: 0x6f,
		comparatorPortA: AvrIoPort.PortBConfig.PORT,
		comparatorPinA: 1,
		comparatorPortB: AvrIoPort.PortBConfig.PORT,
		comparatorPinB: 2,
		comparatorPortC: 0, // Not Available
		comparatorPinC: 0, 
		externalClockPort: AvrIoPort.PortDConfig.PORT,
		externalClockPin: 5,
		// Apply default bits
		tov: DefaultTimerBits.TOV,
		ocfa: DefaultTimerBits.OCFA,
		ocfb: DefaultTimerBits.OCFB,
		ocfc: DefaultTimerBits.OCFC,
		toie: DefaultTimerBits.TOIE,
		ociea: DefaultTimerBits.OCIEA,
		ocieb: DefaultTimerBits.OCIEB,
		ociec: DefaultTimerBits.OCIEC
	);
	public static AvrTimerConfig Timer2Config = new AvrTimerConfig (
		bits: 8,
		dividers: [
			0,
			1,
			8,
			32,
			64,
			128,
			256,
			1024
		],
		captureInterrupt: 0, // Not Available
		comparatorAInterrupt: 0x0e,
		comparatorBInterrupt: 0x10,
		comparatorCInterrupt: 0,
		overflowInterrupt: 0x12,
		tifr: 0x37,
		ocra: 0xb3,
		ocrb: 0xb4,
		ocrc: 0, // Not Available
		icr: 0, // Not Available
		tcnt: 0xb2,
		tccra: 0xb0, 
		tccrb: 0xb1,
		tccrc: 0, // Not Available
		timsk: 0x70,
		comparatorPortA: AvrIoPort.PortBConfig.PORT,
		comparatorPinA: 3,
		comparatorPortB: AvrIoPort.PortDConfig.PORT,
		comparatorPinB: 3,
		comparatorPortC: 0, // Not Available
		comparatorPinC: 0, 
		externalClockPort: 0, // Not Available
		externalClockPin: 0,
		// Apply default bits
		tov: DefaultTimerBits.TOV,
		ocfa: DefaultTimerBits.OCFA,
		ocfb: DefaultTimerBits.OCFB,
		ocfc: DefaultTimerBits.OCFC,
		toie: DefaultTimerBits.TOIE,
		ociea: DefaultTimerBits.OCIEA,
		ocieb: DefaultTimerBits.OCIEB,
		ociec: DefaultTimerBits.OCIEC
	);
	public static WgmConfig[] WgmModes8Bit = [
		new WgmConfig (mode:TimerMode.Normal, timerTopValue: 0xff, ocrUpdateMode: OCRUpdateMode.Immediate, tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseCorrect, timerTopValue: 0xff, ocrUpdateMode: OCRUpdateMode.Top, tovUpdateMode: TOVUpdateMode.Bottom, flags: 0),
		new WgmConfig (mode:TimerMode.CTC, timerTopValue: TopOCRA, ocrUpdateMode: OCRUpdateMode.Immediate, tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.FastPWM, timerTopValue: 0xff, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseCorrect, timerTopValue: TopOCRA, ocrUpdateMode: OCRUpdateMode.Top, tovUpdateMode: TOVUpdateMode.Bottom, flags: OCToggle),
		new WgmConfig (mode:TimerMode.Reserved, timerTopValue: 0xff, ocrUpdateMode: OCRUpdateMode.Immediate,tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.FastPWM, timerTopValue: TopOCRA, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Top, flags: OCToggle),
	];
	public static WgmConfig[] WgmModes16Bits = [
		new WgmConfig (mode:TimerMode.Normal, timerTopValue: 0xffff, ocrUpdateMode: OCRUpdateMode.Immediate, tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseCorrect, timerTopValue: 0x00ff, ocrUpdateMode: OCRUpdateMode.Top, tovUpdateMode: TOVUpdateMode.Bottom, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseCorrect, timerTopValue: 0x01ff, ocrUpdateMode: OCRUpdateMode.Top, tovUpdateMode: TOVUpdateMode.Bottom, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseCorrect, timerTopValue: 0x03ff, ocrUpdateMode: OCRUpdateMode.Top, tovUpdateMode: TOVUpdateMode.Bottom, flags: 0),
		new WgmConfig (mode:TimerMode.CTC, timerTopValue: TopOCRA, ocrUpdateMode: OCRUpdateMode.Immediate, tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.FastPWM, timerTopValue: 0x00ff, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Top, flags: 0),
		new WgmConfig (mode:TimerMode.FastPWM, timerTopValue: 0x01ff, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Top, flags: 0),
		new WgmConfig (mode:TimerMode.FastPWM, timerTopValue: 0x03ff, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Top, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseFrequencyCorrect, timerTopValue: TopICR, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Bottom, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseFrequencyCorrect, timerTopValue: TopOCRA, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Bottom, flags: OCToggle),
		new WgmConfig (mode:TimerMode.PWMPhaseCorrect, timerTopValue: TopICR, ocrUpdateMode: OCRUpdateMode.Top, tovUpdateMode: TOVUpdateMode.Bottom, flags: 0),
		new WgmConfig (mode:TimerMode.PWMPhaseCorrect, timerTopValue: TopOCRA, ocrUpdateMode: OCRUpdateMode.Top, tovUpdateMode: TOVUpdateMode.Bottom, flags: OCToggle),
		new WgmConfig (mode:TimerMode.CTC, timerTopValue: TopICR, ocrUpdateMode: OCRUpdateMode.Immediate, tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.Reserved, timerTopValue: 0xffff, ocrUpdateMode: OCRUpdateMode.Immediate, tovUpdateMode: TOVUpdateMode.Max, flags: 0),
		new WgmConfig (mode:TimerMode.FastPWM, timerTopValue: TopICR, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Top, flags: OCToggle),
		new WgmConfig (mode:TimerMode.FastPWM, timerTopValue: TopOCRA, ocrUpdateMode: OCRUpdateMode.Bottom, tovUpdateMode: TOVUpdateMode.Top, flags: OCToggle),
	];
	
	private static Action? CountParameterLess;
	
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

		if (CountParameterLess == null) {
			CountParameterLess = () => Count ();
		}
		
		_ovf = new AvrInterruptConfig (
			address: config.OverflowInterrupt,
			enableRegister: config.TIMSK,
			enableMask: config.TOIE, 
			flagRegister: config.TIFR,
			flagMask: config.TOV
		);
		
		_ocfa = new AvrInterruptConfig (
			address: config.ComparatorAInterrupt,
			enableRegister: config.TIMSK,
			enableMask: config.OCIEA,
			flagRegister: config.TIFR,
			flagMask: config.OCFA
		);

		_ocfb = new AvrInterruptConfig (
			address: config.ComparatorBInterrupt,
			enableRegister: config.TIMSK,
			enableMask: config.OCIEB,
			flagRegister: config.TIFR,
			flagMask: config.OCFB
		);
		
		_ocfc = new AvrInterruptConfig (
			address: config.ComparatorCInterrupt,
			enableRegister: config.TIMSK,
			enableMask: config.OCIEC,
			flagRegister: config.TIFR,
			flagMask: config.OCFC
		);

		UpdateWgmConfig ();
		
		_cpu.ReadHooks[config.TCNT] = addr => {
			Count (false);
			if (config.Bits == 16) {
				_cpu.Data[addr + 1] = (byte)(_tcnt >> 8);
			}
			return _cpu.Data[addr] = (byte)(_tcnt & 0xff);
		};
		
		_cpu.WriteHooks[config.TCNT] = (value, _, _, _) => {
			_tcntNext = (ushort)((_highByteTemp << 8) | value);
			_countingUp = true;
			_tcntUpdated = true;
			_cpu.UpdateClockEvent (() => Count (), 0);
			if (_divider != 0) {
				TimerUpdated (_tcntNext, _tcntNext);
			}
			return true;
		};
		
		_cpu.WriteHooks[config.OCRA] = (value, _, _, _) => {
			_nextOcrA = (ushort)((_highByteTemp << 8) | value);
			if (_ocrUpdateMode == OCRUpdateMode.Immediate) {
				_ocrA = _nextOcrA;
			}
			return true;
		};
		
		_cpu.WriteHooks[config.OCRB] = (value, _, _, _) => {
			_nextOcrB = (ushort)((_highByteTemp << 8) | value);
			if (_ocrUpdateMode == OCRUpdateMode.Immediate) {
				_ocrB = _nextOcrB;
			}
			return true;
		};
		
		if (_hasOcrC) {
			_cpu.WriteHooks[config.OCRC] = (value, _, _, _) => {
				_nextOcrC = (ushort)((_highByteTemp << 8) | value);
				if (_ocrUpdateMode == OCRUpdateMode.Immediate) {
					_ocrC = _nextOcrC;
				}
				return true;
			};
		}

		if (_config.Bits == 16) {
			_cpu.WriteHooks[config.ICR] = (value, _, _, _) => {
				_icr = (ushort)((_highByteTemp << 8) | value);
				return false;
			};
			
			Func<byte, byte, ushort, byte, bool> updateTempRegister = (value, _, _, _) => {
				_highByteTemp = value;
				return false;
			};
			Func<byte, byte, ushort, byte, bool> updateOCRHighRegister = (value, old, addr, _) => {
				_highByteTemp = (byte)(value & (OcrMask >> 8));
				_cpu.Data[addr] = _highByteTemp;
				return true;
			};
			
			_cpu.WriteHooks[(ushort)(config.TCNT + 1)] = updateTempRegister;
			_cpu.WriteHooks[(ushort)(config.OCRA + 1)] = updateOCRHighRegister;
			_cpu.WriteHooks[(ushort)(config.OCRB + 1)] = updateOCRHighRegister;
			if (_hasOcrC) {
				_cpu.WriteHooks[(ushort)(config.OCRC + 1)] = updateOCRHighRegister;
			}
			_cpu.WriteHooks[(ushort)(config.ICR + 1)] = updateOCRHighRegister;
		}
		
		_cpu.WriteHooks[config.TCCRA] = (value, _, _, _) => {
			_cpu.Data[config.TCCRA] = value;
			UpdateWgmConfig ();
			return true;
		};
		
		_cpu.WriteHooks[config.TCCRB] = (value, _, _, _) => {
			if (_config.TCCRC == 0) {
				CheckForceCompare(value);
				value &= ~(FOCA | FOCB) & 0xff;
			}
			_cpu.Data[_config.TCCRB] = value;
			_updateDivider = true;
			_cpu.ClearClockEvent (CountParameterLess);
			_cpu.AddClockEvent (CountParameterLess, 0);
			UpdateWgmConfig ();
			return true;
		};

		if (_config.TCCRC != 0) {
			_cpu.WriteHooks[config.TCCRC] = (value, _, _, _) => {
				CheckForceCompare(value);
				return false;
			};
		}
		
		_cpu.WriteHooks[config.TIFR] = (value, _, _, _) => {
			_cpu.Data[config.TIFR] = value;
			var boolValue = value != 0;
			_cpu.ClearInterrupt (_ovf, boolValue);
			_cpu.ClearInterrupt (_ocfa, boolValue);
			_cpu.ClearInterrupt (_ocfb, boolValue);
			return true;
		};
		
		_cpu.WriteHooks[config.TIMSK] = (value, _, _, _) => {
			_cpu.UpdateInterruptEnable (_ovf, value);
			_cpu.UpdateInterruptEnable (_ocfa, value);
			_cpu.UpdateInterruptEnable (_ocfb, value);
			return false;
		};
	}

	public void Reset ()
	{
		_divider = 0;
		_lastCycle = 0;
		_ocrA = 0;
		_nextOcrA = 0;
		_ocrB = 0;
		_nextOcrB = 0;
		_ocrC = 0;
		_nextOcrC = 0;
		_icr = 0;
		_tcnt = 0;
		_tcntNext = 0;
		_tcntUpdated = false;
		_countingUp = false;
		_updateDivider = true;
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
				_cpu.AddClockEvent (CountParameterLess, _lastCycle + newDivider - _cpu.Cycles);
			} else if (_externalClockPort != null && (CS == (int)ExternalClockMode.FallingEdge || CS == (int)ExternalClockMode.RisingEdge)) {
				_externalClockPort.ExternalClockListeners[_config.ExternalClockPin] = ExternalClockCallback;
				_externalClockRisingEdge = CS == (int)ExternalClockMode.RisingEdge;
			}
		}
		
		if (reschedule && _divider != 0) {
			_cpu.AddClockEvent (CountParameterLess, _lastCycle + _divider - _cpu.Cycles);
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
	public int[]? Dividers = [];
	
	// Interrupt Vectors
	public readonly byte CaptureInterrupt;
	public readonly byte ComparatorAInterrupt;
	public readonly byte ComparatorBInterrupt;
	public readonly byte ComparatorCInterrupt; // Optional: 0 if not used
	public readonly byte OverflowInterrupt;
	
	// Register Addresses
	public readonly byte TIFR;
	public readonly byte OCRA;
	public readonly byte OCRB;
	public readonly byte OCRC; // Optional: 0 if not used
	public readonly byte ICR;
	public readonly byte TCNT;
	public readonly byte TCCRA;
	public readonly byte TCCRB;
	public readonly byte TCCRC;
	public readonly byte TIMSK;
	
	// TIFR bits
	public readonly byte TOV;
	public readonly byte OCFA;
	public readonly byte OCFB;
	public readonly byte OCFC; // Optional: Only if CompareCInterrupt is != 0
	
	// TIMSK bits
	public readonly byte TOIE;
	public readonly byte OCIEA;
	public readonly byte OCIEB;
	public readonly byte OCIEC; // Optional: Only if CompareCInterrupt is != 0
	
	// Output Compare Inputs
	public readonly ushort ComparatorPortA;
	public readonly byte ComparatorPinA;
	public readonly ushort ComparatorPortB;
	public readonly byte ComparatorPinB;
	public readonly ushort ComparatorPortC; // Optional: 0 if not used
	public readonly byte ComparatorPinC; 
	
	// External clock pin
	public readonly ushort ExternalClockPort;
	public readonly byte ExternalClockPin;
	
	public AvrTimerConfig (
		byte bits = 0, 
		int[]? dividers = null, 
		byte captureInterrupt = 0, 
		byte comparatorAInterrupt = 0, 
		byte comparatorBInterrupt = 0, 
		byte comparatorCInterrupt = 0, 
		byte overflowInterrupt = 0, 
		byte tifr = 0, 
		byte ocra = 0, 
		byte ocrb = 0, 
		byte ocrc = 0, 
		byte icr = 0, 
		byte tcnt = 0, 
		byte tccra = 0, 
		byte tccrb = 0, 
		byte tccrc = 0, 
		byte timsk = 0, 
		byte tov = 0, 
		byte ocfa = 0, 
		byte ocfb = 0, 
		byte ocfc = 0, 
		byte toie = 0, 
		byte ociea = 0, 
		byte ocieb = 0, 
		byte ociec = 0, 
		ushort comparatorPortA = 0, 
		byte comparatorPinA = 0, 
		ushort comparatorPortB = 0, 
		byte comparatorPinB = 0, 
		ushort comparatorPortC = 0, 
		byte comparatorPinC = 0, 
		ushort externalClockPort = 0, 
		byte externalClockPin = 0
	)
	{
		Bits = bits;
		Dividers = dividers;
		CaptureInterrupt = captureInterrupt;
		ComparatorAInterrupt = comparatorAInterrupt;
		ComparatorBInterrupt = comparatorBInterrupt;
		ComparatorCInterrupt = comparatorCInterrupt;
		OverflowInterrupt = overflowInterrupt;
		TIFR = tifr;
		OCRA = ocra;
		OCRB = ocrb;
		OCRC = ocrc;
		ICR = icr;
		TCNT = tcnt;
		TCCRA = tccra;
		TCCRB = tccrb;
		TCCRC = tccrc;
		TIMSK = timsk;
		TOV = tov;
		OCFA = ocfa;
		OCFB = ocfb;
		OCFC = ocfc;
		TOIE = toie;
		OCIEA = ociea;
		OCIEB = ocieb;
		OCIEC = ociec;
		ComparatorPortA = comparatorPortA;
		ComparatorPinA = comparatorPinA;
		ComparatorPortB = comparatorPortB;
		ComparatorPinB = comparatorPinB;
		ComparatorPortC = comparatorPortC;
		ComparatorPinC = comparatorPinC;
		ExternalClockPort = externalClockPort;
		ExternalClockPin = externalClockPin;
	}
}

public class WgmConfig (TimerMode mode, int timerTopValue, OCRUpdateMode ocrUpdateMode, TOVUpdateMode tovUpdateMode, int flags)
{
	public readonly TimerMode Mode = mode;
	public readonly int TimerTopValue = timerTopValue;
	public readonly OCRUpdateMode OCRUpdateMode = ocrUpdateMode;
	public readonly TOVUpdateMode TOVUpdateMode = tovUpdateMode;
	public readonly int Flags = flags;
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
