using System.Globalization;
using System.Threading;
using System.Windows.Media;
using Flow.Launcher.Converters;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Flow.Launcher.Test
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class RoundedIconMaskConverterTest
    {
        private static readonly RoundedIconMaskConverter Converter = new();

        [Test]
        public void GivenRoundedIconTrue_WhenConverted_ThenReturnsFrozenBrush()
        {
            var mask = Converter.Convert(true, typeof(Brush), null, CultureInfo.InvariantCulture);

            ClassicAssert.IsInstanceOf<Brush>(mask);
            ClassicAssert.IsTrue(((Brush)mask).IsFrozen);
        }

        [Test]
        public void GivenRoundedIconTrue_WhenConvertedTwice_ThenReturnsSharedInstance()
        {
            var first = Converter.Convert(true, typeof(Brush), null, CultureInfo.InvariantCulture);
            var second = Converter.Convert(true, typeof(Brush), null, CultureInfo.InvariantCulture);

            ClassicAssert.AreSame(first, second);
        }

        [Test]
        public void GivenRoundedIconFalse_WhenConverted_ThenReturnsNull()
        {
            ClassicAssert.IsNull(Converter.Convert(false, typeof(Brush), null, CultureInfo.InvariantCulture));
        }

        [Test]
        public void GivenNonBoolValue_WhenConverted_ThenReturnsNull()
        {
            ClassicAssert.IsNull(Converter.Convert(null, typeof(Brush), null, CultureInfo.InvariantCulture));
            ClassicAssert.IsNull(Converter.Convert("true", typeof(Brush), null, CultureInfo.InvariantCulture));
        }
    }
}
