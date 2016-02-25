﻿using Textamina.Markdig.Helpers;
using Textamina.Markdig.Parsing;

namespace Textamina.Markdig.Syntax
{
    /// <summary>
    /// Repressents a thematic break.
    /// </summary>
    public class HeadingBlock : LeafBlock
    {
        public new static readonly BlockParser Parser = new ParserInternal();

        public HeadingBlock(BlockParser parser) : base(parser)
        {
        }

        public int Level { get; set; }

        private class ParserInternal : BlockParser
        {
            public override MatchLineResult Match(BlockParserState state)
            {
                if (state.IsCodeIndent)
                {
                    return MatchLineResult.None;
                }

                // 4.2 ATX headings
                // An ATX heading consists of a string of characters, parsed as inline content, 
                // between an opening sequence of 1–6 unescaped # characters and an optional 
                // closing sequence of any number of unescaped # characters. The opening sequence 
                // of # characters must be followed by a space or by the end of line. The optional
                // closing sequence of #s must be preceded by a space and may be followed by spaces
                // only. The opening # character may be indented 0-3 spaces. The raw contents of 
                // the heading are stripped of leading and trailing spaces before being parsed as 
                // inline content. The heading level is equal to the number of # characters in the 
                // opening sequence.
                var column = state.Column;
                var c = state.CurrentChar;

                int leadingCount = 0;
                for (; !state.Line.IsEndOfSlice && leadingCount <= 6; leadingCount++)
                {
                    if (c != '#')
                    {
                        break;
                    }

                    c = state.PeekChar(leadingCount);
                }

                // closing # will be handled later, because anyway we have matched 

                // A space is required after leading #
                if (leadingCount > 0 && leadingCount <=6 && (c.IsSpace() || state.Line.IsEndOfSlice))
                {
                    state.Line.Start = leadingCount + 1;
                    state.NewBlocks.Push(new HeadingBlock(this) {Level = leadingCount, Column = column });

                    // The optional closing sequence of #s must be preceded by a space and may be followed by spaces only.
                    int endState = 0;
                    int countClosingTags = 0;
                    for (int i = state.Line.End; i >= state.Line.Start - 1; i--)  // Go up to Start - 1 in order to match the space after the first ###
                    {
                        c = state.Line[i];
                        if (endState == 0)
                        {
                            if (c.IsSpace()) // TODO: Not clear if it is a space or space+tab in the specs
                            {
                                continue;
                            }
                            endState = 1;
                        }
                        if (endState == 1)
                        {
                            if (c == '#')
                            {
                                countClosingTags++;
                                continue;
                            }

                            if (countClosingTags > 0)
                            {
                                if (c.IsSpace())
                                {
                                    state.Line.End = i - 1;
                                }
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    return MatchLineResult.Last;
                }

                return MatchLineResult.None;
            }

            public override void Close(BlockParserState state)
            {
                var heading = (HeadingBlock) state.Pending;
                heading.Lines.Trim();
            }
        }
    }
}