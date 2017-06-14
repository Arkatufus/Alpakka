﻿using System;
using System.Collections.Generic;
using Akka.IO;
using Akka.Streams.Stage;

namespace Akka.Streams.Csv
{
    /// <summary>
    /// Internal API: Use <see cref="Akka.Streams.Csv.Dsl.CsvParsing"/> instead.
    /// </summary>
    internal sealed class CsvParsingStage: GraphStage<FlowShape<ByteString, List<ByteString>>>
    {
        #region Logic
        private sealed class Logic:InAndOutGraphStageLogic
        {
            private readonly CsvParsingStage _stage;
            private readonly CsvParser _buffer;

            public Logic(CsvParsingStage stage, byte delimiter, byte quoteChar, byte escapeChar):base(stage.Shape)
            {
                _stage = stage;

                _buffer = new CsvParser(delimiter, quoteChar, escapeChar);
                SetHandler(stage.In, this);
                SetHandler(stage.Out, this);
            }

            public override void OnPush()
            {
                _buffer.Offer(Grab(_stage.In));
                TryPollBuffer();
            }

            public override void OnPull()
            {
                TryPollBuffer();
            }

            public override void OnUpstreamFinish()
            {
                EmitRemaining();
                CompleteStage();
            }

            private void TryPollBuffer()
            {
                try
                {
                    var csvLine = _buffer.Poll(requireLineEnd: true);
                    if (csvLine != null)
                    {
                        Push(_stage.Out, csvLine);
                        return;
                    }

                    if (IsClosed(_stage.In))
                    {
                        EmitRemaining();
                        CompleteStage();
                    }
                    else
                        Pull(_stage.In);
                }
                catch (Exception ex)
                {
                    FailStage(ex);
                }
            }

            private void EmitRemaining()
            {
                var csvLine = _buffer.Poll(requireLineEnd: false);
                while (csvLine != null)
                {
                    Emit(_stage.Out, csvLine);
                    csvLine = _buffer.Poll(requireLineEnd: false);
                }
            }
        }
        #endregion

        private readonly byte _delimiter;
        private readonly byte _quoteChar;
        private readonly byte _escapeChar;

        public CsvParsingStage(byte delimiter, byte quoteChar, byte escapeChar)
        {
            _delimiter = delimiter;
            _quoteChar = quoteChar;
            _escapeChar = escapeChar;

            Shape = new FlowShape<ByteString, List<ByteString>>(In, Out);
        }

        protected override Attributes InitialAttributes { get; } = Attributes.CreateName("CsvParsing");

        public Inlet<ByteString> In { get; } = new Inlet<ByteString>("CsvParsing.in");
        public Outlet<List<ByteString>> Out { get; } = new Outlet<List<ByteString>>("CsvParsing.out");

        public override FlowShape<ByteString, List<ByteString>> Shape { get; }

        protected override GraphStageLogic CreateLogic(Attributes inheritedAttributes)
            => new Logic(this, _delimiter, _quoteChar, _escapeChar);

        public override string ToString() => nameof(CsvParsingStage);
    }
}
