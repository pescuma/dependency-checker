using NUnit.Framework;
using org.pescuma.dotnetdependencychecker.config;

namespace org.pescuma.dotnetdependencychecker
{
	[TestFixture]
	public class ConfigParserTest
	{
		private Config Parse(string configFileContents)
		{
			return ConfigParser.ParseLines(configFileContents.Split('\n'));
		}

		[Test]
		public void TestOneInput()
		{
			var config = Parse(@"input: c:\a");

			Assert.AreEqual(1, config.Inputs.Count);
			Assert.AreEqual(@"c:\a", config.Inputs[0]);
		}

		[Test]
		public void TestTwoInput()
		{
			var config = Parse(@"input: c:\a
input: c:\b");

			Assert.AreEqual(2, config.Inputs.Count);
			Assert.AreEqual(@"c:\a", config.Inputs[0]);
			Assert.AreEqual(@"c:\b", config.Inputs[1]);
		}

		[Test]
		public void TestGroupName()
		{
			var config = Parse(@"group: My name -> A");

			Assert.AreEqual(1, config.Groups.Count);

			var group = config.Groups[0];
			Assert.AreEqual(@"My name", group.Name);
		}

		[Test]
		public void TestGroupWithSimpleMatch()
		{
			var config = Parse(@"group: My name -> A.Project.Name");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project("A.Project.Name", null)));
			Assert.AreEqual(false, group.Matches(new Project("X", null)));
		}

		[Test]
		public void TestGroupWithSimpleMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> A.Project.Name");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project("a.pRoJect.Name", null)));
		}

		[Test]
		public void TestGroupWithREMatch()
		{
			var config = Parse(@"group: My name -> re: Ab+");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project("Abbb", null)));
			Assert.AreEqual(false, group.Matches(new Project("A", null)));
		}

		[Test]
		public void TestGroupWithREMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> re: Ab+");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project("abbb", null)));
		}

		[Test]
		public void TestGroupWithExactPathMatch()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project(null, @"C:\a")));
			Assert.AreEqual(false, group.Matches(new Project(null, @"C:\b")));
		}

		[Test]
		public void TestGroupWithExactPathMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project(null, @"c:\A")));
		}

		[Test]
		public void TestGroupWithPathPrefixMatch()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project(null, @"C:\a\X")));
			Assert.AreEqual(false, group.Matches(new Project(null, @"C:\b\X")));
		}

		[Test]
		public void TestGroupWithPathPrefixMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(new Project(null, @"c:\A\X")));
		}
	}
}
