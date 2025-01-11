#nullable enable

namespace AVR8Sharp.Cpu;

public class Cpu
{
	#region Constants
	const int RegisterSpace = 0x100;
	const int MaxInterrupts = 128;
	#endregion

	#region Private Properties
	readonly byte[] _data;
	readonly AvrInterruptConfig?[] _pendingInterrupts = new AvrInterruptConfig?[MaxInterrupts];
	AvrClockEventEntry? _nextClockEvent = null;
	Stack<AvrClockEventEntry?> _clockEventPool = [];
	short _nextInterrupt = -1;
	short _maxInterrupt = 0;
    #endregion

	#region Public Properties
	public Action OnWatchdogReset = () => { };
	public byte[] Data { get => _data; }
	public DataView DataView { get; }
	public Memory<ushort> ProgramMemory { get; }
	public byte[] ProgBytes { get; }
	public CpuMemoryReadHooks ReadHooks { get; } = new CpuMemoryReadHooks ();
	public CpuMemoryWriteHooks WriteHooks { get; } = new CpuMemoryWriteHooks ();
	public bool PC22Bits { get; }
	public uint PC { get; set; }
	public int Cycles { get; set; }
	public ushort SP {
		get {
			return DataView.GetUint16 (93, true);
		}
		set {
			DataView.SetUint16 (93, value, true);
		}
	}
	public byte SREG {
		get {
			return _data[95];
		}
	}
	public bool InterruptsEnabled {
		get {
			return (SREG & 0x80) != 0;
		}
	}
	#endregion

	public Cpu (ushort[] program, int sramBytes = 8192)
	{
		_data = new byte[sramBytes + RegisterSpace];
		DataView = new DataView (ref _data);
		ProgramMemory = new Memory<ushort> (program);
		ProgBytes = new byte[program.Length * 2];
		// Copy the values using array.copy
		Buffer.BlockCopy (program.ToArray (), 0, ProgBytes, 0, program.Length * 2);
		
		// Whether the program counter (PC) can address 22 bits (the default is 16)
		PC22Bits = program.Length > 0x20000;
		
		// Reset the CPU
		Reset ();
	}
	
	public void Reset ()
	{
		// Reset the CPU
		// this.SP = this.Data.Count - 1;
		PC = 0;
		for (var i = 0; i < _pendingInterrupts.Length; i++) {
			_pendingInterrupts[i] = null;
		}
		_nextInterrupt = -1;
		_nextClockEvent = null;
	}

	public byte ReadData (ushort address)
	{
		var hook = ReadHooks[address];
		if (address > 32 && hook != null) 
			return hook(address);
		return Data[address];
	}
	
	public void WriteData (ushort address, byte value, byte mask = 0xff)
	{
		var hook = WriteHooks[address];
		if (hook != null) {
			if (hook(value, Data[address], address, mask)) {
				return;
			}
		} 
		Data[address] = value;
	}

	public void SetInterruptFlag (AvrInterruptConfig interrupt)
	{
		// Original Javascript code
		// const { flagRegister, flagMask, enableRegister, enableMask } = interrupt;
		// if (interrupt.inverseFlag) {
		//   this.data[flagRegister] &= ~flagMask;
		// } else {
		//   this.data[flagRegister] |= flagMask;
		// }
		// if (this.data[enableRegister] & enableMask) {
		//   this.queueInterrupt(interrupt);
		// }
		if (interrupt.InverseFlag) {
			_data[interrupt.FlagRegister] &= (byte)~interrupt.FlagMask;
		}
		else {
			_data[interrupt.FlagRegister] |= interrupt.FlagMask;
		}
		if ((_data[interrupt.EnableRegister] & interrupt.EnableMask) != 0) {
			QueueInterrupt (interrupt);
		}
	}

	public void UpdateInterruptEnable (AvrInterruptConfig interrupt, byte registerValue)
	{
		// Original Javascript code
		// const { enableMask, flagRegister, flagMask, inverseFlag } = interrupt;
		// if (registerValue & enableMask) {
		//   const bitSet = this.data[flagRegister] & flagMask;
		//   if (inverseFlag ? !bitSet : bitSet) {
		//     this.queueInterrupt(interrupt);
		//   }
		// } else {
		//   this.clearInterrupt(interrupt, false);
		// }
		if ((registerValue & interrupt.EnableMask) != 0) {
			var bitSet = (_data[interrupt.FlagRegister] & interrupt.FlagMask) != 0;
			if (interrupt.InverseFlag ? !bitSet : bitSet) {
				QueueInterrupt (interrupt);
			}
		} else {
			ClearInterrupt (interrupt, false);
		}
	}
	
