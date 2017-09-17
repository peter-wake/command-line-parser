using System;
using System.Collections.Generic;
using CommandLine;
using NUnit.Framework;

namespace CommandLineTests
{
    [TestFixture]
    public class CommandLineParserTests
    {
        [Test]
        public void command_line_parser_can_be_constructed()
        {
            var parser = new CommandLineParser("");
            Assert.IsNotNull(parser);
        }

        [Test]
        public void command_line_parser_empty_string_returns_empty_list()
        {
            var parser = new CommandLineParser("");
            var results = parser.Parse(new List<CommandLineParser.OptionMatcher>());
            Assert.IsNotNull(results);
            CollectionAssert.IsEmpty(results);
            Assert.IsNull(parser.ProgramName);
        }

        [Test]
        public void command_line_parser_split_string_empty_string_returns_empty_list()
        {
            var split = CommandLineParser.SplitString("");
            Assert.IsNotNull(split);
            CollectionAssert.IsEmpty(split);
        }

        [Test]
        public void command_line_parser_split_string_no_spaces_returns_single_item()
        {
            var line = "foo";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual(line, split[0]);
        }

        [Test]
        public void command_line_parser_split_string_with_spaces_returns_split_items()
        {
            var line = "foo bar moo";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(3, split.Count);
            Assert.AreEqual("foo", split[0]);
            Assert.AreEqual("bar", split[1]);
            Assert.AreEqual("moo", split[2]);
        }

        [Test]
        public void command_line_parser_split_string_with_multiple_spaces_returns_split_items()
        {
            var line = " foo  bar  moo  ";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(3, split.Count);
            Assert.AreEqual("foo", split[0]);
            Assert.AreEqual("bar", split[1]);
            Assert.AreEqual("moo", split[2]);
        }

        [Test]
        public void command_line_parser_split_string_with_quoted_space_returns_single_item()
        {
            var line = "\"foo bar\"";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("foo bar", split[0]);
        }

        [Test]
        public void command_line_parser_split_string_with_quoted_space_preserves_spaces()
        {
            var line = "\"  foo  bar  \"";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("  foo  bar  ", split[0]);
        }

        [Test]
        public void command_line_parser_split_string_with_quotes_can_escape_quote()
        {
            var line = "\"foo \\\"bar \"";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(1, split.Count);
            Assert.AreEqual("foo \"bar ", split[0]);
        }

        [Test]
        public void command_line_parser_split_string_open_quotes_make_new_item()
        {
            var line = "foo\" bar\"moo"; // close quote doesn't make new item though
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(2, split.Count);
            Assert.AreEqual("foo", split[0]);
            Assert.AreEqual(" barmoo", split[1]);
        }

        [Test]
        public void command_line_parser_split_string_escape_non_quote_does_nothing()
        {
            var line = @"foo \x \\y";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual("foo", split[0]);
            Assert.AreEqual(@"\x", split[1]);
            Assert.AreEqual(@"\y", split[2]);
        }

        [Test]
        public void command_line_split_string_accepts_unquoted_file_paths()
        {
            var line = @"prog -x C:\test\file"; // Should retain the backslashes because they don't escape quotes
            var splits = CommandLineParser.SplitString(line);

            Assert.AreEqual("prog", splits[0]);
            Assert.AreEqual("-x", splits[1]);
            Assert.AreEqual(@"C:\test\file", splits[2]);
        }

        [Test]
        public void command_line_split_string_accepts_quoted_file_paths()
        {
            var line = "prog -x \"C:\\te st\\file\""; // Should retain the backslashes because they don't escape quotes
            var splits = CommandLineParser.SplitString(line);

            Assert.AreEqual("prog", splits[0]);
            Assert.AreEqual("-x", splits[1]);
            Assert.AreEqual(@"C:\te st\file", splits[2]);
        }

        [Test]
        public void command_line_parser_split_string_escaped_space_does_not_split()
        {
            var line = @"prog -x foo\ bar";
            var splits = CommandLineParser.SplitString(line);

            Console.WriteLine("Found {0} items...", splits.Count);
            foreach (var text in splits)
            {
                Console.WriteLine("Found: " + text);
            }

            Assert.AreEqual("prog", splits[0]);
            Assert.AreEqual("-x", splits[1]);
            Assert.AreEqual("foo bar", splits[2]);
        }

        [Test]
        public void command_line_parser_split_string_typical_1()
        {
            var line = " --alpha -a -b \"1 2\"  random ";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(5, split.Count);
            Assert.AreEqual("--alpha", split[0]);
            Assert.AreEqual("-a", split[1]);
            Assert.AreEqual("-b", split[2]);
            Assert.AreEqual("1 2", split[3]);
            Assert.AreEqual("random", split[4]);
        }

