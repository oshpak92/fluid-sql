﻿// <license>
// The MIT License (MIT)
// </license>
// <copyright company="TTRider Technologies, Inc.">
// Copyright (c) 2014-2015 All Rights Reserved
// </copyright>



// ReSharper disable InconsistentNaming


namespace TTRider.FluidSql.Providers.Postgres.Core
{
    public abstract partial class VisitorCore : Visitor
    {
        private static readonly string[] supportedDialects = new[] { "npgsql", "postgresql", "postgres", "ansi" };


        private readonly string[] DbTypeStrings =
        {
            "BIGINT",       //NpgsqlDbType.Bigint,
            "TEXT",        //NpgsqlDbType.Bytea,
            "BOOLEAN",      //NpgsqlDbType.Boolean,
            "CHAR",         //NpgsqlDbType.Char,
            "TIMESTAMP",    //NpgsqlDbType.Timestamp,
            "NUMERIC",      //NpgsqlDbType.Numeric,
            "REAL",         //NpgsqlDbType.Real,
            "TEXT",        //NpgsqlDbType.Bytea,
            "INTEGER",      //NpgsqlDbType.Integer,
            "MONEY",        //NpgsqlDbType.Money,
            "CHAR",         //NpgsqlDbType.Char,
            "TEXT",         //NpgsqlDbType.Text,
            "TEXT",      //NpgsqlDbType.Varchar,
            "DOUBLE PRECISION",//NpgsqlDbType.Double,
            "CHAR(32)",         //NpgsqlDbType.Uuid,
            "TIMESTAMP",    //NpgsqlDbType.Timestamp,
            "SMALLINT",     //NpgsqlDbType.Smallint,
            "REAL",         //NpgsqlDbType.Real,
            "TEXT",         //NpgsqlDbType.Text,
            "TIMESTAMP",    //NpgsqlDbType.Timestamp,
            "SMALLINT",     //NpgsqlDbType.Smallint,
            "TEXT",        //NpgsqlDbType.Bytea,
            "VARCHAR",      //NpgsqlDbType.Varchar,
            "Unknown",      //NpgsqlDbType.Unknown,
            "TEXT",          //NpgsqlDbType.Xml,
            "DATE",         //NpgsqlDbType.Date,
            "TIME",         //NpgsqlDbType.Time,
            "TIMESTAMPTZ",  //NpgsqlDbType.TimestampTZ
            "DATETIMEOFFSET",
            "TEXT",       //NpgsqlDbType.Serial
        };



        protected  override string[] SupportedDialects { get { return supportedDialects; } }

        public VisitorCore()
        {
            this.IdentifierOpenQuote = "\"";
            this.IdentifierCloseQuote = "\"";
            this.LiteralOpenQuote = "'";
            this.LiteralCloseQuote = "'";
            this.CommentOpenQuote = "/*";
            this.CommentCloseQuote = "*/";
        }

        protected override void VisitJoinType(Joins join)
        {
            this.State.Write(this.JoinStrings[(int)join]);
        }

        protected override void VisitType(ITyped typedToken)
        {
            if (typedToken.DbType.HasValue)
            {
                this.State.Write(this.DbTypeStrings[(int)typedToken.DbType]);
            }
        }

        protected override void VisitValue(object value)
        {
            if (value is bool)
            {
                this.State.Write((bool)value ? "true" : "false");
            }
            else
            {
                base.VisitValue(value);
            }
        }

        protected class PostgrSQLSymbols : Symbols
        {
            public const string DATEPART = "DATE_PART";
            public const string TIMESTAMP = "TIMESTAMP";

            public const string BEGIN_LABEL = "<<";
            public const string END_LABEL = ">>";

            public const string DELAY_FORMAT = "pg_sleep({0})";

            //public const string DATEADD = "DATEADD";
            //public const string DATEDIFF = "DATEDIFF";

            //public const string DATETIMEFROMPARTS = "DATETIMEFROMPARTS";
            //public const string GETDATE = "GETDATE";
            //public const string GETUTCDATE = "GETUTCDATE";
            //public const string IDENTITY_INSERT = "IDENTITY_INSERT";
            //public const string NEWID = "NEWID";
            //public const string TIMEFROMPARTS = "TIMEFROMPARTS";
            public const string other = "other";
            public const string AssignValSign = ":=";

            public const string d = "day";
            public const string hh = "hour";
            public const string m = "month";
            public const string mi = "minute";
            public const string ms = "microsecond";
            //public const string s = "s";
            public const string ss = "second";
            public const string ww = "week";
            public const string yy = "year";

            public const string monthsInYear = "12";
            public const string daysInWeek = "7";
            public const string hoursInDay = "24";
            public const string minutesInHour = "60";
            public const string secondInMinute = "60";
            public const string milisecondInSecond = "1000";
        }
    }
}
