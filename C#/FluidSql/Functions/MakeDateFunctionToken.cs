// <license>
// The MIT License (MIT)
// </license>
// <copyright company="TTRider, L.L.C.">
// Copyright (c) 2014-2015 All Rights Reserved
// </copyright>

namespace TTRider.FluidSql
{
    public class MakeDateFunctionToken : FunctionExpressionToken
    {
        public ExpressionToken Year { get; set; }
        public ExpressionToken Month { get; set; }
        public ExpressionToken Day { get; set; }
        public ExpressionToken Hour { get; set; }
        public ExpressionToken Minute { get; set; }
        public ExpressionToken Second { get; set; }
    }
}