namespace AVR8Sharp.Peripherals;

public class AvrClock
{
	public const int CLKPCE = 128;

	public static AvrClockConfig ClockConfig = new AvrClockConfig { CLKPR = 0x61, };
	public static int[] Prescalers = [
		1, 2, 4, 8, 16, 32, 64, 128, 256, 
		
		// The following values are "reserved" according to the datasheet, so we measured
		// with a scope to figure them out (on ATmega328p)
		2, 4, 8, 16, 32, 64, 128, 
	];

	int _clockEnabledCycles = 0;
	int _prescalerValue = 0;
	int _cyclesDelta = 0;
	uint _baseFreqHz = 0;
	Cpu.Cpu _cpu;
	
	public uint Frequency {
		get {
			return (uint)(_baseFreqHz / _prescalerValue);
		}
	}
	
	public int Prescaler {
		get {
			return _prescalerValue;
		}
	}
	
	public uint TimeNanos {
		get {
			return (uint)((_cpu.Cycles + _cyclesDelta) / (double)Frequency * 1e9);
		}
	}
	
	public uint TimeMicros {
		get {
			return (uint)((_cpu.Cycles + _cyclesDelta) / (double)Frequency * 1e6);
		}
	}
	
	public uint TimeMillis {
		get {
			return (uint)((_cpu.Cycles + _cyclesDelta) / (double)Frequency * 1e3);
		}
	}

	public AvrClock (Cpu.Cpu cpu, uint baseFreqHz, AvrClockConfig clockConfig)
	{
		_baseFreqHz = baseFreqHz;
		_cpu = cpu;
		
		cpu.WriteHooks[clockConfig.CLKPR] = (clkpr, _,_ ,_) => {
			if ((_clockEnabledCycles == 0 || _clockEnabledCycles < cpu.Cycles) && clkpr == CLKPCE) {
				_clockEnabledCycles = cpu.Cycles + 4;
			} else if (_clockEnabledCycles != 0 && _clockEnabledCycles >= cpu.Cycles) {
				_clockEnabledCycles = 0;
				var index = clkpr & 0xf;
				var oldPrescaler = _prescalerValue;
				_prescalerValue = Prescalers[index];
				cpu.Data[clockConfig.CLKPR] = (byte)index;
				if (oldPrescaler != _prescalerValue) {
					_cyclesDelta = (cpu.Cycles + _cyclesDelta) * (oldPrescaler / _prescalerValue) - cpu.Cycles;
				}
			}
			return true;
		};
		
	}
}

public struct AvrClockConfig
{
	public byte CLKPR;
}

