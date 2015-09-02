using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Reflection;
using RuntimePInvoke;

namespace Test {
	class Program {
		[RuntimeDllImport]
		static Func<IntPtr, string, string, uint, int> MessageBox;

		static void Main(string[] args) {
			Console.Title = "Test";

			PInvoke.Load(typeof(Program), "user32");
			MessageBox(IntPtr.Zero, "Hello Runtime P/Invoke!", "Caption!", 0);

			Console.WriteLine("Done!");
			Console.ReadLine();
		}
	}
}