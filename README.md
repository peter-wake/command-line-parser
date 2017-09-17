# command-line-parser
Command line parser and handler in C# for .Net

You can use it to parse windows style command line options, or Linux style; for example, allows you to implement command-line handling
for an application that runs in both Core and Desktop, using Linux style options.

A Windows command line parser that handles space delimiters and quotes.
It allows backslash to escape quotes within quotes (Linux style).
It allows you to use hyphens, double-hyphens, forward-slash, or anything you like to indicate option tags.
It can handle errors in a convenient way.
It's easy to set-up and use in a new console project.
It works with .Net Core, or Windows Desktop 4.6.2+
(You could make it work with much older versions just be making a new project file).

It doesn't use NuGet to download the test libs (FakeItEasy and NUnit), but that's something that might get fixed.
FakeItEasy and NUnit as used here are unmodified, and so using the NuGet versions would be trivial, but this way
it's easier to integrate the CommandLine library into your own project, and use reference those support assemblies
from a common location.

A class instance is initialised with a command line string, and the instance provides methods to parse that string.


* The command line string is pre-split according to rules that are independent of the matchers.
* The line is broken when whitespace occurs, unless a quote is encountered, in which case processing runs to the next quote.
* Unless the next quote is pre-escaped with a \ character.
* Whitespace outside quotes is discarded and stripped from option strings.
* Whitespace inside quotes is preserved verbatim and does not cause a break.

CommandLineArguments in CommandLineTests shows how you can use it;
basically, create a new command line handler class and inherit CommandLineArgumentsBase.
In the constructor, set up the Matchers list with appropriate matcher methods, in the order you want to run them.
Finish up with the BadArgumentMatcher and Finish matchers (though you don't *have* to do this, if you have reasons not to).

Put properties for the option values you want to decode on your command line argument handler class.
The real work is already done by the underlying code in CommandLineArgumentsBase, so a matcher can be as simple as this:

        private void VerboseMatcher(List<string> arguments)
        {
            const string verboseFlag = "-v";
            const string ultraVerboseFlag = "-V";

            if (FindFlag(verboseFlag, arguments))
            {
                Verbose = true;
            }

            if (FindFlag(ultraVerboseFlag, arguments))
            {
                Verbose = true;
                UltraVerbose = true;
            }
        }
        
        OR (using lamdas)
        
        private void VerboseMatcher(List<string> arguments)
        {
            const string verboseFlag = "-v";
            const string ultraVerboseFlag = "-V";

            FindFlag(verboseFlag, arguments, () =>
                {
                    Verbose = true;
                });

            FindFlag(ultraVerboseFlag, arguments, () =>
                {
                    Verbose = true;
                    UltraVerbose = true;
                });
        }
        
        Options with arguments can be handled like this:
        
        FindParameter("--age", arguments, age => { AgeInYears = int.Parse(age); });

        If you allow a matcher to throw an exception, an Error will be logged, and will ultimately result in a dump of the Errors and a call to the Help method.
        You add or remove Error items in the Finish matcher (or any other matcher, for that matter).
        
