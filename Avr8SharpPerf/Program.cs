using System.Diagnostics;
using System.Text;
using AVR8Sharp.Cpu;
using AVR8Sharp.Peripherals;
using Newtonsoft.Json;

public class Program
{
	const string HexiUrl = "https://hexi.wokwi.com";
	const string BlinkCode = @"
// Green LED connected to LED_BUILTIN,
// Red LED connected to pin 12. Enjoy!

void setup() {
  Serial.begin(115200);
  pinMode(LED_BUILTIN, OUTPUT);
}

void loop() {
  Serial.println(""Blink"");
  digitalWrite(LED_BUILTIN, HIGH);
  delay(500);
  digitalWrite(LED_BUILTIN, LOW);
  delay(500);
}
";
	public static HttpClient _Client;
	
	static Program ()
	{
		_Client = new HttpClient ();
	}
	
	public static void Main()
	{
		var result = Compile (BlinkCode);
		var hex = result.Hex;
		var program = new byte[0x8000];
		var watch = new Stopwatch ();
		LoadHex (hex, program);
		#if DEBUG
		var runner = new ArduinoRunner (ref program, watch: watch);
		#elif RELEASE
		var runner = new ArduinoRunner (ref program, 1, watch);
        #endif
		var builder = new StringBuilder ();
		runner.PortB.AddListener ((newVal, prevVal) => {
			if (prevVal == newVal)
				return;
			
			Console.WriteLine($"Pin 4: {runner.PortB.GetPinState(4)}");
			Console.WriteLine($"Pin 5: {runner.PortB.GetPinState(5)}");
			
			var millis = (runner.Cpu.Cycles / 16_000_000.0) * 1000;
			Console.WriteLine ($"CPU Time: {millis} ms");
			Console.WriteLine($"Time: {watch.Elapsed.TotalMilliseconds} ms");
		});
		runner.Usart.OnByteTransmit = b => {
			var c = (char)b;
			builder.Append (c);
			if (c != '\n') return;
			Console.WriteLine ($"Serial Output: {builder.ToString().Trim()}");
			builder.Clear ();
			var millis = (runner.Cpu.Cycles / 16_000_000.0) * 1000;
			Console.WriteLine ($"CPU Time: {millis} ms");
			Console.WriteLine($"Time: {watch.Elapsed.TotalMilliseconds} ms");
		};
		Console.WriteLine("Running...");
		const int fiveSecs = (int)(5.5 * 16_000_000);
		watch.Start ();
		// Sync the cpu wth the real time
		while (runner.Cpu.Cycles < fiveSecs) {
			runner.Execute ();
			var millis = (runner.Cpu.Cycles / 16_000_000.0) * 1000;
			while (watch.Elapsed.TotalMilliseconds < millis) {
				// Wait for the real time to catch up
			}
		}
		watch.Stop ();
		Console.WriteLine(builder.ToString ());
		Console.WriteLine($"Execution time: {watch.ElapsedMilliseconds} ms");
	}
	public static void LoadHex (string source, byte[] target)
	{
		foreach (var line in source.Split ('\n')) {
			if (!string.IsNullOrEmpty (line) && line[0] == ':' && line.Substring (7, 2) == "00") {
				var bytes = Convert.ToInt32 (line.Substring (1, 2), 16);
				var addr = Convert.ToInt32 (line.Substring (3, 4), 16);
				for (var i = 0; i < bytes; i++) {
					target[addr + i] = Convert.ToByte (line.Substring (9 + i * 2, 2), 16);
				}
			}
		}
	}

	public static HexiResult Compile (string source)
	{
		var content = new StringContent (JsonConvert.SerializeObject (new {
			sketch = source,
		}), Encoding.UTF8, "application/json");
		var response = _Client.PostAsync ($"{HexiUrl}/build", content).Result;
		var result = response.Content.ReadAsStringAsync ().Result;
		return JsonConvert.DeserializeObject<HexiResult> (result) ?? new HexiResult ();
	}

	public class HexiResult
	{
		public string Stdout { get; set; }
		public string Stderr { get; set; }
		public string Hex { get; set; }
	}

	public class ArduinoRunner
	{
		const int FLASH = 0x8000;

		private readonly ushort[] Program = new ushort[FLASH];
		public readonly AVR8Sharp.Cpu.Cpu Cpu;
		public readonly AvrTimer Timer0;
		public readonly AvrTimer Timer1;
		public readonly AvrTimer Timer2;
		public readonly AvrIoPort PortB;
		public readonly AvrIoPort PortC;
		public readonly AvrIoPort PortD;
		public readonly AvrUsart Usart;
		private readonly uint speed = 16_000_000U; // 16 MHz
		readonly int _workUnitCycles = 500000;
		readonly Stopwatch _watch;
		readonly double _nanosPerCycle = 62.5;
		
		private double CurrentTime {
			get {
				return _watch.Elapsed.TotalSeconds;
			}
		}

		// private readonly LimitedConcurrencyLevelTaskScheduler _scheduler = new LimitedConcurrencyLevelTaskScheduler (1);
		
		private bool _stopped = false;

		public ArduinoRunner (ref byte[] program, int workUnitCycles = 500000, Stopwatch watch = null)
		{
			Cpu = new AVR8Sharp.Cpu.Cpu (program);
			Timer0 = new AvrTimer (Cpu, AvrTimer.Timer0Config);
			Timer1 = new AvrTimer (Cpu, AvrTimer.Timer1Config);
			Timer2 = new AvrTimer (Cpu, AvrTimer.Timer2Config);
			PortB = new AvrIoPort (Cpu, AvrIoPort.PortBConfig);
			PortC = new AvrIoPort (Cpu, AvrIoPort.PortCConfig);
			PortD = new AvrIoPort (Cpu, AvrIoPort.PortDConfig);
			Usart = new AvrUsart (Cpu, AvrUsart.Usart0Config, speed);
			_workUnitCycles = workUnitCycles;
			_watch = watch;
		}

		public void Execute (Action<AVR8Sharp.Cpu.Cpu>? callback = null)
		{
			var cyclesToRun = Cpu.Cycles + _workUnitCycles;
			while (Cpu.Cycles < cyclesToRun) {
				AVR8Sharp.Cpu.Instruction.AvrInstruction (Cpu);
				Cpu.Tick ();
			}

			callback?.Invoke (Cpu);
		}
		
		public void Stop ()
		{
			_stopped = true;
		}
	}
}
