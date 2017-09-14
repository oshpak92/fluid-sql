﻿// <license>
//     The MIT License (MIT)
// </license>
// <copyright company="TTRider Technologies, Inc.">
//     Copyright (c) 2014-2017 All Rights Reserved
// </copyright>

using System;

namespace TTRider.FluidSql
{
    public class Top : Token
    {

        public Top(long value, bool percent, bool withTies)
        {
            if (value < 1) throw new ArgumentException("value");
            this.IntValue = value;
            this.Value = Sql.Scalar(value);
            this.Percent = percent;
            this.WithTies = withTies;
        }

        public Top(Parameter value, bool percent, bool withTies)
        {
            this.IntValue = -1;
            this.Parameters.Add(value);
            this.Value = value;
            this.Percent = percent;
            this.WithTies = withTies;
        }

        public Token Value { get;  }
        public long IntValue { get; set; }
        public bool Percent { get; set; }
        public bool WithTies { get; set; }
    }
}