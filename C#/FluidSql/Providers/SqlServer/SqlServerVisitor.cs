﻿// <license>
// The MIT License (MIT)
// </license>
// <copyright company="TTRider, L.L.C.">
// Copyright (c) 2014-2015 All Rights Reserved
// </copyright>

using System;
using System.Collections.Generic;
using System.Globalization;

namespace TTRider.FluidSql.Providers.SqlServer
{
    internal class SqlServerVisitor : Visitor
    {
        private readonly string[] DbTypeStrings =
        {
            "BIGINT", // BigInt = 0,
            "BINARY", // Binary = 1,
            "BIT", // Bit = 2,
            "CHAR", // Char = 3,
            "DATETIME", // DateTime = 4,
            "DECIMAL", // Decimal = 5,
            "FLOAT", // Float = 6,
            "IMAGE", // Image = 7,
            "INT", // Int = 8,
            "MONEY", // Money = 9,
            "NCHAR", // NChar = 10,
            "NTEXT", // NText = 11,
            "NVARCHAR", // NVarChar = 12,
            "REAL", // Real = 13,
            "UNIQUEIDENTIFIER", // UniqueIdentifier = 14,
            "SMALLDATETIME", // SmallDateTime = 15,
            "SMALLINT", // SmallInt = 16,
            "SMALLMONEY", // SmallMoney = 17,
            "TEXT", // Text = 18,
            "TIMESTAMP", // Timestamp = 19,
            "TINYINT", // TinyInt = 20,
            "VARBINARY", // VarBinary = 21,
            "VARCHAR", // VarChar = 22,
            "SQL_VARIANT", // Variant = 23,
            "Xml", // Xml = 24,
            "DATE", // Date = 25,
            "TIME", // Time = 26,
            "DATETIME2", // DateTime2 = 27,
            "DateTimeOffset" // DateTimeOffset = 28,
        };






        internal static VisitorState Compile(Token token)
        {
            var visitor = new SqlServerVisitor();
            var statement = token as IStatement;
            if (statement != null)
            {
                return visitor.Compile(statement);
            }
            visitor.VisitToken(token);
            return visitor.State;
        }

        public SqlServerVisitor()
        {
            this.IdentifierOpenQuote = "[";
            this.IdentifierCloseQuote = "]";
            this.LiteralOpenQuote = "N'";
            this.LiteralCloseQuote = "'";
            this.CommentOpenQuote = "/*";
            this.CommentCloseQuote = "*/";            
        }

        internal static Name GetTempTableName(Name name)
        {
            var namePart = name.LastPart;

            if (!namePart.StartsWith(Symbols.Pound))
            {
                namePart = Symbols.Pound + namePart;
            }
            return Sql.Name("tempdb", "", namePart);
        }

        internal static string GetTableVariableName(Name name)
        {
            var namePart = name.LastPart;

            if (!namePart.StartsWith(Symbols.At))
            {
                namePart = Symbols.At + namePart;
            }
            return namePart;
        }

        private void Stringify(IStatement statement)
        {
            Stringify(() => VisitStatement(statement));
        }

        private void Stringify(Action fragment)
        {
            State.WriteBeginStringify(LiteralOpenQuote, LiteralCloseQuote);
            fragment();
            State.WriteEndStringify();
        }





        private void VisitType(TypedToken typedToken)
        {
            if (typedToken.DbType.HasValue)
            {
                State.Write(DbTypeStrings[(int)typedToken.DbType]);
            }

            if (typedToken.Length.HasValue || typedToken.Precision.HasValue || typedToken.Scale.HasValue)
            {
                State.Write(Symbols.OpenParenthesis);
                if (typedToken.Length.HasValue)
                {
                    State.Write(typedToken.Length.Value == -1 ? Symbols.MAX : typedToken.Length.Value.ToString(CultureInfo.InvariantCulture));
                }
                else if (typedToken.Precision.HasValue)
                {
                    State.Write(typedToken.Precision.Value.ToString(CultureInfo.InvariantCulture));

                    if (typedToken.Scale.HasValue)
                    {
                        State.Write(Symbols.Comma);
                        State.Write(typedToken.Scale.Value.ToString(CultureInfo.InvariantCulture));
                    }
                }

                State.Write(Symbols.CloseParenthesis);
            }
        }


        #region Statements

        protected override void VisitJoinType(Joins join)
        {
            State.Write(JoinStrings[(int)join]);
        }

