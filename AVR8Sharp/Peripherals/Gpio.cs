using AVR8Sharp.Cpu;
namespace AVR8Sharp.Peripherals;

public class AvrIoPort
{
	public static AvrExternalInterrupt INT0 = new AvrExternalInterrupt {
		EICR = 0x69,
		EIMSK = 0x3d,
		EIFR = 0x3c,
		IscOffset = 0,
		Index = 0,
		Interrupt = 2,
	};
	
	public static AvrExternalInterrupt INT1 = new AvrExternalInterrupt {
		EICR = 0x69,
		EIMSK = 0x3d,
		EIFR = 0x3c,
		IscOffset = 2,
		Index = 1,
		Interrupt = 4,
	};
	
	public static AvrPinChangeInterrupt PCINT0 = new AvrPinChangeInterrupt {
		PCIE = 0,
		PCICR = 0x68,
		PCIFR = 0x3b,
		PCMSK = 0x6b,
		PinChangeInterrupt = 6,
		Mask = 0xFF,
		Offset = 0,
	};
	
	public static AvrPinChangeInterrupt PCINT1 = new AvrPinChangeInterrupt {
		PCIE = 1,
		PCICR = 0x68,
		PCIFR = 0x3b,
		PCMSK = 0x6c,
		PinChangeInterrupt = 8,
		Mask = 0xFF,
		Offset = 0,
	};
	
	public static AvrPinChangeInterrupt PCINT2 = new AvrPinChangeInterrupt {
		PCIE = 2,
		PCICR = 0x68,
		PCIFR = 0x3b,
		PCMSK = 0x6c,
		PinChangeInterrupt = 10,
		Mask = 0xFF,
		Offset = 0,
	};
	
