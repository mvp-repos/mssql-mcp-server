using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace SqlMcpServer.Server.Models
{
    /// <summary>
    /// A visitor class for traversing and analyzing T-SQL fragments in a safe manner, ensuring 
    /// that only allowed SQL constructs are processed.
    /// </summary>
    internal sealed class SafeQueryVisitor : TSqlFragmentVisitor
    {
        /// <summary>
        /// Gets the list of error messages generated during the traversal.
        /// </summary>
        public List<string> Errors { get; } = [];

        // SELECT ... INTO NewTable
        //public override void Visit(QuerySpecification node)
        //{
        //    if (node.Into != null)
        //        Errors.Add("SELECT INTO is not allowed.");

        //    base.Visit(node);
        //}

        // SELECT ... FOR XML
        public override void Visit(XmlForClause node)
            => Errors.Add("FOR XML is not allowed.");

        // SELECT ... FOR JSON
        public override void Visit(JsonForClause node)
            => Errors.Add("FOR JSON is not allowed.");

        // OPENROWSET(...)
        public override void Visit(OpenRowsetTableReference node)
            => Errors.Add("OPENROWSET is not allowed.");

        // OPENQUERY(...)
        public override void Visit(OpenQueryTableReference node)
            => Errors.Add("OPENQUERY is not allowed.");

        // OPENDATASOURCE(...)
        //public override void Visit(OpenTableReference node)
        //    => Errors.Add("OPENDATASOURCE/OPENROWSET is not allowed.");

        // OPENXML(...)
        public override void Visit(OpenXmlTableReference node)
            => Errors.Add("OPENXML is not allowed.");

        // Query hints (OPTION (...))
        public override void Visit(OptimizerHint node)
            => Errors.Add("Query hints are not allowed.");

        // WAITFOR inside a batch
        public override void Visit(WaitForStatement node)
            => Errors.Add("WAITFOR is not allowed.");

        // EXEC inside expressions (rare but possible)
        public override void Visit(ExecutableProcedureReference node)
            => Errors.Add("Stored procedure execution is not allowed.");
    }
}
