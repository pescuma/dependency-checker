using System;
using NUnit.Framework;
using org.pescuma.dotnetdependencychecker.config;
using org.pescuma.dotnetdependencychecker.model;
using org.pescuma.dotnetdependencychecker.rules;

namespace org.pescuma.dotnetdependencychecker
{
	[TestFixture]
	public class ConfigParserTest
	{
		private Config Parse(string configFileContents)
		{
			return new ConfigParser().ParseLines(@"c:\config.txt", configFileContents.Split('\n'));
		}

		private Project ProjWithName(string name)
		{
			return new Project(name, "NO ASSEMBLY NAME", new Guid(), "CSPROJ");
		}

		private Project ProjWithPath(string path)
		{
			return new Project("NO NAME", "NO ASSEMBLY NAME", new Guid(), path);
		}

		[Test]
		[ExpectedException(typeof (ConfigParserException))]
		public void TestInvalidLine()
		{
			Parse(@"!!!");
		}

		[Test]
		public void TestIgnoreCommentedLine()
		{
			Parse(@"#");
		}

		[Test]
		public void TestOneInput()
		{
			var config = Parse(@"input: c:\a");

			Assert.AreEqual(1, config.Inputs.Count);
			Assert.AreEqual(@"c:\a", config.Inputs[0]);
		}

		[Test]
		public void TestOneInputWithoutSpace()
		{
			var config = Parse(@"input:c:\a");

			Assert.AreEqual(1, config.Inputs.Count);
			Assert.AreEqual(@"c:\a", config.Inputs[0]);
		}

		[Test]
		public void TestOneInputRemoveTabs()
		{
			var config = Parse("input:\tc:\\a\t");

			Assert.AreEqual(1, config.Inputs.Count);
			Assert.AreEqual(@"c:\a", config.Inputs[0]);
		}

