using System.Collections;
using AVR8Sharp.Cpu;

namespace AVR8Sharp.Peripherals;

public class AvrAdc
{
	public static ADCMuxConfiguration Atmega328Channels = new ADCMuxConfiguration {
		{ 0, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 0, } },
		{ 1, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 1, } },
		{ 2, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 2, } },
		{ 3, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 3, } },
		{ 4, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 4, } },
		{ 5, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 5, } },
		{ 6, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 6, } },
		{ 7, new AdcMuxInput { Type = AdcMuxInputType.SingleEnded, Channel = 7, } },
		{ 8, new AdcMuxInput { Type = AdcMuxInputType.Temperature, } },
		{ 14, new AdcMuxInput { Type = AdcMuxInputType.Constant, Voltage = 1.1, } },
		{ 15, new AdcMuxInput { Type = AdcMuxInputType.Constant, Voltage = 0, } },
	};
	public static AdcMuxInput FallbackMuxInput = new AdcMuxInput { Type = AdcMuxInputType.Constant, Voltage = 0, };
	public static AdcConfig AdcConfig = new AdcConfig {
		ADMUX = 0x7c,
		ADCSRA = 0x7a,
		ADCSRB = 0x7b,
		ADCL = 0x78,
		ADCH = 0x70,
		DIDR0 = 0x73,
		AdcInterrupt = 0x2a,
		NumChannels = 8,
		MuxInputMask = 0xf,
		MuxChannels = Atmega328Channels,
		AdcReferences = [
			AdcReference.AVCC,
			AdcReference.AREF,
			AdcReference.Internal1V1,
			AdcReference.Internal2V56
		],
	};
	
	public const int ADPS_MASK = 0x7;
	public const int ADIE = 0x8;
	public const int ADIF = 0x10;
	public const int ADSC = 0x40;
	public const int ADEN = 0x80;
	
	public const int MUX_MASK = 0x1f;
	public const int ADLAR = 0x20;
	public const int MUX5 = 0x8;
	public const int REFS2 = 0x8;
	public const int REFS_MASK = 0x3;
	public const int REFS_SHIFT = 6;

	Cpu.Cpu _cpu;
	bool _converting = false;
	int _conversionCycles = 25;
	AdcConfig _config;
	AvrInterruptConfig _adc;
	readonly double[] _channelValues;
	double avcc = 5.0;
	double aref = 5.0;
	
	public int SampleCycles { get { return _conversionCycles * Prescaler; } }
	public int Prescaler {
		get {
			var adcsra = _cpu.Data[_config.ADCSRA];
			var adps = adcsra & ADPS_MASK;
			switch (adps) {
				case 0:
				case 1:
					return 2;
				case 2:
					return 4;
				case 3:
					return 8;
				case 4:
					return 16;
				case 5:
					return 32;
				case 6:
					return 64;
				default:
					return 128;
			}
		}
	}
	public AdcReference ReferenceVoltageType {
		get {
			// const { ADMUX, adcReferences } = this.config;
			// let refs = (this.cpu.data[ADMUX] >> REFS_SHIFT) & REFS_MASK;
			// if (adcReferences.length > 4 && this.cpu.data[ADMUX] & REFS2) {
			//   refs |= 0x4;
			// }
			// return adcReferences[refs] ?? ADCReference.Reserved;
			var admux = _cpu.Data[_config.ADMUX];
			var refs = (admux >> REFS_SHIFT) & REFS_MASK;
			if (_config.AdcReferences.Length > 4 && (admux & REFS2) != 0) {
				refs |= 0x4;
			}
			return _config.AdcReferences[refs] ?? ReferenceVoltageType;
		}
	}
	public double ReferenceVoltage {
		get {
			switch (ReferenceVoltageType) {
				case AdcReference.AVCC:
					return avcc;
				case AdcReference.AREF:
					return aref;
				case AdcReference.Internal1V1:
					return 1.1;
				case AdcReference.Internal2V56:
					return 2.56;
				default:
					return avcc;
			}
		}
	}

	public AvrAdc (Cpu.Cpu cpu, AdcConfig config)
	{
		_cpu = cpu;
		_config = config;
		_adc = new AvrInterruptConfig {
			Address = _config.AdcInterrupt,
			FlagRegister = _config.ADCSRA,
			FlagMask = ADIF,
			EnableRegister = _config.ADCSRA,
			EnableMask = ADIE,
		};
		_channelValues = new double[config.NumChannels];
		_cpu.WriteHooks[config.ADCSRA] = (value, oldValue, _, _) => {
			if ((value & ADEN) != 0 && (oldValue & ADEN) == 0) {
				_conversionCycles = 25;
				
			}
			cpu.Data[config.ADCSRA] = value;
			cpu.UpdateInterruptEnable (_adc, value);
			if (!_converting && (value & ADSC) != 0) {
				if ((value & ADEN) == 0) {
					// Special case: reading while the ADC is not enabled should return 0
					cpu.AddClockEvent (() => CompleteAdcRead (0), SampleCycles);
					return true;
				}
				var channel = cpu.Data[config.ADMUX] & MUX_MASK;
				if ((cpu.Data[config.ADCSRB] & MUX5) != 0) {
					channel |= 0x20;
				}
				channel &= config.MuxInputMask;
				var muxInput = config.MuxChannels[(ushort)channel] ?? FallbackMuxInput;
				_converting = true;
				OnADCRead (muxInput);
				return true;
			}
			return false;
		};
	}
	
	public void OnADCRead (AdcMuxInput input)
	{
		// // Default implementation
		var voltage = 0.0;
		switch (input.Type) {
			case AdcMuxInputType.Constant:
				voltage = input.Voltage;
				break;
			case AdcMuxInputType.SingleEnded:
				voltage = _channelValues[input.Channel];
				break;
			case AdcMuxInputType.Differential:
				voltage = input.Gain *
					(_channelValues[input.PositiveChannel] -
					 _channelValues[input.NegativeChannel]);
				break;
			case AdcMuxInputType.Temperature:
				voltage = 0.378125; // 25 celcius
				break;
		}
		var rawValue = voltage / ReferenceVoltage * 1024;
		var result = Math.Min (Math.Max ((int)Math.Floor (rawValue), 0), 1023);
		_cpu.AddClockEvent (() => CompleteAdcRead (result), SampleCycles);
	}
	
	public void CompleteAdcRead (int result)
	{
		_converting = false;
		_conversionCycles = 13;
		var admux = _config.ADMUX;
		var adcl = _config.ADCL;
		var adch = _config.ADCH;
		var adcsra = _config.ADCSRA;
		if ((_cpu.Data[admux] & ADLAR) != 0) {
			_cpu.Data[adcl] = (byte)((result << 6) & 0xff);
			_cpu.Data[adch] = (byte)(result >> 2);
		} else {
			_cpu.Data[adcl] = (byte)(result & 0xff);
			_cpu.Data[adch] = (byte)((result >> 8) & 0x3);
		}
		_cpu.Data[adcsra] &= ~ADSC & 0xff;
		_cpu.SetInterruptFlag (_adc);
	}
}

