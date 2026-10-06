using System.Collections.Generic;
using System.Threading.Tasks;
using RAIZA.Models;

namespace RAIZA.Interfaces
{
    public interface CompraI
    {
        Task<List<Compra>> GetCompras();
        Task<Compra?> GetCompraById(int id);
        Task<bool> CreateCompra(Compra compra);
        Task<bool> UpdateCompra(Compra compra);
        Task<bool> DeleteCompra(int id);
    }
}