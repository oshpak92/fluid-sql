// <license>
//     The MIT License (MIT)
// </license>
// <copyright company="TTRider Technologies, Inc.">
//     Copyright (c) 2014-2017 All Rights Reserved
// </copyright>

using System.Collections.Generic;

namespace TTRider.FluidSql
{
    public class WhenNotMatchedTokenThenInsertToken : WhenMatchedToken
    {
        public WhenNotMatchedTokenThenInsertToken()
        {
            this.Columns = new List<Name>();
            this.Values = new List<Token>();
        }

        public List<Name> Columns { get; private set; }

        public List<Token> Values { get; private set; }
    }
}