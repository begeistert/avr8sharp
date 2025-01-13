using AVR8Sharp.Cpu;
namespace AVR8Sharp.Peripherals;

public class AvrUsi
{
	const int USICR = 0x2d;
	const int USISR = 0x2e;
	const int USIDR = 0x2f;
	const int USIBR = 0x30;
	
	// USISR bits
	const int USICNT_MASK = 0x0f;
	const int USIDC = 1 << 4;
	const int USIPF = 1 << 5;
	const int USIOIF = 1 << 6;
	const int USISIF = 1 << 7;
	
	// USICR bits
	const int USITC = 1 << 0;
	const int USICLK = 1 << 1;
	const int USICS0 = 1 << 2;
	const int USICS1 = 1 << 3;
	const int USIWM0 = 1 << 4;
	const int USIWM1 = 1 << 5;
	const int USIOIE = 1 << 6;
	const int USISIE = 1 << 7;
	
	static AvrInterruptConfig _start = new AvrInterruptConfig (
		address: 0xd,
		flagRegister: USISR,
		flagMask: USISIF,
		enableRegister: USICR,
		enableMask: USISIE
	);
	private static AvrInterruptConfig _overflow = new AvrInterruptConfig (
		address: 0xe,
		flagRegister: USISR,
		flagMask: USIOIF,
		enableRegister: USICR,
		enableMask: USIOIE
	);
	
	private Cpu.Cpu _cpu;
	private AvrIoPort _port;
	private int _portPin;
	private int _dataPin;
	private int _clockPin;
	
	private ushort _PIN;
	private ushort _PORT;

	public AvrUsi (Cpu.Cpu cpu, AvrIoPort port, int portPin, int dataPin, int clockPin)
	{
		_cpu = cpu;
		_port = port;
		_portPin = portPin;
		_dataPin = dataPin;
		_clockPin = clockPin;

		_PIN = (ushort)portPin;
		_PORT = (ushort)(_PIN + 1);
		
		port.AddListener ((value, _) => {
			var twoWire = (_cpu.Data[USICR] & USIWM1) == USIWM1;
			if (twoWire) {
				if ((value & (1 << _clockPin)) != 0 && (value & (1 << _dataPin)) == 0) {
					// Start condition detected
					_cpu.SetInterruptFlag (_start);
				}
				if ((value & (1 << _clockPin)) != 0 && (value & (1 << _dataPin)) != 0) {
					// Stop condition detected
					_cpu.Data[USISR] |= USIPF;
				}
			}
		});
		
		_cpu.WriteHooks[USISR] = (value, _, _, _) => {
			var writeClearMask = USISIF | USIOIF | USIPF;
			_cpu.Data[USISR] = (byte)((_cpu.Data[USISR] & writeClearMask & ~value) | (value & 0xf));
			_cpu.ClearInterruptByFlag (_start, value);
			_cpu.ClearInterruptByFlag (_overflow, value);
			return true;
		};
		
		_cpu.WriteHooks[USICR] = (value, _, _, _) => {
			_cpu.Data[USICR] = (byte)(value & ~(USICLK | USITC));
			_cpu.UpdateInterruptEnable (_start, value);
			_cpu.UpdateInterruptEnable (_overflow, value);
			var clockSrc = value & ((USICS1 | USICS0) >> 2);
			var mode = value & ((USIWM1 | USIWM0) >> 4);
			var usiClk = value & USICLK;
			_port.OpenCollector = (byte)(mode >= 2 ? (1 << _dataPin) : 0);
			var inputValue = (_cpu.Data[_PIN] & (1 << _dataPin)) != 0 ? 1 : 0;
			if (usiClk != 0 && clockSrc == 0) {
				Shift (inputValue);
				Count ();
			}
			if ((value & USITC) != 0) {
				_cpu.WriteHooks[_PIN]?.Invoke((byte)(1 << _clockPin), _cpu.Data[_PIN], _PIN, 0xff);
				var newValue = _cpu.Data[_PIN] & (1 << _clockPin);
				if (usiClk != 0 && (clockSrc == 2 || clockSrc == 3)) {
					if (clockSrc == 2 && newValue != 0) {
						Shift (inputValue);
					}
					if (clockSrc == 3 && newValue == 0) {
						Shift (inputValue);
					}
					Count ();
				}
				return true;
			}
			return false;
		};
	}
	
	private void UpdateOutput ()
	{
		var oldValue = _cpu.Data[_PORT];
		var newValue = (_cpu.Data[USIDR] & 0x80) != 0 ? oldValue | (1 << _dataPin) : oldValue & ~(1 << _dataPin);
		_cpu.WriteHooks[_PORT]?.Invoke((byte)newValue, oldValue, _PORT, 0xff);
		if ((newValue & 0x80) != 0 && (_cpu.Data[_PIN] & 0x80) == 0) {
			_cpu.Data[USISR] |= USIDC;
		} else {
			_cpu.Data[USISR] &= ~USIDC & 0xff;
		}
	}

	private void Count ()
	{
		var counter = (_cpu.Data[USISR] + 1) & USICNT_MASK;
		_cpu.Data[USISR] = (byte)((_cpu.Data[USISR] & ~USICNT_MASK) | counter);
		if (counter == 0) {
			_cpu.Data[USIBR] = _cpu.Data[USIDR];
			_cpu.SetInterruptFlag (_overflow);
		}
	}
	
	private void Shift (int inputValue)
	{
		_cpu.Data[USIDR] = (byte)((_cpu.Data[USIDR] << 1) | inputValue);
		UpdateOutput ();
	}
}
