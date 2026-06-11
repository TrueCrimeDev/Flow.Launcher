using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Flow.Launcher.Infrastructure.UserSettings;
using Flow.Launcher.Plugin;
using Flow.Launcher.ViewModel;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Flow.Launcher.Test
{
    [TestFixture]
    internal class ResultViewModelRefreshImageTest
    {
        // The unchecked -> checked use case: after the result's icon source changes,
        // RefreshImage() swaps the row's Image in place and notifies the binding,
        // without anyone rebuilding the ResultViewModel (so the preview is untouched).
        //
        // Uses the Icon delegate rather than IcoPath because the icon load path is
        // deterministic in a headless test (it returns the delegate's value directly),
        // whereas the IcoPath path depends on the Windows shell thumbnail provider.
        [Test]
        [Apartment(ApartmentState.STA)]
        public async Task RefreshImage_AfterIconChanges_UpdatesImageAndRaisesPropertyChangedAsync()
        {
            var uncheckedImage = CreateBitmap(32, 32);
            var checkedImage = CreateBitmap(48, 48);
            ClassicAssert.AreNotSame(uncheckedImage, checkedImage, "Precondition: the two icons must be distinct instances.");

            var result = new Result { Title = "Toggle", Icon = () => uncheckedImage };
            var viewModel = new ResultViewModel(result, new Settings());

            // Initial load resolves to the unchecked icon.
            await WaitForImageAsync(viewModel, uncheckedImage);
            ClassicAssert.AreSame(uncheckedImage, viewModel.Image);

            var changedProperties = new List<string>();
            viewModel.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

            // Act: flip the icon and refresh only this row.
            result.Icon = () => checkedImage;
            viewModel.RefreshImage();
            await WaitForImageAsync(viewModel, checkedImage);

            ClassicAssert.AreSame(checkedImage, viewModel.Image);
            CollectionAssert.Contains(changedProperties, nameof(ResultViewModel.Image));
        }

        private static async Task WaitForImageAsync(ResultViewModel viewModel, ImageSource expected)
        {
            for (var i = 0; i < 50 && !ReferenceEquals(viewModel.Image, expected); i++)
            {
                await Task.Delay(10);
            }
        }

        private static BitmapSource CreateBitmap(int width, int height)
        {
            var stride = width * 4;
            var pixels = new byte[stride * height];
            for (var i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 0x40;
                pixels[i + 1] = 0xA0;
                pixels[i + 2] = 0xE0;
                pixels[i + 3] = 0xFF;
            }

            var source = BitmapSource.Create(
                width,
                height,
                dpiX: 96,
                dpiY: 96,
                PixelFormats.Bgra32,
                palette: null,
                pixels,
                stride);
            source.Freeze();
            return source;
        }
    }
}
