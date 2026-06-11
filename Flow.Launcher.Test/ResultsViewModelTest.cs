using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
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
    internal class ResultsViewModelTest
    {
        [Test]
        [Apartment(ApartmentState.STA)]
        public void RefreshImage_RaisesImageChangeOnlyOnTheMatchingResult()
        {
            var resultA = new Result { Title = "A", Icon = CreateIcon() };
            var resultB = new Result { Title = "B", Icon = CreateIcon() };
            var viewModelA = new ResultViewModel(resultA, new Settings());
            var viewModelB = new ResultViewModel(resultB, new Settings());

            var collection = new ResultsViewModel.ResultCollection();
            collection.Update([viewModelA, viewModelB]);

            var aImageChanges = 0;
            var bImageChanges = 0;
            viewModelA.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ResultViewModel.Image)) aImageChanges++; };
            viewModelB.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ResultViewModel.Image)) bImageChanges++; };

            var refreshed = collection.RefreshImage(resultA);

            ClassicAssert.IsTrue(refreshed);
            ClassicAssert.AreEqual(1, aImageChanges, "The matching row's icon should reload.");
            ClassicAssert.AreEqual(0, bImageChanges, "Other rows must not be touched.");
        }

        [Test]
        public void RefreshImage_WhenResultNotInCollection_ReturnsFalse()
        {
            var collection = new ResultsViewModel.ResultCollection();
            collection.Update([ViewModel("Present")]);

            ClassicAssert.IsFalse(collection.RefreshImage(new Result { Title = "Absent" }));
        }


        [Test]
        public void GivenSameCountResults_WhenUpdated_ThenCollectionDoesNotReset()
        {
            var collection = new ResultsViewModel.ResultCollection();
            collection.Update([ViewModel("Answer.")]);

            var actions = new List<NotifyCollectionChangedAction>();
            collection.CollectionChanged += (_, args) => actions.Add(args.Action);

            collection.Update([ViewModel("Answer..")]);

            CollectionAssert.DoesNotContain(actions, NotifyCollectionChangedAction.Reset);
            CollectionAssert.Contains(actions, NotifyCollectionChangedAction.Replace);
            ClassicAssert.AreEqual("Answer..", collection[0].Result.Title);
        }

        [Test]
        public void GivenSameResultViewModelInstance_WhenUpdated_ThenCollectionDoesNotRaiseNoOpReplace()
        {
            var collection = new ResultsViewModel.ResultCollection();
            var viewModel = ViewModel("Answer.");
            collection.Update([viewModel]);

            var actions = new List<NotifyCollectionChangedAction>();
            collection.CollectionChanged += (_, args) => actions.Add(args.Action);

            collection.Update([viewModel]);

            CollectionAssert.IsEmpty(actions);
            ClassicAssert.AreSame(viewModel, collection[0]);
        }

        private static ResultViewModel ViewModel(string title)
        {
            return new ResultViewModel(
                new Result
                {
                    Title = title,
                    SubTitle = "thinking",
                    IcoPath = "Images\\display\\Shorty_Ani1.png",
                    Score = 100
                },
                new Settings());
        }

        private static Result.IconDelegate CreateIcon()
        {
            var source = BitmapSource.Create(
                1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[] { 0x40, 0xA0, 0xE0, 0xFF }, 4);
            source.Freeze();
            return () => source;
        }
    }
}