        protected override void VisitSet(SetStatement statement)
        {
            State.Write(Symbols.SET);

            if (statement.Assign != null)
            {
                VisitToken(statement.Assign);
            }
        }
        protected override void VisitMerge(MergeStatement statement)
        {
            VisitCommonTableExpressions(statement.CommonTableExpressions);

            State.Write(Symbols.MERGE);

            VisitTop(statement.Top);

            VisitInto(statement.Into);

            if (!string.IsNullOrWhiteSpace(statement.Into.Alias))
            {
                State.Write(Symbols.AS);
                State.Write(this.IdentifierOpenQuote,statement.Into.Alias, this.IdentifierCloseQuote);
            }

            State.Write(Symbols.USING);
            VisitToken(statement.Using);

            if ((statement.Using is IAliasToken) && !string.IsNullOrWhiteSpace(((IAliasToken)(statement.Using)).Alias))
            {
                State.Write(Symbols.AS);
                State.Write(this.IdentifierOpenQuote, ((IAliasToken)(statement.Using)).Alias, this.IdentifierCloseQuote);
            }

            State.Write(Symbols.ON);

            VisitToken(statement.On);

            foreach (var when in statement.WhenMatched)
            {
                State.Write(Symbols.WHEN);
                State.Write(Symbols.MATCHED);
                if (when.AndCondition != null)
                {
                    State.Write(Symbols.AND);
                    VisitToken(when.AndCondition);
                }
                State.Write(Symbols.THEN);

                VisitToken(when);
            }

            foreach (var when in statement.WhenNotMatched)
            {
                State.Write(Symbols.WHEN);
                State.Write(Symbols.NOT);
                State.Write(Symbols.MATCHED);
                State.Write(Symbols.BY);
                State.Write(Symbols.TARGET);
                if (when.AndCondition != null)
                {
                    State.Write(Symbols.AND);
                    VisitToken(when.AndCondition);
                }
                State.Write(Symbols.THEN);

                VisitToken(when);
            }

            foreach (var when in statement.WhenNotMatchedBySource)
            {
                State.Write(Symbols.WHEN);
                State.Write(Symbols.NOT);
                State.Write(Symbols.MATCHED);
                State.Write(Symbols.BY);
                State.Write(Symbols.SOURCE);
                if (when.AndCondition != null)
                {
                    State.Write(Symbols.AND);
                    VisitToken(when.AndCondition);
                }
                State.Write(Symbols.THEN);

                VisitToken(when);
            }

            VisitOutput(statement.Output, statement.OutputInto);
        }

        // ReSharper disable once UnusedParameter.Local
        protected override void VisitWhenMatchedThenDelete(WhenMatchedTokenThenDeleteToken token)
        {
            State.Write(Symbols.DELETE);
        }
        protected override void VisitWhenMatchedThenUpdateSet(WhenMatchedTokenThenUpdateSetToken token)
        {
            State.Write(Symbols.UPDATE);
            State.Write(Symbols.SET);
            VisitTokenSet(token.Set);
        }
        protected override void VisitWhenNotMatchedThenInsert(WhenNotMatchedTokenThenInsertToken token)
        {
            State.Write(Symbols.INSERT);
            if (token.Columns.Count > 0)
            {
                VisitTokenSetInParenthesis(token.Columns);
            }
            if (token.Values.Count > 0)
            {
                VisitTokenSetInParenthesis(token.Values, () => State.Write(Symbols.VALUES));
            }
            else
            {
                State.Write(Symbols.DEFAULT);
                State.Write(Symbols.VALUES);
            }
        }

