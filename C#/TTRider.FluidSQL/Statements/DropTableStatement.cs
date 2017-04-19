﻿// <license>
//     The MIT License (MIT)
// </license>
// <copyright company="TTRider Technologies, Inc.">
//     Copyright (c) 2014-2017 All Rights Reserved
// </copyright>

namespace TTRider.FluidSql
{
    public class DropTableStatement : IStatement
    {
        public Name Name { get; set; }
        public bool CheckExists { get; set; }
        public bool IsTemporary { get; set; }
        public bool? IsCascade { get; set; }
    }
}