		[Test]
		public void TestOneInputWithComment()
		{
			var config = Parse(@"input: c:\a   # bla");

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
			Assert.AreEqual(true, group.Matches(ProjWithName("a.pRoJect.Name")));
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

		[Test]
		public void TestOutputProjects()
		{
			var config = Parse(@"output projects: c:\lp.txt");

			Assert.AreEqual(1, config.Output.Projects.Count);
			Assert.AreEqual(@"c:\lp.txt", config.Output.Projects[0]);
		}

		[Test]
		public void TestOutputProjectsTwoTimes()
		{
			var config = Parse(@"output projects: c:\lp.txt
output projects: c:\b.out");

			Assert.AreEqual(2, config.Output.Projects.Count);
			Assert.AreEqual(@"c:\lp.txt", config.Output.Projects[0]);
			Assert.AreEqual(@"c:\b.out", config.Output.Projects[1]);
		}

		[Test]
		public void TestOutputGroups()
		{
			var config = Parse(@"output groups: c:\lp.txt");

			Assert.AreEqual(1, config.Output.Groups.Count);
			Assert.AreEqual(@"c:\lp.txt", config.Output.Groups[0]);

			Assert.AreEqual(0, config.Groups.Count);
		}

		[Test]
		public void TestOutputDependencies()
		{
			var config = Parse(@"output dependencies: c:\lp.txt");

			Assert.AreEqual(1, config.Output.Dependencies.Count);
			Assert.AreEqual(@"c:\lp.txt", config.Output.Dependencies[0]);
		}

		[Test]
		public void TestAllowRuleSimple()
		{
			var config = Parse(@"rule: A -> B");

			Assert.AreEqual(1, config.Rules.Count);

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(true, rule.Allow);

			Assert.AreEqual(true, rule.Source(ProjWithName("A")));
			Assert.AreEqual(false, rule.Source(ProjWithName("B")));

			Assert.AreEqual(false, rule.Target(ProjWithName("A")));
			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestDenyRuleSimple()
		{
			var config = Parse(@"rule: A -X-> B");

			Assert.AreEqual(1, config.Rules.Count);

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(false, rule.Allow);

			Assert.AreEqual(true, rule.Source(ProjWithName("A")));
			Assert.AreEqual(false, rule.Source(ProjWithName("B")));

			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
			Assert.AreEqual(false, rule.Target(ProjWithName("A")));
		}

		[Test]
		public void TestAllowRuleWithBothREs()
		{
			var config = Parse(@"rule: re: Ab+ -> re: Ba+");

			Assert.AreEqual(1, config.Rules.Count);

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(true, rule.Allow);

			Assert.AreEqual(true, rule.Source(ProjWithName("Abb")));
			Assert.AreEqual(false, rule.Source(ProjWithName("A")));

			Assert.AreEqual(true, rule.Target(ProjWithName("Baa")));
			Assert.AreEqual(false, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestRuleDontAllowCircularDependencies()
		{
			var config = Parse(@"rule: don't allow circular dependencies");

			Assert.AreEqual(1, config.Rules.Count);

			var rule = config.Rules[0];
			Assert.AreEqual(true, rule is NoCircularDepenendenciesRule);
		}

		[Test]
		public void TestIgnoreSimple()
		{
			var config = Parse(@"ignore: A");

			Assert.AreEqual(1, config.Ignores.Count);
			var ignore = config.Ignores[0];
			Assert.AreEqual(true, ignore.Matches(ProjWithName("A")));
			Assert.AreEqual(false, ignore.Matches(ProjWithName("B")));
		}

		[Test]
		public void TestIgnoreAllNonLocalProjects()
		{
			var config = Parse("input: c:\\a\n" + //
			                   "ignore all references not in includes");

			Assert.AreEqual(1, config.Ignores.Count);
			var ignore = config.Ignores[0];
			Assert.AreEqual(false, ignore.Matches(ProjWithPath(@"c:\a\p.csproj")));
			Assert.AreEqual(true, ignore.Matches(ProjWithPath(@"c:\b\p.csproj")));
		}

		[Test]
		public void TestIgnoreAllNonLocalProjectsCaseInsensitive()
		{
			var config = Parse("input: C:\\A\n" + //
			                   "ignore all references not in includes");

			Assert.AreEqual(1, config.Ignores.Count);
			var ignore = config.Ignores[0];
			Assert.AreEqual(false, ignore.Matches(ProjWithPath(@"c:\a\p.csproj")));
			Assert.AreEqual(true, ignore.Matches(ProjWithPath(@"c:\b\p.csproj")));
		}

		[Test]
		public void TestRuleLocation_FirstLine()
		{
			var config = Parse("rule: a -> B");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(1, rule.Location.LineNum);
			Assert.AreEqual("rule: a -> B", rule.Location.LineText);
		}

		[Test]
		public void TestRuleLocation_SecondLine()
		{
			var config = Parse("\nrule: a -> B");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(2, rule.Location.LineNum);
			Assert.AreEqual("rule: a -> B", rule.Location.LineText);
		}

		[Test]
		public void TestRuleSeverity_None()
		{
			var config = Parse("rule: a -> B");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
		}

		[Test]
		public void TestRuleSeverity_Warning()
		{
			var config = Parse("rule: a -> B [warning]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Warn, rule.Severity);
			Assert.AreEqual(true, rule.Source(ProjWithName("a")));
			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestRuleSeverity_Error()
		{
			var config = Parse("rule: a -> B [error]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(true, rule.Source(ProjWithName("a")));
			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestRuleSeverity_Info()
		{
			var config = Parse("rule: a -> B [info]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, rule.Source(ProjWithName("a")));
			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestRuleSeverity_CaseInsensitive()
		{
			var config = Parse("rule: a -> B [iNfO]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, rule.Source(ProjWithName("a")));
			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestRuleSeverity_WithTabs()
		{
			var config = Parse("rule: a -> B\t\t\t \t[iNfO]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, rule.Source(ProjWithName("a")));
			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestRuleSeverity_NoSpace()
		{
			var config = Parse("rule: a -> B[iNfO]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, rule.Source(ProjWithName("a")));
			Assert.AreEqual(true, rule.Target(ProjWithName("B")));
		}

		[Test]
		public void TestNoCircularDependenciesRuleSeverity()
		{
			var config = Parse("rule: don't allow circular dependencies [info]");

			var rule = (NoCircularDepenendenciesRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
		}
	}
}
