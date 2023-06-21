using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Drones.Gui.Core
{
    public class MaterialIconConverter:IValueConverter
    {
        public static IValueConverter  Instance { get; } = new MaterialIconConverter();
        
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MaterialIconKind kind)
            {
                return new DrawingImage
                {
                    Drawing = new GeometryDrawing
                    {
                        Geometry = PathGeometry.Parse(MaterialIconDataProvider.GetData(kind))
                    }
                };
            }

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}