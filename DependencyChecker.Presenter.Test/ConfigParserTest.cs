﻿using NUnit.Framework;
using org.pescuma.dependencychecker.presenter.config;
using org.pescuma.dependencychecker.presenter.output.dependencies;
using org.pescuma.dependencychecker.presenter.rules;

namespace org.pescuma.dependencychecker.presenter
{
	[TestFixture]
	public class ConfigParserTest
	{
		private Config Parse(string configFileContents)
		{
			return new ConfigParser().ParseLines(@"c:\config.txt", configFileContents.Split('\n'));
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
			var config = Parse(@"group: My name += A");

			Assert.AreEqual(1, config.Groups.Count);

			var group = config.Groups[0];
			Assert.AreEqual(@"My name", group.Name);
		}

		private static bool MatchesProjWithName(LibraryMatcher matcher, string name)
		{
			return matcher(TestUtils.ProjWithName(name), Matchers.NullReporter);
		}

		private static bool MatchesProjWithPath(LibraryMatcher matcher, string path)
		{
			return matcher(TestUtils.ProjWithPath(path), Matchers.NullReporter);
		}

		[Test]
		public void TestGroupWithSimpleMatch()
		{
			var config = Parse(@"group: My name += A.Project.Name");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithName(group.Matches, "A.Project.Name"));
			Assert.AreEqual(false, MatchesProjWithName(group.Matches, "X"));
		}

