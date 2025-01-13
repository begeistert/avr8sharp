namespace AVR8Sharp.Cpu;

public static class Instruction
{
	public static void AvrInstruction (Cpu cpu)
	{
		var opcode = cpu.ProgramMemory[(int)cpu.PC];
		if ((opcode & 0xfc00) == 0x1c00) {
			/* ADC, 0001 11rd dddd rrrr */
			var d = cpu.Data[(opcode & 0x1f0) >> 4];
			var r = cpu.Data[(opcode & 0xf) | (opcode & 0x200) >> 5];
			var sum = d + r + (cpu.Data[95] & 1);
			var R = (byte)(sum & 255);
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((R ^ r) & (d ^ R) & 128) != 0 ? 8 : 0;
			sreg |= (sreg >> 2 & 1 ^ sreg >> 3 & 1) != 0 ? 0x10 : 0;
			sreg |= (sum & 256) != 0 ? 1 : 0;
			sreg |= (1 & (d & r | r & ~R | ~R & d)) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		}
		else if ((opcode & 0xfc00) == 0xc00) {
			/* ADD, 0000 11rd dddd rrrr */
			var d = cpu.Data[(opcode & 0x1f0) >> 4];
			var r = cpu.Data[(opcode & 0xf) | (opcode & 0x200) >> 5];
			var R = (byte)(d + r);
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((R ^ r) & (R ^ d) & 128) != 0 ? 8 : 0;
			sreg |= (sreg >> 2 & 1 ^ sreg >> 3 & 1) != 0 ? 0x10 : 0;
			sreg |= (d + r & 256) != 0 ? 1 : 0;
			sreg |= (1 & (d & r | r & ~R | ~R & d)) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		}else if ((opcode & 0xff00) == 0x9600) {
			/* ADIW, 1001 0110 KKdd KKKK */
			var addr = (ushort)(2 * ((opcode & 0x30) >> 4) + 24);
			var value = cpu.DataView.GetUint16(addr, true);
			var R = (ushort)(value + ((opcode & 0xf) | ((opcode & 0xc0) >> 2)) & 0xffff);
			cpu.DataView.SetUint16(addr, R, true);
			var sreg = cpu.Data[95] & 0xe0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (0x8000 & R) != 0 ? 4 : 0;
			sreg |= (~value & R & 0x8000) != 0 ? 8 : 0;
			sreg |= ((sreg >> 2 & 1) ^ (sreg >> 3 & 1)) != 0 ? 0x10 : 0;
			sreg |= (~R & value & 0x8000) != 0 ? 1 : 0;
			cpu.Data[95] = (byte)sreg;
			cpu.Cycles++;
		}
		else if ((opcode & 0xfc00) == 0x2000) {
			/* AND, 0010 00rd dddd rrrr */
			var R = (byte)(cpu.Data[(opcode & 0x1f0) >> 4] & cpu.Data[(opcode & 0xf) | (opcode & 0x200) >> 5]);
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			var sreg = cpu.Data[95] & 0xe1;
			sreg |= R == 0 ? 2 : 0;	
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((sreg >> 2 & 1) ^ (sreg >> 3 & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		}
		else if ((opcode & 0xf000) == 0x7000) {
			/* ANDI, 0111 KKKK dddd KKKK */
			var R = (byte)(cpu.Data[((opcode & 0xf0) >> 4) + 16] & ((opcode & 0xf) | ((opcode & 0xf00) >> 4)));
			cpu.Data[((opcode & 0xf0) >> 4) + 16] = R;
			var sreg = cpu.Data[95] & 0xe1;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((sreg >> 2 & 1) ^ (sreg >> 3 & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		}
		else if ((opcode & 0xfe0f) == 0x9405) {
			/* ASR, 1001 010d dddd 0101 */
			var value = cpu.Data[(opcode & 0x1f0) >> 4];
			var R = (byte)((value >> 1) | (128 & value));
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			var sreg = cpu.Data[95] & 0xe0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= value & 1;
			sreg |= (((sreg >> 2) & 1) ^ (sreg & 1)) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		}
		else if ((opcode & 0xff8f) == 0x9488) {
			/* BCLR, 1001 0100 1sss 1000 */
			cpu.Data[95] &= (byte)~(1 << ((opcode & 0x70) >> 4));
		}
		else if ((opcode & 0xfe08) == 0xf800) {
			/* BLD, 1111 100d dddd 0bbb */
			var b = opcode & 7;
			var d = (opcode & 0x1f0) >> 4;
			cpu.Data[d] = (byte)((~(1 << b) & cpu.Data[d]) | (((cpu.Data[95] >> 6) & 1) << b));
		}
		else if ((opcode & 0xfc00) == 0xf400) {
			/* BRBC, 1111 01kk kkkk ksss */
			if ((cpu.Data[95] & (1 << (opcode & 7))) == 0) {
				cpu.PC = (ushort)(cpu.PC + (((opcode & 0x1f8) >> 3) - ((opcode & 0x200) != 0 ? 0x40 : 0)));
				cpu.Cycles++;
			}
		}
		else if ((opcode & 0xfc00) == 0xf000) {
			/* BRBS, 1111 00kk kkkk ksss */
			if ((cpu.Data[95] & (1 << (opcode & 7))) != 0) {
				cpu.PC = (ushort)(cpu.PC + (((opcode & 0x1f8) >> 3) - ((opcode & 0x200) != 0 ? 0x40 : 0)));
				cpu.Cycles++;
			}
		}
		else if ((opcode & 0xff8f) == 0x9408) {
			/* BSET, 1001 0100 0sss 1000 */
			cpu.Data[95] |= (byte)(1 << ((opcode & 0x70) >> 4));
		}
		else if ((opcode & 0xfe08) == 0xfa00) {
			/* BST, 1111 101d dddd 0bbb */
			var d = cpu.Data[(opcode & 0x1f0) >> 4];
			var b = opcode & 7;
			cpu.Data[95] = (byte)(((cpu.Data[95] & 0xbf) | ((d >> b) & 1)) != 0 ? 0x40 : 0);
		}
		else if ((opcode & 0xfe0e) == 0x940e) {
			/* CALL, 1001 010k kkkk 111k kkkk kkkk kkkk kkkk */
			var k = (ushort)(cpu.ProgramMemory[(int)(cpu.PC + 1)] | ((opcode & 1) << 16) | ((opcode & 0x1f0) << 13));
			var ret = cpu.PC + 2;
			var sp = cpu.DataView.GetUint16(93, true);
			cpu.Data[sp] = (byte)(ret & 255);
			cpu.Data[sp - 1] = (byte)((ret >> 8) & 255);
			if (cpu.PC22Bits) {
				cpu.Data[sp - 2] = (byte)((ret >> 16) & 255);
			}
			cpu.DataView.SetUint16(93, (ushort)(sp - (cpu.PC22Bits ? 3 : 2)), true);
			cpu.PC = (ushort)(k - 1);
			cpu.Cycles += cpu.PC22Bits ? 4 : 3;
		}
		else if ((opcode & 0xff00) == 0x9800) {
			/* CBI, 1001 1000 AAAA Abbb */
			var A = opcode & 0xf8;
			var b = opcode & 7;
			var R = cpu.ReadData((ushort)((A >> 3) + 32));
			var mask = (byte)(1 << b);
			cpu.WriteData((ushort)((A >> 3) + 32), (byte)(R & ~mask), mask);
		} else if ((opcode & 0xfe0f) == 0x9400) {
			/* COM, 1001 010d dddd 0000 */
			var d = (opcode & 0x1f0) >> 4;
			var R = (byte)(255 - cpu.Data[d]);
			cpu.Data[d] = R;
			var sreg = (cpu.Data[95] & 0xe1) | 1;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((sreg >> 2) & 1 ^ (sreg >> 3) & 1) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xfc00) == 0x1400) {
			/* CP, 0001 01rd dddd rrrr */
			var val1 = cpu.Data[(opcode & 0x1f0) >> 4];
			var val2 = cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)];
			var R = val1 - val2;
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((val1 ^ val2) & (val1 ^ R) & 128) != 0 ? 8 : 0;
			sreg |= ((sreg >> 2 & 1) ^ (sreg >> 3 & 1)) != 0 ? 0x10 : 0;
			sreg |= val2 > val1 ? 1 : 0;
			sreg |= (1 & (~val1 & val2 | val2 & R | R & ~val1)) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xfc00) == 0x400) {
			/* CPC, 0000 01rd dddd rrrr */
			var arg1 = cpu.Data[(opcode & 0x1f0) >> 4];
			var arg2 = cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)];
			int sreg = cpu.Data[95];
			var r = arg1 - arg2 - (sreg & 1);
			// TODO: Check if this is correct
			sreg = (sreg & 0xc0) | ((~r & (sreg >> 1 & 1)) != 0 ? 2 : 0) | (arg2 + (sreg & 1) > arg1 ? 1 : 0);
			sreg |= ((128 & r) != 0 ? 4 : 0);
			sreg |= ((arg1 ^ arg2) & (arg1 ^ r) & 128) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= (1 & ((~arg1 & arg2) | (arg2 & r) | (r & ~arg1))) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xf000) == 0x3000) {
			/* CPI, 0011 KKKK dddd KKKK */
			var arg1 = cpu.Data[((opcode & 0xf0) >> 4) + 16];
			var arg2 = (opcode & 0xf) | ((opcode & 0xf00) >> 4);
			var r = arg1 - arg2;
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= r == 0 ? 2 : 0;
			sreg |= (128 & r) != 0 ? 4 : 0;
			sreg |= ((arg1 ^ arg2) & (arg1 ^ r) & 128) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= arg2 > arg1 ? 1 : 0;
			sreg |= (1 & ((~arg1 & arg2) | (arg2 & r) | (r & ~arg1))) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xfc00) == 0x1000) {
			/* CPSE, 0001 00rd dddd rrrr */
			if (cpu.Data[(opcode & 0x1f0) >> 4] == cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)]) {
				var nextOpcode = cpu.ProgramMemory[(int)(cpu.PC + 1)];
				var skipSize = IsTwoWordInstruction(nextOpcode) ? 2 : 1;
				cpu.PC += (ushort)skipSize;
				cpu.Cycles += skipSize;
			}
		} else if ((opcode & 0xfe0f) == 0x940a) {
			/* DEC, 1001 010d dddd 1010 */
			var value = cpu.Data[(opcode & 0x1f0) >> 4];
			var r = (byte)(value - 1);
			cpu.Data[(opcode & 0x1f0) >> 4] = r;
			var sreg = cpu.Data[95] & 0xe1;
			sreg |= r == 0 ? 2 : 0;
			sreg |= (128 & r) != 0 ? 4 : 0;
			sreg |= 128 == value ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if (opcode == 0x9519) {
			/* EICALL, 1001 0101 0001 1001 */
			var retAddr = cpu.PC + 1;
			var sp = cpu.DataView.GetUint16(93, true);
			var eind = cpu.Data[0x5c];
			cpu.Data[sp] = (byte)(retAddr & 255);
			cpu.Data[sp - 1] = (byte)((retAddr >> 8) & 255);
			cpu.Data[sp - 2] = (byte)((retAddr >> 16) & 255);
			cpu.DataView.SetUint16(93, (ushort)(sp - 3), true);
			cpu.PC = (uint)((eind << 16) | cpu.DataView.GetUint16(30, true) - 1);
			cpu.Cycles += 3;
		} else if (opcode == 0x9419) {
			/* EIJMP, 1001 0100 0001 1001 */
			var eind = cpu.Data[0x5c];
			cpu.PC = (uint)((eind << 16) | cpu.DataView.GetUint16(30, true) - 1);
			cpu.Cycles++;
		} else if (opcode == 0x95d8) {
			/* ELPM, 1001 0101 1101 1000 */
			var rampz = cpu.Data[0x5b];
			cpu.Data[0] = cpu.ProgBytes[(rampz << 16) | cpu.DataView.GetUint16(30, true)];
			cpu.Cycles += 2;
		} else if ((opcode & 0xfe0f) == 0x9006) {
			/* ELPM(REG), 1001 000d dddd 0110 */
			var rampz = cpu.Data[0x5b];
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ProgBytes[rampz << 16 | cpu.DataView.GetUint16(30, true)];
			cpu.Cycles += 2;
		} else if ((opcode & 0xfe0f) == 0x9007) {
			/* ELPM(INC), 1001 000d dddd 0111 */
			var rampz = cpu.Data[0x5b];
			var i = cpu.DataView.GetUint16(30, true);
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ProgBytes[rampz << 16 | i];
			cpu.DataView.SetUint16(30, (ushort)(i + 1), true);
			if (i == 0xffff) {
				cpu.Data[0x5b] = (byte)((rampz + 1) % (cpu.ProgBytes.Length >> 16));
			}
			cpu.Cycles += 2;
		} else if ((opcode & 0xfc00) == 0x2400) {
			/* EOR, 0010 01rd dddd rrrr */
			var R = (byte)(cpu.Data[(opcode & 0x1f0) >> 4] ^ cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)]);
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			var sreg = cpu.Data[95] & 0xe1;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((sreg >> 2 & 1) ^ (sreg >> 3 & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xff88) == 0x308) {
			/* FMUL, 0000 0011 0ddd 1rrr */
			var v1 = cpu.Data[((opcode & 0x70) >> 4) + 16];
			var v2 = cpu.Data[(opcode & 7) + 16];
			var R = (v1 * v2) << 1;
			cpu.DataView.SetUint16(0, (ushort)R, true);
			cpu.Data[95] = (byte)(cpu.Data[95] & 0xfc | ((0xffff & R) != 0 ? 0 : 2) | ((v1 * v2 & 0x8000) != 0 ? 1 : 0));
			cpu.Cycles++;
		} else if ((opcode & 0xff88) == 0x380) {
			/* FMULS, 0000 0011 1ddd 0rrr */
			var v1 = cpu.DataView.GetInt8(((opcode & 0x70) >> 4) + 16);
			var v2 = cpu.DataView.GetInt8((opcode & 7) + 16);
			var R = (v1 * v2) << 1;
			cpu.DataView.SetInt16(0, (short)R, true);
			cpu.Data[95] = (byte)(cpu.Data[95] & 0xfc | ((0xffff & R) != 0 ? 0 : 2) | ((v1 * v2 & 0x8000) != 0 ? 1 : 0));
			cpu.Cycles++;
		} else if ((opcode & 0xff88) == 0x388) {
			/* FMULSU, 0000 0011 1ddd 1rrr */
			var v1 = cpu.DataView.GetInt8(((opcode & 0x70) >> 4) + 16);
			var v2 = cpu.Data[(opcode & 7) + 16];
			var R = (v1 * v2) << 1;
			cpu.DataView.SetInt16(0, (short)R, true);
			cpu.Data[95] = (byte)((((cpu.Data[95] & 0xfc) | (0xffff & R)) != 0 ? 0 : 2) | ((v1 * v2 & 0x8000) != 0 ? 1 : 0));
			cpu.Cycles++;
		} else if (opcode == 0x9509) {
			/* ICALL, 1001 0101 0000 1001 */
			var retAddr = cpu.PC + 1;
			var sp = cpu.DataView.GetUint16(93, true);
			cpu.Data[sp] = (byte)(retAddr & 255);
			cpu.Data[sp - 1] = (byte)((retAddr >> 8) & 255);
			if (cpu.PC22Bits) {
				cpu.Data[sp - 2] = (byte)((retAddr >> 16) & 255);
			}
			cpu.DataView.SetUint16(93, (ushort)(sp - (cpu.PC22Bits ? 3 : 2)), true);
			cpu.PC = (uint)(cpu.DataView.GetUint16(30, true) - 1);
			cpu.Cycles += cpu.PC22Bits ? 3 : 2;
		} else if (opcode == 0x9409) {
			/* IJMP, 1001 0100 0000 1001 */
			cpu.PC = (uint)(cpu.DataView.GetUint16(30, true) - 1);
			cpu.Cycles++;
		} else if ((opcode & 0xf800) == 0xb000) {
			/* IN, 1011 0AAd dddd AAAA */
			var i = cpu.ReadData((ushort)((opcode & 0xf) | ((opcode & 0x600) >> 5) + 32));
			cpu.Data[(opcode & 0x1f0) >> 4] = i;
		} else if ((opcode & 0xfe0f) == 0x9403) {
			/* INC, 1001 010d dddd 0011 */
			var d = cpu.Data[(opcode & 0x1f0) >> 4];
			var r = (d + 1) & 255;
			cpu.Data[(opcode & 0x1f0) >> 4] = (byte)r;
			var sreg = cpu.Data[95] & 0xe1;
			sreg |= r == 0 ? 2 : 0;
			sreg |= (128 & r) != 0 ? 4 : 0;
			sreg |= 127 == d ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xfe0e) == 0x940c) {
			/* JMP, 1001 010k kkkk 110k kkkk kkkk kkkk kkkk */
			cpu.PC = (uint)(cpu.ProgramMemory[(int)(cpu.PC + 1)] | (opcode & 1) << 16 | (opcode & 0x1f0) << 13) - 1;
			cpu.Cycles += 2;
		} else if ((opcode & 0xfe0f) == 0x9206) {
			/* LAC, 1001 001r rrrr 0110 */
			var r = (opcode & 0x1f0) >> 4;
			var clear = cpu.Data[r];
			var value = cpu.ReadData(cpu.DataView.GetUint16(30, true));
			cpu.WriteData(cpu.DataView.GetUint16(30, true), (byte)(value & (255 - clear)));
			cpu.Data[r] = value;
		} else if ((opcode & 0xfe0f) == 0x9205) {
			/* LAS, 1001 001r rrrr 0101 */
			var r = (opcode & 0x1f0) >> 4;
			var set = cpu.Data[r];
			var value = cpu.ReadData(cpu.DataView.GetUint16(30, true));
			cpu.WriteData(cpu.DataView.GetUint16(30, true), (byte)(value | set));
			cpu.Data[r] = value;
		} else if ((opcode & 0xfe0f) == 0x9207) {
			/* LAT, 1001 001r rrrr 0111 */
			var r = cpu.Data[(opcode & 0x1f0) >> 4];
			var R = cpu.ReadData(cpu.DataView.GetUint16(30, true));
			cpu.WriteData(cpu.DataView.GetUint16(30, true), (byte)(r ^ R));
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
		} else if ((opcode & 0xf000) == 0xe000) {
			/* LDI, 1110 KKKK dddd KKKK */
			cpu.Data[((opcode & 0xf0) >> 4) + 16] = (byte)((opcode & 0xf) | ((opcode & 0xf00) >> 4));
		} else if ((opcode & 0xfe0f) == 0x9000) {
			/* LDS, 1001 000d dddd 0000 kkkk kkkk kkkk kkkk */
			cpu.Cycles++;
			var value = cpu.ReadData(cpu.ProgramMemory[(int)(cpu.PC + 1)]);
			cpu.Data[(opcode & 0x1f0) >> 4] = value;
			cpu.PC++;
		} else if ((opcode & 0xfe0f) == 0x900c) {
			/* LDX, 1001 000d dddd 1100 */
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(cpu.DataView.GetUint16(26, true));
		} else if ((opcode & 0xfe0f) == 0x900d) {
			/* LDX(INC), 1001 000d dddd 1101 */
			var x = cpu.DataView.GetUint16(26, true);
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(x);
			cpu.DataView.SetUint16(26, (ushort)(x + 1), true);
		} else if ((opcode & 0xfe0f) == 0x900e) {
			/* LDX(DEC), 1001 000d dddd 1110 */
			var x = cpu.DataView.GetUint16(26, true) - 1;
			cpu.DataView.SetUint16(26, (ushort)x, true);
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData((ushort)x);
		} else if ((opcode & 0xfe0f) == 0x8008) {
			/* LDY, 1000 000d dddd 1000 */
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(cpu.DataView.GetUint16(28, true));
		} else if ((opcode & 0xfe0f) == 0x9009) {
			/* LDY(INC), 1001 000d dddd 1001 */
			var y = cpu.DataView.GetUint16(28, true);
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(y);
			cpu.DataView.SetUint16(28, (ushort)(y + 1), true);
		} else if ((opcode & 0xfe0f) == 0x900a) {
			/* LDY(DEC), 1001 000d dddd 1010 */
			var y = cpu.DataView.GetUint16(28, true) - 1;
			cpu.DataView.SetUint16(28, (ushort)y, true);
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData((ushort)y);
		} else if ((opcode & 0xd208) == 0x8008 && ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8)) != 0) {
			/* LDDY, 10q0 qq0d dddd 1qqq */
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(
				(ushort)(cpu.DataView.GetUint16(28, true) + ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8)))
			);
		} else if ((opcode & 0xfe0f) == 0x8000) {
			/* LDZ, 1000 000d dddd 0000 */
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(cpu.DataView.GetUint16(30, true));
		} else if ((opcode & 0xfe0f) == 0x9001) {
			var z = cpu.DataView.GetUint16(30, true);
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(z);
			cpu.DataView.SetUint16(30, (ushort)(z + 1), true);
		} else if ((opcode & 0xfe0f) == 0x9002) {
			/* LDZ(DEC), 1001 000d dddd 0010 */
			var z = cpu.DataView.GetUint16(30, true) - 1;
			cpu.DataView.SetUint16(30, (ushort)z, true);
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData((ushort)z);
		} else if ((opcode & 0xd208) == 0x8000 && ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8)) != 0) {
			/* LDDZ, 10q0 qq0d dddd 0qqq */
			cpu.Cycles++;
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ReadData(
				(ushort)(cpu.DataView.GetUint16(30, true) + ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8)))
			);
		} else if (opcode == 0x95c8) {
			/* LPM, 1001 0101 1100 1000 */
			cpu.Data[0] = cpu.ProgBytes[cpu.DataView.GetUint16(30, true)];
			cpu.Cycles += 2;
		} else if ((opcode & 0xfe0f) == 0x9004) {
			/* LPM(REG), 1001 000d dddd 0100 */
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ProgBytes[cpu.DataView.GetUint16(30, true)];
			cpu.Cycles += 2;
		} else if ((opcode & 0xfe0f) == 0x9005) {
			/* LPM(INC), 1001 000d dddd 0101 */
			var i = cpu.DataView.GetUint16(30, true);
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.ProgBytes[i];
			cpu.DataView.SetUint16(30, (ushort)(i + 1), true);
			cpu.Cycles += 2;
		} else if ((opcode & 0xfe0f) == 0x9406) {
			/* LSR, 1001 010d dddd 0110 */
			var value = cpu.Data[(opcode & 0x1f0) >> 4];
			var R = (byte)(value >> 1);
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			var sreg = cpu.Data[95] & 0xe0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= value & 1;
			sreg |= ((sreg >> 2) & 1 ^ (sreg & 1)) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xfc00) == 0x2c00) {
			/* MOV, 0010 11rd dddd rrrr */
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)];
		} else if ((opcode & 0xff00) == 0x100) {
			/* MOVW, 0000 0001 dddd rrrr */
			var r2 = 2 * (opcode & 0xf);
			var d2 = 2 * ((opcode & 0xf0) >> 4);
			cpu.Data[d2] = cpu.Data[r2];
			cpu.Data[d2 + 1] = cpu.Data[r2 + 1];
		} else if ((opcode & 0xfc00) == 0x9c00) {
			/* MUL, 1001 11rd dddd rrrr */
			var R = cpu.Data[(opcode & 0x1f0) >> 4] * cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)];
			cpu.DataView.SetUint16(0, (ushort)R, true);
			cpu.Data[95] = (byte)(cpu.Data[95] & 0xfc | ((0xffff & R) != 0 ? 0 : 2) | ((0x8000 & R) != 0 ? 1 : 0));
			cpu.Cycles++;
		} else if ((opcode & 0xff00) == 0x200) {
			/* MULS, 0000 0010 dddd rrrr */
			var R = cpu.DataView.GetInt8(((opcode & 0xf0) >> 4) + 16) * cpu.DataView.GetInt8((opcode & 0xf) + 16);
			cpu.DataView.SetInt16(0, (short)R, true);
			cpu.Data[95] = (byte)(cpu.Data[95] & 0xfc | ((0xffff & R) != 0 ? 0 : 2) | ((0x8000 & R) != 0 ? 1 : 0));
			cpu.Cycles++;
		} else if ((opcode & 0xff88) == 0x300) {
			/* MULSU, 0000 0011 0ddd 0rrr */
			var R = cpu.DataView.GetInt8(((opcode & 0x70) >> 4) + 16) * cpu.Data[(opcode & 7) + 16];
			cpu.DataView.SetInt16(0, (short)R, true);
			cpu.Data[95] = (byte)(cpu.Data[95] & 0xfc | ((0xffff & R) != 0 ? 0 : 2) | ((0x8000 & R) != 0 ? 1 : 0));
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x9401) {
			/* NEG, 1001 010d dddd 0001 */
			var d = (opcode & 0x1f0) >> 4;
			var value = cpu.Data[d];
			var R = (byte)(0 - value);
			cpu.Data[d] = R;
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= 128 == R ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= R == 0 ? 0 : 1;
			sreg |= (1 & (R | value)) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if (opcode == 0) {
			/* NOP, 0000 0000 0000 0000 */
			/* NOP */
		} else if ((opcode & 0xfc00) == 0x2800) {
			/* OR, 0010 10rd dddd rrrr */
			var R = cpu.Data[(opcode & 0x1f0) >> 4] | cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)];
			cpu.Data[(opcode & 0x1f0) >> 4] = (byte)R;
			var sreg = cpu.Data[95] & 0xe1;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((sreg >> 2 & 1) ^ (sreg >> 3 & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xf000) == 0x6000) {
			/* SBR, 0110 KKKK dddd KKKK */
			var R = cpu.Data[((opcode & 0xf0) >> 4) + 16] | ((opcode & 0xf) | ((opcode & 0xf00) >> 4));
			cpu.Data[((opcode & 0xf0) >> 4) + 16] = (byte)R;
			var sreg = cpu.Data[95] & 0xe1;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((sreg >> 2 & 1) ^ (sreg >> 3 & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xf800) == 0xb800) {
			/* OUT, 1011 1AAr rrrr AAAA */
			cpu.WriteData ((ushort)((opcode & 0xf) | ((opcode & 0x600) >> 5) + 32), cpu.Data[(opcode & 0x1f0) >> 4]);
		} else if ((opcode & 0xfe0f) == 0x900f) {
			/* POP, 1001 000d dddd 1111 */
			var value = cpu.DataView.GetUint16(93, true) + 1;
			cpu.DataView.SetUint16(93, (ushort)value, true);
			cpu.Data[(opcode & 0x1f0) >> 4] = cpu.Data[value];
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x920f) {
			/* PUSH, 1001 001d dddd 1111 */
			var value = cpu.DataView.GetUint16(93, true);
			cpu.Data[value] = cpu.Data[(opcode & 0x1f0) >> 4];
			cpu.DataView.SetUint16(93, (ushort)(value - 1), true);
			cpu.Cycles++;
		} else if ((opcode & 0xf000) == 0xd000) {
			/* RCALL, 1101 kkkk kkkk kkkk */
			var k = (opcode & 0x7ff) - ((opcode & 0x800) != 0 ? 0x800 : 0);
			var retAddr = cpu.PC + 1;
			var sp = cpu.DataView.GetUint16(93, true);
			cpu.Data[sp] = (byte)(retAddr & 255);
			cpu.Data[sp - 1] = (byte)((retAddr >> 8) & 255);
			if (cpu.PC22Bits) {
				cpu.Data[sp - 2] = (byte)((retAddr >> 16) & 255);
			}
			cpu.DataView.SetUint16(93, (ushort)(sp - (cpu.PC22Bits ? 3 : 2)), true);
			cpu.PC += (ushort)k;
			cpu.Cycles += cpu.PC22Bits ? 3 : 2;
		} else if (opcode == 0x9508) {
			/* RET, 1001 0101 0000 1000 */
			var i = cpu.DataView.GetUint16(93, true) + (cpu.PC22Bits ? 3 : 2);
			cpu.DataView.SetUint16(93, (ushort)i, true);
			cpu.PC = (uint)((cpu.Data[i - 1] << 8) + cpu.Data[i] - 1);
			if (cpu.PC22Bits) {
				cpu.PC |= (uint)(cpu.Data[i - 2] << 16);
			}
			cpu.Cycles += cpu.PC22Bits ? 4 : 3;
		} else if (opcode == 0x9518) {
			/* RETI, 1001 0101 0001 1000 */
			var i = cpu.DataView.GetUint16(93, true) + (cpu.PC22Bits ? 3 : 2);
			cpu.DataView.SetUint16(93, (ushort)i, true);
			cpu.PC = (uint)((cpu.Data[i - 1] << 8) + cpu.Data[i] - 1);
			if (cpu.PC22Bits) {
				cpu.PC |= (uint)(cpu.Data[i - 2] << 16);
			}
			cpu.Cycles += cpu.PC22Bits ? 4 : 3;
			cpu.Data[95] |= 0x80; // Enable interrupts
		} else if ((opcode & 0xf000) == 0xc000) {
			/* RJMP, 1100 kkkk kkkk kkkk */
			cpu.PC = (uint)(cpu.PC + ((opcode & 0x7ff) - ((opcode & 0x800) != 0 ? 0x800 : 0)));
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x9407) {
			/* ROR, 1001 010d dddd 0111 */
			var d = cpu.Data[(opcode & 0x1f0) >> 4];
			var r = (byte)((d >> 1) | ((cpu.Data[95] & 1) << 7));
			cpu.Data[(opcode & 0x1f0) >> 4] = r;
			var sreg = cpu.Data[95] & 0xe0;
			sreg |= r == 0 ? 2 : 0;
			sreg |= (128 & r) != 0 ? 4 : 0;
			sreg |= d & 1;
			sreg |= ((sreg >> 2) & 1 ^ (sreg & 1)) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xfc00) == 0x800) {
			/* SBC, 0000 10rd dddd rrrr */
			
			var val1 = cpu.Data[(opcode & 0x1f0) >> 4];
			var val2 = cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)];
			int sreg = cpu.Data[95];
			var R = (byte)(val1 - val2 - (sreg & 1));
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			// TODO: Check if this is correct
			sreg = ((sreg & 0xc0) | ((~R & (sreg >> 1) & 1) != 0 ? 2 : 0) | (val2 + (sreg & 1) > val1 ? 1 : 0));
			sreg |= ((128 & R) != 0 ? 4 : 0);
			sreg |= ((val1 ^ val2) & (val1 ^ R) & 128) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= (1 & ((~val1 & val2) | (val2 & R) | (R & ~val1))) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
			
		} else if ((opcode & 0xf000) == 0x4000) {
			/* SBCI, 0100 KKKK dddd KKKK */
			var val1 = cpu.Data[((opcode & 0xf0) >> 4) + 16];
			var val2 = (opcode & 0xf) | ((opcode & 0xf00) >> 4);
			int sreg = cpu.Data[95];
			var R = (byte)(val1 - val2 - (sreg & 1));
			cpu.Data[((opcode & 0xf0) >> 4) + 16] = R;
			// TODO: Check if this is correct
			sreg = (sreg & 0xc0) | ((~R & (sreg >> 1) & 1) != 0 ? 2 : 0) | (val2 + (sreg & 1) > val1 ? 1 : 0);
			sreg |= ((128 & R) != 0 ? 4 : 0);
			sreg |= ((val1 ^ val2) & (val1 ^ R) & 128) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= (1 & ((~val1 & val2) | (val2 & R) | (R & ~val1))) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xff00) == 0x9a00) {
			/* SBI, 1001 1010 AAAA Abbb */
			var target = ((opcode & 0xf8) >> 3) + 32;
			var mask = 1 << (opcode & 7);
			cpu.WriteData ((ushort)target, (byte)(cpu.ReadData ((ushort)target) | mask), (byte)mask);
			cpu.Cycles++;
		} else if ((opcode & 0xff00) == 0x9900) {
			/* SBIC, 1001 1001 AAAA Abbb */
			var value = cpu.ReadData((ushort)(((opcode & 0xf8) >> 3) + 32));
			if ((value & (1 << (opcode & 7))) == 0) {
				var nextOpcode = cpu.ProgBytes[cpu.PC + 1];
				var skipSize = IsTwoWordInstruction(nextOpcode) ? 2 : 1;
				cpu.PC += (ushort)skipSize;
				cpu.Cycles += skipSize;
			}
		} else if ((opcode & 0xff00) == 0x9b00) {
			/* SBIS, 1001 1011 AAAA Abbb */
			var value = cpu.ReadData((ushort)(((opcode & 0xf8) >> 3) + 32));
			if ((value & (1 << (opcode & 7))) != 0) {
				var nextOpcode = cpu.ProgBytes[cpu.PC + 1];
				var skipSize = IsTwoWordInstruction(nextOpcode) ? 2 : 1;
				cpu.PC += (ushort)skipSize;
				cpu.Cycles += skipSize;
			}
		} else if ((opcode & 0xff00) == 0x9700) {
			/* SBIW, 1001 0111 KKdd KKKK */
			var i = 2 * ((opcode & 0x30) >> 4) + 24;
			var a = cpu.DataView.GetUint16((ushort)i, true);
			var l = (opcode & 0xf) | ((opcode & 0xc0) >> 2);
			var R = a - l;
			cpu.DataView.SetUint16((ushort)i, (ushort)R, true);
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (0x8000 & R) != 0 ? 4 : 0;
			sreg |= (a & ~R & 0x8000) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= l > a ? 1 : 0;
			sreg |= (1 & ((~a & l) | (l & R) | (R & ~a))) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
			cpu.Cycles++;
		} else if ((opcode & 0xfe08) == 0xfc00) {
			/* SBRC, 1111 110r rrrr 0bbb */
			if ((cpu.Data[(opcode & 0x1f0) >> 4] & (1 << (opcode & 7))) == 0) {
				var nextOpcode = cpu.ProgBytes[cpu.PC + 1];
				var skipSize = IsTwoWordInstruction(nextOpcode) ? 2 : 1;
				cpu.PC += (ushort)skipSize;
				cpu.Cycles += skipSize;
			}
		} else if ((opcode & 0xfe08) == 0xfe00) {
			/* SBRS, 1111 111r rrrr 0bbb */
			if ((cpu.Data[(opcode & 0x1f0) >> 4] & (1 << (opcode & 7))) != 0) {
				var nextOpcode = cpu.ProgBytes[cpu.PC + 1];
				var skipSize = IsTwoWordInstruction(nextOpcode) ? 2 : 1;
				cpu.PC += (ushort)skipSize;
				cpu.Cycles += skipSize;
			}
		} else if (opcode == 0x9588) {
			/* SLEEP, 1001 0101 1000 1000 */
			/* not implemented */
		} else if (opcode == 0x95e8) {
			/* SPM, 1001 0101 1110 1000 */
			/* not implemented */
		} else if (opcode == 0x95f8) {
			/* SPM(INC), 1001 0101 1111 1000 */
			/* not implemented */
		} else if ((opcode & 0xfe0f) == 0x9200) {
			/* STS, 1001 001d dddd 0000 kkkk kkkk kkkk kkkk */
			var value = cpu.Data[(opcode & 0x1f0) >> 4];
			var addr = cpu.ProgramMemory[(int)(cpu.PC + 1)];
			cpu.WriteData(addr, value);
			cpu.PC++;
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x920c) {
			/* STX, 1001 001r rrrr 1100 */
			cpu.WriteData (cpu.DataView.GetUint16(26, true), cpu.Data[(opcode & 0x1f0) >> 4]);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x920d) {
			/* STX(INC), 1001 001r rrrr 1101 */
			var x = cpu.DataView.GetUint16(26, true);
			cpu.WriteData(x, cpu.Data[(opcode & 0x1f0) >> 4]);
			cpu.DataView.SetUint16(26, (ushort)(x + 1), true);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x920e) {
			/* STX(DEC), 1001 001r rrrr 1110 */
			var i = cpu.Data[(opcode & 0x1f0) >> 4];
			var x = cpu.DataView.GetUint16(26, true) - 1;
			cpu.DataView.SetUint16(26, (ushort)x, true);
			cpu.WriteData((ushort)x, i);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x8208) {
			/* STY, 1000 001r rrrr 1000 */
			cpu.WriteData (cpu.DataView.GetUint16(28, true), cpu.Data[(opcode & 0x1f0) >> 4]);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x9209) {
			/* STY(INC), 1001 001r rrrr 1001 */
			var i = cpu.Data[(opcode & 0x1f0) >> 4];
			var y = cpu.DataView.GetUint16(28, true);
			cpu.WriteData(y, i);
			cpu.DataView.SetUint16(28, (ushort)(y + 1), true);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x920a) {
			/* STY(DEC), 1001 001r rrrr 1010 */
			var i = cpu.Data[(opcode & 0x1f0) >> 4];
			var y = cpu.DataView.GetUint16(28, true) - 1;
			cpu.DataView.SetUint16(28, (ushort)y, true);
			cpu.WriteData((ushort)y, i);
			cpu.Cycles++;
		} else if ((opcode & 0xd208) == 0x8208 && ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8)) != 0) {
			/* STDY, 10q0 qq1r rrrr 1qqq */
			cpu.WriteData (
				(ushort)(cpu.DataView.GetUint16(28, true) + ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8))),
				cpu.Data[(opcode & 0x1f0) >> 4]
			);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x8200) {
			/* STZ, 1000 001r rrrr 0000 */
			cpu.WriteData (cpu.DataView.GetUint16(30, true), cpu.Data[(opcode & 0x1f0) >> 4]);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x9201) {
			/* STZ(INC), 1001 001r rrrr 0001 */
			var z = cpu.DataView.GetUint16(30, true);
			cpu.WriteData(z, cpu.Data[(opcode & 0x1f0) >> 4]);
			cpu.DataView.SetUint16(30, (ushort)(z + 1), true);
			cpu.Cycles++;
		} else if ((opcode & 0xfe0f) == 0x9202) {
			/* STZ(DEC), 1001 001r rrrr 0010 */
			var i = cpu.Data[(opcode & 0x1f0) >> 4];
			var z = cpu.DataView.GetUint16(30, true) - 1;
			cpu.DataView.SetUint16(30, (ushort)z, true);
			cpu.WriteData((ushort)z, i);
			cpu.Cycles++;
		} else if ((opcode & 0xd208) == 0x8200 && ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8)) != 0) {
			/* STDZ, 10q0 qq1r rrrr 0qqq */
			cpu.WriteData (
				(ushort)(cpu.DataView.GetUint16(30, true) + ((opcode & 7) | ((opcode & 0xc00) >> 7) | ((opcode & 0x2000) >> 8))),
				cpu.Data[(opcode & 0x1f0) >> 4]
			);
			cpu.Cycles++;
		} else if ((opcode & 0xfc00) == 0x1800) {
			/* SUB, 0001 10rd dddd rrrr */
			var val1 = cpu.Data[(opcode & 0x1f0) >> 4];
			var val2 = cpu.Data[(opcode & 0xf) | ((opcode & 0x200) >> 5)];
			var R = (byte)(val1 - val2);
			
			cpu.Data[(opcode & 0x1f0) >> 4] = R;
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((val1 ^ val2) & (val1 ^ R) & 128) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= val2 > val1 ? 1 : 0;
			sreg |= (1 & ((~val1 & val2) | (val2 & R) | (R & ~val1))) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xf000) == 0x5000) {
			/* SUBI, 0101 KKKK dddd KKKK */
			var val1 = cpu.Data[((opcode & 0xf0) >> 4) + 16];
			var val2 = (opcode & 0xf) | ((opcode & 0xf00) >> 4);
			var R = (byte)(val1 - val2);
			cpu.Data[((opcode & 0xf0) >> 4) + 16] = R;
			var sreg = cpu.Data[95] & 0xc0;
			sreg |= R == 0 ? 2 : 0;
			sreg |= (128 & R) != 0 ? 4 : 0;
			sreg |= ((val1 ^ val2) & (val1 ^ R) & 128) != 0 ? 8 : 0;
			sreg |= (((sreg >> 2) & 1) ^ ((sreg >> 3) & 1)) != 0 ? 0x10 : 0;
			sreg |= val2 > val1 ? 1 : 0;
			sreg |= (1 & ((~val1 & val2) | (val2 & R) | (R & ~val1))) != 0 ? 0x20 : 0;
			cpu.Data[95] = (byte)sreg;
		} else if ((opcode & 0xfe0f) == 0x9402) {
			/* SWAP, 1001 010d dddd 0010 */
			var d = (opcode & 0x1f0) >> 4;
			var i = cpu.Data[d];
			cpu.Data[d] = (byte)(((15 & i) << 4) | ((240 & i) >> 4));
		} else if (opcode == 0x95a8) {
			/* WDR, 1001 0101 1010 1000 */
			cpu.OnWatchdogReset();
		} else if ((opcode & 0xfe0f) == 0x9204) {
			/* XCH, 1001 001r rrrr 0100 */
			var r = (opcode & 0x1f0) >> 4;
			var val1 = cpu.Data[r];
			var val2 = cpu.Data[cpu.DataView.GetUint16(30, true)];
			cpu.Data[cpu.DataView.GetUint16(30, true)] = val1;
			cpu.Data[r] = val2;
		}

		cpu.PC = (uint)((cpu.PC + 1) % cpu.ProgramMemory.Length);
		cpu.Cycles++;
	}
	
	private static bool IsTwoWordInstruction(ushort opcode)
	{
		return 
			/* LDS */
			(opcode & 0xfe0f) == 0x9000 ||
			/* STS */
			(opcode & 0xfe0f) == 0x9200 || 
			/* CALL */
			(opcode & 0xfe0e) == 0x940e || 
			/* JMP */
			(opcode & 0xfe0e) == 0x940c;
	}
}