        protected override void VisitSelect(SelectStatement statement)
        {
            VisitCommonTableExpressions(statement.CommonTableExpressions);

            State.Write(Symbols.SELECT);

            if (statement.Distinct)
            {
                State.Write(Symbols.DISTINCT);
            }

            if (statement.Offset == null)
            {
                VisitTop(statement.Top);
            }

            // assignments
            if (statement.Set.Count > 0)
            {
                VisitTokenSet(statement.Set);
            }
            else
            {
                // output columns
                if (statement.Output.Count == 0)
                {
                    State.Write(Symbols.Asterisk);
                }
                else
                {
                    VisitAliasedTokenSet(statement.Output, (string)null, Symbols.Comma, null);
                }
            }

            VisitInto(statement.Into);

            if (statement.From.Count > 0)
            {
                State.Write(Symbols.FROM);
                VisitFromToken(statement.From);
            }

            VisitJoin(statement.Joins);

            VisitWhereToken(statement.Where);

            VisitGroupByToken(statement.GroupBy);

            VisitHavingToken(statement.Having);

            VisitOrderByToken(statement.OrderBy);

            if (statement.Offset != null)
            {
                State.Write(Symbols.OFFSET);
                VisitToken(statement.Offset);
                State.Write(Symbols.ROWS);
                State.Write(Symbols.FETCH);
                State.Write(Symbols.NEXT);
                if (statement.Top.Value!=null)
                {
                    VisitToken(statement.Top.Value);
                }
                State.Write(Symbols.ROWS);
                State.Write(Symbols.ONLY);
            }

            //WITH CUBE or WITH ROLLUP
        }
        private void VisitInto(Name into)
        {
            if (into != null)
            {
                State.Write(Symbols.INTO);
                VisitNameToken(into);
            }
        }
        protected override void VisitDelete(DeleteStatement statement)
        {
            VisitCommonTableExpressions(statement.CommonTableExpressions);

            State.Write(Symbols.DELETE);

            VisitTop(statement.Top);

            // if 'FROM' has an alias or joins , we need to re-arrange tokens in a statement
            // even more, if it has joins, it MUST have an alias :)
            var hasAlias = statement.RecordsetSource != null &&
                           !string.IsNullOrWhiteSpace(statement.RecordsetSource.Alias);

            if (statement.Joins.Count > 0 || hasAlias)
            {
                if (hasAlias)
                {
                    State.Write(this.IdentifierOpenQuote, statement.RecordsetSource.Alias, this.IdentifierCloseQuote);
                }
                VisitOutput(statement.Output, statement.OutputInto);

                State.Write(Symbols.FROM);

                VisitFromToken(statement.RecordsetSource);

                VisitJoin(statement.Joins);

                VisitWhereToken(statement.Where);
            }
            else
            {
                State.Write(Symbols.FROM);

                VisitFromToken(statement.RecordsetSource);

                VisitOutput(statement.Output, statement.OutputInto);

                VisitWhereToken(statement.Where);
            }
        }
        protected override void VisitUpdate(UpdateStatement statement)
        {
            VisitCommonTableExpressions(statement.CommonTableExpressions);

            State.Write(Symbols.UPDATE);

            VisitTop(statement.Top);

            VisitToken(statement.Target, true);

            State.Write(Symbols.SET);
            VisitTokenSet(statement.Set);

            VisitOutput(statement.Output, statement.OutputInto);

            VisitFromToken(statement.RecordsetSource);

            VisitJoin(statement.Joins);

            VisitWhereToken(statement.Where);
        }
        protected override void VisitInsert(InsertStatement statement)
        {
            VisitCommonTableExpressions(statement.CommonTableExpressions);

            State.Write(Symbols.INSERT);

            VisitTop(statement.Top);

            VisitInto(statement.Into);

            VisitAliasedTokenSet(statement.Columns, Symbols.OpenParenthesis, Symbols.Comma, Symbols.CloseParenthesis);

            VisitOutput(statement.Output, statement.OutputInto);

            if (statement.DefaultValues)
            {
                State.Write(Symbols.DEFAULT);
                State.Write(Symbols.VALUES);

            }
            else if (statement.Values.Count > 0)
            {
                var separator = Symbols.VALUES;
                foreach (var valuesSet in statement.Values)
                {
                    State.Write(separator);
                    separator = Symbols.Comma;

                    VisitTokenSetInParenthesis(valuesSet);
                }
            }
            else if (statement.From != null)
            {
                VisitStatement(statement.From);
            }
        }

        protected override void VisitBeginTransaction(BeginTransactionStatement statement)
        {
            State.Write(Symbols.BEGIN);
            State.Write(Symbols.TRANSACTION);
            if (VisitTransactionName(statement) && !string.IsNullOrWhiteSpace(statement.Description))
            {
                State.Write(Symbols.WITH);
                State.Write(Symbols.MARK);
                State.Write(this.LiteralOpenQuote, statement.Description, this.LiteralCloseQuote);
            }
        }
        protected override void VisitCommitTransaction(CommitTransactionStatement statement)
        {
            State.Write(Symbols.COMMIT);
            State.Write(Symbols.TRANSACTION);
            VisitTransactionName(statement);
        }
        protected override void VisitRollbackTransaction(RollbackTransactionStatement statement)
        {
            State.Write(Symbols.ROLLBACK);
            State.Write(Symbols.TRANSACTION);
            VisitTransactionName(statement);
        }
        protected override void VisitSaveTransaction(SaveTransactionStatement statement)
        {
            State.Write(Symbols.SAVE);
            State.Write(Symbols.TRANSACTION);
            VisitTransactionName(statement);
        }

