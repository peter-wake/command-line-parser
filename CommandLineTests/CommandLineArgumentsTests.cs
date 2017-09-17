using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using NUnit.Framework;

namespace CommandLineTests
{
    [TestFixture]
    public class CommandLineArgumentsTests
    {
        private const string AddressA = "10.1.1.10";
        private const string AddressB = "10.1.1.11";
        private const int PortA = 25000;
        private const int PortB = 25001;
        private const string LocalAddressA = "127.0.0.1";
        private const string LocalAddressB = "127.0.0.1";

        private const uint Seconds = 30;

        private static readonly List<string> EmptyList = new List<string>();

        private CommandLineArguments GetArguments()
        {
            return new CommandLineArguments(AddressA, AddressB, PortA, PortB, LocalAddressA, LocalAddressB, EnumeratedKind.One, Seconds);
        }

        [Test]
        public void command_line_arguments_can_be_created()
        {
            var matchers = GetArguments();
            Assert.IsNotNull(matchers);
        }

        [Test]
        public void command_line_arguments_port_address_valid_after_finish()
        {
            var matchers = GetArguments();
            matchers.Finish(EmptyList);
            Assert.IsNotNull(matchers.PortAddressA);

            Assert.AreEqual(PortA, matchers.PortAddressA.Port);
            Assert.AreEqual(AddressA, matchers.PortAddressA.Address.ToString());
            Assert.AreEqual(PortB, matchers.PortAddressB.Port);
            Assert.AreEqual(AddressB, matchers.PortAddressB.Address.ToString());
        }

        [Test]
        public void command_line_arguments_no_errors_after_finish_on_empty()
        {
            var matchers = GetArguments();
            matchers.Finish(EmptyList);
            Assert.IsNotNull(matchers.Errors);
            Assert.IsFalse(matchers.Errors.Any());
        }

        [Test]
        public void command_line_arguments_remainder_has_non_flag_values()
        {
            var matchers = GetArguments();
            var list = new List<string> { "foo", "bar" };
            matchers.BadArgumentMatcher(list);
            Assert.IsNotNull(matchers.Errors);
            CollectionAssert.IsEmpty(matchers.Errors);
            Assert.AreEqual(2, list.Count);
        }

        [Test]
        public void command_line_arguments_errors_from_bad_argument_matcher_on_unknown()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-z", "-x" };
            matchers.BadArgumentMatcher(list);
            Assert.IsNotNull(matchers.Errors);
            Assert.AreEqual(2, matchers.Errors.Count);
            Assert.IsTrue(matchers.Errors[0].Contains("-z"));
            Assert.IsTrue(matchers.Errors[1].Contains("-x"));
        }

        [Test]
        public void command_line_arguments_errors_from_valid_arguments_up_to_bad()
        {
            var matchers = GetArguments();
            var list = new List<string> { "a", "b", "-z", "y" };
            matchers.BadArgumentMatcher(list);
            Assert.IsNotNull(matchers.Errors);
            Assert.AreEqual(3, matchers.Errors.Count);
            Assert.IsTrue(matchers.Errors[0].Contains("a"));
            Assert.IsTrue(matchers.Errors[1].Contains("b"));
            Assert.IsTrue(matchers.Errors[2].Contains("-z"));

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual("y", list[0]);
        }