	public void QueueInterrupt (AvrInterruptConfig interrupt)
	{
		// Original Javascript code
		// const { address } = interrupt;
		// this.pendingInterrupts[address] = interrupt;
		// if (this.nextInterrupt === -1 || this.nextInterrupt > address) {
		//   this.nextInterrupt = address;
		// }
		// if (address > this.maxInterrupt) {
		//   this.maxInterrupt = address;
		// }
		_pendingInterrupts[interrupt.Address] = interrupt;
		if (_nextInterrupt == -1 || _nextInterrupt > interrupt.Address) {
			_nextInterrupt = interrupt.Address;
		}
		if (interrupt.Address > _maxInterrupt) {
			_maxInterrupt = interrupt.Address;
		}
	}
	
	public void ClearInterrupt (AvrInterruptConfig interrupt, bool clearFlag = true)
	{
		// Original Javascript code
		// if (clearFlag) {
		//   this.data[flagRegister] &= ~flagMask;
		// }
		// const { pendingInterrupts, maxInterrupt } = this;
		// if (!pendingInterrupts[address]) {
		//   return;
		// }
		// pendingInterrupts[address] = null;
		// if (this.nextInterrupt === address) {
		//   this.nextInterrupt = -1;
		//   for (let i = address + 1; i <= maxInterrupt; i++) {
		//     if (pendingInterrupts[i]) {
		//       this.nextInterrupt = i;
		//       break;
		//     }
		//   }
		// }
		if (clearFlag) {
			_data[interrupt.FlagRegister] &= (byte)~interrupt.FlagMask;
		}
		if (_pendingInterrupts[interrupt.Address] == null) {
			return;
		}
		_pendingInterrupts[interrupt.Address] = null;
		if (_nextInterrupt != interrupt.Address) return;
		_nextInterrupt = -1;
		for (var i = interrupt.Address + 1; i <= _maxInterrupt; i++) {
			if (_pendingInterrupts[i] == null) continue;
			_nextInterrupt = (short)i;
			break;
		}
	}
	
	public void ClearInterruptByFlag (AvrInterruptConfig interrupt, byte registerValue)
	{
		// Original Javascript code
		// const { flagRegister, flagMask } = interrupt;
		// if (registerValue & flagMask) {
		//   this.data[flagRegister] &= ~flagMask;
		//   this.clearInterrupt(interrupt);
		// }
		if ((registerValue & interrupt.FlagMask) == 0) return;
		_data[interrupt.FlagRegister] &= (byte)~interrupt.FlagMask;
		ClearInterrupt (interrupt);
	}
	
	public Action AddClockEvent (Action callback, int cycles)
	{
		// Original Javascript code
		// const { clockEventPool } = this;
		// cycles = this.cycles + Math.max(1, cycles);
		// const maybeEntry = clockEventPool.pop();
		// const entry: AVRClockEventEntry = maybeEntry ?? { cycles, callback, next: null };
		// entry.cycles = cycles;
		// entry.callback = callback;
		// let { nextClockEvent: clockEvent } = this;
		// let lastItem = null;
		// while (clockEvent && clockEvent.cycles < cycles) {
		//   lastItem = clockEvent;
		//   clockEvent = clockEvent.next;
		// }
		// if (lastItem) {
		//   lastItem.next = entry;
		//   entry.next = clockEvent;
		// } else {
		//   this.nextClockEvent = entry;
		//   entry.next = clockEvent;
		// }
		// return callback;
		cycles = Cycles + Math.Max (1, cycles);
		var maybeEntry = _clockEventPool.Count > 0 ? _clockEventPool.Pop () : null;
		var entry = maybeEntry ?? new AvrClockEventEntry { Cycles = cycles, Callback = callback, Next = null, };
		entry.Cycles = cycles;
		entry.Callback = callback;
		var clockEvent = _nextClockEvent;
		AvrClockEventEntry? lastItem = null;
		while (clockEvent != null && clockEvent.Cycles < cycles) {
			lastItem = clockEvent;
			clockEvent = clockEvent.Next;
		}
		if (lastItem != null) {
			lastItem.Next = entry;
		} else {
			_nextClockEvent = entry;
		}
		entry.Next = clockEvent;
		return callback;
	}

	public bool UpdateClockEvent (Action callback, int cycles)
	{
		// Original Javascript code
		// if (this.clearClockEvent(callback)) {
		//   this.addClockEvent(callback, cycles);
		//   return true;
		// }
		// return false;
		if (ClearClockEvent (callback)) {
			AddClockEvent (callback, cycles);
			return true;
		}
		return false;
	}
	
	public bool ClearClockEvent (Action callback)
	{
		// Original Javascript code
		// let { nextClockEvent: clockEvent } = this;
		// if (!clockEvent) {
		//   return false;
		// }
		// const { clockEventPool } = this;
		// let lastItem = null;
		// while (clockEvent) {
		//   if (clockEvent.callback === callback) {
		//     if (lastItem) {
		//       lastItem.next = clockEvent.next;
		//     } else {
		//       this.nextClockEvent = clockEvent.next;
		//     }
		//     if (clockEventPool.length < 10) {
		//       clockEventPool.push(clockEvent);
		//     }
		//     return true;
		//   }
		//   lastItem = clockEvent;
		//   clockEvent = clockEvent.next;
		// }
		// return false;
		var clockEvent = _nextClockEvent;
		if (clockEvent == null) {
			return false;
		}
		AvrClockEventEntry? lastItem = null;
		while (clockEvent != null) {
			if (clockEvent.Callback == callback) {
				if (lastItem != null) {
					lastItem.Next = clockEvent.Next;
				} else {
					_nextClockEvent = clockEvent.Next;
				}
				if (_clockEventPool.Count < 10) {
					_clockEventPool.Push (clockEvent);
				}
				return true;
			}
			lastItem = clockEvent;
			clockEvent = clockEvent.Next;
		}
		return false;
	}