        protected override void VisitDeclareStatement(DeclareStatement statement)
        {
            if (statement.Variable != null)
            {
                State.Variables.Add(statement.Variable);

                State.Write(Symbols.DECLARE);
                State.Write(statement.Variable.Name);

                VisitType(statement.Variable);

                if (statement.Initializer != null)
                {
                    State.Write(Symbols.AssignVal);
                    VisitToken(statement.Initializer);
                }
            }
        }

        // ReSharper disable once UnusedParameter.Local
        protected override void VisitBreakStatement(BreakStatement statement)
        {
            State.Write(Symbols.BREAK);
        }

        // ReSharper disable once UnusedParameter.Local
        protected override void VisitContinueStatement(ContinueStatement statement)
        {
            State.Write(Symbols.CONTINUE);
        }
        protected override void VisitGotoStatement(GotoStatement statement)
        {
            State.Write(Symbols.GOTO);
            State.Write(statement.Label);
        }
        protected override void VisitReturnStatement(ReturnStatement statement)
        {
            State.Write(Symbols.RETURN);
            if (statement.ReturnExpression != null)
            {
                VisitToken(statement.ReturnExpression);
            }

        }
        protected override void VisitThrowStatement(ThrowStatement statement)
        {
            State.Write(Symbols.THROW);
            if (statement.ErrorNumber != null && statement.Message != null && statement.State != null)
            {
                VisitToken(statement.ErrorNumber);
                State.Write(Symbols.Comma);
                VisitToken(statement.Message);
                State.Write(Symbols.Comma);
                VisitToken(statement.State);
            }
        }
        protected override void VisitTryCatchStatement(TryCatchStatement stmt)
        {
            State.Write(Symbols.BEGIN);
            State.Write(Symbols.TRY);
            State.WriteCRLF();
            VisitStatement(stmt.TryStatement);
            State.WriteStatementTerminator();
            State.Write(Symbols.END);
            State.Write(Symbols.TRY);
            State.WriteCRLF();
            State.Write(Symbols.BEGIN);
            State.Write(Symbols.CATCH);
            State.WriteCRLF();
            if (stmt.CatchStatement != null)
            {
                VisitStatement(stmt.CatchStatement);
                State.WriteStatementTerminator();
            }
            State.Write(Symbols.END);
            State.Write(Symbols.CATCH);
            State.WriteStatementTerminator();
        }
        protected override void VisitLabelStatement(LabelStatement stmt)
        {
            State.Write(stmt.Label, Symbols.Colon);
        }
        protected override void VisitWaitforDelayStatement(WaitforDelayStatement stmt)
        {
            State.Write(Symbols.WAITFOR);
            State.Write(Symbols.DELAY);
            State.Write(LiteralOpenQuote, stmt.Delay.ToString("HH:mm:ss"), LiteralCloseQuote);
        }
        protected override void VisitWaitforTimeStatement(WaitforTimeStatement stmt)
        {
            State.Write(Symbols.WAITFOR);
            State.Write(Symbols.TIME);
            State.Write(LiteralOpenQuote, stmt.Time.ToString("yyyy-MM-ddTHH:mm:ss"), LiteralCloseQuote);
        }
        protected override void VisitWhileStatement(WhileStatement stmt)
        {
            if (stmt.Condition != null)
            {
                State.Write(Symbols.WHILE);
                VisitToken(stmt.Condition);

                if (stmt.Do != null)
                {
                    State.WriteCRLF();
                    State.Write(Symbols.BEGIN);
                    State.WriteStatementTerminator();

                    VisitStatement(stmt.Do);
                    State.WriteStatementTerminator();

                    State.Write(Symbols.END);
                    State.WriteStatementTerminator();
                }
            }
        }
        protected override void VisitIfStatement(IfStatement ifs)
        {
            if (ifs.Condition != null)
            {
                State.Write(Symbols.IF);
                VisitToken(ifs.Condition);

                if (ifs.Then != null)
                {
                    State.WriteCRLF();
                    State.Write(Symbols.BEGIN);
                    State.WriteStatementTerminator();

                    VisitStatement(ifs.Then);
                    State.WriteStatementTerminator();

                    State.Write(Symbols.END);
                    State.WriteStatementTerminator();

                    if (ifs.Else != null)
                    {
                        State.Write(Symbols.ELSE);
                        State.WriteCRLF();
                        State.Write(Symbols.BEGIN);
                        State.WriteStatementTerminator();

                        VisitStatement(ifs.Else);
                        State.WriteStatementTerminator();

                        State.Write(Symbols.END);
                        State.WriteStatementTerminator();
                    }
                }
            }
        }
        protected override void VisitDropTableStatement(DropTableStatement statement)
        {
            var tableName = ResolveName((statement.IsTemporary) ? GetTempTableName(statement.Name) : statement.Name);

            if (statement.CheckExists)
            {
                State.Write(Symbols.IF);
                State.Write(Symbols.OBJECT_ID);
                State.Write(Symbols.OpenParenthesis);
                State.Write(LiteralOpenQuote, tableName, LiteralCloseQuote);
                State.Write(Symbols.Comma);
                State.Write(LiteralOpenQuote, "U", LiteralCloseQuote);
                State.Write(Symbols.CloseParenthesis);
                State.Write(Symbols.IS);
                State.Write(Symbols.NOT);
                State.Write(Symbols.NULL);
            }

            State.Write(Symbols.DROP);
            State.Write(Symbols.TABLE);
            State.Write(tableName);
        }
        protected override void VisitCreateTableStatement(CreateTableStatement createStatement)
        {
            if (createStatement.IsTableVariable)
            {
                State.Write(Symbols.DECLARE);
                State.Write(GetTableVariableName(createStatement.Name));
                State.Write(Symbols.TABLE);
            }
            else
            {
                var tableName =
                    ResolveName((createStatement.IsTemporary) ? GetTempTableName(createStatement.Name) : createStatement.Name);

                if (createStatement.CheckIfNotExists)
                {
                    State.Write(Symbols.IF);
                    State.Write(Symbols.OBJECT_ID);
                    State.Write(Symbols.OpenParenthesis);
                    State.Write(LiteralOpenQuote, tableName, LiteralCloseQuote);
                    State.Write(Symbols.Comma);
                    State.Write(LiteralOpenQuote, "U", LiteralCloseQuote);
                    State.Write(Symbols.CloseParenthesis);
                    State.Write(Symbols.IS);
                    State.Write(Symbols.NULL);

                    State.WriteCRLF();
                    State.Write(Symbols.BEGIN);
                    State.WriteStatementTerminator();
                }

                State.Write(Symbols.CREATE);
                State.Write(Symbols.TABLE);
                State.Write(tableName);
                if (createStatement.AsFiletable)
                {
                    State.Write(Symbols.AS);
                    State.Write(Symbols.FILETABLE);
                }
            }


            var separator = Symbols.OpenParenthesis;
            foreach (var column in createStatement.Columns)
            {
                State.Write(separator);
                separator = Symbols.Comma;

                State.Write(this.IdentifierOpenQuote, column.Name, this.IdentifierCloseQuote);

                VisitType(column);

                if (column.Sparse)
                {
                    State.Write(Symbols.SPARSE);
                }
                if (column.Null.HasValue)
                {
                    if (!column.Null.Value)
                    {
                        State.Write(Symbols.NOT);
                    }
                    State.Write(Symbols.NULL);
                }
                if (column.Identity.On)
                {
                    State.Write(Symbols.IDENTITY);
                    State.Write(Symbols.OpenParenthesis);
                    State.Write(column.Identity.Seed.ToString(CultureInfo.InvariantCulture));
                    State.Write(Symbols.Comma);
                    State.Write(column.Identity.Increment.ToString(CultureInfo.InvariantCulture));
                    State.Write(Symbols.CloseParenthesis);
                }
                if (column.RowGuid)
                {
                    State.Write(Symbols.ROWGUIDCOL);
                }
                if (column.DefaultValue != null)
                {
                    State.Write(Symbols.DEFAULT);
                    State.Write(Symbols.OpenParenthesis);
                    VisitToken(column.DefaultValue);
                    State.Write(Symbols.CloseParenthesis);
                }
            }

            if (createStatement.PrimaryKey != null)
            {
                State.Write(Symbols.Comma);
                if (!createStatement.IsTableVariable)
                {
                    State.Write(Symbols.CONSTRAINT);
                    VisitNameToken(createStatement.PrimaryKey.Name);
                }

                State.Write(Symbols.PRIMARY);
                State.Write(Symbols.KEY);
                VisitTokenSetInParenthesis(createStatement.PrimaryKey.Columns);
            }

            foreach (var unique in createStatement.UniqueConstrains)
            {
                State.Write(Symbols.Comma);
                if (!createStatement.IsTableVariable)
                {
                    State.Write(Symbols.CONSTRAINT);
                    VisitNameToken(unique.Name);
                }

                State.Write(Symbols.UNIQUE);
                if (unique.Clustered.HasValue)
                {
                    State.Write(unique.Clustered.Value ? Symbols.CLUSTERED : Symbols.NONCLUSTERED);
                }
                VisitTokenSetInParenthesis(unique.Columns);
            }

            State.Write(Symbols.CloseParenthesis);
            State.WriteStatementTerminator();

            // if indecies are set, create them
            if (createStatement.Indicies.Count > 0 && !createStatement.IsTableVariable)
            {

                foreach (var createIndexStatement in createStatement.Indicies)
                {
                    VisitCreateIndexStatement(createIndexStatement);
                    State.WriteStatementTerminator();
                }
            }

            if (createStatement.CheckIfNotExists && !createStatement.IsTableVariable)
            {
                State.Write(Symbols.END);
            }
        }

