﻿// <license>
//     The MIT License (MIT)
// </license>
// <copyright company="TTRider Technologies, Inc.">
//     Copyright (c) 2014-2017 All Rights Reserved
// </copyright>

namespace TTRider.FluidSql
{
    public class ReturnStatement : Token, IStatement
    {
        public Token ReturnExpression { get; set; }
    }
}