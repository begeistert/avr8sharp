namespace AVr8SharpTests;

[TestFixture]
public class Cpu
{
	[Test(Description = "The initial value of the stack pointer should be the lasr byte of internal SRAM")]
	public void InitialStackPointerValue()
	{
		var cpu = new AVR8Sharp.Cpu.Cpu(new byte[1024], 0x1000);
		Assert.That(cpu.SP, Is.EqualTo(0x10FF));
	}
	
	[TestFixture]
	public class Events
	{
		[Test(Description = "The queued events should be executed after a given number of cycles")]
		public void ExecuteQueuedEvents ()
		{
			var cpu = new AVR8Sharp.Cpu.Cpu(new byte[1024], 0x1000);
			var events = new List<KeyValuePair<int, int>> ();
			int[] list = [1, 4, 10, ];
			for (int i = 0; i < list.Length; i++) {
				var value = list[i];
				cpu.AddClockEvent (() => {
					events.Add (new KeyValuePair<int, int> (value, cpu.Cycles));
				}, value);
			}
			for (var i = 0; i < 10; i++) {
				cpu.Cycles++;
				cpu.Tick ();
			}
			
			// Events length should be 3
			Assert.That(events, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
	            // events[0] should be (1, 1)
                Assert.That(events[0].Key, Is.EqualTo(1));
                Assert.That(events[0].Value, Is.EqualTo(1));
                // events[1] should be (4, 4)
                Assert.That(events[1].Key, Is.EqualTo(4));
                Assert.That(events[1].Value, Is.EqualTo(4));
                // events[2] should be (10, 10)
                Assert.That(events[2].Key, Is.EqualTo(10));
                Assert.That(events[2].Value, Is.EqualTo(10));
            });
        }

		[Test(Description = "The queued events should be correctly sorted when added in reverse order")]
		public void OrderReversedEvents ()
		{
			var cpu = new AVR8Sharp.Cpu.Cpu(new byte[1024], 0x1000);
			var events = new List<KeyValuePair<int, int>> ();
			int[] list = [10, 4, 1, ];
			for (int i = 0; i < list.Length; i++) {
				var value = list[i];
				cpu.AddClockEvent (() => {
					events.Add (new KeyValuePair<int, int> (value, cpu.Cycles));
				}, value);
			}
			for (var i = 0; i < 10; i++) {
				cpu.Cycles++;
				cpu.Tick ();
			}
			
			// Events length should be 3
			Assert.That(events, Has.Count.EqualTo(3));
			Assert.Multiple(() =>
			{
	            // events[0] should be (1, 1)
				Assert.That(events[0].Key, Is.EqualTo(1));
				Assert.That(events[0].Value, Is.EqualTo(1));
				// events[1] should be (4, 4)
				Assert.That(events[1].Key, Is.EqualTo(4));
				Assert.That(events[1].Value, Is.EqualTo(4));
				// events[2] should be (10, 10)
				Assert.That(events[2].Key, Is.EqualTo(10));
				Assert.That(events[2].Value, Is.EqualTo(10));
			});
		}

		class UpdateClockEvent
		{
			
		}
	}
}