        protected override void VisitStringifyStatement(StringifyStatement statement)
        {
            this.Stringify(statement.Content);
        }
        protected override void VisitExecuteStatement(ExecuteStatement statement)
        {
            State.Write(Symbols.EXEC);
            State.Write(Symbols.OpenParenthesis);
            this.Stringify(statement.Target);
            State.Write(Symbols.CloseParenthesis);
        }
        protected override void VisitCreateIndexStatement(CreateIndexStatement createIndexStatement)
        {
            State.Write(Symbols.CREATE);

            if (createIndexStatement.Unique)
            {
                State.Write(Symbols.UNIQUE);
            }

            if (createIndexStatement.Clustered.HasValue)
            {
                State.Write(createIndexStatement.Clustered.Value ? Symbols.CLUSTERED : Symbols.NONCLUSTERED);
            }
            State.Write(Symbols.INDEX);

            VisitToken(createIndexStatement.Name);

            State.Write(Symbols.ON);

            VisitToken(createIndexStatement.On);

            // columns
            VisitTokenSetInParenthesis(createIndexStatement.Columns);

            VisitTokenSetInParenthesis(createIndexStatement.Include, ()=>State.Write(Symbols.INCLUDE));

            VisitWhereToken(createIndexStatement.Where);

            if (createIndexStatement.With.IsDefined)
            {
                State.Write(Symbols.WITH);
                State.Write(Symbols.OpenParenthesis);

                VisitWith(createIndexStatement.With.PadIndex, Symbols.PAD_INDEX);
                VisitWith(createIndexStatement.With.Fillfactor, Symbols.FILLFACTOR);
                VisitWith(createIndexStatement.With.SortInTempdb, Symbols.SORT_IN_TEMPDB);
                VisitWith(createIndexStatement.With.IgnoreDupKey, Symbols.IGNORE_DUP_KEY);
                VisitWith(createIndexStatement.With.StatisticsNorecompute, Symbols.STATISTICS_NORECOMPUTE);
                VisitWith(createIndexStatement.With.DropExisting, Symbols.DROP_EXISTING);
                VisitWith(createIndexStatement.With.Online, Symbols.ONLINE);
                VisitWith(createIndexStatement.With.AllowRowLocks, Symbols.ALLOW_ROW_LOCKS);
                VisitWith(createIndexStatement.With.AllowPageLocks, Symbols.ALLOW_PAGE_LOCKS);
                VisitWith(createIndexStatement.With.MaxDegreeOfParallelism, Symbols.MAXDOP);

                State.Write(Symbols.CloseParenthesis);
            }

            State.Write(Symbols.Semicolon);
        }
        protected override void VisitAlterIndexStatement(AlterIndexStatement alterStatement)
        {
            State.Write(Symbols.ALTER);
            State.Write(Symbols.INDEX);

            if (alterStatement.Name == null)
            {
                State.Write(Symbols.ALL);
            }
            else
            {
                VisitToken(alterStatement.Name);
            }

            State.Write(Symbols.ON);

            VisitToken(alterStatement.On);

            if (alterStatement.Rebuild)
            {
                State.Write(Symbols.REBUILD);

                //TODO: [PARTITION = ALL]
                if (alterStatement.RebuildWith.IsDefined)
                {
                    State.Write(Symbols.WITH);
                    State.Write(Symbols.OpenParenthesis);

                    VisitWith(alterStatement.RebuildWith.PadIndex, Symbols.PAD_INDEX);
                    VisitWith(alterStatement.RebuildWith.Fillfactor, Symbols.FILLFACTOR);
                    VisitWith(alterStatement.RebuildWith.SortInTempdb, Symbols.SORT_IN_TEMPDB);
                    VisitWith(alterStatement.RebuildWith.IgnoreDupKey, Symbols.IGNORE_DUP_KEY);
                    VisitWith(alterStatement.RebuildWith.StatisticsNorecompute, Symbols.STATISTICS_NORECOMPUTE);
                    VisitWith(alterStatement.RebuildWith.DropExisting, Symbols.DROP_EXISTING);
                    VisitWith(alterStatement.RebuildWith.Online, Symbols.ONLINE);
                    VisitWith(alterStatement.RebuildWith.AllowRowLocks, Symbols.ALLOW_ROW_LOCKS);
                    VisitWith(alterStatement.RebuildWith.AllowPageLocks, Symbols.ALLOW_PAGE_LOCKS);
                    VisitWith(alterStatement.RebuildWith.MaxDegreeOfParallelism, Symbols.MAXDOP);

                    State.Write(Symbols.CloseParenthesis);
                }
            }
            else if (alterStatement.Disable)
            {
                State.Write(Symbols.DISABLE);
            }
            else if (alterStatement.Reorganize)
            {
                State.Write(Symbols.REORGANIZE);
            }
            else
            {
                VisitWith(alterStatement.Set.AllowRowLocks, Symbols.ALLOW_ROW_LOCKS);
                VisitWith(alterStatement.Set.AllowPageLocks, Symbols.ALLOW_PAGE_LOCKS);
                VisitWith(alterStatement.Set.IgnoreDupKey, Symbols.IGNORE_DUP_KEY);
                VisitWith(alterStatement.Set.StatisticsNorecompute, Symbols.STATISTICS_NORECOMPUTE);
            }
        }
        protected override void VisitDropIndexStatement(DropIndexStatement dropIndexStatement)
        {
            State.Write(Symbols.DROP);
            State.Write(Symbols.INDEX);
            VisitToken(dropIndexStatement.Name);

            State.Write(Symbols.ON);

            VisitToken(dropIndexStatement.On);

            if (dropIndexStatement.With.IsDefined)
            {
                State.Write(Symbols.WITH);
                State.Write(Symbols.OpenParenthesis);

                if (dropIndexStatement.With.Online.HasValue)
                {
                    State.Write(Symbols.ONLINE);
                    State.Write(Symbols.AssignVal);
                    State.Write(dropIndexStatement.With.Online.Value ? Symbols.ON : Symbols.OFF);
                }
                if (dropIndexStatement.With.MaxDegreeOfParallelism.HasValue)
                {
                    State.Write(Symbols.MAXDOP);
                    State.Write(Symbols.AssignVal);
                    State.Write(dropIndexStatement.With.MaxDegreeOfParallelism.Value.ToString(CultureInfo.InvariantCulture));
                }

                State.Write(Symbols.CloseParenthesis);
            }
        }
        protected override void VisitCreateViewStatement(CreateViewStatement createStatement)
        {
            var viewName = ResolveName(createStatement.Name);

            if (createStatement.CheckIfNotExists)
            {
                State.Write(Symbols.IF);
                State.Write(Symbols.OBJECT_ID);
                State.Write(Symbols.OpenParenthesis);
                State.Write(LiteralOpenQuote, viewName, LiteralCloseQuote);
                State.Write(Symbols.CloseParenthesis);
                State.Write(Symbols.IS);
                State.Write(Symbols.NULL);
                State.Write(Symbols.EXEC);
                State.Write(Symbols.OpenParenthesis);

                Stringify(() =>
                {
                    State.Write(Symbols.CREATE);
                    State.Write(Symbols.VIEW);
                    State.Write(viewName);
                    State.Write(Symbols.AS);
                    VisitStatement(createStatement.DefinitionStatement);
                });
                State.Write(Symbols.CloseParenthesis);
            }
            else
            {
                State.Write(Symbols.CREATE);
                State.Write(Symbols.VIEW);
                State.Write(viewName);
                State.Write(Symbols.AS);
                VisitStatement(createStatement.DefinitionStatement);
            }
        }
        protected override void VisitCreateOrAlterViewStatement(CreateOrAlterViewStatement createStatement)
        {
            var viewName = ResolveName(createStatement.Name);
            State.Write(Symbols.IF);
            State.Write(Symbols.OBJECT_ID);
            State.Write(Symbols.OpenParenthesis);
            State.Write(LiteralOpenQuote, viewName, LiteralCloseQuote);
            State.Write(Symbols.CloseParenthesis);
            State.Write(Symbols.IS);
            State.Write(Symbols.NULL);
            State.Write(Symbols.EXEC);
            State.Write(Symbols.OpenParenthesis);

            Stringify(() =>
            {
                State.Write(Symbols.CREATE);
                State.Write(Symbols.VIEW);
                State.Write(viewName);
                State.Write(Symbols.AS);
                VisitStatement(createStatement.DefinitionStatement);
            });

            State.Write(Symbols.CloseParenthesis);
            State.WriteStatementTerminator();
            State.Write(Symbols.ELSE);
            State.Write(Symbols.EXEC);
            State.Write(Symbols.OpenParenthesis);

            Stringify(() =>
            {
                State.Write(Symbols.ALTER);
                State.Write(Symbols.VIEW);
                State.Write(viewName);
                State.Write(Symbols.AS);
                VisitStatement(createStatement.DefinitionStatement);
            });

            State.Write(Symbols.CloseParenthesis);
        }

