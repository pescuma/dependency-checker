# dependency-checker

The objective of this project is to allow to fail a build when an incorrect depepdency is created.

To do that, it identifies the projects and libraries being used and allows you to specify the rules that must be followed.

There is also a second command line utility called `dependency-console` that can be used to interactively query the dependency data.


## Executing

You just call `dependency-checker <config file>`. All the information needed comes from the config file. 

The executable exit code is the number of errors found. So you can just fail the build if it return a non 0 result. Please not that it has 3 different message levels: Info, Warning and Error and only errors are counted on the result code. This can be used to allow some incorrect dependencies to exist but not fail the build. (more on that latter)


## Config file

As an example, the config file for dependency checker itself is:

```
# List of folders to search for projects. 
# All subfolders are searched.
# All paths are relative to the config file.
input: ..


# Projects and libraries can be ignored
ignore: System

# Ignore all libraries or projects that are not inside the input folders.
ignore: non local: *

# Ignore all libraries that are not projects.
ignore: lib: *

# Both previous could be written as
#ignore: not: local project: *


# An assembly can be in only one group.
# The first line that matches will be used.
group: Command line += *.Cli
group: Tests += *.Test


# These rules always run
rule: don't allow circular dependencies
rule: no two projects with same name
rule: no two projects with same GUID
rule: no two projects with same name and GUID
rule: avoid same dependency twice				[warning]

# Dependency rules are different. The first one that matches wins.
# They can allow or disallow a dependency.
# -> means that the dependency is allowed
# -X-> means that the dependency is NOT allowed
rule: * -X-> Command line
rule: DependencyChecker.Utils -X-> *
rule: DependencyChecker.Model -X-> DependencyChecker.Presenter
rule: Tests -X-> Tests
rule: * -X-> Tests


# This can be used to ignore some infos (but those help to debug this configuration file).
#in output: ignore loading infos
#in output: ignore config infos


# Different kinds of output.
# Please note that the file extension change the file format.
# All paths are relative to the config file.
output projects:		Results\Reports\DependenciesChecker\projects.txt
output groups:			Results\Reports\DependenciesChecker\groups.txt
output dependencies:	Results\Reports\DependenciesChecker\dependencies.txt 
output dependencies:	Results\Reports\DependenciesChecker\dependencies.xml
output dependencies:	Results\Reports\DependenciesChecker\dependencies.dot
output dependencies with errors: Results\Reports\DependenciesChecker\dependencies-with-errors.dot
output architecture:	Results\Reports\DependenciesChecker\architecture.txt 
output architecture:	Results\Reports\DependenciesChecker\architecture.xml
output architecture:	Results\Reports\DependenciesChecker\architecture.dot 
output results:			Results\Reports\DependenciesChecker\errors.txt
output results:			Results\Reports\DependenciesChecker\errors.xml
output results: console
```

Let's see what that means:


### Comemnts

Comments are all lines that start with `#` and also all text after a `#` in a line.


### Inputs - `input:`

Inputs are the paths where the projects and libraries are read. Currently, Visual Studio and Java Eclipse projects are supported. 

All sub-directories are searched too.

The projects found are called projects, and the libraries used (that are not projects or the compilation results of projects) are called libraries.

Please note that if you have a project called P1 that compiles to `c:\P1.dll` and other projects depends on `c:\tmp\P1.dll`, a dependency between the projects will be created (even if the paths are different).

Also note that a project can have more than 1 name: one for the project itself and one for the dll (or jar) file created. They also can have multiple paths: one for the project and other(s) for the dlls that are used by other projects.

One last detail, there are 2 types of libraries/projects: local and non local. A local one means that it was found inside one of the inputs. A non local one was found outside the inputs. If a project references another that is outside the input folders, it will also be read.


### Ignoring projects in the analysis - `ignore:`

You can ignore any project or library loaded from the inputs. The rules of matching are listed bellow (in Matching projects and library names).

For ex: to ignore everything that is not inside the inputs, you can add the line `ignore: non local: *`


### Creating groups - `group:`

It's possible to create groups of projects and/or libraries. This helps when writing rules, because you can reference the whole group by its name.

To add elements to a group, use `group: Group name += Project.Name`. This creates the group `Group Name` and adds the project `Project.Name` to it. (all matchings described in Matching projects and library names can be used)

Note that a library/project can be in only one group, so the first line that matches the project identify its group. This means that the order of the group lines in the config file can impact the results.


### Creating rules - `rule:`

This is the core part of the file. The rules describe what dependencies can or cannot exist. There is also some rules to detect problems in the project files.

##### Allowing a dependency

To allow a dependency between 2 libraries/projects, use `->`. For ex: `rule: Tests -> Command line` says that the tests can depend on the command line projects. Note that these can be group names (or any matchings described in Matching projects and library names).

Allowed dependencies do not generate errors for them.

##### Banning a dependency

To ban a dependency between 2 libraries/projects, use `-X->`. For ex: `rule:  * -X-> Tests` says that nothing can depend on the tests. Those do generate errors.

##### Order

Note that the order of the rules is important. The first rule that matches a dependency is used and defines if the dependency is allowed or not. 

##### Severity

You can also specify a severity at the end of the rule line. The possible values are: `info`, `warning` and `error`. Error is the default. For ex: `rule:  * -X-> Tests [warning]`.

This can be used to avoid build errors on dependencies that should not exist, but do exist. For example, you can write:
```
rule:  WrongProject -X-> Tests [warning]
rule:  * -X-> Tests
```
This will make the dependency of `WrongProject` with `Tests` to generate a warning but it won't affect the exit code.





### Matching projects and library names

In some of the lines you need to specify a library name (this include projects names too). There are multiple ways to specify a library name: (all matching is case insensitive)

###### By name
Just any of the project or library names. For ex: `Project.Name`.

###### Using `*`
Similar to filename matching. For ex: `*.Name` matches all libraries/projects with name ending in `.Name`.

###### Using regular expressions - `regex:`
Any regular expression can be used. Prefix the regex with `regex:`. For ex: `regex: .*\.Name` matches all libraries/projects with name ending in `.Name`.

###### Match by path - `path:`
Can be the full path of a folder name. When using a folder name all sub-folders match too. For ex: `path: C:\` will match all libraries/projects in the `C` drive.

###### Match by language - `lang:`
Matches all libraries/projects that are in the language. Please note that, for libraries, not always the language can ge guessed. For ex: `lang: java` matches all java libraries/projects

###### Match by type - `project:` or `lib:`

###### Match only local or not local - `local:` or `non local:`
Local libraries/projects are the ones inside one of the inputs. Non local are the ones that are outside the input folders. If a project references a librarie or a project outside the input paths, those are also read.

###### Match by type and local or not local - `local project:` or `non local project:` or `local lib:` or `lnon local lib:`

###### Inverting a test - `not:`
Inverts the test that cames after it. For ex: `not: lang: C#` matches all projects that are not in C#. 

Please note that `not: local project: *` is different from `non local project: *`. The first one includes all the libraries too.


