﻿// <copyright company="TTRider, L.L.C.">
// Copyright (c) 2014 All Rights Reserved
// </copyright>

using System.Collections.Generic;

namespace TTRider.FluidSql
{
    public interface ISetStatement
    {
        IList<BinaryEqualToken> Set { get; }
    }
}