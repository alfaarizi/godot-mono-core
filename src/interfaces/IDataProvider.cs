public interface IDataProvider<TData> where TData : class
{
    TData? Data { get; set; }
    void SetData(TData? data);
}