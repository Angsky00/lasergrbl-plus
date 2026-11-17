using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using LaserGRBL.SvgConverter;
using Xunit;

namespace LaserGRBL.Tests.GCode
{
	// El generador usa estado estático (SvgConverter.gcode, Settings): sin paralelismo.
	[CollectionDefinition("GCode", DisableParallelization = true)]
	public class GCodeCollection { }

	/// <summary>
	/// G-code de referencia del LaserGRBL original (sin modificar) con la configuración por defecto.
	/// Sirve de base para comprobar que, con una sola capa, las funciones nuevas producen el mismo G-code.
	/// </summary>
	[Collection("GCode")]
	public class GCodeReferenceTests
	{
		[Fact]
		public void Svg_DosColores_ConfiguracionPorDefecto()
		{
			// Igual que GrblFile.LoadImportedSVG con los valores por defecto de Settings.
			var converter = new GCodeFromSVG();
			converter.GCodeXYFeed = Settings.GetObject("GrayScaleConversion.VectorizeOptions.BorderSpeed", 1000);
			converter.UseLegacyBezier = !Settings.GetObject("Vector.UseSmartBezier", true);

			string gcode = converter.convertFromFile(GoldenFiles.InputPath("dos_colores.svg"), null, ColorFilter.All);

			// Misma normalización que hace LoadImportedSVG al crear los GrblCommand.
			var commands = new List<GrblCommand>();
			foreach (string l in gcode.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
			{
				string line = l.Trim();
				if (line.Length > 0)
				{
					var cmd = new GrblCommand(line);
					if (!cmd.IsEmpty) commands.Add(cmd);
				}
			}

			GoldenFiles.AssertMatches("svg_dos_colores.gcode", GoldenFiles.Join(commands));
		}

		[Fact]
		public void Raster_Line2Line_Degradado()
		{
			var file = new GrblFile();
			var conf = new GrblFile.L2LConf
			{
				res = 5,                // 5 px/mm
				fres = 5,
				oX = 0,
				oY = 0,
				markSpeed = 1000,
				borderSpeed = 1000,
				minPower = 0,
				maxPower = 1000,
				lOn = "M3",
				lOff = "M5",
				dir = RasterConverter.ImageProcessor.Direction.Horizontal,
				pwm = true,
				firmwareType = Firmware.Grbl,
			};

			using (Bitmap bmp = Degradado(20, 10))
				file.LoadImageL2L(bmp, "degradado.png", conf, false, null);

			GoldenFiles.AssertMatches("raster_l2l_degradado.gcode", GoldenFiles.Join(file));
		}

		[Fact]
		public void EncabezadoYCierrePorDefecto_SinCambios()
		{
			Assert.Equal("G90 (use absolute coordinates)", GrblCore.GCODE_STD_HEADER);
			Assert.Equal("G0 X0 Y0 Z0 (move back to origin)", GrblCore.GCODE_STD_FOOTER);
		}

		/// <summary>Imagen sintética determinista: degradado horizontal con un hueco blanco.</summary>
		private static Bitmap Degradado(int w, int h)
		{
			var bmp = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			for (int y = 0; y < h; y++)
				for (int x = 0; x < w; x++)
				{
					int v = (x * 255) / (w - 1);
					if (x >= 8 && x <= 10 && y >= 3 && y <= 5) v = 255; // hueco blanco (láser apagado)
					bmp.SetPixel(x, y, Color.FromArgb(255, v, v, v));
				}
			return bmp;
		}
	}
}
