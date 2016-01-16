// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.DotNet.CodeFormatting.Tests
{
    public sealed class UseKeywordOverTypesTests : LocalSemanticRuleTestBase
    {
        internal override ILocalSemanticFormattingRule Rule
        {
            get { return new Rules.UseKeywordsOverTypes(); }
        }

        [Fact]
        public void FieldsWithTypesInsteadOfKeywords()
        {
            var text = @"
class C1
{
    Boolean _field1;
    Byte    _field2;
    Char    _field3;
    Double  _field4;
    Decimal _field5;
    Float   _field6;
    Int16   _field7;
    Int32   _field8;
    Int64   _field9;
    Object  _field10;
    SByte   _field11;
    String  _field12;
    UInt16  _field13;
    UInt32  _field14;
    UInt64  _field15;
    Void    _field16;   
}
";

            var expected = @"
class C1
{
    bool    _field1;
    byte    _field2;
    char    _field3;
    double  _field4;
    decimal _field5;
    float   _field6;
    short   _field7;
    int     _field8;
    long    _field9;
    object  _field10;
    sbyte   _field11;
    string  _field12;
    ushort  _field13;
    uint    _field14;
    ulong   _field15;
    void    _field16;
}
";
            Verify(text, expected);
        }
    }
}