	public static AvrPortConfig PortAConfig = new AvrPortConfig {
		PIN = 0x20,
		DDR = 0x21,
		PORT = 0x22,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortBConfig = new AvrPortConfig {
		PIN = 0x23,
		DDR = 0x24,
		PORT = 0x25,
		
		// Interrupt settings
		PinChange = PCINT0,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortCConfig = new AvrPortConfig {
		PIN = 0x26,
		DDR = 0x27,
		PORT = 0x28,
		
		// Interrupt settings
		PinChange = PCINT1,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortDConfig = new AvrPortConfig {
		PIN = 0x29,
		DDR = 0x2a,
		PORT = 0x2b,
		
		// Interrupt settings
		PinChange = PCINT2,
		ExternalInterrupts = [null, null, INT0, INT1, ],
	};
	
	public static AvrPortConfig PortEConfig = new AvrPortConfig {
		PIN = 0x2c,
		DDR = 0x2d,
		PORT = 0x2e,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortFConfig = new AvrPortConfig {
		PIN = 0x2f,
		DDR = 0x30,
		PORT = 0x31,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortGConfig = new AvrPortConfig {
		PIN = 0x32,
		DDR = 0x33,
		PORT = 0x34,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortHConfig = new AvrPortConfig {
		PIN = 0x100,
		DDR = 0x101,
		PORT = 0x102,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortJConfig = new AvrPortConfig {
		PIN = 0x103,
		DDR = 0x104,
		PORT = 0x105,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortKConfig = new AvrPortConfig {
		PIN = 0x106,
		DDR = 0x107,
		PORT = 0x108,
		ExternalInterrupts = [],
	};
	
	public static AvrPortConfig PortLConfig = new AvrPortConfig {
		PIN = 0x109,
		DDR = 0x10a,
		PORT = 0x10b,
		ExternalInterrupts = [],
	};
	
	private readonly List<AvrInterruptConfig?> _externalInts = [];
	private readonly AvrInterruptConfig? _pcint;
	private readonly List<Action<byte, byte>> _listeners = [];
	private readonly Cpu.Cpu _cpu;
	private readonly AvrPortConfig _portConfig;
	private int _pinValue;
	private byte _overrideMask = 0xff;
	private byte _overrideValue = 0;
	private byte _lastValue = 0;
	private byte _lastDdr = 0;
	private byte _lastPin = 0;
	private byte _openCollector = 0;
	
	public List<Action<bool>>? ExternalClockListeners { get; set; }

	public AvrIoPort (Cpu.Cpu cpu, AvrPortConfig portConfig)
	{
		_cpu = cpu;
		_portConfig = portConfig;
		
		_cpu.GpioPorts.Add (this);
		_cpu.GpioByPort[_portConfig.PORT] = this;
		
		_cpu.WriteHooks[portConfig.DDR] = (value, _, _, _) => {
			var portValue = _cpu.Data[portConfig.PORT];
			_cpu.Data[portConfig.DDR] = value;
			WriteGpio (portValue, value);
			UpdatePinRegister (value);
			return true;
		};
		
		_cpu.WriteHooks[portConfig.PORT] = (value, _, _, _) => {
			var ddrMask = _cpu.Data[portConfig.DDR];
			_cpu.Data[portConfig.PORT] = value;
			WriteGpio (value, ddrMask);
			UpdatePinRegister (ddrMask);
			return true;
		};
		
		_cpu.WriteHooks[portConfig.PIN] = (value, oldValue, addr, mask) => {
			// Writing to 1 PIN toggles PORT bits
			var oldPortValue = _cpu.Data[portConfig.PORT];
			var ddrMask = _cpu.Data[portConfig.DDR];
			var portValue = (byte)(oldPortValue ^ (value & mask));
			_cpu.Data[portConfig.PORT] = portValue;
			WriteGpio (portValue, ddrMask);
			UpdatePinRegister (ddrMask);
			return true;
		};
		
		// External interrupts
		// const { externalInterrupts } = portConfig;
		// this.externalInts = externalInterrupts.map((externalConfig) =>
		//   externalConfig
		//     ? {
		//         address: externalConfig.interrupt,
		//         flagRegister: externalConfig.EIFR,
		//         flagMask: 1 << externalConfig.index,
		//         enableRegister: externalConfig.EIMSK,
		//         enableMask: 1 << externalConfig.index,
		//       }
		//     : null
		// );
		// const EICR = new Set(externalInterrupts.map((item) => item?.EICR));
		// for (const EICRx of EICR) {
		//   this.attachInterruptHook(EICRx || 0);
		// }
		// const EIMSK = externalInterrupts.find((item) => item && item.EIMSK)?.EIMSK ?? 0;
		// this.attachInterruptHook(EIMSK, 'mask');
		// const EIFR = externalInterrupts.find((item) => item && item.EIFR)?.EIFR ?? 0;
		// this.attachInterruptHook(EIFR, 'flag');
		
		_externalInts = portConfig.ExternalInterrupts.Select (externalConfig => {
			if (externalConfig != null) {
				return new AvrInterruptConfig {
					Address = externalConfig.Value.Interrupt,
					FlagRegister = externalConfig.Value.EIFR,
					FlagMask = (byte)(1 << externalConfig.Value.Index),
					EnableRegister = externalConfig.Value.EIMSK,
					EnableMask = (byte)(1 << externalConfig.Value.Index),
				};
			}
			return null;
		}).ToList ();
		
		var eicr = new HashSet<byte> (portConfig.ExternalInterrupts.Select (item => item?.EICR ?? 0));
		foreach (var eicrx in eicr) {
			AttachInterruptHook (eicrx);
		}
		
		var eimsk = portConfig.ExternalInterrupts.FirstOrDefault (item => item != null && item.Value.EIMSK != 0)?.EIMSK ?? 0;
		AttachInterruptHook (eimsk, "mask");
		
		var eifr = portConfig.ExternalInterrupts.FirstOrDefault (item => item != null && item.Value.EIFR != 0)?.EIFR ?? 0;
		AttachInterruptHook (eifr, "flag");
		
		_pcint = portConfig.PinChange != null ? new AvrInterruptConfig {
			Address = portConfig.PinChange.Value.PinChangeInterrupt,
			FlagRegister = portConfig.PinChange.Value.PCIFR,
			FlagMask = 1 << portConfig.PinChange.Value.PCIE,
			EnableRegister = portConfig.PinChange.Value.PCICR,
			EnableMask = 1 << portConfig.PinChange.Value.PCIE,
		} : null;
		
		if (portConfig.PinChange != null) {
			var pcifr = portConfig.PinChange.Value.PCIFR;
			_cpu.WriteHooks[pcifr] = (value, _, _, _) => {
				foreach (var gpio in _cpu.GpioPorts) {
					var pcint = gpio._pcint;
					if (pcint != null) {
						_cpu.ClearInterruptByFlag (pcint, value);
					}
				}
				return true;
			};
			
			var pcmsk = portConfig.PinChange.Value.PCMSK;
			_cpu.WriteHooks[pcmsk] = (value, _, _, _) => {
				_cpu.Data[pcmsk] = value;
				foreach (var gpio in _cpu.GpioPorts) {
					var pcint = gpio._pcint;
					if (pcint != null) {
						_cpu.UpdateInterruptEnable (pcint, value);
					}
				}
				return true;
			};
		}
	}
	
	public void AddListener (Action<byte, byte> listener)
	{
		_listeners.Add (listener);
	}
	
	public void RemoveListener (Action<byte, byte> listener)
	{
		_listeners.Remove (listener);
	}

	/// <summary>
	/// Get the state of a given pin
	/// </summary>
	/// <param name="index">Pin index to return from 0 to 7</param>
	/// <returns>inState.Low or PinState.High if the pin is set to output, PinState.Input if the pin is set
	/// to input, and PinState.InputPullUp if the pin is set to input and the internal pull-up resistor has
	/// been enabled.</returns>
	public PinState GetPinState (byte index)
	{
		var ddr = _cpu.Data[_portConfig.DDR];
		var port = _cpu.Data[_portConfig.PORT];
		var bitMask = (byte)(1 << index);
		var openState = (port & bitMask) != 0 ? PinState.InputPullup : PinState.Input;
		var highValue = (_openCollector & bitMask) != 0 ? openState : PinState.High;
		if ((ddr & bitMask) != 0) {
			return (_lastValue & bitMask) != 0 ? highValue : PinState.Low;
		}
		return openState;
	}

	/// <summary>
	/// Sets the input value for the given pin. This is the value that
	/// will be returned when reading from the PIN register.
	/// </summary>
	/// <param name="index">Pin index to set from 0 to 7</param>
	/// <param name="value">The value to set</param>
	public void SetPinValue (byte index, bool value)
	{ 
		var bitMask = 1 << index;
		_pinValue &= ~bitMask;
		if (value) {
			_pinValue |= bitMask;
		}
		UpdatePinRegister (_cpu.Data[_portConfig.DDR]);
	}

	public void TimerOverridePin (byte pin, PinOverrideMode mode)
	{
		var bitMask = 1 << pin;
		if (mode == PinOverrideMode.None) {
			_overrideMask |= (byte)bitMask;
			_overrideValue &= (byte)~bitMask;
		} else {
			_overrideMask &= (byte)~bitMask;
			switch (mode) {
				case PinOverrideMode.Enable:
					_overrideValue &= (byte)~bitMask;
					_overrideValue |= (byte)(_cpu.Data[_portConfig.PORT] & bitMask);
					break;
				case PinOverrideMode.Set:
					_overrideValue |= (byte)bitMask;
					break;
				case PinOverrideMode.Clear:
					_overrideValue &= (byte)~bitMask;
					break;
				case PinOverrideMode.Toggle:
					_overrideValue ^= (byte)bitMask;
					break;
			}
		}
		
		var ddrMask = _cpu.Data[_portConfig.DDR];
		WriteGpio (_cpu.Data[_portConfig.PORT], ddrMask);
		UpdatePinRegister (ddrMask);
	}
	
	private void UpdatePinRegister (byte ddr)
	{
		var newPin = (byte)(((_pinValue & ~ddr) | (_lastValue & ddr)) & 0xff);
		_cpu.Data[_portConfig.PIN] = newPin;
		if (_lastPin == newPin) return;
		for (var index = 0; index < 8; index++) {
			if (((newPin & (1 << index)) != (_lastPin & (1 << index)))) {
				var value = (newPin & (1 << index)) != 0;
				ToggleInterrupt ((byte)index, value);
				ExternalClockListeners?[index]?.Invoke (value);
			}
		}
		_lastPin = newPin;
	}

	private void ToggleInterrupt (byte index, bool risingEdge)
	{
		var externalConfig = _portConfig.ExternalInterrupts?[index];
		var external = _externalInts[index];
		if (external != null && externalConfig != null) {
			var eimsk = externalConfig.Value.EIMSK;
			var eicr = externalConfig.Value.EICR;
			var iscOffset = externalConfig.Value.IscOffset;
			if ((_cpu.Data[eimsk] & (1 << externalConfig.Value.Index)) != 0) {
				var configuration = (InterruptMode)((_cpu.Data[eicr] >> iscOffset) & 0x3);
				var generateInterrupt = false;
				external.Constant = false;
				switch (configuration) {
					case InterruptMode.LowLevel:
						generateInterrupt = !risingEdge;
						external.Constant = true;
						break;
					case InterruptMode.Change:
						generateInterrupt = true;
						break;
					case InterruptMode.FallingEdge:
						generateInterrupt = !risingEdge;
						break;
					case InterruptMode.RisingEdge:
						generateInterrupt = risingEdge;
						break;
				}
				if (generateInterrupt) {
					_cpu.SetInterruptFlag (external);
				} else if (external.Constant) {
					_cpu.ClearInterrupt (external, true);
				}
			}
		}
		
		if (_pcint != null && _portConfig.PinChange != null && (_portConfig.PinChange.Value.Mask & (1 << index)) != 0) {
			var pcmsk = _portConfig.PinChange.Value.PCMSK;
			if ((_cpu.Data[pcmsk] & (1 << (index + _portConfig.PinChange.Value.Offset))) != 0) {
				_cpu.SetInterruptFlag (_pcint);
			}
		}
	}
	
	private void AttachInterruptHook (byte register, string registerType = "other")
	{
		if (register == 0) return;
		_cpu.WriteHooks[register] = (value, _, _, _) => {
			if (registerType != "flag") {
				_cpu.Data[register] = value;
			}
			foreach (var gpio in _cpu.GpioPorts) {
				foreach (var external in gpio._externalInts) {
					if (external != null && registerType == "mask") {
						_cpu.UpdateInterruptEnable (external, value);
					}
					if (external != null && !external.Constant && registerType == "flag") {
						_cpu.ClearInterruptByFlag (external, value);
					}
				}
				gpio.CheckExternalInterrupts ();
			}
			return true;
		};
	}

	public void CheckExternalInterrupts ()
	{

		for (var pin = 0; pin < 8; pin++) {
			var external = _portConfig.ExternalInterrupts?[pin];
			if (external == null) continue;
			var pinValue = (_lastPin & (1 << pin)) != 0;
			var eifr = external.Value.EIFR;
			var eimsk = external.Value.EIMSK;
			var index = external.Value.Index;
			var eicr = external.Value.EICR;
			var iscOffset = external.Value.IscOffset;
			var interrupt = external.Value.Interrupt;
			if ((_cpu.Data[eimsk] & (1 << index)) == 0 || pinValue) continue;
			var configuration = (byte)((_cpu.Data[eicr] >> iscOffset) & 0x3);
			if (configuration == (byte)InterruptMode.LowLevel) {
				_cpu.QueueInterrupt (new AvrInterruptConfig {
					Address = interrupt,
					FlagRegister = eifr,
					FlagMask = (byte)(1 << index),
					EnableRegister = eimsk,
					EnableMask = (byte)(1 << index),
					Constant = true,
				});
			}
		}
	}
	
	private void WriteGpio (byte value, byte ddr)
	{
		var newValue = (byte)((((value & _overrideMask) | _overrideValue) & ddr) | (value & ~ddr));
		var prevValue = _lastValue;
		if (newValue == prevValue && ddr == _lastDdr) return;
		_lastValue = newValue;
		_lastDdr = ddr;
		foreach (var listener in _listeners) {
			listener (newValue, prevValue);
		}
	}
}

public struct AvrExternalInterrupt
{
	public byte EICR;
	public byte EIMSK;
	public byte EIFR;
	
	public byte IscOffset;
	public byte Index;

	public byte Interrupt;
}

public struct AvrPinChangeInterrupt
{
	public byte PCIE;
	public byte PCICR;
	public byte PCIFR;
	public byte PCMSK;
	public byte PinChangeInterrupt;
	public byte Mask;
	public byte Offset;
}

public struct AvrPortConfig
{
	public ushort PIN;
	public ushort DDR;
	public ushort PORT;
	
	public AvrPinChangeInterrupt? PinChange;
	public AvrExternalInterrupt?[] ExternalInterrupts;
}

public enum PinState
{
	Low = 0,
	High = 1,
	Input = 2,
	InputPullup = 3,
}

/* This mechanism allows timers to override specific GPIO pins */
public enum PinOverrideMode {
	None,
	Enable,
	Set,
	Clear,
	Toggle,
}

public enum InterruptMode {
	LowLevel,
	Change,
	FallingEdge,
	RisingEdge,
}
