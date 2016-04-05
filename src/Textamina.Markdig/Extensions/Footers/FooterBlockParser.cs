﻿// Copyright (c) Alexandre Mutel. All rights reserved.
// This file is licensed under the BSD-Clause 2 license. 
// See the license.txt file in the project root for more information.

using Textamina.Markdig.Helpers;
using Textamina.Markdig.Parsers;
using Textamina.Markdig.Syntax;

namespace Textamina.Markdig.Extensions.Footers
{
    /// <summary>
    /// A block parser for a <see cref="FooterBlock"/>.
    /// </summary>
    /// <seealso cref="Textamina.Markdig.Parsers.BlockParser" />
    public class FooterBlockParser : BlockParser
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FooterBlockParser"/> class.
        /// </summary>
        public FooterBlockParser()
        {
            OpeningCharacters = new[] {'^'};
        }

        public override BlockState TryOpen(BlockProcessor processor)
        {
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }

            var column = processor.Column;

            // A footer
            // A Footer marker consists of 0-3 spaces of initial indent, plus (a) the characters ^^ together with a following space, or (b) a double character ^^ not followed by a space.
            var openingChar = processor.CurrentChar;
            if (processor.PeekChar(1) != openingChar)
            {
                return BlockState.None;
            }
            processor.NextChar(); // Grab 2nd^
            var c = processor.NextChar(); // grab space
            if (c.IsSpaceOrTab())
            {
                processor.NextColumn();
            }
            processor.NewBlocks.Push(new FooterBlock(this) { OpeningCharacter = openingChar, Column = column});
            return BlockState.Continue;
        }

        public override BlockState TryContinue(BlockProcessor processor, Block block)
        {
            if (processor.IsCodeIndent)
            {
                return BlockState.None;
            }

            var quote = (FooterBlock) block;

            // A footer
            // A Footer marker consists of 0-3 spaces of initial indent, plus (a) the characters ^^ together with a following space, or (b) a double character ^^ not followed by a space.
            var c = processor.CurrentChar;
            if (c != quote.OpeningCharacter || processor.PeekChar(1) != c)
            {
                return processor.IsBlankLine ? BlockState.BreakDiscard : BlockState.None;
            }

            processor.NextChar(); // Skip ^^ char (1st)
            c = processor.NextChar(); // Skip ^^ char (2nd)
            if (c.IsSpace())
            {
                processor.NextChar(); // Skip following space
            }

            return BlockState.Continue;
        }
    }
}