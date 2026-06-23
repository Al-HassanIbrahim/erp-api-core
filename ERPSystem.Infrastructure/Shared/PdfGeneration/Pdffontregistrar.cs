using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using System.IO;
using System.Reflection;

namespace ERPSystem.Infrastructure.Shared.PdfGeneration
{
    /// <summary>
    /// Registers embedded Arabic and Latin fonts with QuestPDF at application startup.
    /// Call PdfFontRegistrar.RegisterAll() once in Program.cs before any PDF generation.
    ///
    /// Embed the following font files as Embedded Resources inside ERPSystem.Infrastructure:
    ///   Resources/Fonts/NotoSansArabic-Regular.ttf
    ///   Resources/Fonts/NotoSansArabic-Bold.ttf
    ///   Resources/Fonts/NotoSans-Regular.ttf
    ///   Resources/Fonts/NotoSans-Bold.ttf
    ///
    /// In the .csproj set their Build Action to "Embedded Resource":
    ///   <EmbeddedResource Include="Resources\Fonts\*.ttf" />
    /// </summary>
    public static class PdfFontRegistrar
    {
        public static void RegisterAll()
        {
            var assembly = Assembly.GetExecutingAssembly();
            RegisterFont(assembly, "ERPSystem.Infrastructure.Resources.Fonts.NotoSansArabic-Regular.ttf");
            RegisterFont(assembly, "ERPSystem.Infrastructure.Resources.Fonts.NotoSansArabic-Bold.ttf");
            RegisterFont(assembly, "ERPSystem.Infrastructure.Resources.Fonts.NotoSans-Regular.ttf");
            RegisterFont(assembly, "ERPSystem.Infrastructure.Resources.Fonts.NotoSans-Bold.ttf");
        }

        private static void RegisterFont(Assembly assembly, string resourceName)
        {
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream is null)
                return; // Font file not embedded — PDF will fall back to system font for this weight

            // Directly pass the stream to RegisterFont, as it expects a Stream, not a byte[]
            FontManager.RegisterFont(stream);
        }
    }
}