		[Test]
		public void TestGroupWithSimpleMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name += A.Project.Name");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithName(group.Matches, "a.pRoJect.Name"));
		}

		[Test]
		public void TestGroupWithSimpleMatchAndNot()
		{
			var config = Parse(@"group: My name += not: A.Project.Name");

			var group = config.Groups[0];
			Assert.AreEqual(false, MatchesProjWithName(group.Matches, "A.Project.Name"));
			Assert.AreEqual(true, MatchesProjWithName(group.Matches, "X"));
		}

		[Test]
		public void TestGroupWithREMatch()
		{
			var config = Parse(@"group: My name += regex: Ab+");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithName(group.Matches, "Abbb"));
			Assert.AreEqual(false, MatchesProjWithName(group.Matches, "A"));
		}

		[Test]
		public void TestGroupWithREMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name += regex: Ab+");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithName(group.Matches, "abbb"));
		}

		[Test]
		public void TestGroupWithExactPathMatch()
		{
			var config = Parse(@"group: My name += path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithPath(group.Matches, @"C:\a"));
			Assert.AreEqual(false, MatchesProjWithPath(group.Matches, @"C:\b"));
		}

		[Test]
		public void TestGroupWithExactPathMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name += path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithPath(group.Matches, @"c:\A"));
		}

		[Test]
		public void TestGroupWithPathPrefixMatch()
		{
			var config = Parse(@"group: My name += path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithPath(group.Matches, @"C:\a\X"));
			Assert.AreEqual(false, MatchesProjWithPath(group.Matches, @"C:\b\X"));
		}

		[Test]
		public void TestGroupWithPathPrefixMatchIsCaseInsensitive()
		{
			var config = Parse(@"group: My name += path: C:\a");

			var group = config.Groups[0];
			Assert.AreEqual(true, MatchesProjWithPath(group.Matches, @"c:\A\X"));
		}

		[Test]
		public void TestGroupsKeepFileOrder()
		{
			var config = Parse(@"group: My name += A
group: My name += B");

			Assert.AreEqual(true, MatchesProjWithName(config.Groups[0].Matches, "A"));
			Assert.AreEqual(true, MatchesProjWithName(config.Groups[1].Matches, "B"));
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
			Assert.IsTrue(config.Output.Dependencies[0] is TextDependenciesOutputer);
		}

		[Test]
		public void TestAllowRuleSimple()
		{
			var config = Parse(@"rule: A -> B");

			Assert.AreEqual(1, config.Rules.Count);

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(true, rule.Allow);

			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "A"));
			Assert.AreEqual(false, MatchesProjWithName(rule.Source, "B"));

			Assert.AreEqual(false, MatchesProjWithName(rule.Target, "A"));
			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
		}

		[Test]
		public void TestDenyRuleSimple()
		{
			var config = Parse(@"rule: A -X-> B");

			Assert.AreEqual(1, config.Rules.Count);

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(false, rule.Allow);

			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "A"));
			Assert.AreEqual(false, MatchesProjWithName(rule.Source, "B"));

			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
			Assert.AreEqual(false, MatchesProjWithName(rule.Target, "A"));
		}

		[Test]
		public void TestAllowRuleWithBothREs()
		{
			var config = Parse(@"rule: regex: Ab+ -> regex: Ba+");

			Assert.AreEqual(1, config.Rules.Count);

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(true, rule.Allow);

			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "Abb"));
			Assert.AreEqual(false, MatchesProjWithName(rule.Source, "A"));

			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "Baa"));
			Assert.AreEqual(false, MatchesProjWithName(rule.Target, "B"));
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
			Assert.AreEqual(true, MatchesProjWithName(ignore.Matches, "A"));
			Assert.AreEqual(false, MatchesProjWithName(ignore.Matches, "B"));
		}

		[Test]
		public void TestIgnoreAllNonLocalProjects()
		{
			var config = Parse("input: c:\\a\n" + //
			                   "ignore: non local: regex: .*");

			Assert.AreEqual(1, config.Ignores.Count);
			var ignore = config.Ignores[0];
			Assert.AreEqual(false, ignore.Matches(TestUtils.LocalProj(), Matchers.NullReporter));
			Assert.AreEqual(true, ignore.Matches(TestUtils.NonLocalProj(), Matchers.NullReporter));
		}

		[Test]
		public void TestIgnoreAllLocalProjects()
		{
			var config = Parse("input: c:\\a\n" + //
			                   "ignore: local: regex: .*");

			Assert.AreEqual(1, config.Ignores.Count);
			var ignore = config.Ignores[0];
			Assert.AreEqual(true, ignore.Matches(TestUtils.LocalProj(), Matchers.NullReporter));
			Assert.AreEqual(false, ignore.Matches(TestUtils.NonLocalProj(), Matchers.NullReporter));
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
			Assert.AreEqual(Severity.Warning, rule.Severity);
			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "a"));
			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
		}

		[Test]
		public void TestRuleSeverity_Error()
		{
			var config = Parse("rule: a -> B [error]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "a"));
			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
		}

		[Test]
		public void TestRuleSeverity_Info()
		{
			var config = Parse("rule: a -> B [info]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "a"));
			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
		}

		[Test]
		public void TestRuleSeverity_CaseInsensitive()
		{
			var config = Parse("rule: a -> B [iNfO]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "a"));
			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
		}

		[Test]
		public void TestRuleSeverity_WithTabs()
		{
			var config = Parse("rule: a -> B\t\t\t \t[iNfO]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "a"));
			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
		}

		[Test]
		public void TestRuleSeverity_NoSpace()
		{
			var config = Parse("rule: a -> B[iNfO]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
			Assert.AreEqual(true, MatchesProjWithName(rule.Source, "a"));
			Assert.AreEqual(true, MatchesProjWithName(rule.Target, "B"));
		}

		[Test]
		public void TestNoCircularDependenciesRuleSeverity()
		{
			var config = Parse("rule: don't allow circular dependencies [info]");

			var rule = (NoCircularDepenendenciesRule) config.Rules[0];
			Assert.AreEqual(Severity.Info, rule.Severity);
		}

		private static bool MatchesLibraryDependency(DepenendencyRule rule, string p1, string p2, string path)
		{
			return rule.Dependency(TestUtils.LibraryDependency(p1, p2, path), Matchers.NullReporter);
		}

		private static bool MatchesProjectDependency(DepenendencyRule rule, string p1, string p2)
		{
			return rule.Dependency(TestUtils.ProjectDependency(p1, p2), Matchers.NullReporter);
		}

		[Test]
		public void TestRuleDependencyType_Library()
		{
			var config = Parse("rule: a -> b[dep: library]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(true, MatchesLibraryDependency(rule, "a", "b", "c:"));
			Assert.AreEqual(false, MatchesProjectDependency(rule, "a", "b"));
		}

		[Test]
		public void TestRuleDependency_Type_Project()
		{
			var config = Parse("rule: a -> b[dep: project]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(false, MatchesLibraryDependency(rule, "a", "b", "c:"));
			Assert.AreEqual(true, MatchesProjectDependency(rule, "a", "b"));
		}

		[Test]
		public void TestRuleDependency_Type_Library_CaseInsensitive()
		{
			var config = Parse("rule: a -> b[dep: LiBrAry]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(true, MatchesLibraryDependency(rule, "a", "b", "c:"));
			Assert.AreEqual(false, MatchesProjectDependency(rule, "a", "b"));
		}

		[Test]
		public void TestRuleDependency_Type_Project_CaseInsensitive()
		{
			var config = Parse("rule: a -> b[dep: pRoJeCt]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(false, MatchesLibraryDependency(rule, "a", "b", "c:"));
			Assert.AreEqual(true, MatchesProjectDependency(rule, "a", "b"));
		}

		[Test]
		public void TestRuleDependency_Type_Library_WithTabs()
		{
			var config = Parse("rule: a -> b[dep:\t\tlibrary]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(true, MatchesLibraryDependency(rule, "a", "b", "c:"));
			Assert.AreEqual(false, MatchesProjectDependency(rule, "a", "b"));
		}

		[Test]
		public void TestRuleDependency_Path()
		{
			var config = Parse(@"rule: a -> b[dep path: c:\x]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(true, MatchesLibraryDependency(rule, "a", "b", @"c:\x\y.dll"));
			Assert.AreEqual(false, MatchesLibraryDependency(rule, "a", "b", @"c:\y\y.dll"));
			Assert.AreEqual(false, MatchesProjectDependency(rule, "a", "b"));
		}

		[Test]
		public void TestRuleDependency_PathRE()
		{
			var config = Parse(@"rule: a -> b[dep path regex: .*\\x.*]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(true, MatchesLibraryDependency(rule, "a", "b", @"c:\x\y.dll"));
			Assert.AreEqual(true, MatchesLibraryDependency(rule, "a", "b", @"c:\y\x.dll"));
			Assert.AreEqual(false, MatchesLibraryDependency(rule, "a", "b", @"c:\y\y.dll"));
			Assert.AreEqual(false, MatchesProjectDependency(rule, "a", "b"));
		}

		[Test]
		public void TestRuleDependencyType_Not()
		{
			var config = Parse("rule: a -> b[not: dep: library]");

			var rule = (DepenendencyRule) config.Rules[0];
			Assert.AreEqual(Severity.Error, rule.Severity);
			Assert.AreEqual(false, MatchesLibraryDependency(rule, "a", "b", "c:"));
			Assert.AreEqual(true, MatchesProjectDependency(rule, "a", "b"));
		}
	}
}
