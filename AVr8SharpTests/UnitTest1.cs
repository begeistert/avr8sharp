using System.Text;
using AVR8Sharp.Cpu;
using AVR8Sharp.Peripherals;
using Newtonsoft.Json;
namespace AVr8SharpTests;

public class Arduino
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

	static Arduino ()
	{
		_Client = new HttpClient ();
	}

	[SetUp]
	public void Setup ()
	{
	}
	
	public void Test1 ()
	{
		var hexi = new Arduino ();
		var result = hexi.Compile (BlinkCode);
		var hex = result.Hex;
		var program = new byte[0x8000];
		hexi.LoadHex (hex, program);
		var runner = new ArduinoRunner (ref program);
		var builder = new StringBuilder ();
		runner.Usart.OnByteTransmit = b => {
			builder.Append ((char)b);
		};
		while (true) {
			runner.Execute (cpu => {
				var time = cpu.Cycles / 16_000_000.0;
				Console.WriteLine ($"Time: {time} seconds");
			});
		}
	}

	public void LoadHex (string source, byte[] target)
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

	public HexiResult Compile (string source)
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
		readonly int workUnitCycles = 500000;
		
		// private readonly LimitedConcurrencyLevelTaskScheduler _scheduler = new LimitedConcurrencyLevelTaskScheduler (1);
		
		private bool _stopped = false;

		public ArduinoRunner (ref byte[] program)
		{
			Cpu = new AVR8Sharp.Cpu.Cpu (program);
			Timer0 = new AvrTimer (Cpu, AvrTimer.Timer0Config);
			Timer1 = new AvrTimer (Cpu, AvrTimer.Timer1Config);
			Timer2 = new AvrTimer (Cpu, AvrTimer.Timer2Config);
			PortB = new AvrIoPort (Cpu, AvrIoPort.PortBConfig);
			PortC = new AvrIoPort (Cpu, AvrIoPort.PortCConfig);
			PortD = new AvrIoPort (Cpu, AvrIoPort.PortDConfig);
			Usart = new AvrUsart (Cpu, AvrUsart.Usart0Config, speed);
		}

		public void Execute (Action<AVR8Sharp.Cpu.Cpu> callback)
		{
			var cyclesToRun = Cpu.Cycles + workUnitCycles;
			while (Cpu.Cycles < cyclesToRun) {
				Instruction.AvrInstruction (Cpu);
				Cpu.Tick ();
			}

			callback (Cpu);
			
			// Run the next work unit
			if (_stopped) 
				return;
			// Task.Factory.StartNew (() => Execute (callback), CancellationToken.None, TaskCreationOptions.None, _scheduler);
		}
		
		public void Stop ()
		{
			_stopped = true;
		}
	}
}
