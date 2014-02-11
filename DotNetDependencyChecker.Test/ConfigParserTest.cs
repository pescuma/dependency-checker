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

		private Project ProjWithName(string name)
		{
			return new Project(name, null);
		}

		private Project ProjWithPath(string path)
		{
			return new Project(null, path);
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
			Assert.AreEqual(true, group.Matches(ProjWithName("A.Project.Name")));
			Assert.AreEqual(false, group.Matches(ProjWithName("X")));
		}

		[Test]
		public void TestGroupWithSimpleMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> A.Project.Name");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(ProjWithName(("a.pRoJect.Name")));
		}

		[Test]
		public void TestGroupWithREMatch()
		{
			var config = Parse(@"group: My name -> re: Ab+");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(ProjWithName("Abbb")));
			Assert.AreEqual(false, group.Matches(ProjWithName("A")));
		}

		[Test]
		public void TestGroupWithREMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> re: Ab+");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(ProjWithName("abbb")));
		}

		[Test]
		public void TestGroupWithExactPathMatch()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(ProjWithPath(@"C:\a")));
			Assert.AreEqual(false, group.Matches(ProjWithPath(@"C:\b")));
		}

		[Test]
		public void TestGroupWithExactPathMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(ProjWithPath(@"c:\A")));
		}

		[Test]
		public void TestGroupWithPathPrefixMatch()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(ProjWithPath(@"C:\a\X")));
			Assert.AreEqual(false, group.Matches(ProjWithPath(@"C:\b\X")));
		}

		[Test]
		public void TestGroupWithPathPrefixMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name -> path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, group.Matches(ProjWithPath(@"c:\A\X")));
		}

		[Test]
		public void TestGroupsKeepFileOrder()
		{
			var config = Parse(@"group: My name -> A
group: My name -> B");

			Assert.AreEqual(true, config.Groups[0].Matches(ProjWithName("A")));
			Assert.AreEqual(true, config.Groups[1].Matches(ProjWithName("B")));
		}
	}
}