        [Test]
        public void command_line_parser_split_string_typical_2()
        {
            var line = " -a-b -c\"1 2\" -d jam\\\"hat";
            var split = CommandLineParser.SplitString(line);
            Assert.IsNotNull(split);
            Assert.AreEqual(5, split.Count);
            Assert.AreEqual("-a-b", split[0]);
            Assert.AreEqual("-c", split[1]);
            Assert.AreEqual("1 2", split[2]);
            Assert.AreEqual("-d", split[3]);
            Assert.AreEqual("jam\"hat", split[4]);
        }

        private static void TestMatcherA(List<string> splitFields)
        {
            _foundA = false;
            int foundAt = splitFields.FindIndex(ff => ff == "-a");
            if (foundAt >= 0)
            {
                splitFields.RemoveAt(foundAt);
                _foundA = true;
            }
        }

        private static void TestMatcherB(List<string> splitFields)
        {
            _foundB = false;
            _foundP = null;
            int foundAt = splitFields.FindIndex(ff => ff == "-b");
            if (foundAt >= 0 && foundAt < splitFields.Count - 1)
            {
                splitFields.RemoveAt(foundAt);
                _foundB = true;
                _foundP = splitFields[foundAt];
                splitFields.RemoveAt(foundAt);
            }
        }

        private static void ForwardSlashMatcherA(List<string> splitFields)
        {
            _foundA = false;
            int foundAt = splitFields.FindIndex(ff => ff == "/argA");
            if (foundAt >= 0)
            {
                splitFields.RemoveAt(foundAt);
                _foundA = true;
            }
        }


        [Test]
        public void command_line_parser_sets_first_token_as_program_name()
        {
            var line = "Program.exe x";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[0]);
            Assert.IsNotNull(remainder);
            Assert.AreEqual(1, remainder.Count);
            Assert.AreEqual("Program.exe", parser.ProgramName);
        }

        [Test]
        public void command_line_parser_simple_matcher_matches()
        {
            var line = "prog -a";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { TestMatcherA });
            Assert.IsNotNull(remainder);
            CollectionAssert.IsEmpty(remainder);
            Assert.IsTrue(_foundA);
            Assert.AreEqual("prog", parser.ProgramName);
        }

        [Test]
        public void command_line_parser_simple_matcher_leaves_remainder()
        {
            var line = "prog -a -b";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { TestMatcherA });
            Assert.IsNotNull(remainder);
            Assert.AreEqual(1, remainder.Count);
            Assert.AreEqual("-b", remainder[0]);
            Assert.IsTrue(_foundA);
            Assert.AreEqual("prog", parser.ProgramName);
        }

        [Test]
        public void command_line_parser_simple_matchers_both_match()
        {
            var line = "prog -a -b foo";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { TestMatcherA, TestMatcherB });
            Assert.IsNotNull(remainder);
            CollectionAssert.IsEmpty(remainder);
            Assert.IsTrue(_foundA);
            Assert.IsTrue(_foundB);
            Assert.AreEqual("foo", _foundP);
            Assert.AreEqual("prog", parser.ProgramName);
        }

        [Test]
        public void command_line_parser_simple_matcher_no_match()
        {
            var line = "prog   -x";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { TestMatcherA });
            Assert.IsNotNull(remainder);
            Assert.AreEqual(1, remainder.Count);
            Assert.AreEqual("-x", remainder[0]);
            Assert.IsFalse(_foundA);
            Assert.AreEqual("prog", parser.ProgramName);
        }

        [Test]
        public void command_line_parser_simple_matchers_leaves_remainders()
        {
            var line = "prog -x -b foo -a  temp";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { TestMatcherA, TestMatcherB });
            Assert.IsNotNull(remainder);
            Assert.AreEqual(2, remainder.Count);
            Assert.AreEqual("-x", remainder[0]);
            Assert.AreEqual("temp", remainder[1]);
            Assert.AreEqual("prog", parser.ProgramName);
        }

        [Test]
        public void command_line_parser_simple_matchers_one_match()
        {
            var line = "prog -x -b foo temp";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { TestMatcherA, TestMatcherB });
            Assert.IsNotNull(remainder);
            Assert.AreEqual(2, remainder.Count);
            Assert.IsTrue(_foundB);
            Assert.AreEqual("foo", _foundP);
        }

        [Test]
        public void command_line_parser_handles_non_matching_forward_slash()
        {
            var line = "prog /argA /argB foof";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { TestMatcherA, TestMatcherB });

            Assert.IsNotNull(remainder);
            CollectionAssert.AreEqual(new [] {"/argA", "/argB", "foof"}, remainder);
        }

        [Test]
        public void command_line_parser_handles_matching_forward_slash()
        {
            var line = "prog /argA /argB foof";
            var parser = new CommandLineParser(line);
            var remainder = parser.Parse(new CommandLineParser.OptionMatcher[] { ForwardSlashMatcherA });

            Assert.IsNotNull(remainder);
            Assert.IsTrue(_foundA);
            CollectionAssert.AreEqual(new[] { "/argB", "foof" }, remainder);
        }

        private static bool _foundA;
        private static bool _foundB;
        private static string _foundP;
    }
}