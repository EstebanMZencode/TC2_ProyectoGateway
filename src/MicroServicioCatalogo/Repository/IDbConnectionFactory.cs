using System.Data;

namespace MicroServicioCatalogo.Repository
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
