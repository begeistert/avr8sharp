using AVR8Sharp.Cpu;
namespace AVR8Sharp.Peripherals;

public class AvrSpi
{
	const int SPCR_SPIE = 0x80; // SPI Interrupt Enable
	const int SPCR_SPE = 0x40; // SPI Enable
	const int SPCR_DORD = 0x20; // Data Order (0:MSB first / 1:LSB first)
	const int SPCR_MSTR = 0x10; // Master/Slave select
	const int SPCR_CPOL = 0x08; // Clock Polarity
	const int SPCR_CPHA = 0x04; // Clock Phase
	const int SPCR_SPR1 = 0x02; // SPI Clock Rate Select 1
	const int SPCR_SPR0 = 0x01; // SPI Clock Rate Select 0
	const int SPSR_SPR_MASK = SPCR_SPR1 | SPCR_SPR0; // SPI Clock Rate Select Mask
	
	const int SPSR_SPIF = 0x80; // SPI Interrupt Flag
	const int SPSR_WCOL = 0x40; // Write COLlision Flag
	const int SPSR_SPI2X = 0x01; // Double SPI Speed Bit
	
	const byte BitsPerByte = 8;
	
	public static AvrSpiConfig SpiConfig = new AvrSpiConfig {
		SpiInterrupt = 0x22,
		
		SPCR = 0x4c,
		SPSR = 0x4d,
		SPDR = 0x4e
	};
	
	Cpu.Cpu _cpu;
	AvrSpiConfig _config;
	uint _freqHz;
	
	bool _transmissionActive = false;
	
	AvrInterruptConfig _spi;
	
	public Func<byte, int> OnTransfer = _ => 0;
	public Action<byte> OnByte;
	public bool IsMaster {
		get {
			return (_cpu.Data[_config.SPCR] & SPCR_MSTR) != 0;
		}
	}
	public SpiDataOrder DataOrder {
		get {
			return (_cpu.Data[_config.SPCR] & SPCR_DORD) != 0 ? SpiDataOrder.LsbFirst : SpiDataOrder.MsbFirst;
		}
	}
	public int SpiMode {
		get {
			var cpha = _cpu.Data[_config.SPCR] & SPCR_CPHA;
			var cpol = _cpu.Data[_config.SPCR] & SPCR_CPOL;
			return (cpha != 0 ? 2 : 0) | (cpol != 0 ? 1 : 0);
		}
	}
	public int ClockDivider {
		get {
			var baseDivider = (_cpu.Data[_config.SPSR] & SPSR_SPI2X) != 0 ? 2 : 4;
			switch (_cpu.Data[_config.SPCR] & SPSR_SPR_MASK) {
				case 0b00:
					return baseDivider;
				case 0b01:
					return baseDivider * 4;
				case 0b10:
					return baseDivider * 16;
				case 0b11:
					return baseDivider * 32;
				default:
					throw new Exception ("Invalid divider value!");
			}
		}
	}
	public int TransferCycles {
		get {
			return BitsPerByte * ClockDivider;
		}
	}
	public long SpiFrequency {
		get {
			return _freqHz / ClockDivider;
		}
	}

	public AvrSpi (Cpu.Cpu cpu, AvrSpiConfig config, uint freqHz)
	{
		_cpu = cpu;
		_config = config;
		_freqHz = freqHz;
		
		_spi = new AvrInterruptConfig (
			address: _config.SpiInterrupt,
			flagRegister: _config.SPSR,
			flagMask: SPSR_SPIF,
			enableRegister: _config.SPCR,
			enableMask: SPCR_SPIE
		);
		
		OnByte = value => {
			var valueIn = OnTransfer(value);
			_cpu.AddClockEvent (() => CompleteTransfer (valueIn), TransferCycles);
		};
		
		_cpu.WriteHooks[_config.SPDR] = (value,_ ,_ ,_ ) => {
			if ((_cpu.Data[_config.SPCR] & SPCR_SPE) == 0) {
				// SPI not enabled, ignore write
				return false;
			}
			
			// Write collision
			if (_transmissionActive) {
				_cpu.Data[_config.SPSR] |= SPSR_WCOL;
				return true;
			}
			
			// Clear write collision / interrupt flags
			_cpu.Data[_config.SPSR] &= ~SPSR_WCOL & 0xFF;
			_cpu.ClearInterrupt (_spi);
			
			_transmissionActive = true;
			OnByte(value);
			return true;
		};
		
		_cpu.WriteHooks[_config.SPCR] = (value, _, _, _) => {
			_cpu.UpdateInterruptEnable (_spi, value);
			return false;
		};
		
		_cpu.WriteHooks[_config.SPSR] = (value, _, _, _) => {
			_cpu.Data[_config.SPSR] = value;
			_cpu.ClearInterruptByFlag (_spi, value);
			return false;
		};
	}

	public void CompleteTransfer (int receivedByte)
	{
		_cpu.Data[_config.SPDR] = (byte)receivedByte;
		_cpu.SetInterruptFlag (_spi);
		_transmissionActive = false;
	}
}

public class AvrSpiConfig
{
	public byte SpiInterrupt;
	
	public byte SPCR;
	public byte SPSR;
	public byte SPDR;
}

public enum SpiDataOrder
{
	MsbFirst,
	LsbFirst
}
