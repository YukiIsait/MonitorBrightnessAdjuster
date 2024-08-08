using System.IO;
using System.Reflection;
using System.Text;

namespace MonitorBrightnessAdjuster {
    public static class AboutUtil {
        public static string GetAboutInformation() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            AssemblyProductAttribute product = (AssemblyProductAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
            AssemblyFileVersionAttribute version = (AssemblyFileVersionAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyFileVersionAttribute));
            AssemblyDescriptionAttribute description = (AssemblyDescriptionAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
            AssemblyCopyrightAttribute copyright = (AssemblyCopyrightAttribute) Attribute.GetCustomAttribute(assembly, typeof(AssemblyCopyrightAttribute));

            StringBuilder sb = new();
            sb.Append(product.Product)
              .Append(' ')
              .Append(version.Version)
              .Append(Environment.NewLine)
              .Append(copyright.Copyright)
              .Append(Environment.NewLine)
              .Append(Environment.NewLine)
              .Append(description.Description);
            return sb.ToString();
        }
    }
}