        protected override void VisitDropViewStatement(DropViewStatement statement)
        {
            var viewName = ResolveName(statement.Name);

            if (statement.CheckExists)
            {
                State.Write(Symbols.IF);
                State.Write(Symbols.OBJECT_ID);
                State.Write(Symbols.OpenParenthesis);
                State.Write(LiteralOpenQuote, viewName, LiteralCloseQuote);
                State.Write(Symbols.CloseParenthesis);
                State.Write(Symbols.IS);
                State.Write(Symbols.NOT);
                State.Write(Symbols.NULL);
                State.Write(Symbols.EXEC);
                State.Write(Symbols.OpenParenthesis);

                Stringify(() =>
                {
                    State.Write(Symbols.DROP);
                    State.Write(Symbols.VIEW);
                    State.Write(viewName);
                    State.WriteStatementTerminator(false);
                });

                State.Write(Symbols.CloseParenthesis);
            }
            else
            {
                State.Write(Symbols.DROP);
                State.Write(Symbols.VIEW);
                VisitNameToken(statement.Name);
            }
        }

        protected override void VisitAlterViewStatement(AlterViewStatement statement)
        {
            State.Write(Symbols.ALTER);
            State.Write(Symbols.VIEW);
            VisitNameToken(statement.Name);
            State.Write(Symbols.AS);
            VisitStatement(statement.DefinitionStatement);
        }


