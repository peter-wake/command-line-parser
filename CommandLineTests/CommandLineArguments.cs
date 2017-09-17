using System;
using System.Collections.Generic;
using CommandLine;

namespace CommandLineTests
{
    internal class CommandLineArguments : CommandLineArgumentsBase
    {
        public int PortA { get; private set; }
        public int PortB { get; private set; }

        public bool Verbose { get; private set; }
        public bool UltraVerbose { get; private set; }

        public EnumeratedKind Kind { get; private set; }

        public double TimeoutInSeconds { get; private set; }

        public IList<string> ArgumentNames { get; private set; }

        public PortAddress LocalAddressA { get; private set; }
        public PortAddress LocalAddressB { get; private set; }

        public PortAddress PortAddressA { get; private set; }
        public PortAddress PortAddressB { get; private set; }


        public CommandLineArguments(string addressA,
            string addressB,
            int portA,
            int portB,
            string localAddressA,
            string localAddressB,
            EnumeratedKind kind,
            double seconds)
        {
            _addressA = addressA;
            _addressB = addressB;
            PortA = portA;
            PortB = portB;
            _localAddressA = localAddressA;
            _localAddressB = localAddressB;

            TimeoutInSeconds = seconds;

            Matchers = new CommandLineParser.OptionMatcher[]
            {
                VerboseMatcher,
                HostMatcher,
                PortMatcher,
                LocalAddressMatcher,
                TimeoutMatcher,
                KindMatcher,
                BadArgumentMatcher,
                Finish
            };
        }


        public override string GetHelp()
        {
            return "Sample help string";
        }

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

        public void HostMatcher(List<string> arguments)
        {
            string parameter;
            while (FindParameter("-host", arguments, out parameter))
            {
                _addressA = parameter;
                _addressB = parameter;
            }

            while (FindParameter("-hostA", arguments, out parameter))
            {
                _addressA = parameter;
            }

            while (FindParameter("-hostB", arguments, out parameter))
            {
                _addressB = parameter;
            }

        }

        public void PortMatcher(List<string> arguments)
        {
            string parameter;
            while (FindParameter("-port", arguments, out parameter))
            {
                int port;
                bool failed = !int.TryParse(parameter, out port);

                if (failed || port < 1 || port > 65535)
                {
                    Errors.Add($"Could not parse '-port {parameter}' - not a valid port number");
                }
                else
                {
                    PortA = port;
                    PortB = port + 1;
                }
            }

            while (FindParameter("-portA", arguments, out parameter))
            {
                int port;
                bool failed = !int.TryParse(parameter, out port);

                if (failed || port < 1 || port > 65535)
                {
                    Errors.Add($"Could not parse '-portA {parameter}' - not a valid port number");
                }
                else
                {
                    PortA = port;
                }
            }

            while (FindParameter("-portB", arguments, out parameter))
            {
                int port;
                bool failed = !int.TryParse(parameter, out port);

                if (failed || port < 1 || port > 65535)
                {
                    Errors.Add($"Could not parse '-portB {parameter}' - not a valid port number");
                }
                else
                {
                    PortB = port;
                }
            }
        }

        private void LocalAddressMatcher(List<string> arguments)
        {
            string parameter;
            while (FindParameter("-local", arguments, out parameter))
            {
                _localAddressA = parameter;
                _localAddressB = parameter;
            }
            while (FindParameter("-localA", arguments, out parameter))
            {
                _localAddressA = parameter;
            }
            while (FindParameter("-localB", arguments, out parameter))
            {
                _localAddressB = parameter;
            }
        }



        public void TimeoutMatcher(List<string> arguments)
        {
            string parameter;
            while (FindParameter("-timeout", arguments, out parameter))
            {
                double timeout;
                bool failed = !double.TryParse(parameter, out timeout);

                if (failed || timeout < 0 || timeout > 60 * 60)
                {
                    Errors.Add($"Could not parse '-timeout {parameter}' - not a value in seconds, between 0 and 1 hour (inclusive)");
                }
                else
                {
                    TimeoutInSeconds = timeout;
                }
            }
        }

        public void KindMatcher(List<string> arguments)
        {
            string parameter;
            while (FindParameter("-kind", arguments, out parameter))
            {
                var text = parameter.ToLower();
                if ("one" == text)
                {
                    Kind = EnumeratedKind.One;
                }
                else if ("two" == text)
                {
                    Kind = EnumeratedKind.Two;
                }
                else if ("three" == text)
                {
                    Kind = EnumeratedKind.Three;
                }
                else
                {
                    Errors.Add($"Could not parse '-kind {parameter}' - not a valid test kind (one, two, three)");
                }
            }
        }


        public override void Finish(List<string> arguments)
        {
            ArgumentNames = arguments;

            try
            {
                if (!string.IsNullOrWhiteSpace(_addressA) && PortA > 0)
                {
                    PortAddressA = new PortAddress(_addressA, PortA);
                }

                if (!string.IsNullOrWhiteSpace(_addressB) && PortB > 0)
                {
                    PortAddressB = new PortAddress(_addressB, PortB);
                }
            }
            catch (Exception e)
            {
                Errors.Add($"Invalid host address: {e.Message}");
            }

            try
            {
                LocalAddressA = new PortAddress(_localAddressA, 0);
                LocalAddressB = new PortAddress(_localAddressB, 0);
            }
            catch (Exception e)
            {
                Errors.Add($"Invalid local address: '{_localAddressA}'/'{_localAddressB}' - {e.Message}");
            }
        }

        private string _addressA;
        private string _addressB;

        private string _localAddressA;
        private string _localAddressB;

    }
}
