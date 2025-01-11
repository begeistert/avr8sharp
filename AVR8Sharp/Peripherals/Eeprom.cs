using AVR8Sharp.Cpu;
namespace AVR8Sharp.Peripherals;

public class AvrEeprom
{
	public const byte EERE = 1 << 0;
	public const byte EEPE = 1 << 1;
	public const byte EEMPE = 1 << 2;
	public const byte EERIE = 1 << 3;
	public const byte EEPM0 = 1 << 4;
	public const byte EEPM1 = 1 << 5;
	public const byte EECR_WRITE_MASK = EERE | EEMPE | EERIE | EEPM0 | EEPM1;
	
	public static AvrEepromConfig EepromConfig = new AvrEepromConfig {
		EepromReadyInterrupt = 0x2c,
		EECR = 0x3f,
		EEDR = 0x40,
		EEARL = 0x41,
		EEARH = 0x42,
		EraseCycles = 28800,
		WriteCycles = 28800,
	};
	
	private uint _writeEnabledCycles = 0;
	private uint _writeCompleteCycles = 0;
	private AvrEepromConfig _config;
	AvrInterruptConfig _eer;
	Cpu.Cpu _cpu;
	IEepromBackend _backend;

	public AvrEeprom (Cpu.Cpu cpu, IEepromBackend backend, AvrEepromConfig? config = null)
	{
		_cpu = cpu;
		_backend = backend;
		_config = config ?? EepromConfig;
		_eer = new AvrInterruptConfig {
			Address = _config.EepromReadyInterrupt,
			FlagRegister = _config.EECR,
			FlagMask = EEPE,
			EnableRegister = _config.EECR,
			EnableMask = EERIE,
			Constant = true,
			InverseFlag = true,
		};
		
		// this.cpu.writeHooks[this.config.EECR] = (eecr) => {
		//   const { EEARH, EEARL, EECR, EEDR } = this.config;

		//   const addr = (this.cpu.data[EEARH] << 8) | this.cpu.data[EEARL];

		//   this.cpu.data[EECR] = (this.cpu.data[EECR] & ~EECR_WRITE_MASK) | (eecr & EECR_WRITE_MASK);
		//   this.cpu.updateInterruptEnable(this.EER, eecr);

		//   if (eecr & EERE) {
		//     this.cpu.clearInterrupt(this.EER);
		//   }

		//   if (eecr & EEMPE) {
		//     const eempeCycles = 4;
		//     this.writeEnabledCycles = this.cpu.cycles + eempeCycles;
		//     this.cpu.addClockEvent(() => {
		//       this.cpu.data[EECR] &= ~EEMPE;
		//     }, eempeCycles);
		//   }

		//   // Read
		//   if (eecr & EERE) {
		//     this.cpu.data[EEDR] = this.backend.readMemory(addr);
		//     // When the EEPROM is read, the CPU is halted for four cycles before the
		//     // next instruction is executed.
		//     this.cpu.cycles += 4;
		//     return true;
		//   }

		//   // Write
		//   if (eecr & EEPE) {
		//     //  If EEMPE is zero, setting EEPE will have no effect.
		//     if (this.cpu.cycles >= this.writeEnabledCycles) {
		//       this.cpu.data[EECR] &= ~EEPE;
		//       return true;
		//     }
		//     // Check for write-in-progress
		//     if (this.cpu.cycles < this.writeCompleteCycles) {
		//       return true;
		//     }

		//     const eedr = this.cpu.data[EEDR];

		//     this.writeCompleteCycles = this.cpu.cycles;

		//     // Erase
		//     if (!(eecr & EEPM1)) {
		//       this.backend.eraseMemory(addr);
		//       this.writeCompleteCycles += this.config.eraseCycles;
		//     }
		//     // Write
		//     if (!(eecr & EEPM0)) {
		//       this.backend.writeMemory(addr, eedr);
		//       this.writeCompleteCycles += this.config.writeCycles;
		//     }

		//     this.cpu.data[EECR] |= EEPE;

		//     this.cpu.addClockEvent(() => {
		//       this.cpu.setInterruptFlag(this.EER);
		//     }, this.writeCompleteCycles - this.cpu.cycles);

		//     // When EEPE has been set, the CPU is halted for two cycles before the
		//     // next instruction is executed.
		//     this.cpu.cycles += 2;
		//   }

		//   return true;
		// };
		
		cpu.WriteHooks[_config.EECR] = (eecr, _, _, _) => {
			var addr = (ushort)((cpu.Data[_config.EEARH] << 8) | cpu.Data[_config.EEARL]);
			
			cpu.Data[_config.EECR] = (byte)((cpu.Data[_config.EECR] & ~EECR_WRITE_MASK) | (eecr & EECR_WRITE_MASK));
			cpu.UpdateInterruptEnable (_eer, eecr);
			
			if ((eecr & EERE) != 0) {
				cpu.ClearInterrupt (_eer);
			}
			
			if ((eecr & EEMPE) != 0) {
				var eempeCycles = 4;
				_writeEnabledCycles = (uint)(cpu.Cycles + eempeCycles);
				cpu.AddClockEvent (() => {
					cpu.Data[_config.EECR] &= ~EEMPE & 0xFF;
				}, eempeCycles);
			}
			
			// Read
			if ((eecr & EERE) != 0) {
				cpu.Data[_config.EEDR] = _backend.ReadMemory (addr);
				// When the EEPROM is read, the CPU is halted for four cycles before the
				// next instruction is executed.
				cpu.Cycles += 4;
				return true;
			}
			
			// Write
			if ((eecr & EEPE) != 0) {
				// If EEMPE is zero, setting EEPE will have no effect.
				if (cpu.Cycles >= _writeEnabledCycles) {
					cpu.Data[_config.EECR] &= ~EEPE & 0xFF;
					return true;
				}
				
				if (cpu.Cycles < _writeCompleteCycles) {
					return true;
				}
				
				var eedr = cpu.Data[_config.EEDR];
				
				_writeCompleteCycles = (uint)cpu.Cycles;
				
				// Erase
				if ((eecr & EEPM1) == 0) {
					_backend.EraseMemory (addr);
					_writeCompleteCycles += _config.EraseCycles;
				}
				
				// Write
				if ((eecr & EEPM0) == 0) {
					_backend.WriteMemory (addr, eedr);
					_writeCompleteCycles += _config.WriteCycles;
				}
				
				cpu.Data[_config.EECR] |= EEPE;
				
				cpu.AddClockEvent (() => {
					cpu.SetInterruptFlag (_eer);
				}, (int)(_writeCompleteCycles - cpu.Cycles));
				
				cpu.Cycles += 2;
			}
			
			return true;
			
		};
	}
}

public interface IEepromBackend
{
	byte ReadMemory (uint address);
	void WriteMemory (uint address, byte value);
	void EraseMemory (uint address);
}

public class EepromMemoryBackend : IEepromBackend
{
	private readonly byte[] _memory;
	public EepromMemoryBackend (uint size)
	{
		_memory = new byte[size];
		// Fill with 0xFF using C# 8.0 feature
		_memory.AsSpan().Fill(0xFF);
	}
	
	public byte ReadMemory (uint address)
	{
		return _memory[address];
	}
	
	public void WriteMemory (uint address, byte value)
	{
		_memory[address] &= value;
	}
	
	public void EraseMemory (uint address)
	{
		_memory[address] = 0xFF;
	}
}

public struct AvrEepromConfig
{
	public byte EepromReadyInterrupt;
	
	public byte EECR;
	public byte EEDR;
	public byte EEARL;
	public byte EEARH;
	
	public uint EraseCycles;
	public uint WriteCycles;
}
