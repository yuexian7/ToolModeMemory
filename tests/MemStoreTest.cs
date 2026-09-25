// Offline T3 checks for MemoryStore JSON round-trip (no game assemblies).
// Build: csc /nologo /t:exe /out:memtest.exe Memory\MemoryStore.cs tests\MemStoreTest.cs
// Or run logic assertions below with any C# script host.
using System;
using System.Collections.Generic;
using ToolModeMemory.Memory;

static class MemStoreTest
{
	static int fails;

	static void Check(bool cond, string msg)
	{
		if (!cond)
		{
			fails++;
			Console.WriteLine("FAIL " + msg);
		}
		else
		{
			Console.WriteLine("OK   " + msg);
		}
	}

	static int Main()
	{
		MemoryStore a = new MemoryStore();
		a.SetSaveName("My City / 1");
		a.Set("net.draw", "G:1", 2);
		a.Set("net.snap", "S", 15);
		string json = a.Serialize();
		Check(json.Contains("\"net.draw\""), "serialize has net.draw");
		Check(json.Contains("My City"), "serialize has save name");

		Dictionary<string, Dictionary<string, int>> into = new Dictionary<string, Dictionary<string, int>>();
		MemoryStore.Parse(json, into);
		Check(into.ContainsKey("net.draw") && into["net.draw"]["G:1"] == 2, "parse net.draw G:1 == 2");
		Check(into.ContainsKey("net.snap") && into["net.snap"]["S"] == 15, "parse net.snap S == 15");

		Check(MemoryStore.SanitizeFileName("a/b:c*?") == "a_b_c__", "sanitize invalid chars");

		MemoryStore b = new MemoryStore();
		b.SetSaveName("T1");
		b.Set("obj.place", "P:9", 4);
		string j2 = b.Serialize();
		Dictionary<string, Dictionary<string, int>> into2 = new Dictionary<string, Dictionary<string, int>>();
		MemoryStore.Parse(j2, into2);
		Check(into2["obj.place"]["P:9"] == 4, "parse obj.place");

		// empty / malformed must not throw
		Dictionary<string, Dictionary<string, int>> into3 = new Dictionary<string, Dictionary<string, int>>();
		MemoryStore.Parse("{}", into3);
		MemoryStore.Parse("not json", into3);
		Check(true, "malformed parse is safe");

		Console.WriteLine(fails == 0 ? "ALL PASS" : ("FAILURES=" + fails));
		return fails == 0 ? 0 : 1;
	}
}
