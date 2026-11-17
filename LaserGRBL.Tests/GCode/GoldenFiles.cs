using System;
using System.IO;
using System.Text;
using Xunit;

namespace LaserGRBL.Tests.GCode
{
	/// <summary>
	/// Compara el G-code generado contra archivos de referencia (golden files) en GCode\Golden.
	/// Las referencias se generan con el código de LaserGRBL sin modificar; siempre se escribe
	/// la salida actual en "gcode-out" (CI la sube como artefacto) para poder revisarla o regenerarla.
	/// </summary>
	internal static class GoldenFiles
	{
		private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);
		private static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;

		public static string InputPath(string name) => Path.Combine(BaseDir, "GCode", "Inputs", name);

		public static void AssertMatches(string name, string actual)
		{
			string outDir = Path.Combine(BaseDir, "gcode-out");
			Directory.CreateDirectory(outDir);
			byte[] actualBytes = Utf8NoBom.GetBytes(actual);
			File.WriteAllBytes(Path.Combine(outDir, name), actualBytes);

			string golden = Path.Combine(BaseDir, "GCode", "Golden", name);
			Assert.True(File.Exists(golden), $"Falta el archivo de referencia GCode/Golden/{name}. La salida actual quedó en gcode-out/{name}.");

			byte[] expectedBytes = File.ReadAllBytes(golden);
			Assert.Equal(Utf8NoBom.GetString(expectedBytes), actual); // diff legible
			Assert.Equal(expectedBytes, actualBytes);                   // byte por byte
		}

		/// <summary>Une los comandos como los escribe GrblFile.SaveGCODE en Windows (CRLF), sin encabezado ni cierre.</summary>
		public static string Join(System.Collections.Generic.IEnumerable<GrblCommand> commands)
		{
			var sb = new StringBuilder();
			foreach (GrblCommand cmd in commands)
				sb.Append(cmd.Command).Append("\r\n");
			return sb.ToString();
		}
	}
}
