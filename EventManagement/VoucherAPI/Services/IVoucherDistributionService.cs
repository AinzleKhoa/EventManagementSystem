using System.Threading.Tasks;
using VoucherAPI.DTOs;

namespace VoucherAPI.Services
{
    public interface IVoucherDistributionService
    {
        Task DistributeToUserByEmail(VoucherDistributeByEmailDTO dto);
    }
}