public enum AdcReference
{
	AVCC = 0,
	AREF = 1,
	Internal1V1 = 2,
	Internal2V56 = 3,
	Reserved = 4,
}

public enum AdcMuxInputType
{
	SingleEnded = 0,
	Differential = 1,
	Constant = 2,
	Temperature = 3,
}

public struct AdcMuxInput
{
	public AdcMuxInputType Type;
	public int Channel;
	public double Voltage;
	public int PositiveChannel;
	public int NegativeChannel;
	public int Gain;
}

public class ADCMuxConfiguration : IEnumerable
{
	private readonly Dictionary<ushort, AdcMuxInput?> _inputs = [];
	public AdcMuxInput? this [ushort address] {
		get {
			return _inputs[address];
		}
		set {
			_inputs[address] = value;
		}
	}
	public void Add (ushort address, AdcMuxInput input)
	{
		_inputs[address] = input;
	}
	public IEnumerator GetEnumerator ()
	{
		return _inputs.GetEnumerator ();
	}
}

public class AdcConfig
{
	public byte ADMUX;
	public byte ADCSRA;
	public byte ADCSRB;
	public byte ADCL;
	public byte ADCH;
	public byte DIDR0;
	public byte AdcInterrupt;
	public byte NumChannels;
	public byte MuxInputMask;
	public ADCMuxConfiguration MuxChannels;
	public AdcReference?[] AdcReferences;
}