        #endregion Statements

        private void VisitOutput(IEnumerable<ExpressionToken> columns, Name outputInto)
        {
            VisitAliasedTokenSet(columns, Symbols.OUTPUT, Symbols.Comma, null);
            if (outputInto != null)
            {
                State.Write(Symbols.INTO);
                VisitNameToken(outputInto);
            }

        }

        private void VisitTop(Top top)
        {
            if (top != null)
            {
                State.Write(Symbols.TOP);
                State.Write(Symbols.OpenParenthesis);
                if (top.Value != null)
                {
                    VisitToken(top.Value);
                }
                State.Write(Symbols.CloseParenthesis);

                if (top.Percent)
                {
                    State.Write(Symbols.PERCENT);
                }
                if (top.WithTies)
                {
                    State.Write(Symbols.WITH);
                    State.Write(Symbols.TIES);
                }
            }
        }




        private void VisitWith(bool? value, string name)
        {
            if (value.HasValue)
            {
                State.Write(name);
                State.Write(Symbols.AssignVal);
                State.Write(value.Value ? Symbols.ON : Symbols.OFF);
            }
        }

        private void VisitWith(int? value, string name)
        {
            if (value.HasValue)
            {
                State.Write(name);
                State.Write(Symbols.AssignVal);
                State.Write(value.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        protected override void VisitStringifyToken(StringifyToken token)
        {
            Stringify(() => VisitToken(token.Content));
        }
    }
}