        [Test]
        public void command_line_arguments_matches_host()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-host", "10.11.1.2" };
            matchers.HostMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            Assert.AreEqual("10.11.1.2", matchers.PortAddressA.Address.ToString());
            Assert.AreEqual("10.11.1.2", matchers.PortAddressB.Address.ToString());
            CollectionAssert.IsEmpty(list);
            Assert.IsFalse(matchers.Errors.Any());
        }

        [Test]
        public void command_line_arguments_errors_for_bad_host()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-host", "X.Y.Z.2" };
            matchers.HostMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            CollectionAssert.IsEmpty(list);
            Assert.AreEqual(1, matchers.Errors.Count);
            Assert.IsTrue(matchers.Errors[0].Contains("host address"));
        }

        [Test]
        public void command_line_arguments_matches_port()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-port", "25" };
            matchers.PortMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            Assert.AreEqual(25, matchers.PortA);
            Assert.AreEqual(26, matchers.PortB);
            CollectionAssert.IsEmpty(list);
            Assert.IsFalse(matchers.Errors.Any());
        }

        [Test]
        public void command_line_arguments_error_for_bad_port()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-port", "99999" };
            matchers.PortMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            CollectionAssert.IsEmpty(list);
            Assert.AreEqual(1, matchers.Errors.Count);
            Assert.IsTrue(matchers.Errors[0].Contains("port"));
        }

        [Test]
        public void command_line_arguments_matches_timeout()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-timeout", "60" };
            matchers.TimeoutMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            Assert.AreEqual(60.0, matchers.TimeoutInSeconds);
            CollectionAssert.IsEmpty(list);
            Assert.IsFalse(matchers.Errors.Any());
        }

        [Test]
        public void command_line_arguments_error_for_bad_timeout()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-timeout", "A" };
            matchers.TimeoutMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            CollectionAssert.IsEmpty(list);
            Assert.AreEqual(1, matchers.Errors.Count);
            Assert.IsTrue(matchers.Errors[0].Contains("timeout"));
        }

        [Test]
        public void command_line_arguments_error_for_negative_timeout()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-timeout", "-0.1" };
            matchers.TimeoutMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            CollectionAssert.IsEmpty(list);
            Assert.AreEqual(1, matchers.Errors.Count);
            Assert.IsTrue(matchers.Errors[0].Contains("timeout"));
        }

        [Test]
        public void command_line_arguments_matches_kind_one()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-kind", "One" };
            matchers.KindMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            Assert.AreEqual(EnumeratedKind.One, matchers.Kind);
            CollectionAssert.IsEmpty(list);
            Assert.IsFalse(matchers.Errors.Any());
        }

        [Test]
        public void command_line_arguments_matches_kind_two()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-kind", "two" };
            matchers.KindMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            Assert.AreEqual(EnumeratedKind.Two, matchers.Kind);
            CollectionAssert.IsEmpty(list);
            Assert.IsFalse(matchers.Errors.Any());
        }

        [Test]
        public void command_line_arguments_matches_kind_three()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-kind", "THREE" };
            matchers.KindMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            Assert.AreEqual(EnumeratedKind.Three, matchers.Kind);
            CollectionAssert.IsEmpty(list);
            Assert.IsFalse(matchers.Errors.Any());
        }

        [Test]
        public void command_line_arguments_error_for_bad_kind()
        {
            var matchers = GetArguments();
            var list = new List<string> { "-kind", "foo" };
            matchers.KindMatcher(list);
            matchers.BadArgumentMatcher(list);
            matchers.Finish(list);
            CollectionAssert.IsEmpty(list);
            Assert.AreEqual(1, matchers.Errors.Count);
            Assert.IsTrue(matchers.Errors[0].Contains("kind"));
        }

        [Test]
        [Category("Integration")]
        public void command_line_arguments_in_parser_parse_address()
        {
            var matchers = GetArguments();
            var parser = new CommandLineParser("program -host 10.1.1.2 argName");
            var argumentNames = parser.Parse(matchers.Matchers);

            Assert.IsNotNull(argumentNames);
            Assert.AreEqual(1, argumentNames.Count);
            Assert.AreEqual("argName", argumentNames[0]);

            CollectionAssert.IsEmpty(matchers.Errors);
            Assert.AreEqual("program", parser.ProgramName);

            Assert.AreEqual("10.1.1.2", matchers.PortAddressA.Address.ToString());
            Assert.AreEqual("10.1.1.2", matchers.PortAddressB.Address.ToString());
        }

        [Test]
        [Category("Integration")]
        public void command_line_arguments_in_parser_parse_port()
        {
            var matchers = GetArguments();
            var parser = new CommandLineParser("program -port 44000 testName");
            var testNames = parser.Parse(matchers.Matchers);

            Assert.IsNotNull(testNames);
            Assert.AreEqual(1, testNames.Count);
            Assert.AreEqual("testName", testNames[0]);

            CollectionAssert.IsEmpty(matchers.Errors);
            Assert.AreEqual("program", parser.ProgramName);

            Assert.AreEqual(44000, matchers.PortAddressA.Port);
            Assert.AreEqual(44001, matchers.PortAddressB.Port);
        }

        [Test]
        [Category("Integration")]
        public void command_line_arguments_in_parser_parse_timeout()
        {
            var matchers = GetArguments();
            var parser = new CommandLineParser("program -timeout 0.5 testName");
            var testNames = parser.Parse(matchers.Matchers);

            Assert.IsNotNull(testNames);
            Assert.AreEqual(1, testNames.Count);
            Assert.AreEqual("testName", testNames[0]);

            CollectionAssert.IsEmpty(matchers.Errors);
            Assert.AreEqual("program", parser.ProgramName);

            Assert.AreEqual(0.5, matchers.TimeoutInSeconds);
        }

        [Test]
        [Category("Integration")]
        public void command_line_arguments_in_parser_parse_kind()
        {
            var matchers = GetArguments();
            var parser = new CommandLineParser("program -kind ONE testName");
            var testNames = parser.Parse(matchers.Matchers);

            Assert.IsNotNull(testNames);
            Assert.AreEqual(1, testNames.Count);
            Assert.AreEqual("testName", testNames[0]);

            CollectionAssert.IsEmpty(matchers.Errors);
            Assert.AreEqual("program", parser.ProgramName);

            Assert.AreEqual(EnumeratedKind.One, matchers.Kind);
        }

        [Test]
        [Category("Integration")]
        public void command_line_arguments_in_parser_parse_all()
        {
            var matchers = GetArguments();
            var parser =
                new CommandLineParser(
                    "program -host 11.1.1.2 -port 8080 -timeout 1.5 -kind Two testName");
            var argumentNames = parser.Parse(matchers.Matchers);

            Assert.IsNotNull(argumentNames);
            Assert.AreEqual(1, argumentNames.Count);
            Assert.AreEqual("testName", argumentNames[0]);

            CollectionAssert.IsEmpty(matchers.Errors);
            Assert.AreEqual("program", parser.ProgramName);

            Assert.AreEqual("11.1.1.2", matchers.PortAddressA.Address.ToString());
            Assert.AreEqual(8080, matchers.PortAddressA.Port);
            Assert.AreEqual("11.1.1.2", matchers.PortAddressB.Address.ToString());
            Assert.AreEqual(8081, matchers.PortAddressB.Port);
            Assert.AreEqual(1.5, matchers.TimeoutInSeconds);
            Assert.AreEqual(EnumeratedKind.Two, matchers.Kind);
        }

        [Test]
        [Category("Integration")]
        public void command_line_arguments_in_parser_parse_duplicates()
        {
            var matchers = GetArguments();
            var parser =
                new CommandLineParser("program -port 8080 -timeout 1.5 -kind two  -port 8081 -timeout 1.8 -kind three");
            var argumentNames = parser.Parse(matchers.Matchers);

            Assert.IsNotNull(argumentNames);
            CollectionAssert.IsEmpty(argumentNames);

            Assert.IsFalse(matchers.Errors.Any());

            Assert.AreEqual("program", parser.ProgramName);

            Assert.AreEqual(8081, matchers.PortAddressA.Port);
            Assert.AreEqual(8082, matchers.PortAddressB.Port);
            Assert.AreEqual(1.8, matchers.TimeoutInSeconds);
        }

        [Test]
        public void command_line_arguments_find_parameters_zero_arguments_expected()
        {
            var arguments = new List<string> { "foo", "-Q", "-b" };
            var found = TestArguments.TestFindParameters("-Q", 0, arguments);
            Assert.IsNotNull(found);
            Console.WriteLine("Found {0} items", found.Count);

            Console.WriteLine("Remain: {0}", string.Join(", ", arguments));
            Assert.AreEqual(2, arguments.Count);
            Assert.AreEqual("foo", arguments[0]);
            Assert.AreEqual("-b", arguments[1]);
        }

        [Test]
        public void command_line_arguments_find_parameters_argument_present()
        {
            var arguments = new List<string> { "foo", "-Q", "a-b" };
            var found = TestArguments.TestFindParameters("-Q", 1, arguments);
            Assert.IsNotNull(found);
            Assert.AreEqual(1, found.Count);
            Console.WriteLine("Found: '{0}'", found[0]);

            Console.WriteLine("Remain: {0}", string.Join(", ", arguments));
            Assert.AreEqual(1, arguments.Count);
            Assert.AreEqual("foo", arguments[0]);
        }

        [Test]
        public void command_line_arguments_find_parameters_arguments_present()
        {
            var arguments = new List<string> { "foo", "-Q", "a-b", "c-d" };
            var found = TestArguments.TestFindParameters("-Q", 2, arguments);
            Assert.IsNotNull(found);
            Assert.AreEqual(2, found.Count);
            Console.WriteLine("Found: {0}", string.Join(", ", found));

            Console.WriteLine("Remain: {0}", string.Join(", ", arguments));
            Assert.AreEqual(1, arguments.Count);
            Assert.AreEqual("foo", arguments[0]);
        }

        [Test]
        public void command_line_arguments_find_parameters_no_argument_present()
        {
            var arguments = new List<string> { "foo", "-Q" };
            var found = TestArguments.TestFindParameters("-Q", 1, arguments);
            //Assert.IsNull(found);

            Console.WriteLine("Remain: {0}", string.Join(", ", arguments));
            Assert.AreEqual(1, arguments.Count);
            Assert.AreEqual("foo", arguments[0]);
        }

        [Test]
        public void command_line_arguments_find_parameters_no_flag_present()
        {
            var arguments = new List<string> { "foo", "-Q", "c" };
            var found = TestArguments.TestFindParameters("-q", 1, arguments);
            Assert.IsNull(found);

            Assert.AreEqual(3, arguments.Count);
        }

        [Test]
        public void command_line_arguments_find_parameters_insufficient_arguments_present()
        {
            var arguments = new List<string> { "foo", "-Q", "a-b" };
            var found = TestArguments.TestFindParameters("-Q", 2, arguments);
            Assert.IsNull(found);

            Assert.AreEqual(1, arguments.Count);
            Assert.AreEqual("foo", arguments[0]);
        }

        [Test]
        public void command_line_arguments_find_parameters_insufficient_arguments_present_due_to_flag()
        {
            var arguments = new List<string> { "foo", "-Q", "a-b", "-c" };
            var found = TestArguments.TestFindParameters("-Q", 2, arguments);
            Assert.IsNull(found);

            Assert.AreEqual(2, arguments.Count);
            Assert.AreEqual("foo", arguments[0]);
            Assert.AreEqual("-c", arguments[1]);
        }

        private class TestArguments : CommandLineArgumentsBase
        {
            public override string GetHelp() { return ""; }

            public static List<string> TestFindParameters(string flag, int requiredArguments, List<string> arguments)
            {
                return FindParameters(flag, requiredArguments, arguments);
            }
        }
    }
}