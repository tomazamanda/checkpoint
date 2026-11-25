namespace checkpoint.Interface
{
    public interface IDao<T>
    {
        void Insert(T t);
        void Update(T t);
        void Delete(int id);
        List<T> List();
        T BuscarID(int id);
    }
}