	public void Tick ()
	{
		// Original Javascript code
		// const { nextClockEvent } = this;
		// if (nextClockEvent && nextClockEvent.cycles <= this.cycles) {
		//   nextClockEvent.callback();
		//   this.nextClockEvent = nextClockEvent.next;
		//   if (this.clockEventPool.length < 10) {
		//     this.clockEventPool.push(nextClockEvent);
		//   }
		// }

		// const { nextInterrupt } = this;
		// if (this.interruptsEnabled && nextInterrupt >= 0) {
		//   // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
		//   const interrupt = this.pendingInterrupts[nextInterrupt]!;
		//   avrInterrupt(this, interrupt.address);
		//   if (!interrupt.constant) {
		//     this.clearInterrupt(interrupt);
		//   }
		// }

		if (_nextClockEvent != null && _nextClockEvent.Cycles <= Cycles) {
			_nextClockEvent.Callback ();
			_nextClockEvent = _nextClockEvent.Next;
			if (_clockEventPool.Count < 10) {
				_clockEventPool.Push (_nextClockEvent);
			}
		}

		if (!InterruptsEnabled || _nextInterrupt < 0) return;
		var interrupt = _pendingInterrupts[_nextInterrupt];
		if (interrupt == null) return;
		var value = interrupt.Value;
		Interrupt.AvrInterrupt (this, value.Address);
		if (!value.Constant) {
			ClearInterrupt (value);
		}
	}
}

public class CpuMemoryReadHooks
{
	private readonly Dictionary<ushort, Func<ushort, byte>> _hooks = [];
	public Func<ushort, byte>? this [ushort address] {
		get {
			return _hooks.GetValueOrDefault (address);
		}
		set {
			if (value != null) {
				_hooks[address] = value;
			} else {
				_hooks.Remove (address);
			}
		}
	}
}

public class CpuMemoryWriteHooks
{
	private readonly Dictionary<ushort, Func<byte, byte, ushort, byte, bool>> _hooks = [];
	public Func<byte, byte, ushort, byte, bool>? this [ushort address] {
		get {
			return _hooks.GetValueOrDefault (address);
		}
		set {
			if (value != null) {
				_hooks[address] = value;
			} else {
				_hooks.Remove (address);
			}
		}
	}
}

public struct AvrInterruptConfig
{
	public byte Address;
	public ushort EnableRegister;
	public byte EnableMask;
	public ushort FlagRegister;
	public byte FlagMask;
	public bool Constant;
	public bool InverseFlag;
}

public class AvrClockEventEntry
{
	public int Cycles;
	public Action Callback = () => { };
	public AvrClockEventEntry? Next;
}

public class DataView (ref byte[] data)
{
	private readonly byte[] _data = data;
	public sbyte GetInt8(int byteOffset)
	{
		return (sbyte)_data[byteOffset];
	}
	public void SetInt8(int byteOffset, sbyte value)
	{
		_data[byteOffset] = (byte)value;
	}
	public short GetInt16(int byteOffset, bool littleEndian = false)
	{
		if (littleEndian) {
			return (short)(_data[byteOffset] | _data[byteOffset + 1] << 8);
		}
		return (short)(_data[byteOffset] << 8 | _data[byteOffset + 1]);
	}
	public void SetInt16(int byteOffset, short value, bool littleEndian = false)
	{
		if (littleEndian) {
			_data[byteOffset] = (byte)(value & 0xff);
			_data[byteOffset + 1] = (byte)(value >> 8);
		} else {
			_data[byteOffset] = (byte)(value >> 8);
			_data[byteOffset + 1] = (byte)(value & 0xff);
		}
	}
	public ushort GetUint16(int byteOffset, bool littleEndian = false)
	{
		if (littleEndian) {
			return (ushort)(_data[byteOffset] | _data[byteOffset + 1] << 8);
		}
		return (ushort)(_data[byteOffset] << 8 | _data[byteOffset + 1]);
	}
	public void SetUint16(int byteOffset, ushort value, bool littleEndian = false)
	{
		if (littleEndian) {
			_data[byteOffset] = (byte)(value & 0xff);
			_data[byteOffset + 1] = (byte)(value >> 8);
		} else {
			_data[byteOffset] = (byte)(value >> 8);
			_data[byteOffset + 1] = (byte)(value & 0xff);
		}
	}
}
