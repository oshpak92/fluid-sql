// <license>
//     The MIT License (MIT)
// </license>
// <copyright company="TTRider Technologies, Inc.">
//     Copyright (c) 2014-2017 All Rights Reserved
// </copyright>

using System.Collections.Generic;

namespace TTRider.FluidSql
{
    public class CoalesceFunctionToken : FunctionExpressionToken
    {
        public CoalesceFunctionToken()
        {
            this.Arguments = new List<Token>();
        }

        public List<Token> Arguments { get; private set; }
    }
}