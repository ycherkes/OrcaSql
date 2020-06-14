namespace OrcaSql.Core.MetaData.DMVs
{
    public interface IDataType
    {
        bool IsNullable { get; }
        byte SystemTypeID { get; }
        int UserTypeID { get; }
        short MaxLength { get; }
        byte Precision { get; }
        byte Scale { get; }
    }
}