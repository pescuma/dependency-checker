using NUnit.Framework;

namespace org.pescuma.dependencychecker.input.loaders
{
	[TestFixture]
	public class EclipseProjectsLoaderTest
	{
		private void AssertGuess(string expected, string initial)
		{
			Assert.AreEqual(expected, EclipseProjectsLoader.GuessLibraryName(initial));
		}

		[Test]
		public void GuessLibraryName_Simple()
		{
			AssertGuess("a", "a.jar");
		}

		[Test]
		public void GuessLibraryName_UnknownSuffix()
		{
			AssertGuess("a-sufff", "a-sufff.jar");
		}

		[Test]
		public void GuessLibraryName_KnownSuffix()
		{
			AssertGuess("a", "a-jar.jar");
		}

		[Test]
		public void GuessLibraryName_TowKnownSuffixes()
		{
			AssertGuess("a-jar", "a-jar-bundle.jar");
		}

		[Test]
		public void GuessLibraryName_Version1()
		{
			AssertGuess("a-1", "a-1.jar");
		}

		[Test]
		public void GuessLibraryName_Version11()
		{
			AssertGuess("a", "a-1.1.jar");
		}

		[Test]
		public void GuessLibraryName_Version111()
		{
			AssertGuess("a", "a-1.1.1.jar");
		}

		[Test]
		public void GuessLibraryName_Version1111()
		{
			AssertGuess("a", "a-1.1.1.1.jar");
		}

		[Test]
		public void GuessLibraryName_Version2333()
		{
			AssertGuess("a", "a-1.234.567.890.jar");
		}
	}
}
