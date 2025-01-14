using System.Diagnostics;
using System.Text;
using AVR8Sharp;
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
	
	[Test]
	public void Run ()
	{
		var hexi = new Arduino ();
		var result = hexi.Compile (BlinkCode);
		var hex = result.Hex;
		var runner = AvrBuilder.Create ()
			.SetSpeed (16_000_000)
			.SetHex (hex)
			.AddGpioPort (AvrIoPort.PortBConfig, out var portB)
			.AddGpioPort (AvrIoPort.PortCConfig, out _)
			.AddGpioPort (AvrIoPort.PortDConfig, out _)
			.AddUsart (AvrUsart.Usart0Config, out var usart)
			.AddTimer (AvrTimer.Timer0Config, out _)
			.AddTimer (AvrTimer.Timer1Config, out _)
			.AddTimer (AvrTimer.Timer2Config, out _)
			.Build ();
		
		var builder = new StringBuilder ();
		usart.OnByteTransmit = b => {
			builder.Append ((char)b);
		};
		const int fiveSecs = 5 * 16_000_000;
		var watch = Stopwatch.StartNew ();
		while (runner.Cpu.Cycles < fiveSecs) {
			runner.Execute ();
		}
		watch.Stop ();
		Assert.That (builder.ToString (), Does.Contain ("Blink"));
		Assert.That (watch.ElapsedMilliseconds, Is.LessThan (5500));
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
}
