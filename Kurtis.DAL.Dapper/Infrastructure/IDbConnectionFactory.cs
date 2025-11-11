
using System.Data;

namespace Kurtis.DAL.Dapper.Infrastructure
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}
